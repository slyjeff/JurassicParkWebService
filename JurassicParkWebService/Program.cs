using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTransient<ICageStore, CageStore>()
    .AddTransient<IDinosaurStore, DinosaurStore>()
    .AddTransient<ISpeciesStore, SpeciesStore>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();