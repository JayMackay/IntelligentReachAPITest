using Ir.ApiTest.Interfaces;
using Ir.FakeMarketplace.Controllers;
using Ir.IntegrationTest.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ir.ApiTest.Tests.Controllers
{
  public class ProductsControllerTests
  {
    private readonly Mock<IProductService> _mockProductService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
      _mockProductService = new Mock<IProductService>();
      _controller = new ProductsController(_mockProductService.Object);
    }

    [Fact]
    public async Task GetProducts_ReturnsOkWithCorrectProductCount()
    {
      // Arrange
      int page = 1;
      int pageSize = 10; // Adjust according to your needs
      var mockProducts = new List<ProductDto>
            {
                new ProductDto { Id = "1", Name = "Product 1" },
                new ProductDto { Id = "2", Name = "Product 2" }
            };
      _mockProductService.Setup(x => x.GetProductsAsync(page, pageSize)).ReturnsAsync(mockProducts);

      // Act
      var result = await _controller.GetProducts(page, pageSize);

      // Assert
      var okResult = Assert.IsType<OkObjectResult>(result);
      var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
      Assert.NotNull(products);
      Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsOkWithCorrectProduct()
    {
      // Arrange
      string existingId = "1";
      var mockProduct = new ProductDto { Id = existingId, Name = "Product 1" };
      _mockProductService.Setup(x => x.GetProductAsync(existingId)).ReturnsAsync(mockProduct);

      // Act
      var result = await _controller.GetProduct(existingId);

      // Assert
      var okResult = Assert.IsType<OkObjectResult>(result);
      var product = Assert.IsType<ProductDto>(okResult.Value);
      Assert.NotNull(product);
      Assert.Equal(existingId, product.Id);
    }
  }
}