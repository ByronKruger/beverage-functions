using Coffeeg.Extensions;
using Coffeeg.Helpers.AutoMapperProfiles;
using Coffeeg.Interfaces.Repositories;
using Coffeeg.Interfaces.Services;
using Coffeeg.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestFunctionApp.Data;
using TestFunctionApp.Repositories;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddDataContext<CoffeegDbContext>(builder.Configuration);
builder.Services.AddCoffeegIdentityCore<CoffeegDbContext>();

builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAdminBeverageManagementService, AdminBeverageManagementService>();
builder.Services.AddScoped<IAdminBeverageManagementRepository, AdminBeverageManagementRepository>();

builder.Services.AddScoped<IBeverageCustomisationService, BeverageCustomisationService>();
builder.Services.AddScoped<IBeverageCustomisationRepository, BeverageCustomisationRepository>();

builder.Services.AddAutoMapper(typeof(BeverageCustomisationProfiles));   // scans for profiles in that assembly
builder.Services.AddAutoMapper(typeof(UserProfile));   // scans for profiles in that assembly

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
