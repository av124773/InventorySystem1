using InventorySyetem1.Models;
using InventorySyetem1.Repositories;
using InventorySyetem1.Utils;
using Org.BouncyCastle.Bcpg;

namespace InventorySyetem1.Services;

public class InventoryServices
{
    // 1. 資料庫相關
    private readonly IProductRepository _productRepository;

    // 透過建構子，注入介面
    public InventoryServices(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    // // 2. 通知相關
    // // 使用EmainNotier
    // INotifier emailNotifier = new EmailNotifier();
    // NotificationService emailService = new NotificationService(emailNotifier);
    
    public List<Product> GetAllProducts()
    {
        try
        {
            // 呼叫介面，而非實作
            List<Product> products = _productRepository.GetAllProducts();
            if (!products.Any())
            {
                Console.WriteLine("No products found");
            }
            return products;
        }
        catch (Exception e)
        {
            Console.WriteLine($"讀取產品列表失敗: {e.Message}");
            return new List<Product>();
        }
    }

    public Product GetProductById(int id)
    {
        try
        {
            Product product = _productRepository.GetProductById(id);
            if (product == null)
            {
                Console.WriteLine("No products found, please check your ID");
            }
            return product;
        }
        catch (Exception e)
        {
            Console.WriteLine($"讀取產品列表失敗: {e.Message}");
            return new Product();
        }
    }

    public void AddProduct(string? name, decimal price, int quantity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("產品名稱不能為空。");
            }
            if (price <= 0)
            {
                throw new ArgumentException("產品價格必須大於零。");
            }
            if (quantity < 0)
            {
                throw new ArgumentException("產品數量必須大於等於零。");
            }
            // 使用需要付ID的建構子
            var product = new Product(_productRepository.GetNextProductId(), name, price, quantity);
            // 使用不需要ID的建構子
            // var product = new Product(name, price, quantity);
            _productRepository.AddProduct(product);
        }
        catch (Exception e)
        {
            Console.WriteLine($"錯誤: {e.Message}");
            throw;
        }
    }

    public void UpdateProduct(Product product, string? name, decimal price, int quantity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("產品名稱不能為空。");
            }
            if (price <= 0)
            {
                throw new ArgumentException("產品價格必須大於零。");
            }
            if (quantity < 0)
            {
                throw new ArgumentException("產品數量必須大於等於零。");
            }
            // 執行跟新(覆蓋原始值)
            product.Name = name;
            product.Price = price;
            product.Quantity = quantity;
            _productRepository.UpdateProduct(product);
            Console.WriteLine($"產品ID:{product.Id} 已更新。");
        }
        catch (Exception e)
        {
            Console.WriteLine($"錯誤: {e.Message}");
            throw;
        }
    }

    public List<Product> SearchProducts(string? input)
    {
        try
        {
            List<Product> products = _productRepository.GetAllProducts();
            if (string.IsNullOrWhiteSpace(input))
            {
                return products;
            }
            
            var results = products
                .Where(product => product.Name.ToLower().Contains(input.ToLower()))
                .OrderBy(product => product.Name)
                .ToList();
            // var results = products.Where(product => keyword(product, input)).ToList();

            if (!results.Any())
            {
                Console.WriteLine("No products found!");
            }

            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine($"讀取產品列表失敗: {e.Message}");
            return new List<Product>();
        }
    }

    bool keyword(Product product, string input)
    {
        return product.Name.ToLower().Contains(input.ToLower());
    }

    public List<Product> SearchLowStock()
    {
        try
        {
            List<Product> products = _productRepository.GetAllProducts();
            
            var results = products
                .Where(product => product.Quantity < 10)
                .Where(products => products.Status == Product.ProductStatus.LowStock)
                .OrderBy(product => product.Name)
                .ToList();

            if (!results.Any())
            {
                Console.WriteLine("No products found");
            }
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine($"讀取產品列表失敗: {e.Message}");
            return new List<Product>();
        }
    }

    public List<Product> SearchOutOfStockProducts()
    {
        try
        {
            List<Product> products = _productRepository.GetOutOfStockProducts();

            if (!products.Any())
            {
                Console.WriteLine("No products found");
            }
            return products;
        }
        catch (Exception e)
        {
            Console.WriteLine($"讀取產品列表失敗: {e.Message}");
            return new List<Product>();
        }
    }
}