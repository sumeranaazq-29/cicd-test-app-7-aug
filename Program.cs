using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => "API is running");

app.MapGet("/api/dbcheck", async () =>
{
    var connString = "Host=localhost;Username=appuser;Password=lily@29;Database=appdb";
    try
    {
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();
        return Results.Ok("Database connected successfully");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run("http://0.0.0.0:5000");
