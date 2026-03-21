using CodeSage.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        o => o.UseVector()
    );
});


var app = builder.Build();

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "gumba!"});
});

app.Run();

// needed for webapplication factory in tests.
public partial class Program { }