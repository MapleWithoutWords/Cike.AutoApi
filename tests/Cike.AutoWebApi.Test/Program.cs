using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;
using Cike.Service.Test;
using Cike.Service.Test.TestService;
using Cike.Service.Test.TestService.Impl;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddTransient<ITestService, TestService>();
builder.Services.AddTransient<TestService>();

builder.Services.AddControllers();


builder.Services.AddAutoApiService(opt =>
{
    opt.DefaultRootPath = "v1";
    opt.CreateConventional(typeof(CikeServiceTest).Assembly);
});
builder.Services.AddAutoApiService(opt =>
{
    opt.CreateConventional(typeof(Program).Assembly,opt=>opt.RootPath="");
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
    c.DocInclusionPredicate((docName, action) => true);
});

var app = builder.Build();



app.UseSwagger();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test"));
// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

var endpointRouteBuilder = app.Services.GetRequiredService<IActionDescriptorCollectionProvider>();

app.Run();
