using QuotationApi.Data;
using QuotationApi.Models.DTOs;
using QuotationApi.Models.Entities;

namespace QuotationApi.Services
{
    public class QuotationValidationService : IQuotationValidationService
    {
        private readonly IQuotationRepository _repository;
        private readonly ILogger<QuotationValidationService> _logger;

        public QuotationValidationService(IQuotationRepository repository, ILogger<QuotationValidationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateQuotationAsync(CreateQuotationRequest request)
        {
            var result = new ValidationResult();

            // 基础验证
            if (request.UnitPrice <= 0)
                result.Errors.Add("单价必须大于0");

            if (request.Quantity <= 0)
                result.Errors.Add("数量必须大于0");

            if (string.IsNullOrEmpty(request.BearingNumber))
                result.Errors.Add("轴承型号不能为空");

            if (request.DemandId <= 0)
                result.Errors.Add("需求ID无效");

            if (request.SupplierId <= 0)
                result.Errors.Add("供应商ID无效");

            // 业务规则验证
            var existingQuotations = await _repository.GetByDemandIdAsync(request.DemandId);
            var supplierQuotation = existingQuotations.FirstOrDefault(q => q.SupplierId == request.SupplierId);

            if (supplierQuotation != null)
                result.Errors.Add($"供应商已为该需求提交过报价: {supplierQuotation.QuotationNumber}");

            // 价格合理性检查
            var similarQuotations = await _repository.FindSimilarQuotationsAsync(request.BearingNumber, 10);
            if (similarQuotations.Any())
            {
                var avgPrice = similarQuotations.Average(q => q.UnitPrice);
                var priceDiff = Math.Abs(request.UnitPrice - avgPrice) / avgPrice;

                if (priceDiff > 0.5m) // 价格偏离超过50%
                    result.Warnings.Add($"报价价格与市场平均价格({avgPrice:F2})差异较大");
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        public async Task<ValidationResult> ValidateQuotationUpdateAsync(long quotationId, UpdateQuotationRequest request)
        {
            var result = new ValidationResult();
            var quotation = await _repository.GetByIdAsync(quotationId);

            if (quotation == null)
            {
                result.Errors.Add("报价单不存在");
                return result;
            }

            // 状态检查
            if (quotation.Status != QuotationStatus.Draft && quotation.Status != QuotationStatus.Pending)
                result.Errors.Add("只有草稿或待审核状态的报价单可以修改");

            // 价格验证
            if (request.UnitPrice.HasValue && request.UnitPrice <= 0)
                result.Errors.Add("单价必须大于0");

            if (request.Quantity.HasValue && request.Quantity <= 0)
                result.Errors.Add("数量必须大于0");

            result.IsValid = !result.Errors.Any();
            return result;
        }

        public async Task<ValidationResult> ValidateQuotationSubmissionAsync(long quotationId)
        {
            var result = new ValidationResult();
            var quotation = await _repository.GetByIdAsync(quotationId);

            if (quotation == null)
            {
                result.Errors.Add("报价单不存在");
                return result;
            }

            // 状态检查
            if (quotation.Status != QuotationStatus.Draft)
                result.Errors.Add("只有草稿状态的报价单可以提交");

            // 有效期检查
            if (quotation.ExpiresAt.HasValue && quotation.ExpiresAt <= DateTime.UtcNow)
                result.Errors.Add("报价单已过期，无法提交");

            // 必填字段检查
            if (quotation.UnitPrice <= 0)
                result.Errors.Add("单价必须大于0");

            if (quotation.Quantity <= 0)
                result.Errors.Add("数量必须大于0");

            result.IsValid = !result.Errors.Any();
            return result;
        }

        public async Task<ValidationResult> ValidateQuotationAcceptanceAsync(long quotationId, long customerId)
        {
            var result = new ValidationResult();
            var quotation = await _repository.GetByIdAsync(quotationId);

            if (quotation == null)
            {
                result.Errors.Add("报价单不存在");
                return result;
            }

            // 状态检查
            if (quotation.Status != QuotationStatus.Submitted)
                result.Errors.Add("只有已提交状态的报价单可以被接受");

            // 有效期检查
            if (quotation.ExpiresAt.HasValue && quotation.ExpiresAt <= DateTime.UtcNow)
                result.Errors.Add("报价单已过期，无法接受");

            result.IsValid = !result.Errors.Any();
            return result;
        }

        public async Task<List<string>> ValidateQuotationNumberAsync(string quotationNumber)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(quotationNumber))
                errors.Add("报价单号不能为空");

            if (quotationNumber.Length < 5)
                errors.Add("报价单号长度不能少于5个字符");

            // 检查是否已存在
            var existing = await _repository.GetByQuotationNumberAsync(quotationNumber);
            if (existing != null)
                errors.Add("报价单号已存在");

            return errors;
        }

        public async Task<decimal> CalculateMatchScoreAsync(Quotation quotation, long demandId)
        {
            decimal score = 0.0m;

            // 价格匹配度 (40%)
            var similarQuotations = await _repository.FindSimilarQuotationsAsync(quotation.BearingNumber, 20);
            if (similarQuotations.Any())
            {
                var avgPrice = similarQuotations.Average(q => q.UnitPrice);
                var priceDiff = Math.Abs(quotation.UnitPrice - avgPrice) / avgPrice;
                score += (1.0m - Math.Min(priceDiff, 1.0m)) * 0.4m;
            }

            // 供应商信誉度 (30%) - 这里简化处理
            var supplierQuotations = await _repository.GetBySupplierIdAsync(quotation.SupplierId);
            var acceptanceRate = supplierQuotations.Any() ?
                (decimal)supplierQuotations.Count(q => q.Status == QuotationStatus.Accepted) / supplierQuotations.Count : 0.5m;
            score += acceptanceRate * 0.3m;

            // 交货时间匹配度 (20%)
            var deliveryScore = quotation.DeliveryDays <= 7 ? 1.0m :
                               quotation.DeliveryDays <= 14 ? 0.7m : 0.3m;
            score += deliveryScore * 0.2m;

            // 质量认证匹配度 (10%)
            var qualityScore = string.IsNullOrEmpty(quotation.QualityStandard) ? 0.5m : 0.9m;
            score += qualityScore * 0.1m;

            return Math.Min(score, 1.0m);
        }
    }
}
