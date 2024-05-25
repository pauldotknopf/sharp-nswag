using Pivotte.ExampleWeb.Models;

namespace Pivotte.ExampleWeb.Services;

public class ProductService : IProductService
{
    public Product GetProduct(int id)
    {
        return new Product
        {
            Id = id,
            Name = "Product " + id,
            Price = id * 10f
        };
    }
}