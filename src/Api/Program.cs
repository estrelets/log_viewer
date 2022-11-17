using System.Diagnostics;
using Api;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Asp
var services = builder.Services;
services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
services.AddControllers();
services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = Int32.MaxValue);
services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = Int32.MaxValue;
    options.MultipartBodyLengthLimit = Int32.MaxValue;
    options.MultipartHeadersLengthLimit = Int32.MaxValue;
});

// Swagger
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
// Logic
services.AddSingleton<Reader>();
services.AddSingleton<LogStore>();


var app = builder.Build();
app.UseCors("AllowAll");
// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Asp
app.UseStaticFiles();

app.MapControllers();
Process.Start("xdg-open", "http://localhost:5999/index.html");
app.Run("http://localhost:5999");
