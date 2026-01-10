using Supplier.Models;
using Supplier.Models.DTOs;

namespace Supplier.Services
{
    public interface ISupplierService
    {
        Task<Models.Supplier> CreateSupplierAsync(CreateSupplierRequest request);
        Task<Models.Supplier?> GetSupplierAsync(long id);
        Task<Models.Supplier?> GetSupplierByEmailAsync(string email);
        Task<List<Models.Supplier>> GetSuppliersAsync(SupplierQuery query);
        Task<Models.Supplier> UpdateSupplierAsync(long id, UpdateSupplierRequest request);
        Task<bool> DeleteSupplierAsync(long id);
        Task<Models.Supplier> ApproveSupplierAsync(long id);
        Task<Models.Supplier> SuspendSupplierAsync(long id);

        // 产品管理
        Task<SupplierProduct> AddProductAsync(long supplierId, AddProductRequest request);
        Task<List<SupplierProduct>> GetProductsAsync(long supplierId);
        Task<bool> RemoveProductAsync(long supplierId, long productId);

        // 证书管理
        Task<SupplierCertificate> AddCertificateAsync(long supplierId, AddCertificateRequest request);
        Task<List<SupplierCertificate>> GetCertificatesAsync(long supplierId);

        Task<int> GetSuppliersCountAsync(SupplierQuery query);
    }
}
