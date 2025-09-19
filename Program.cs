using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDbTimeSeries;
using MongoDbTimeSeries.Models;

var builder = WebApplication.CreateBuilder(args);

// Bind settings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Database"));

// Mongo client + database
var conn = builder.Configuration.GetConnectionString("Mongo")!;
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(conn));
builder.Services.AddSingleton(sp =>
{
    var set = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return sp.GetRequiredService<IMongoClient>().GetDatabase(set.Name);
});

// Time-series collection injection
builder.Services.AddSingleton(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    var set = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return db.GetCollection<SensorReading>(set.Collections.SensorReadings);
});

// Ensure TS collection on startup
builder.Services.AddHostedService<TimeSeriesInitializer>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
