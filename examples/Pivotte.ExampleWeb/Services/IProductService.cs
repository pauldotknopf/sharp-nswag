using Microsoft.AspNetCore.Mvc;
using Pivotte.ExampleWeb.Models;

namespace Pivotte.ExampleWeb.Services;

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