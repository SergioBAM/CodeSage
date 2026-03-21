var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "gumba!"});
});

app.Run();

// needed for webapplication factory in tests.
public partial class Program { }