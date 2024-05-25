using Microsoft.AspNetCore.Mvc;
using Pivotte.ExampleWeb.Models;

namespace Pivotte.ExampleWeb.Services;

public interface IProductService
{
    [Route("get/{id}")]
    [HttpGet]
    Product GetProduct([FromRoute]int id);
}