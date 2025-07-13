using InventorySyetem1.Models;

namespace InventorySyetem1.Repositories;

public interface IProductRepository
{
    List<Product> GetAllProducts();
    Product GetProductById(int id);
    void AddProduct(Product product);
    int GetNextProductId();
    void UpdateProduct(Product product);
    List<Product> GetOutOfStockProducts();
}