using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ICageStore, CageStore>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
