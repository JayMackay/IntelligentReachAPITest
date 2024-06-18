using Ir.ApiTest.Interfaces;
using Ir.ApiTest.Services;
using Ir.IntegrationTest.Contracts;
using Ir.IntegrationTest.Entity;
using Ir.IntegrationTest.Entity.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Ir.ApiTest.Tests.Services
{
  public class ProductServiceTests
  {
    private readonly Mock<Context> _mockContext;
    private readonly IProductService _productService;

    public ProductServiceTests()
    {
      // Mock DBContext using DbContextOptionsBuilder
      var options = new DbContextOptionsBuilder<Context>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;
      _mockContext = new Mock<Context>(options);

      _productService = new ProductService(_mockContext.Object);
    }

    [Fact]
    public async Task CreateProductAsync_ValidProductParameters_ReturnsCreatedProduct()
    {
      // Arrange
      var productDto = new ProductDto
      {
        Id = "1",
        Name = "Test Product",
        Size = "Large",
        Colour = "Blue",
        Price = (double)19.99m,
        Hash = "hashvalue"
      };

      var mockDbSet = new Mock<DbSet<Product>>();
      _mockContext.Setup(x => x.Products).Returns(mockDbSet.Object);

      mockDbSet.Setup(x => x.Add(It.IsAny<Product>()))
               .Callback<Product>(product =>
               {
                 // Simulate adding the product to the context
                 product.Id = productDto.Id;
                 product.Name = productDto.Name;
                 product.Size = productDto.Size;
                 product.Colour = productDto.Colour;
                 product.Price = productDto.Price;
                 product.Created = productDto.Created;
                 product.LastUpdated = productDto.LastUpdated;
                 product.Hash = productDto.Hash;
               });

      _mockContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

      _mockContext.Setup(x => x.Products.AnyAsync(p => p.Id == productDto.Id, default))
                  .ReturnsAsync(false); // Simulate that no product with the same ID exists initially

      // Act
      var createdProduct = await _productService.CreateProductAsync(productDto);

      // Assert
      Assert.NotNull(createdProduct);
      Assert.Equal(productDto.Id, createdProduct.Id);
      Assert.Equal(productDto.Name, createdProduct.Name);
      Assert.Equal(productDto.Size, createdProduct.Size);
      Assert.Equal(productDto.Colour, createdProduct.Colour);
      Assert.Equal(productDto.Price, createdProduct.Price);
      Assert.Equal(productDto.Hash, createdProduct.Hash);
      Assert.Equal(productDto.Created, createdProduct.Created); // Ensure Created timestamp is set
      Assert.Equal(productDto.LastUpdated, createdProduct.LastUpdated); // Ensure LastUpdated timestamp is set
    }

    [Fact]
    public async Task GetProductAsync_ExistingId_ReturnsProduct()
    {
      // Arrange
      var productId = "1";
      var productEntity = new Product
      {
        Id = productId,
        Name = "Test Product",
        Size = "Large",
        Colour = "Blue",
        Price = (double)19.99m,
        Created = DateTime.UtcNow,
        LastUpdated = DateTime.UtcNow,
        Hash = "hashvalue"
      };

      _mockContext.Setup(x => x.Products.FindAsync(productId))
                  .ReturnsAsync(productEntity);

      // Act
      var productDto = await _productService.GetProductAsync(productId);

      // Assert
      Assert.NotNull(productDto);
      Assert.Equal(productEntity.Id, productDto.Id);
      Assert.Equal(productEntity.Name, productDto.Name);
      Assert.Equal(productEntity.Size, productDto.Size);
      Assert.Equal(productEntity.Colour, productDto.Colour);
      Assert.Equal(productEntity.Price, productDto.Price);
      Assert.Equal(productEntity.Created, productDto.Created);
      Assert.Equal(productEntity.LastUpdated, productDto.LastUpdated);
      Assert.Equal(productEntity.Hash, productDto.Hash);
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsProductsPaged()
    {
      // Arrange
      int page = 1;
      int pageSize = 10;
      var products = new List<Product>
      {
        new Product { Id = "1", Name = "Product 1", Size = "Small", Colour = "Red", Price = (double)10.0m, Created = DateTime.UtcNow, LastUpdated = DateTime.UtcNow, Hash = "hash1" },
        new Product { Id = "2", Name = "Product 2", Size = "Medium", Colour = "Green", Price = (double)20.0m, Created = DateTime.UtcNow, LastUpdated = DateTime.UtcNow, Hash = "hash2" },
        new Product { Id = "3", Name = "Product 3", Size = "Large", Colour = "Blue", Price = (double)30.0m, Created = DateTime.UtcNow, LastUpdated = DateTime.UtcNow, Hash = "hash3" }
      }.AsQueryable();

      // Setup DbSet mock using IQueryable
      var mockSet = new Mock<DbSet<Product>>();
      mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
      mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
      mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
      mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

      _mockContext.Setup(x => x.Products).Returns(mockSet.Object);

      // Act
      var result = await _productService.GetProductsAsync(page, pageSize);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(3, result.Count()); // Total products in the mock context
      Assert.Equal(pageSize, result.Count()); // Number of products returned should be equal to pageSize
    }

    [Fact]
    public async Task UpdateProductAsync_ExistingId_ValidUpdateParameters_ReturnsUpdatedProduct()
    {
      // Arrange
      var productId = "1";
      var productEntity = new Product
      {
        Id = "2", // Updated product Id should not change
        Name = "Test Product",
        Size = "Large",
        Colour = "Blue",
        Price = (double)19.99m,
        Created = DateTime.UtcNow,
        LastUpdated = DateTime.UtcNow,
        Hash = "hashvalue"
      };

      var patchDoc = new JsonPatchDocument<ProductDto>();
      patchDoc.Replace(p => p.Name, "Updated Product Name");

      _mockContext.Setup(x => x.Products.FindAsync(productId))
                  .ReturnsAsync(productEntity);

      // Act
      var updatedProductDto = await _productService.UpdateProductAsync(productId, patchDoc);

      // Assert
      Assert.NotNull(updatedProductDto);
      Assert.NotEqual(productId, updatedProductDto.Id); // Ensure that the ProductId is not changed
      Assert.Equal("Updated Product Name", updatedProductDto.Name);
      Assert.Equal(productEntity.Size, updatedProductDto.Size);
      Assert.Equal(productEntity.Colour, updatedProductDto.Colour);
      Assert.Equal(productEntity.Price, updatedProductDto.Price);
      Assert.Equal(productEntity.Created, updatedProductDto.Created);
      Assert.NotEqual(productEntity.LastUpdated, updatedProductDto.LastUpdated); // LastUpdated should be updated
      Assert.Equal(productEntity.Hash, updatedProductDto.Hash);
    }
  }
}