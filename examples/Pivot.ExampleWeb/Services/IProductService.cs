using Microsoft.AspNetCore.Mvc;
using Pivot.ExampleWeb.Models;

namespace Pivot.ExampleWeb.Services;

public interface IProductService
{
    [Route("get/{id}")]
    [HttpGet]
    Product GetProduct([FromRoute]int id);
}