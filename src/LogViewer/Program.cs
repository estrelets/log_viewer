using Api;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings/appsettings.json");
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<LogStore>();
builder.Services.AddSingleton<Reader>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = builder.Environment.ApplicationName,
        Version = "v1" 
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
        $"{builder.Environment.ApplicationName} v1"));
};

app.MapGet("/", () => "Hello World!");

app.MapPost("/read/", (
        HttpRequest request, 
        Reader reader,
        LogStore store) =>
    {
        var file = request.Form.Files.FirstOrDefault();
        if (file == null) return Results.BadRequest();
        
        using var stream = file.OpenReadStream();
        var logs = reader.Read(stream);
        store.Fill(logs);
        
        return Results.Ok();
    })
    .Accepts<HttpRequest>("multipart/form-data")
    .Produces(200)
    .Produces(400);

app.MapGet("/logs/", (
    [FromQuery] DateTime from, 
    [FromQuery] DateTime? to,
    LogStore store) =>
{
    return store.Logs!
        .Where(x => x.Time >= from)
        .Where(x => to == null || x.Time <= to)
        .ToArray();
})
    .Produces<LogEntry[]>();


app.Run();