using Serilog;
using Shop.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.InitServices(configuration);
builder.Host.UseSerilog();

var app = builder.Build();
await app.InitAppConfigAsync();

app.Run();

// NOTE: Make the implicit Program class public so test projects can access it
public partial class Program;