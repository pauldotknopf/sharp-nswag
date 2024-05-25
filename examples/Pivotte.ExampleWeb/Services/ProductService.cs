using Pivotte.ExampleWeb.Models;

namespace Pivotte.ExampleWeb.Services;

public class ProductService : IProductService
{
    public Product GetProduct(int id, GetProductRequest request)
    {
        return new Product
        {
            Id = id,
            Name = "Product " + id + " " + request.Country,
            Price = id * 10f
        };
    }
}