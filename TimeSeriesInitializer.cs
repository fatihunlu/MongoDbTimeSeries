using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDbTimeSeries.Models;

namespace MongoDbTimeSeries;
public class MongoSettings
{
    public string Name { get; set; } = "AppDb";
    public Collections Collections { get; set; } = new();
    public TimeSeriesOptionsConfig TimeSeries { get; set; } = new();
}

public class Collections { public string SensorReadings { get; set; } = "SensorReadings"; }

public class TimeSeriesOptionsConfig
{
    public string Granularity { get; set; } = "Seconds"; // Seconds|Minutes|Hours
    public int TtlDays { get; set; } = 30;
}

public class TimeSeriesInitializer : IHostedService
{
    private readonly IMongoClient _client;
    private readonly MongoSettings _settings;

    public TimeSeriesInitializer(IMongoClient client, IOptions<MongoSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var db = _client.GetDatabase(_settings.Name);
        var collName = _settings.Collections.SensorReadings;

        var existing = await db.ListCollectionNames().ToListAsync(cancellationToken);
        if (!existing.Contains(collName))
        {
            var gran = _settings.TimeSeries.Granularity switch
            {
                "Minutes" => TimeSeriesGranularity.Minutes,
                "Hours" => TimeSeriesGranularity.Hours,
                _ => TimeSeriesGranularity.Seconds
            };

            var ts = new TimeSeriesOptions(
                timeField: nameof(SensorReading.Timestamp),
                metaField: nameof(SensorReading.Meta),
                granularity: gran
            );

            var opts = new CreateCollectionOptions
            {
                TimeSeriesOptions = ts,
                ExpireAfter = TimeSpan.FromDays(_settings.TimeSeries.TtlDays)
            };

            await db.CreateCollectionAsync(collName, opts, cancellationToken);

            var col = db.GetCollection<SensorReading>(collName);
            var metaIndex = Builders<SensorReading>.IndexKeys
                .Ascending(x => x.Meta.DeviceId)
                .Ascending(x => x.Meta.Location);
            await col.Indexes.CreateOneAsync(new CreateIndexModel<SensorReading>(metaIndex), cancellationToken: cancellationToken);
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}