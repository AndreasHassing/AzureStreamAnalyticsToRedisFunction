using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace AzureStreamAnalyticsToRedisFunction
{
    public static class AzureStreamAnalyticsToRedis
    {
        private static readonly IConnectionMultiplexer redis;

        static AzureStreamAnalyticsToRedis()
        {
            string redisConnectionString = Environment.GetEnvironmentVariable("RedisConnectionString", EnvironmentVariableTarget.Process);

            redis = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        [FunctionName("AzureStreamAnalyticsToRedis")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await req.ReadRequestBodyAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                // Azure Stream Analytics connectivity request
                return new NoContentResult();
            }

            if (!(requestBody.TrimStart().StartsWith("[") && requestBody.TrimEnd().EndsWith("]")))
            {
                string error = "Received metrics are not a JSON-array";

                log.LogError(error);

                return new BadRequestObjectResult(new { error });
            }

            JArray metricPoints = JArray.Parse(requestBody);

            IDatabase db = redis.GetDatabase();

            List<string> errors = new();
            foreach (JObject metricPoint in metricPoints)
            {
                if (!(metricPoint.TryGetValue("MetricKey", StringComparison.OrdinalIgnoreCase, out JToken metricKey) && metricKey.Type == JTokenType.String))
                {
                    string error = $"Missing string MetricKey property for {metricPoint}";

                    log.LogError(error);
                    errors.Add(error);
                }

                string redisKey = metricKey.Value<string>();
                string redisValue = JsonConvert.SerializeObject(metricPoint);

                await db.SortedSetAddAsync(redisKey, redisValue, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }

            return errors.Count == 0
                ? new OkResult()
                : new BadRequestObjectResult(new { errors });
        }
    }
}
