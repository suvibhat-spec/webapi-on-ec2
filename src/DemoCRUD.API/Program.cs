using DemoCRUD.API;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
var assemblyName = typeof(Program).Assembly.GetName().Name;
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(policy=>
    policy.AddPolicy("CorsPolicy",opt=>
    {
        opt.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    })
);

builder.Services.AddControllers(options=>
{
    options.Filters.Add<CustomAuthFilter>();
});

builder.Services.AddScoped<CustomAuthFilter>();//demo unautorized filter

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
        b => b.MigrationsAssembly(assemblyName)));

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
if (dbContext.Database.GetPendingMigrations().Any())
{
    await dbContext.Database.MigrateAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/User", (User user, ApplicationDbContext db) =>
{
    db.Users.Add(user);
    db.SaveChanges();
    return Results.Created($"/User/{user.Id}", user);
});

app.MapGet("/all", async(ApplicationDbContext db) =>
{
    var users = await db.Users.ToListAsync();
    return Results.Ok(users);
    
});


app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
