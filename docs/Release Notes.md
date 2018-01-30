# LogJam.Extensions.Logging and LogJam.AspNetCore Release Notes

## Version 0.5.0
1. Enables using LogJam as a LoggerProvider for .NET standard 1.3 and 2.0 apps and libraries
2. Enables using LogJam within ASP.NET Core 1.1 and 2.0 apps
3. The LogJam LoggerProvider can be configured using Microsoft.Extensions.Configuration and standard Logger config:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "System": "Debug"
    },
    "LogJam": {
      "LogLevel": {
        "Microsoft": "None"
      }
    }
  }
}
```
