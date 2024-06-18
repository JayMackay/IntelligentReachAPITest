using Ir.ApiTest.Interfaces;
using Ir.ApiTest.Services;
using Ir.IntegrationTest.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
  options.SerializerSettings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
  options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Context>(options => options.UseInMemoryDatabase(databaseName: "Database"));

// Services dependency injection
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
