namespace MongoDbTimeSeries.Models;

public class SensorReading
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public SensorMeta Meta { get; set; } = default!;
}
