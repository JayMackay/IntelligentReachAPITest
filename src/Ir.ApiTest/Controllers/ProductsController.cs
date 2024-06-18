using Ir.ApiTest.Interfaces;
using Ir.IntegrationTest.Contracts;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ir.FakeMarketplace.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProductsController : ControllerBase
  {
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
      _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    // Added optional pagination ?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
      try
      {
        if(page <= 0 || pageSize <= 0)
        {
          return BadRequest(new { message = "Error page and page size must be greater than zero" });
        }

        var products = await _productService.GetProductsAsync(page, pageSize);
        return Ok(products);
      }
      catch(Exception ex)
      {
        return StatusCode(500, new { message = "An error occured while trying to fetch all the products", error = ex.Message });
      }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct([FromRoute] string id)
    {
      try
      {
        var product = await _productService.GetProductAsync(id);
        if(product == null)
        {
          return NotFound(new { message = $"The following Product with ID: {id} not found" });
        }
        return Ok(product);
      }
      catch(Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while fetching the product with ID: {id}", error = ex.Message });
      }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto product)
    {
      if(product == null)
      {
        return BadRequest(new { message = "Please input the required product data" });
      }

      try
      {
        var createdProduct = await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
      }
      catch(Exception ex)
      {
        return StatusCode(500, new { message = "An error occurred while creating the product", error = ex.Message });
      }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateProduct([FromRoute] string id, [FromBody] JsonPatchDocument<ProductDto> productUpdateParameters)
    {
      if(productUpdateParameters == null)
      {
        return BadRequest(new { message = "Error please input valid product update parameters in the request" });
      }

      try
      {
        var updatedProduct = await _productService.UpdateProductAsync(id, productUpdateParameters);
        if(updatedProduct == null)
        {
          return NotFound(new { message = $"Product with ID: {id} could not be found" });
        }
        return Ok(updatedProduct);
      }
      catch(Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while trying to update the product with ID: {id}", error = ex.Message });
      }
    }
  }
}