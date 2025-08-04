using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Function
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Escalator Diagnosis Triggered...");

            Request? readings;
            try
            {
                readings = await JsonSerializer.DeserializeAsync<Request>(req.Body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            }
            catch (JsonException)
            {
                return new BadRequestObjectResult("Invalid JSON format");
            }

            if(readings == null || readings.Readings == null || readings.Readings.Count == 0)
            {
                return new BadRequestObjectResult("Pass in Readings as input!");
            }

            List<DriveGearStatus> statuses = [];
            foreach(var r in readings.Readings)
            {
                var status = string.Empty;
                if(r.Temperature <= 25)
                {
                    status = "OK";
                }
                else if(r.Temperature <= 50)
                {
                    status = "WARNING";
                }
                else
                {
                    status = "DANGER";
                }
                _logger.LogInformation("DriveGearId: {DriveGearId}", r.DriveGearId);
                _logger.LogInformation("Status: {Status}" + status);
                var driveGearStatus = new DriveGearStatus
                {
                    DriveGearId = r.DriveGearId,
                    Status = status
                };
                statuses.Add(driveGearStatus);
            }
            return new OkObjectResult(statuses);
        }
    }
}

public class DriveGearStatus
{
    public int DriveGearId { get; set; }
    public string Status { get; set; }
}

public class Reading
{
    public int DriveGearId { get; set; }
    public long Timestamp { get; set; }
    public int Temperature { get; set; }
}

public class Request
{
    public List<Reading> Readings { get; set; }
}
