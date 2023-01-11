using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using NET.Service.Test;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoApiService(optionAction: opt =>
{
    opt.CreateConventional(typeof(NETServiceTest).Assembly);
});



var apiInfo = new OpenApiInfo
{
    Title = "Test",
    Version = "v1",
    Contact = new OpenApiContact { Name = "Test", }
};
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", apiInfo);

    foreach (var item in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml"))
    {
        c.IncludeXmlComments(item, true);
    }

});

builder.Services.AddControllers();

var app = builder.Build();

app.UseAutoApiService();
// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test"));

app.Run();
