using Microsoft.AspNetCore.Mvc;
using ProtoPivot.ExampleWeb.Models;

namespace ProtoPivot.ExampleWeb.Services;

public interface IProductService
{
    [Route("get/{id}")]
    [HttpGet]
    Product GetProduct([FromRoute]int id);
}