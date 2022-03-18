# Introduction

> Todo!

## For local development

Make sure to create a `local.settings.json`-file with the following:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "FUNCTIONS_EXTENSION_VERSION": "~4",
    "RedisConnectionString": "<your StackExchange.Redis connection string>"
  }
}
```
