using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using QuotationApi.Data;
using QuotationApi.Models.DTOs;
using QuotationApi.Models.Entities;
using QuotationApi.Models.Events;

namespace QuotationApi.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _repository;
        private readonly IQuotationValidationService _validationService;
        private readonly ILogger<QuotationService> _logger;
        private readonly DaprClient _daprClient;
        private readonly QuotationDbContext _context;

        public QuotationService(
            IQuotationRepository repository,
            IQuotationValidationService validationService,
            ILogger<QuotationService> logger,
            DaprClient daprClient,
            QuotationDbContext context)
        {
            _repository = repository;
            _validationService = validationService;
            _logger = logger;
            _daprClient = daprClient;
            _context = context;
        }

        public async Task<QuotationDetailResponse> CreateQuotationAsync(CreateQuotationRequest request)
        {
            // 验证请求数据
            var validationResult = await _validationService.ValidateQuotationAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(string.Join("; ", validationResult.Errors));
            }

            // 生成报价单号
            var quotationNumber = await GenerateQuotationNumberAsync();

            var quotation = new Quotation
            {
                QuotationNumber = quotationNumber,
                DemandId = request.DemandId,
                SupplierId = request.SupplierId,
                BearingNumber = request.BearingNumber,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                TotalAmount = request.UnitPrice * request.Quantity,
                Currency = request.Currency,
                DeliveryDays = request.DeliveryDays,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(request.DeliveryDays),
                DeliveryAddress = request.DeliveryAddress,
                Incoterms = request.Incoterms,
                QualityStandard = request.QualityStandard,
                Notes = request.Notes,
                ExpiresAt = request.ExpiresAt,
                Type = Enum.TryParse<QuotationType>(request.Type, out var type) ? type : QuotationType.Standard,
                Status = QuotationStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 计算匹配度
            quotation.MatchScore = await _validationService.CalculateMatchScoreAsync(quotation, request.DemandId);
            quotation.IsRecommended = quotation.MatchScore >= 0.7m;

            var result = await _repository.AddAsync(quotation);

            // 发布事件
            await PublishQuotationCreatedEventAsync(result);

            _logger.LogInformation("报价单创建成功: {QuotationNumber}", quotationNumber);

            return MapToDetailResponse(result);
        }

        public async Task<PagedResponse<QuotationResponse>> SearchQuotationsAsync(QuotationSearchRequest request)
        {
            var result = await _repository.SearchAsync(request);

            return new PagedResponse<QuotationResponse>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }

        public async Task<List<QuotationResponse>> GetRecommendedQuotationsAsync(long demandId, int limit = 10)
        {
            var quotations = await _repository.GetRecommendedQuotationsAsync(demandId, limit);
            return quotations.Select(MapToResponse).ToList();
        }

        public async Task<bool> SendQuotationRemindersAsync()
        {
            var expiringQuotations = await _repository.GetExpiringQuotationsAsync(DateTime.UtcNow.AddDays(3));

            foreach (var quotation in expiringQuotations)
            {
                try
                {
                    await _daprClient.PublishEventAsync("pubsub", "quotation-reminder", new
                    {
                        QuotationId = quotation.Id,
                        QuotationNumber = quotation.QuotationNumber,
                        SupplierId = quotation.SupplierId,
                        ExpiresAt = quotation.ExpiresAt,
                        ReminderSentAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "发送报价提醒失败: {QuotationId}", quotation.Id);
                }
            }

            return expiringQuotations.Any();
        }

        public async Task<QuotationItemResponse> AddQuotationItemAsync(long quotationId, AddQuotationItemRequest request)
        {
            var quotation = await _repository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            var item = new QuotationItem
            {
                QuotationId = quotationId,
                BearingNumber = request.BearingNumber,
                Description = request.Description,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                Brand = request.Brand,
                Material = request.Material,
                Standard = request.Standard,
                DisplayOrder = request.DisplayOrder
            };

            var result = await _repository.AddQuotationItemAsync(item);

            // 重新计算总金额
            await RecalculateQuotationTotalAsync(quotationId);

            return new QuotationItemResponse
            {
                Id = result.Id,
                BearingNumber = result.BearingNumber,
                Description = result.Description,
                Quantity = result.Quantity,
                UnitPrice = result.UnitPrice,
                TotalPrice = result.TotalPrice,
                Brand = result.Brand,
                Material = result.Material,
                Standard = result.Standard,
                DisplayOrder = result.DisplayOrder,
                CreatedAt = result.CreatedAt
            };
        }

        // 私有辅助方法
        private async Task<string> GenerateQuotationNumberAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.Quotations
                .CountAsync(q => q.QuotationNumber.StartsWith($"QT{today}"));

            return $"QT{today}{count + 1:0000}";
        }

        private async Task RecalculateQuotationTotalAsync(long quotationId)
        {
            var quotation = await _repository.GetByIdAsync(quotationId);
            if (quotation == null) return;

            var items = await _repository.GetQuotationItemsAsync(quotationId);
            quotation.TotalAmount = items.Sum(i => i.TotalPrice);

            await _repository.UpdateAsync(quotation);
        }

        private async Task<long> GetQuotationIdFromResponse(QuotationDetailResponse response)
        {
            var quotation = await _repository.GetByQuotationNumberAsync(response.QuotationNumber);
            return quotation?.Id ?? 0;
        }

        private string GetWeekKey(DateTime date)
        {
            var week = System.Globalization.ISOWeek.GetWeekOfYear(date);
            return $"{date.Year}-W{week:00}";
        }

        // 事件发布方法
        private async Task PublishQuotationCreatedEventAsync(Quotation quotation)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "-created", new
                {
                    QuotationId = quotation.Id,
                    QuotationNumber = quotation.QuotationNumber,
                    DemandId = quotation.DemandId,
                    SupplierId = quotation.SupplierId,
                    BearingNumber = quotation.BearingNumber,
                    UnitPrice = quotation.UnitPrice,
                    Quantity = quotation.Quantity,
                    TotalAmount = quotation.TotalAmount,
                    Status = quotation.Status.ToString(),
                    CreatedAt = quotation.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布报价创建事件失败");
            }
        }

        // 映射方法
        private QuotationResponse MapToResponse(Quotation quotation)
        {
            return new QuotationResponse
            {
                Id = quotation.Id,
                QuotationNumber = quotation.QuotationNumber,
                DemandId = quotation.DemandId,
                SupplierId = quotation.SupplierId,
                SupplierName = quotation.SupplierName,
                SupplierContact = quotation.SupplierContact,
                BearingNumber = quotation.BearingNumber,
                BearingName = quotation.BearingName,
                Brand = quotation.Brand,
                UnitPrice = quotation.UnitPrice,
                Quantity = quotation.Quantity,
                TotalAmount = quotation.TotalAmount,
                Currency = quotation.Currency,
                DeliveryDays = quotation.DeliveryDays,
                EstimatedDeliveryDate = quotation.EstimatedDeliveryDate,
                DeliveryAddress = quotation.DeliveryAddress,
                Incoterms = quotation.Incoterms,
                QualityStandard = quotation.QualityStandard,
                WarrantyMonths = quotation.WarrantyMonths,
                Status = quotation.Status.ToString(),
                Type = quotation.Type.ToString(),
                IsRecommended = quotation.IsRecommended,
                MatchScore = quotation.MatchScore,
                Notes = quotation.Notes,
                CertificateRequirements = quotation.CertificateRequirements,  // 添加这个属性
                CreatedAt = quotation.CreatedAt,
                UpdatedAt = quotation.UpdatedAt,
                ExpiresAt = quotation.ExpiresAt,
                SubmittedAt = quotation.SubmittedAt,
                AcceptedAt = quotation.AcceptedAt,
                RejectedAt = quotation.RejectedAt
            };
        }

        private QuotationDetailResponse MapToDetailResponse(Quotation quotation)
        {
            var detailResponse = new QuotationDetailResponse
            {
                Id = quotation.Id,
                QuotationNumber = quotation.QuotationNumber,
                DemandId = quotation.DemandId,
                SupplierId = quotation.SupplierId,
                SupplierName = quotation.SupplierName,
                SupplierContact = quotation.SupplierContact,
                SupplierPhone = quotation.SupplierPhone,
                SupplierEmail = quotation.SupplierEmail,
                BearingNumber = quotation.BearingNumber,
                BearingName = quotation.BearingName,
                Brand = quotation.Brand,
                UnitPrice = quotation.UnitPrice,
                Quantity = quotation.Quantity,
                TotalAmount = quotation.TotalAmount,
                Currency = quotation.Currency,
                DeliveryDays = quotation.DeliveryDays,
                EstimatedDeliveryDate = quotation.EstimatedDeliveryDate,
                DeliveryAddress = quotation.DeliveryAddress,
                Incoterms = quotation.Incoterms,
                QualityStandard = quotation.QualityStandard,
                WarrantyMonths = quotation.WarrantyMonths,
                Status = quotation.Status.ToString(),
                Type = quotation.Type.ToString(),
                IsRecommended = quotation.IsRecommended,
                MatchScore = quotation.MatchScore,
                Notes = quotation.Notes,
                CertificateRequirements = quotation.CertificateRequirements,
                CreatedAt = quotation.CreatedAt,
                UpdatedAt = quotation.UpdatedAt,
                ExpiresAt = quotation.ExpiresAt,
                SubmittedAt = quotation.SubmittedAt,
                AcceptedAt = quotation.AcceptedAt,
                RejectedAt = quotation.RejectedAt
            };

            return detailResponse;
        }

        public async Task<QuotationDetailResponse?> GetQuotationAsync(long id)
        {
            var quotation = await _repository.GetByIdAsync(id);
            return quotation != null ? MapToDetailResponse(quotation) : null;
        }

        public async Task<QuotationDetailResponse?> GetQuotationByNumberAsync(string quotationNumber)
        {
            var quotation = await _repository.GetByQuotationNumberAsync(quotationNumber);
            return quotation != null ? MapToDetailResponse(quotation) : null;
        }

        public async Task<PagedResponse<QuotationResponse>> GetQuotationsAsync(QuotationQuery query)
        {
            var quotations = await _repository.GetListAsync(query);
            var totalCount = await _repository.GetCountAsync(query);

            return new PagedResponse<QuotationResponse>
            {
                Items = quotations.Select(MapToResponse).ToList(),
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<QuotationDetailResponse> UpdateQuotationAsync(long id, UpdateQuotationRequest request)
        {
            var quotation = await _repository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            // 验证更新
            var validationResult = await _validationService.ValidateQuotationUpdateAsync(id, request);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(string.Join("; ", validationResult.Errors));
            }

            // 更新字段
            if (request.UnitPrice.HasValue)
            {
                quotation.UnitPrice = request.UnitPrice.Value;
                quotation.TotalAmount = quotation.UnitPrice * quotation.Quantity;
            }

            if (request.Quantity.HasValue)
            {
                quotation.Quantity = request.Quantity.Value;
                quotation.TotalAmount = quotation.UnitPrice * quotation.Quantity;
            }

            if (!string.IsNullOrEmpty(request.Currency))
                quotation.Currency = request.Currency;

            if (request.DeliveryDays.HasValue)
            {
                quotation.DeliveryDays = request.DeliveryDays.Value;
                quotation.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(quotation.DeliveryDays);
            }

            if (!string.IsNullOrEmpty(request.DeliveryAddress))
                quotation.DeliveryAddress = request.DeliveryAddress;

            if (!string.IsNullOrEmpty(request.Incoterms))
                quotation.Incoterms = request.Incoterms;

            if (!string.IsNullOrEmpty(request.QualityStandard))
                quotation.QualityStandard = request.QualityStandard;

            if (!string.IsNullOrEmpty(request.Notes))
                quotation.Notes = request.Notes;

            if (request.ExpiresAt.HasValue)
                quotation.ExpiresAt = request.ExpiresAt.Value;

            quotation.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(quotation);
            return MapToDetailResponse(result);
        }

        public async Task<bool> DeleteQuotationAsync(long id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<QuotationDetailResponse> SubmitQuotationAsync(long id, string? submissionNotes = null)
        {
            var quotation = await _repository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            // 验证提交
            var validationResult = await _validationService.ValidateQuotationSubmissionAsync(id);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", validationResult.Errors));
            }

            quotation.Status = QuotationStatus.Submitted;
            quotation.SubmittedAt = DateTime.UtcNow;
            quotation.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(submissionNotes))
                quotation.Notes = submissionNotes;

            var result = await _repository.UpdateAsync(quotation);

            // 发布提交事件
            await PublishQuotationSubmittedEventAsync(result);

            _logger.LogInformation("报价单已提交: {QuotationNumber}", quotation.QuotationNumber);

            return MapToDetailResponse(result);
        }

        public async Task<QuotationDetailResponse> AcceptQuotationAsync(long id, long customerId)
        {
            var quotation = await _repository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            // 验证接受
            var validationResult = await _validationService.ValidateQuotationAcceptanceAsync(id, customerId);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", validationResult.Errors));
            }

            quotation.Status = QuotationStatus.Accepted;
            quotation.AcceptedAt = DateTime.UtcNow;
            quotation.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(quotation);

            // 发布接受事件
            await PublishQuotationAcceptedEventAsync(result);

            _logger.LogInformation("报价单已被接受: {QuotationNumber}", quotation.QuotationNumber);

            return MapToDetailResponse(result);
        }

        public async Task<QuotationDetailResponse> RejectQuotationAsync(long id, string? rejectionReason = null)
        {
            var quotation = await _repository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            quotation.Status = QuotationStatus.Rejected;
            quotation.RejectedAt = DateTime.UtcNow;
            quotation.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(rejectionReason))
                quotation.Notes = rejectionReason;

            var result = await _repository.UpdateAsync(quotation);
            return MapToDetailResponse(result);
        }

        public async Task<QuotationDetailResponse> WithdrawQuotationAsync(long id, string? reason = null)
        {
            var quotation = await _repository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            quotation.Status = QuotationStatus.Withdrawn;
            quotation.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(reason))
                quotation.Notes = reason;

            var result = await _repository.UpdateAsync(quotation);
            return MapToDetailResponse(result);
        }

        public async Task<QuotationDetailResponse> ExpireQuotationAsync(long id)
        {
            var quotation = await _repository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            quotation.Status = QuotationStatus.Expired;
            quotation.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(quotation);

            // 发布过期事件
            await PublishQuotationExpiredEventAsync(result);

            _logger.LogInformation("报价单已过期: {QuotationNumber}", quotation.QuotationNumber);

            return MapToDetailResponse(result);
        }

        public async Task<QuotationDetailResponse> SetRecommendedAsync(long id, bool isRecommended, decimal? matchScore = null)
        {
            var quotation = await _repository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            quotation.IsRecommended = isRecommended;
            if (matchScore.HasValue)
                quotation.MatchScore = matchScore.Value;

            quotation.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(quotation);
            return MapToDetailResponse(result);
        }

        public async Task<List<QuotationResponse>> GetQuotationsByDemandAsync(long demandId)
        {
            var quotations = await _repository.GetByDemandIdAsync(demandId);
            return quotations.Select(MapToResponse).ToList();
        }

        public async Task<List<QuotationResponse>> GetQuotationsBySupplierAsync(long supplierId)
        {
            var quotations = await _repository.GetBySupplierIdAsync(supplierId);
            return quotations.Select(MapToResponse).ToList();
        }

        public async Task<List<QuotationResponse>> GetPendingQuotationsAsync()
        {
            var quotations = await _repository.GetPendingQuotationsAsync();
            return quotations.Select(MapToResponse).ToList();
        }

        public async Task<List<QuotationResponse>> GetExpiredQuotationsAsync()
        {
            var cutoffDate = DateTime.UtcNow;
            var quotations = await _repository.GetExpiredQuotationsAsync(cutoffDate);
            return quotations.Select(MapToResponse).ToList();
        }

        public async Task<QuotationComparisonResponse> CompareQuotationsAsync(long demandId, string bearingNumber)
        {
            var quotations = await _repository.GetByDemandIdAsync(demandId);
            var filteredQuotations = quotations
                .Where(q => q.BearingNumber == bearingNumber &&
                           (q.Status == QuotationStatus.Submitted || q.Status == QuotationStatus.Accepted))
                .ToList();

            var comparison = new QuotationComparisonResponse
            {
                DemandId = demandId,
                BearingNumber = bearingNumber,
                TotalSuppliers = filteredQuotations.Count,
                LowestPrice = filteredQuotations.Any() ? filteredQuotations.Min(q => q.UnitPrice) : null,
                HighestPrice = filteredQuotations.Any() ? filteredQuotations.Max(q => q.UnitPrice) : null,
                AveragePrice = filteredQuotations.Any() ? filteredQuotations.Average(q => q.UnitPrice) : null
            };

            comparison.SupplierQuotations = filteredQuotations.Select(q => new SupplierQuotation
            {
                SupplierId = q.SupplierId,
                SupplierName = q.SupplierName ?? "未知供应商",
                UnitPrice = q.UnitPrice,
                DeliveryDays = q.DeliveryDays,
                QualityStandard = q.QualityStandard,
                WarrantyMonths = q.WarrantyMonths,
                MatchScore = q.MatchScore,
                IsRecommended = q.IsRecommended,
                Status = q.Status.ToString()
            }).OrderBy(sq => sq.UnitPrice).ToList();

            return comparison;
        }

        public async Task<QuotationStatisticsResponse> GetQuotationStatisticsAsync()
        {
            var totalQuotations = await _repository.GetTotalQuotationsCountAsync();
            var pendingQuotations = await _repository.GetQuotationsCountByStatusAsync(QuotationStatus.Pending);
            var submittedQuotations = await _repository.GetQuotationsCountByStatusAsync(QuotationStatus.Submitted);
            var acceptedQuotations = await _repository.GetQuotationsCountByStatusAsync(QuotationStatus.Accepted);
            var rejectedQuotations = await _repository.GetQuotationsCountByStatusAsync(QuotationStatus.Rejected);
            var expiredQuotations = await _repository.GetQuotationsCountByStatusAsync(QuotationStatus.Expired);

            var statusDistribution = await _repository.GetQuotationStatsByStatusAsync();
            var brandDistribution = await _repository.GetQuotationStatsByBrandAsync();

            // 计算平均价格
            var allQuotations = await _repository.GetListAsync(new QuotationQuery());
            var totalAmount = allQuotations.Where(q => q.TotalAmount > 0).Sum(q => q.TotalAmount);
            var averageUnitPrice = allQuotations.Any() ? allQuotations.Average(q => q.UnitPrice) : 0;

            // 计算接受率
            var acceptanceRate = (submittedQuotations + acceptedQuotations) > 0 ?
                (decimal)acceptedQuotations / (submittedQuotations + acceptedQuotations) : 0;

            return new QuotationStatisticsResponse
            {
                TotalQuotations = totalQuotations,
                PendingQuotations = pendingQuotations,
                SubmittedQuotations = submittedQuotations,
                AcceptedQuotations = acceptedQuotations,
                RejectedQuotations = rejectedQuotations,
                ExpiredQuotations = expiredQuotations,
                TotalQuotedAmount = totalAmount,
                AverageUnitPrice = averageUnitPrice,
                AcceptanceRate = acceptanceRate,
                StatusDistribution = statusDistribution.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                BrandDistribution = brandDistribution
            };
        }

        public async Task<QuotationStatisticsResponse> GetSupplierStatisticsAsync(long supplierId)
        {
            var quotations = await _repository.GetBySupplierIdAsync(supplierId);

            return new QuotationStatisticsResponse
            {
                TotalQuotations = quotations.Count,
                PendingQuotations = quotations.Count(q => q.Status == QuotationStatus.Pending),
                SubmittedQuotations = quotations.Count(q => q.Status == QuotationStatus.Submitted),
                AcceptedQuotations = quotations.Count(q => q.Status == QuotationStatus.Accepted),
                RejectedQuotations = quotations.Count(q => q.Status == QuotationStatus.Rejected),
                ExpiredQuotations = quotations.Count(q => q.Status == QuotationStatus.Expired),
                TotalQuotedAmount = quotations.Where(q => q.TotalAmount > 0).Sum(q => q.TotalAmount),
                AverageUnitPrice = quotations.Any() ? quotations.Average(q => q.UnitPrice) : 0,
                AcceptanceRate = quotations.Count(q => q.Status == QuotationStatus.Submitted || q.Status == QuotationStatus.Accepted) > 0 ?
                    (decimal)quotations.Count(q => q.Status == QuotationStatus.Accepted) /
                    quotations.Count(q => q.Status == QuotationStatus.Submitted || q.Status == QuotationStatus.Accepted) : 0,
                StatusDistribution = quotations.GroupBy(q => q.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                BrandDistribution = quotations
                    .Where(q => !string.IsNullOrEmpty(q.Brand))
                    .GroupBy(q => q.Brand)
                    .ToDictionary(g => g.Key!, g => g.Count())
            };
        }

        public async Task<Dictionary<string, decimal>> GetPriceTrendsAsync(string bearingNumber, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var quotations = await _repository.GetListAsync(new QuotationQuery
            {
                BearingNumber = bearingNumber,
                StartDate = startDate
            });

            var acceptedQuotations = quotations
                .Where(q => q.Status == QuotationStatus.Accepted)
                .ToList();

            var trends = new Dictionary<string, decimal>();

            // 按周分组计算平均价格
            var weeklyGroups = acceptedQuotations
                .GroupBy(q => GetWeekKey(q.CreatedAt))
                .OrderBy(g => g.Key);

            foreach (var group in weeklyGroups)
            {
                trends[group.Key] = group.Average(q => q.UnitPrice);
            }

            return trends;
        }

        public async Task<List<QuotationResponse>> CreateBulkQuotationsAsync(List<CreateQuotationRequest> requests)
        {
            var results = new List<QuotationResponse>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var request in requests)
                {
                    try
                    {
                        var quotation = await CreateQuotationAsync(request);
                        results.Add(MapToResponse(await _repository.GetByIdAsync(
                            await GetQuotationIdFromResponse(quotation))));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "批量创建报价失败: {DemandId}", request.DemandId);
                    }
                }

                await transaction.CommitAsync();
                return results;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "批量创建报价事务失败");
                throw;
            }
        }

        public async Task<bool> ExpireOldQuotationsAsync(DateTime cutoffDate)
        {
            var expiredQuotations = await _repository.GetExpiredQuotationsAsync(cutoffDate);
            var successCount = 0;

            foreach (var quotation in expiredQuotations)
            {
                try
                {
                    await ExpireQuotationAsync(quotation.Id);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "过期报价单失败: {QuotationId}", quotation.Id);
                }
            }

            _logger.LogInformation("成功过期 {Count} 个报价单", successCount);
            return successCount > 0;
        }

        public async Task<QuotationAttachmentResponse> AddAttachmentAsync(long quotationId, AddAttachmentRequest request)
        {
            var quotation = await _repository.GetByIdAsync(quotationId);
            if (quotation == null)
                throw new KeyNotFoundException("报价单不存在");

            var attachment = new QuotationAttachment
            {
                QuotationId = quotationId,
                AttachmentType = request.AttachmentType,
                FileName = request.FileName,
                FileUrl = request.FileUrl,
                Description = request.Description,
                FileSize = request.FileSize,
                UploadedAt = DateTime.UtcNow
            };

            var result = await _repository.AddQuotationAttachmentAsync(attachment);

            return new QuotationAttachmentResponse
            {
                Id = result.Id,
                AttachmentType = result.AttachmentType,
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                Description = result.Description,
                FileSize = result.FileSize,
                UploadedAt = result.UploadedAt
            };
        }

        public async Task<bool> RemoveAttachmentAsync(long attachmentId)
        {
            return await _repository.RemoveQuotationAttachmentAsync(attachmentId);
        }

        public async Task<List<QuotationAttachmentResponse>> GetAttachmentsAsync(long quotationId)
        {
            var attachments = await _repository.GetQuotationAttachmentsAsync(quotationId);
            return attachments.Select(a => new QuotationAttachmentResponse
            {
                Id = a.Id,
                AttachmentType = a.AttachmentType,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                Description = a.Description,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt
            }).ToList();
        }

        public async Task<bool> RemoveQuotationItemAsync(long itemId)
        {
            var item = await _context.QuotationItems.FindAsync(itemId);
            if (item == null) return false;

            var quotationId = item.QuotationId;
            var success = await _repository.RemoveQuotationItemAsync(itemId);

            if (success)
            {
                await RecalculateQuotationTotalAsync(quotationId);
            }

            return success;
        }

        public async Task<List<QuotationItemResponse>> GetQuotationItemsAsync(long quotationId)
        {
            var items = await _repository.GetQuotationItemsAsync(quotationId);
            return items.Select(i => new QuotationItemResponse
            {
                Id = i.Id,
                BearingNumber = i.BearingNumber,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Brand = i.Brand,
                Material = i.Material,
                Standard = i.Standard,
                DisplayOrder = i.DisplayOrder,
                CreatedAt = i.CreatedAt
            }).ToList();
        }


        // 事件发布方法
        private async Task PublishQuotationSubmittedEventAsync(Quotation quotation)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "quotation-submitted", new
                {
                    QuotationId = quotation.Id,
                    QuotationNumber = quotation.QuotationNumber,
                    DemandId = quotation.DemandId,
                    SupplierId = quotation.SupplierId,
                    SubmittedAt = quotation.SubmittedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布报价提交事件失败");
            }
        }

        private async Task PublishQuotationAcceptedEventAsync(Quotation quotation)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "quotation-accepted", new
                {
                    QuotationId = quotation.Id,
                    QuotationNumber = quotation.QuotationNumber,
                    DemandId = quotation.DemandId,
                    SupplierId = quotation.SupplierId,
                    AcceptedAt = quotation.AcceptedAt,
                    TotalAmount = quotation.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布报价接受事件失败");
            }
        }

        private async Task PublishQuotationExpiredEventAsync(Quotation quotation)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "quotation-expired", new
                {
                    QuotationId = quotation.Id,
                    QuotationNumber = quotation.QuotationNumber,
                    ExpiredAt = quotation.ExpiresAt,
                    Status = quotation.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布报价过期事件失败");
            }
        }       
    }
}
