using Ir.ApiTest.Interfaces;
using Ir.IntegrationTest.Contracts;
using Ir.IntegrationTest.Entity;
using Ir.IntegrationTest.Entity.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Ir.ApiTest.Services
{
  public class ProductService : IProductService
  {
    private readonly Context _context;

    public ProductService(Context context)
    {
      _context = context;
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto product)
    {
      if(product == null)
        throw new ArgumentNullException(nameof(product));

      // Check if product with the same Id already exists
      if(await _context.Products.AnyAsync(p => p.Id == product.Id))
        throw new InvalidOperationException($"Product with ID: {product.Id} already exists");

      // Set Created and LastUpdated properties
      var now = DateTime.UtcNow;
      product.Created = now;
      product.LastUpdated = now;

      // Map ProductDto to Product entity and save to database
      var entity = new Product
      {
        Id = product.Id,
        Size = product.Size,
        Colour = product.Colour,
        Name = product.Name,
        Price = product.Price,
        Created = product.Created,
        LastUpdated = product.LastUpdated,
        Hash = product.Hash
      };

      _context.Products.Add(entity);
      await _context.SaveChangesAsync();

      // Return the saved ProductDto
      return product;
    }

    public async Task<ProductDto> GetProductAsync(string id)
    {
      var product = await _context.Products.FindAsync(id);

      if(product == null)
        return null;

      return new ProductDto
      {
        Id = product.Id,
        Size = product.Size,
        Colour = product.Colour,
        Name = product.Name,
        Price = product.Price,
        Created = product.Created,
        LastUpdated = product.LastUpdated,
        Hash = product.Hash
      };
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
      var products = await _context.Products.ToListAsync();

      return products.Select(p => new ProductDto
      {
        Id = p.Id,
        Size = p.Size,
        Colour = p.Colour,
        Name = p.Name,
        Price = p.Price,
        Created = p.Created,
        LastUpdated = p.LastUpdated,
        Hash = p.Hash
      });
    }

    public async Task<ProductDto> UpdateProductAsync(string id, JsonPatchDocument<ProductDto> productUpdateParameters)
    {
      var product = await _context.Products.FindAsync(id);

      if(product == null)
        return null;

      var productDto = new ProductDto
      {
        Id = product.Id,
        Size = product.Size,
        Colour = product.Colour,
        Name = product.Name,
        Price = product.Price,
        Created = product.Created,
        LastUpdated = product.LastUpdated,
        Hash = product.Hash
      };

      productUpdateParameters.ApplyTo(productDto);

      // Ensure Id and Created fields are not modified
      productDto.Id = product.Id;
      productDto.Created = product.Created;

      // Update LastUpdated timestamp
      productDto.LastUpdated = DateTime.UtcNow;

      // Map updated ProductDto back to Product entity
      product.Size = productDto.Size;
      product.Colour = productDto.Colour;
      product.Name = productDto.Name;
      product.Price = productDto.Price;
      product.LastUpdated = productDto.LastUpdated;
      product.Hash = productDto.Hash;

      await _context.SaveChangesAsync();

      return productDto;
    }
  }
}