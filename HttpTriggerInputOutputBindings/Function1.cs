using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Core;
using System.Text.Json.Serialization;

namespace HttpTriggerInputOutputBindings
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookmarks/{id}")] HttpRequest req,
            [CosmosDB(databaseName: "func-io-learn-db", containerName: "Bookmarks", Connection = "CosmosDB", Id = "{id}", PartitionKey = "{id}")] Bookmark bookmark,
            [CosmosDB(databaseName: "func-io-learn-db", containerName: "Bookmarks", Connection = "CosmosDB")] IAsyncCollector<Bookmark> newBookmark,
            [Queue("bookmarks-post-process", Connection = "StorageAccount")] IAsyncCollector<Bookmark> bookmarkToQueue,
            string id,
            ILogger log)
        {
            if(bookmark == null)
            {
                log.LogInformation("Bookmark already exists");
                return new ObjectResult(new { StatusCode = 422, Value = "Bookmark already exists" });
            }
            var nb = await JsonSerializer.DeserializeAsync<RequestBody>(req.Body);
            var newNewBookmark = new Bookmark { Id = nb.Id, Url = nb.Url };
            await newBookmark.AddAsync(newNewBookmark);
            await bookmarkToQueue.AddAsync(newNewBookmark);
            log.LogInformation("Bookmark " + nb.Id + " added");
            return new OkObjectResult(bookmark);
        }
    }

    public class RequestBody
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class Bookmark
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("_rid")]
        public string Rid { get; set; }

        [JsonPropertyName("_self")]
        public string Self { get; set; }

        [JsonPropertyName("_etag")]
        public string ETag { get; set; }

        [JsonPropertyName("_attachments")]
        public string Attachments { get; set; }

        [JsonPropertyName("_ts")]
        public int Timestamp { get; set; }
    }
}
