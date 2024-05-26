# Overview

```Pivotte``` is a library that helps you declare your APIs separately from your implementation.

```csharp
public class GetProductRequest
{
    public string Country { get; set; }
}

[PivotteService("Product")]
public interface IProductService
{
    [Route("get/{id}")]
    [HttpPost]
    Product GetProduct([FromRoute]int id, [FromBody]GetProductRequest request);
}
```

Auto wire-up the definition to wire up an implementation, using ASP.NET Minimal APIs.

```csharp
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
// ...
builder.Services.AddSingleton<IProductService, ProductService>();
// ...
app.MapPivotteService<IProductService>("api"); // endpoint "/api/get/{id}
// ...
```

With ```Pivotte.Generators```, you can generate client-side code (supported by NSwag).

```csharp
var generator = sp.GetService<IGenerator>();
var code = generator.GenerateClientTypeScript(config =>
{
    config.AddService<IProductService>();
});
// save 'code' wherever makes sense to you
```

With ```Pivotte.NetClient```, you can invoke an API from another .NET project with the same api definition as well.

```csharp
var clientGenerator = sp.GetService<IPivotteClientGenerator>();
var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://somewhere.com/api")
};
// "client" is a runtime-generated implementation of IProductService.
// invoking methods will cause an HTTP request underneath the hood.
var client = clientGenerator.Generate<IProductService>(httpClient);
var product = await client.GetProduct(3, new GetProductRequest
{
    Country = "US"
});
```