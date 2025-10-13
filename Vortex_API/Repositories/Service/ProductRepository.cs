using Microsoft.EntityFrameworkCore;
using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Repositories.Service
{
    public class ProductRepository:IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<Product>> GetAllProduct(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // Lọc theo trường và giá trị
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                switch (filterOn.ToLower())
                {
                    case "title":
                        products = products.Where(p => p.Title.Contains(filterQuery));
                        break;

                    case "description":
                        products = products.Where(p => p.Description != null && p.Description.Contains(filterQuery));
                        break;

                    case "category":
                        products = products.Where(p => p.Category.Name.Contains(filterQuery));
                        break;

                    case "ishot":
                        if (bool.TryParse(filterQuery, out bool isHot))
                            products = products.Where(p => p.IsHot == isHot);
                        break;
                    case "isnew":
                        if (bool.TryParse(filterQuery, out bool isNew))
                            products = products.Where(p => p.IsNew == isNew);
                        break;

                    default:
                        break;
                }
            }

            // sắp xếp
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "title":
                        products = isAscending
                            ? products.OrderBy(p => p.Title)
                            : products.OrderByDescending(p => p.Title);
                        break;

                    case "price":
                        products = isAscending
                            ? products.OrderBy(p => p.Price)
                            : products.OrderByDescending(p => p.Price);
                        break;

                    

                    case "category":
                        products = isAscending
                            ? products.OrderBy(p => p.Category.Name)
                            : products.OrderByDescending(p => p.Category.Name);
                        break;

                    default:
                        break;
                }
            }

            //  Phân trang
            var skipResults = (pageNumber - 1) * pageSize;
            products = products.Skip(skipResults).Take(pageSize);

            return await products.ToListAsync();
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProduct(ProductDTO dto)
        {
            var product = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                IsHot = dto.IsHot,
                IsNew = dto.IsNew,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Nếu có upload ảnh
            if (dto.Images != null && dto.Images.Any())
            {
                // Đảm bảo WebRootPath luôn hợp lệ
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                //  Tạo đường dẫn upload an toàn
                var uploadPath = Path.Combine(webRoot, "uploads", "products");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in dto.Images)
                {
                    if (file == null || file.Length == 0) continue;

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    var img = new Image
                    {
                        ProductId = product.Id,
                        FileName = fileName,
                        FileExtension = Path.GetExtension(file.FileName),
                        FileSizeInBytes = file.Length,
                        FilePath = $"/uploads/products/{fileName}"
                    };

                    _context.Images.Add(img);
                }

                await _context.SaveChangesAsync();
            }

            return product;
        }

        public async Task<Product?> UpdateProduct(int id, EdditProductDTO dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            product.Title = dto.Title;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.IsHot = dto.IsHot;
            product.IsNew = dto.IsNew;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return false;

            // Xóa ảnh vật lý
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (product.Images != null)
            {
                foreach (var img in product.Images)
                {
                    var fullPath = Path.Combine(webRoot, img.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
