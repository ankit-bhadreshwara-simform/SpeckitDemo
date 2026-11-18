using SpeckitDemo.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddSpeckitDemoServices();

var app = builder.Build();

app.UseSpeckitDemoPipeline();

app.Run();
