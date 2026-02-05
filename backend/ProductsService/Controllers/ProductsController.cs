using Microsoft.AspNetCore.Mvc;
using ProductsService.Models;

namespace ProductsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> Products = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 1200 },
        new Product { Id = 2, Name = "Phone", Price = 800 }
    };

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(Products);
    }
}
