using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using SupplierApi.Data;
using SupplierApi.Models;
using SupplierApi.Models.DTOs;

namespace SupplierApi.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;
        private readonly SupplierDbContext _context;
        private readonly DaprClient _daprClient;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(
            ISupplierRepository repository,
            SupplierDbContext context,
            DaprClient daprClient,
            ILogger<SupplierService> logger)
        {
            _repository = repository;
            _context = context;
            _daprClient = daprClient;
            _logger = logger;
        }

        public async Task<Supplier> CreateSupplierAsync(CreateSupplierRequest request)
        {
            // 检查邮箱是否已存在
            var existingSupplier = await _repository.GetByEmailAsync(request.Email);
            if (existingSupplier != null)
                throw new InvalidOperationException("邮箱已被注册");

            var supplier = new Supplier
            {
                CompanyName = request.CompanyName,
                ContactPerson = request.ContactPerson,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PostalCode = request.PostalCode,
                BusinessLicense = request.BusinessLicense,
                TaxId = request.TaxId,
                Status = SupplierStatus.Pending,
                Type = request.SupplierType
            };

            var result = await _repository.AddAsync(supplier);

            // 发布供应商创建事件
            await _daprClient.PublishEventAsync("pubsub", "supplier-created",
                new SupplierCreatedEvent
                {
                    SupplierId = result.Id,
                    CompanyName = result.CompanyName,
                    Email = result.Email,
                    CreatedAt = DateTime.UtcNow
                });

            _logger.LogInformation("供应商创建成功: {SupplierId}", result.Id);
            return result;
        }

        public async Task<Supplier?> GetSupplierAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Supplier?> GetSupplierByEmailAsync(string email)
        {
            return await _repository.GetByEmailAsync(email);
        }

        public async Task<List<Supplier>> GetSuppliersAsync(SupplierQuery query)
        {
            return await _repository.GetListAsync(query);
        }

        public async Task<Supplier> UpdateSupplierAsync(long id, UpdateSupplierRequest request)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null)
                throw new KeyNotFoundException("供应商不存在");

            // 更新字段
            supplier.CompanyName = request.CompanyName ?? supplier.CompanyName;
            supplier.ContactPerson = request.ContactPerson ?? supplier.ContactPerson;
            supplier.Phone = request.Phone ?? supplier.Phone;
            supplier.Address = request.Address ?? supplier.Address;
            supplier.City = request.City ?? supplier.City;
            supplier.Country = request.Country ?? supplier.Country;
            supplier.PostalCode = request.PostalCode ?? supplier.PostalCode;
            supplier.UpdatedAt = DateTime.UtcNow;

            return await _repository.UpdateAsync(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(long id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<Supplier> ApproveSupplierAsync(long id)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null)
                throw new KeyNotFoundException("供应商不存在");

            supplier.Status = SupplierStatus.Approved;
            supplier.ApprovedAt = DateTime.UtcNow;
            supplier.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(supplier);

            // 发布供应商批准事件
            await _daprClient.PublishEventAsync("pubsub", "supplier-approved",
                new SupplierApprovedEvent
                {
                    SupplierId = result.Id,
                    CompanyName = result.CompanyName,
                    ApprovedAt = DateTime.UtcNow
                });

            return result;
        }

        public async Task<Supplier> SuspendSupplierAsync(long id)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null)
                throw new KeyNotFoundException("供应商不存在");

            supplier.Status = SupplierStatus.Suspended;
            supplier.UpdatedAt = DateTime.UtcNow;

            return await _repository.UpdateAsync(supplier);
        }

        // 产品管理方法
        public async Task<SupplierProduct> AddProductAsync(long supplierId, AddProductRequest request)
        {
            var supplier = await _repository.GetByIdAsync(supplierId);
            if (supplier == null)
                throw new KeyNotFoundException("供应商不存在");

            var product = new SupplierProduct
            {
                SupplierId = supplierId,
                BearingNumber = request.BearingNumber,
                Specification = request.Specification,
                Material = request.Material,
                Brand = request.Brand,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                MinimumOrder = request.MinimumOrder
            };

            _context.SupplierProducts.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<List<SupplierProduct>> GetProductsAsync(long supplierId)
        {
            return await _context.SupplierProducts
                .Where(p => p.SupplierId == supplierId && p.IsActive)
                .ToListAsync();
        }

        public async Task<bool> RemoveProductAsync(long supplierId, long productId)
        {
            var product = await _context.SupplierProducts
                .FirstOrDefaultAsync(p => p.Id == productId && p.SupplierId == supplierId);

            if (product == null) return false;

            product.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // 证书管理方法
        public async Task<SupplierCertificate> AddCertificateAsync(long supplierId, AddCertificateRequest request)
        {
            var supplier = await _repository.GetByIdAsync(supplierId);
            if (supplier == null)
                throw new KeyNotFoundException("供应商不存在");

            var certificate = new SupplierCertificate
            {
                SupplierId = supplierId,
                CertificateType = request.CertificateType,
                CertificateNumber = request.CertificateNumber,
                IssueDate = request.IssueDate,
                ExpiryDate = request.ExpiryDate,
                IssuingAuthority = request.IssuingAuthority,
                FileUrl = request.FileUrl
            };

            _context.SupplierCertificates.Add(certificate);
            await _context.SaveChangesAsync();

            return certificate;
        }

        public async Task<List<SupplierCertificate>> GetCertificatesAsync(long supplierId)
        {
            return await _context.SupplierCertificates
                .Where(c => c.SupplierId == supplierId)
                .ToListAsync();
        }

        public Task<int> GetSuppliersCountAsync(SupplierQuery query)
        {
            // 计算符合条件的供应商数量
            var queryable = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(query.CompanyName))
                queryable = queryable.Where(s => s.CompanyName.Contains(query.CompanyName));

            if (!string.IsNullOrEmpty(query.Email))
                queryable = queryable.Where(s => s.Email.Contains(query.Email));

            if (query.Status.HasValue)
                queryable = queryable.Where(s => s.Status == query.Status.Value);

            if (query.Type.HasValue)
                queryable = queryable.Where(s => s.Type == query.Type.Value);

            return queryable.CountAsync();
        }
    }
}
