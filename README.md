# MongoDbTimeSeries

A simple ASP.NET Core Web API project that demonstrates how to use MongoDB Time Series Collections with C#.
We create a time-series collection for sensor data, insert bulk readings, query by time ranges, and run quick stats (avg/min/max).

[Read Article](https://medium.com/@unlu-fa/time-series-in-mongodb-using-c-6670e7af3917)

---

## Run MongoDB with Docker
docker run -d --name mongo \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=secret \
  mongo:7
  
## Run MongoDB with Docker

Start MongoDB using Docker:

```bash
docker run -d --name mongo \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=secret \
  mongo:latest
```

## Connect with MongoDB Compass
```bash
mongodb://admin:secret@localhost:27017/?authSource=admin
```

## Run docker container
```bash
dotnet run
```

Compass connection string:

```bash
mongodb://admin:secret@localhost:27017/?authSource=admin
```

## Example Requests

Seed data (generate fake readings):

```bash
curl -X POST "https://localhost:7093/api/readings/seed?count=1000&deviceId=meter-ist-013" -k
```


Query by time range:

```bash
curl -G "https://localhost:7093/api/readings" \
  --data-urlencode "deviceId=meter-ist-013" \
  --data-urlencode "from=2025-09-20T18:24:00Z" \
  --data-urlencode "to=2025-09-20T18:34:00Z" -k
```


Get quick stats (avg/min/max):

```bash
curl -G "https://localhost:7121/api/readings/stats" \
  --data-urlencode "deviceId=meter-ist-013" \
  --data-urlencode "from=2025-09-20T18:24:00Z" \
  --data-urlencode "to=2025-09-20T18:34:00Z" -k
```


