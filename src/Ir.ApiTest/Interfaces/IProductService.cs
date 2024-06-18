using Ir.IntegrationTest.Contracts;
using Microsoft.AspNetCore.JsonPatch;

namespace Ir.ApiTest.Interfaces
{
  public interface IProductService
  {
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<ProductDto> GetProductAsync(string id);
    Task<ProductDto> CreateProductAsync(ProductDto product);
    Task<ProductDto> UpdateProductAsync(string id, JsonPatchDocument<ProductDto> productUpdateParameters);
  }
}
