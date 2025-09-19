using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDbTimeSeries.Models;

namespace MongoDbTimeSeries.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly IMongoCollection<SensorReading> _col;
    public ReadingsController(IMongoCollection<SensorReading> col) => _col = col;

    // POST /api/readings/seed?count=1000&deviceId=device-F13
    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromQuery] int count = 1000, [FromQuery] string deviceId = "meter-ist-013", CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var batch = Enumerable.Range(0, count).Select(i => new SensorReading
        {
            Timestamp = now.AddSeconds(i),
            Value = Math.Sin(i / 10.0),
            Meta = new SensorMeta { DeviceId = deviceId, Location = "Istanbul/OfficeHQ", Model = "E350-Meter" }
        }).ToList();

        await _col.InsertManyAsync(batch, cancellationToken: ct);
        return Ok(new { inserted = batch.Count, deviceId });
    }

    // POST /api/readings  (custom bulk insert)
    [HttpPost]
    public async Task<IActionResult> InsertMany([FromBody] List<SensorReading> readings, CancellationToken ct)
    {
        if (readings is null || readings.Count == 0) return BadRequest("No readings.");
        await _col.InsertManyAsync(readings, cancellationToken: ct);
        return Ok(new { inserted = readings.Count });
    }

    // GET /api/readings?deviceId=device-F13&from=2025-01-01T00:00:00Z&to=2025-01-01T00:10:00Z
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string deviceId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var filter = Builders<SensorReading>.Filter.And(
            Builders<SensorReading>.Filter.Gte(x => x.Timestamp, from),
            Builders<SensorReading>.Filter.Lte(x => x.Timestamp, to),
            Builders<SensorReading>.Filter.Eq(x => x.Meta.DeviceId, deviceId)
        );

        var items = await _col.Find(filter).SortBy(x => x.Timestamp).ToListAsync(ct);
        return Ok(items);
    }

    // GET /api/readings/stats?deviceId=device-F13&from=...&to=...
    [HttpGet("stats")]
    public async Task<IActionResult> Stats([FromQuery] string deviceId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var stats = await _col.Aggregate()
            .Match(x => x.Meta.DeviceId == deviceId && x.Timestamp >= from && x.Timestamp <= to)
            .Group(x => 1, g => new
            {
                Count = g.Count(),
                Avg = g.Average(x => x.Value),
                Min = g.Min(x => x.Value),
                Max = g.Max(x => x.Value)
            })
            .FirstOrDefaultAsync(ct);

        return Ok(stats ?? new { Count = 0, Avg = 0.0, Min = 0.0, Max = 0.0 });
    }
}