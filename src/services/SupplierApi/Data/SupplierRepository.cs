using Microsoft.EntityFrameworkCore;
using SupplierApi.Models;
using SupplierApi.Models.DTOs;
using System.Xml.Linq;

namespace SupplierApi.Data
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(long id);
        Task<Supplier?> GetByEmailAsync(string email);
        Task<List<Supplier>> GetListAsync(SupplierQuery query);
        Task<Supplier> AddAsync(Supplier supplier);
        Task<Supplier> UpdateAsync(Supplier supplier);
        Task<bool> DeleteAsync(long id);
        Task<List<Supplier>> GetPendingApprovalAsync();
        Task<List<Supplier>> GetActiveSuppliersAsync();
    }

    public class SupplierRepository : ISupplierRepository
    {
        private readonly SupplierDbContext _context;

        public SupplierRepository(SupplierDbContext context)
        {
            _context = context;
        }

        public async Task<Supplier?> GetByIdAsync(long id)
        {
            return await _context.Suppliers
                .Include(s => s.Products)
                .Include(s => s.Certificates)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Supplier?> GetByEmailAsync(string email)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<List<Supplier>> GetListAsync(SupplierQuery query)
        {
            var suppliers = _context.Suppliers.AsQueryable();

            // 应用筛选条件
            if (!string.IsNullOrEmpty(query.CompanyName))
                suppliers = suppliers.Where(s => s.CompanyName.Contains(query.CompanyName));

            if (!string.IsNullOrEmpty(query.Email))
                suppliers = suppliers.Where(s => s.Email.Contains(query.Email));

            if (query.Status.HasValue)
                suppliers = suppliers.Where(s => s.Status == query.Status.Value);

            if (query.Type.HasValue)
                suppliers = suppliers.Where(s => s.Type == query.Type.Value);

            if (query.City != null)
                suppliers = suppliers.Where(s => s.City == query.City);

            if (query.Country != null)
                suppliers = suppliers.Where(s => s.Country == query.Country);

            // 应用排序
            suppliers = query.SortBy?.ToLower() switch
            {
                "companyname" => query.SortDescending ? suppliers.OrderByDescending(s => s.CompanyName) : suppliers.OrderBy(s => s.CompanyName),
                "createdat" => query.SortDescending ? suppliers.OrderByDescending(s => s.CreatedAt) : suppliers.OrderBy(s => s.CreatedAt),
                "rating" => query.SortDescending ? suppliers.OrderByDescending(s => s.Rating) : suppliers.OrderBy(s => s.Rating),
                _ => query.SortDescending ? suppliers.OrderByDescending(s => s.CreatedAt) : suppliers.OrderBy(s => s.CreatedAt)
            };

            // 分页
            if (query.PageSize > 0)
            {
                suppliers = suppliers
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return await suppliers.ToListAsync();
        }

        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            supplier.UpdatedAt = DateTime.UtcNow;
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var supplier = await GetByIdAsync(id);
            if (supplier == null) return false;

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Supplier>> GetPendingApprovalAsync()
        {
            return await _context.Suppliers
                .Where(s => s.Status == SupplierStatus.Pending)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Supplier>> GetActiveSuppliersAsync()
        {
            return await _context.Suppliers
                .Where(s => s.Status == SupplierStatus.Approved)
                .OrderByDescending(s => s.Rating)
                .ToListAsync();
        }
    }
}
