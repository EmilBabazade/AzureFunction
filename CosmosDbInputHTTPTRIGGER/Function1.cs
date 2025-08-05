using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;

namespace CosmosDbInputHTTPTRIGGER
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "bookmarks/{id}")] HttpRequest req,
            [CosmosDB(databaseName: "func-io-learn-db", containerName: "Bookmarks", Connection = "CosmosDB", Id = "{id}", PartitionKey = "{id}")] Bookmark bookmark,
            string id,
            ILogger log)
        {
            log.LogInformation("Bookmark " + bookmark?.Id);
            if(bookmark != null)
            {
                return new OkObjectResult(bookmark);
            }
            else
            {
                log.LogInformation("Bookmark not found");
                return new NotFoundObjectResult("Bookmark not found");
            }
        }
    }

    public class Bookmark
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("_rid")]
        public string Rid { get; set; }

        [JsonProperty("_self")]
        public string Self { get; set; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }

        [JsonProperty("_attachments")]
        public string Attachments { get; set; }

        [JsonProperty("_ts")]
        public int Timestamp { get; set; }
    }
}
