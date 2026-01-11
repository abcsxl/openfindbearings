using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Dapr.Client;
using SupplierApi.Models;
using SupplierApi.Services;
using SupplierApi.Models.DTOs;

namespace SupplierApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly DaprClient _daprClient;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(
            ISupplierService supplierService,
            DaprClient daprClient,
            ILogger<SupplierController> logger)
        {
            _supplierService = supplierService;
            _daprClient = daprClient;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,SupplierAdmin")]
        public async Task<ActionResult<ApiResponse<SupplierResponse>>> CreateSupplier(
            [FromBody] CreateSupplierRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<SupplierResponse>.ErrorResponse("请求数据无效"));

                var supplier = await _supplierService.CreateSupplierAsync(request);

                _logger.LogInformation("供应商创建成功: {SupplierId}", supplier.Id);

                return Ok(ApiResponse<SupplierResponse>.SuccessResponse(new SupplierResponse
                {
                    Id = supplier.Id,
                    CompanyName = supplier.CompanyName,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Status = supplier.Status,
                    Type = supplier.Type,
                    CreatedAt = supplier.CreatedAt
                }));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "供应商创建失败: 邮箱已存在");
                return BadRequest(ApiResponse<SupplierResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "供应商创建失败");
                return StatusCode(500, ApiResponse<SupplierResponse>.ErrorResponse("创建失败"));
            }
        }

        [HttpGet("{id:long}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SupplierDetailResponse>>> GetSupplier(long id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierAsync(id);
                if (supplier == null)
                    return NotFound(ApiResponse<SupplierDetailResponse>.ErrorResponse("供应商不存在"));

                var response = new SupplierDetailResponse
                {
                    Id = supplier.Id,
                    CompanyName = supplier.CompanyName,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address,
                    City = supplier.City,
                    Country = supplier.Country,
                    PostalCode = supplier.PostalCode,
                    BusinessLicense = supplier.BusinessLicense,
                    TaxId = supplier.TaxId,
                    Status = supplier.Status,
                    Type = supplier.Type,
                    Rating = supplier.Rating,
                    TotalTransactions = supplier.TotalTransactions,
                    SuccessfulTransactions = supplier.SuccessfulTransactions,
                    CreatedAt = supplier.CreatedAt,
                    UpdatedAt = supplier.UpdatedAt,
                    ApprovedAt = supplier.ApprovedAt
                };

                // 确保Products不为null
                response.Products = supplier.Products?
                    .Select(p => new ProductResponse
                    {
                        Id = p.Id,
                        BearingNumber = p.BearingNumber,
                        Specification = p.Specification,
                        Material = p.Material,
                        Brand = p.Brand,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        MinimumOrder = p.MinimumOrder,
                        IsActive = p.IsActive
                    })
                    .ToList() ?? new List<ProductResponse>();

                // 确保Certificates不为null
                response.Certificates = supplier.Certificates?
                    .Select(c => new CertificateResponse
                    {
                        Id = c.Id,
                        CertificateType = c.CertificateType,
                        CertificateNumber = c.CertificateNumber,
                        IssueDate = c.IssueDate,
                        ExpiryDate = c.ExpiryDate,
                        IssuingAuthority = c.IssuingAuthority,
                        FileUrl = c.FileUrl
                    })
                    .ToList() ?? new List<CertificateResponse>();

                return Ok(ApiResponse<SupplierDetailResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商详情失败");
                return StatusCode(500, ApiResponse<SupplierDetailResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResponse<SupplierResponse>>>> GetSuppliers(
            [FromQuery] SupplierQueryRequest query)
        {
            try
            {
                var suppliers = await _supplierService.GetSuppliersAsync(query.ToQuery());
                var totalCount = await _supplierService.GetSuppliersCountAsync(query.ToQuery());

                return Ok(ApiResponse<PagedResponse<SupplierResponse>>.SuccessResponse(new PagedResponse<SupplierResponse>
                {
                    Items = suppliers.Select(s => new SupplierResponse
                    {
                        Id = s.Id,
                        CompanyName = s.CompanyName,
                        ContactPerson = s.ContactPerson,
                        Email = s.Email,
                        Phone = s.Phone,
                        Status = s.Status,
                        Type = s.Type,
                        Rating = s.Rating,
                        CreatedAt = s.CreatedAt
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商列表失败");
                return StatusCode(500, ApiResponse<PagedResponse<SupplierResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "SystemAdmin,SupplierAdmin")]
        public async Task<ActionResult<ApiResponse<SupplierResponse>>> UpdateSupplier(
            long id, [FromBody] UpdateSupplierRequest request)
        {
            try
            {
                var supplier = await _supplierService.UpdateSupplierAsync(id, request);

                return Ok(ApiResponse<SupplierResponse>.SuccessResponse(new SupplierResponse
                {
                    Id = supplier.Id,
                    CompanyName = supplier.CompanyName,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Status = supplier.Status,
                    Type = supplier.Type,
                    UpdatedAt = supplier.UpdatedAt
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<SupplierResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新供应商失败");
                return StatusCode(500, ApiResponse<SupplierResponse>.ErrorResponse("更新失败"));
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSupplier(long id)
        {
            try
            {
                var result = await _supplierService.DeleteSupplierAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("供应商不存在"));

                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除供应商失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除失败"));
            }
        }

        [HttpPost("{id:long}/approve")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<ActionResult<ApiResponse<SupplierResponse>>> ApproveSupplier(long id)
        {
            try
            {
                var supplier = await _supplierService.ApproveSupplierAsync(id);

                return Ok(ApiResponse<SupplierResponse>.SuccessResponse(new SupplierResponse
                {
                    Id = supplier.Id,
                    CompanyName = supplier.CompanyName,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Status = supplier.Status,
                    Type = supplier.Type,
                    ApprovedAt = supplier.ApprovedAt
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<SupplierResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批准供应商失败");
                return StatusCode(500, ApiResponse<SupplierResponse>.ErrorResponse("批准失败"));
            }
        }

        [HttpPost("{id:long}/suspend")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<ActionResult<ApiResponse<SupplierResponse>>> SuspendSupplier(long id)
        {
            try
            {
                var supplier = await _supplierService.SuspendSupplierAsync(id);

                return Ok(ApiResponse<SupplierResponse>.SuccessResponse(new SupplierResponse
                {
                    Id = supplier.Id,
                    CompanyName = supplier.CompanyName,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Status = supplier.Status,
                    Type = supplier.Type
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<SupplierResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "暂停供应商失败");
                return StatusCode(500, ApiResponse<SupplierResponse>.ErrorResponse("暂停失败"));
            }
        }

        // 产品管理端点
        [HttpPost("{id:long}/products")]
        [Authorize(Roles = "SystemAdmin,SupplierAdmin,SupplierUser")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> AddProduct(
            long id, [FromBody] AddProductRequest request)
        {
            try
            {
                var product = await _supplierService.AddProductAsync(id, request);

                return Ok(ApiResponse<ProductResponse>.SuccessResponse(new ProductResponse
                {
                    Id = product.Id,
                    BearingNumber = product.BearingNumber,
                    Specification = product.Specification,
                    Material = product.Material,
                    Brand = product.Brand,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    MinimumOrder = product.MinimumOrder
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ProductResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加产品失败");
                return StatusCode(500, ApiResponse<ProductResponse>.ErrorResponse("添加失败"));
            }
        }

        [HttpGet("{id:long}/products")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetProducts(long id)
        {
            try
            {
                var products = await _supplierService.GetProductsAsync(id);

                return Ok(ApiResponse<List<ProductResponse>>.SuccessResponse(products.Select(p => new ProductResponse
                {
                    Id = p.Id,
                    BearingNumber = p.BearingNumber,
                    Specification = p.Specification,
                    Material = p.Material,
                    Brand = p.Brand,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    MinimumOrder = p.MinimumOrder,
                    IsActive = p.IsActive
                }).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品列表失败");
                return StatusCode(500, ApiResponse<List<ProductResponse>>.ErrorResponse("获取失败"));
            }
        }
    }
}
