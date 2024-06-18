using System;

namespace Ir.IntegrationTest.Contracts
{
  public class ProductDto
  {
    public string Id { get; set; }
    public string Size { get; set; }
    public string Colour { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Hash { get; set; }
  };
}