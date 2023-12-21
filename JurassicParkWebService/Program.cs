using JurassicParkWebService;
using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var databaseConfiguration = builder.Configuration.GetRequiredSection("DatabaseConfiguration").Get<DatabaseConfiguration>();

builder.Services
    .AddSingleton<IDatabaseConfiguration>(databaseConfiguration)
    .AddTransient<ICageStore, CageStore>()
    .AddTransient<IDinosaurStore, DinosaurStore>()
    .AddTransient<ISpeciesStore, SpeciesStore>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();