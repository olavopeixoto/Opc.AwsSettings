# Opc.AwsSettings

[![NuGet version](https://img.shields.io/nuget/vpre/Opc.AwsSettings.svg)](https://www.nuget.org/packages/Opc.AwsSettings/)

A .NET library that simplifies loading configuration from AWS services including Parameter Store, Secrets Manager, and AppConfig into your application's configuration system.

## Features

- 🔧 **AWS Systems Manager Parameter Store** - Load configuration from hierarchical parameters
- 🔐 **AWS Secrets Manager** - Securely load secrets into your configuration
- ⚙️ **AWS AppConfig** - Load feature flags and freeform configuration
- 🔄 **Auto-reload** - Optionally reload configuration at specified intervals
- 🎯 **Easy Integration** - Seamlessly integrates with .NET's `IConfiguration` system
- 🌍 **Environment-aware** - Support for environment-specific configurations

## Installation

Install the NuGet package:

```bash
dotnet add package Opc.AwsSettings
```

## Quick Start

### Basic Usage with Host Builder

```csharp
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .AddAwsSettings() // Add AWS configuration sources
    .ConfigureServices(services =>
    {
        // Your service configuration
    })
    .Build();

await host.RunAsync();
```

### Manual Configuration Builder Usage

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAwsSettings() // Add AWS configuration sources
    .Build();
```

## Configuration

Configure the library using the `AwsSettings` section in your `appsettings.json`:

```json
{
  "AwsSettings": {
    "ReloadAfter": "00:05:00",
    "ParameterStore": {
      "Paths": ["/myapp/prod"],
      "Keys": []
    },
    "SecretsManager": {
      "LoadAll": false,
      "AcceptedSecretArns": [],
      "Prefix": null,
      "Optional": false
    },
    "AppConfig": {
      "ApplicationIdentifier": "my-app",
      "UseLambdaCacheLayer": false,
      "ConfigurationProfiles": []
    }
  }
}
```

## Usage Examples

### 1. Parameter Store Configuration

#### Simple Path-Based Loading

Load all parameters under a specific path prefix:

```json
{
  "AwsSettings": {
    "ParameterStore": {
      "Paths": ["/myservice"]
    }
  }
}
```

If your Parameter Store contains:

- `/myservice/Database/ConnectionString` = `"Server=..."`
- `/myservice/Database/MaxPoolSize` = `"100"`
- `/myservice/Api/Endpoint` = `"https://api.example.com"`

They will be mapped to:

```json
{
  "Database": {
    "ConnectionString": "Server=...",
    "MaxPoolSize": "100"
  },
  "Api": {
    "Endpoint": "https://api.example.com"
  }
}
```

#### Custom Key Mapping with Aliases

Use custom aliases to map Parameter Store keys to cleaner configuration names:

```json
{
  "AwsSettings": {
    "ParameterStore": {
      "Keys": [
        {
          "Path": "/aws/reference/secretsmanager/prod-db-credentials",
          "Alias": "Database:ConnectionString"
        },
        {
          "Path": "/myservice/config/api-key",
          "Alias": "ApiSettings:Key",
          "Optional": true
        }
      ]
    }
  }
}
```

#### Loading Secrets Manager via Parameter Store

Reference Secrets Manager values through Parameter Store:

```json
{
  "AwsSettings": {
    "ParameterStore": {
      "Keys": [
        {
          "Path": "/aws/reference/secretsmanager/my-secret",
          "Alias": "MySecret"
        }
      ]
    }
  }
}
```

### 2. Secrets Manager Configuration

#### Load All Secrets

Load all secrets from Secrets Manager:

```json
{
  "AwsSettings": {
    "SecretsManager": {
      "LoadAll": true,
      "Optional": false
    }
  }
}
```

#### Load Specific Secrets by ARN

```json
{
  "AwsSettings": {
    "SecretsManager": {
      "LoadAll": false,
      "AcceptedSecretArns": [
        "arn:aws:secretsmanager:us-east-1:123456789012:secret:prod/database",
        "arn:aws:secretsmanager:us-east-1:123456789012:secret:prod/api-keys"
      ]
    }
  }
}
```

#### Load Secrets with Prefix

Filter secrets by name prefix:

```json
{
  "AwsSettings": {
    "SecretsManager": {
      "LoadAll": true,
      "Prefix": "prod/myapp/"
    }
  }
}
```

### 3. AppConfig Configuration

#### Load AppConfig Profiles

```json
{
  "AwsSettings": {
    "AppConfig": {
      "ApplicationIdentifier": "my-application",
      "ConfigurationProfiles": [
        {
          "Identifier": "my-config-profile",
          "Optional": false
        },
        {
          "Identifier": "feature-flags",
          "Optional": true
        }
      ]
    }
  }
}
```

#### With Lambda Cache Layer

For AWS Lambda functions, enable the cache layer for improved performance:

```json
{
  "AwsSettings": {
    "AppConfig": {
      "ApplicationIdentifier": "my-application",
      "UseLambdaCacheLayer": true,
      "ConfigurationProfiles": [
        {
          "Identifier": "my-config-profile"
        }
      ]
    }
  }
}
```

### 4. Auto-Reload Configuration

Enable automatic reloading of configuration at specified intervals:

```json
{
  "AwsSettings": {
    "ReloadAfter": "00:05:00",  // Reload every 5 minutes
    "ParameterStore": {
      "Paths": ["/myapp"]
    }
  }
}
```

### 5. Environment-Specific Configuration

The library respects the `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT` variables:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .AddAwsSettings() // Automatically uses current environment
    .Build();
```

Or specify the environment explicitly:

```csharp
var configuration = new ConfigurationBuilder()
    .AddAwsSettings(environmentName: "Production")
    .Build();
```

### 6. Using Configuration in Your Application

#### Bind to Strongly-Typed Options

```csharp
public class DatabaseSettings
{
    public string ConnectionString { get; set; }
    public int MaxPoolSize { get; set; }
}

// In Program.cs
services.Configure<DatabaseSettings>(
    configuration.GetSection("Database")
);

// In your service
public class MyService
{
    private readonly DatabaseSettings _settings;
    
    public MyService(IOptions<DatabaseSettings> options)
    {
        _settings = options.Value;
    }
}
```

#### Direct Configuration Access

```csharp
var connectionString = configuration["Database:ConnectionString"];
var apiEndpoint = configuration["Api:Endpoint"];
```

#### Using GetSettings Extension Method

```csharp
var awsSettings = configuration.GetSettings<AwsSettings>();
```

### 7. With Logging

Enable logging to see what configuration is being loaded:

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => 
{
    builder.AddConsole();
});

var logger = loggerFactory.CreateLogger("AwsSettings");

var host = Host.CreateDefaultBuilder(args)
    .AddAwsSettings(logger)
    .Build();
```

## Complete Example

Here's a complete example combining multiple AWS configuration sources:

```json
{
  "AwsSettings": {
    "ReloadAfter": "00:10:00",
    "ParameterStore": {
      "Paths": ["/myapp/prod"],
      "Keys": [
        {
          "Path": "/aws/reference/secretsmanager/db-connection",
          "Alias": "Database:ConnectionString"
        }
      ]
    },
    "SecretsManager": {
      "LoadAll": false,
      "AcceptedSecretArns": [
        "arn:aws:secretsmanager:us-east-1:123456789012:secret:api-keys"
      ]
    },
    "AppConfig": {
      "ApplicationIdentifier": "myapp",
      "ConfigurationProfiles": [
        {
          "Identifier": "application-config"
        },
        {
          "Identifier": "feature-flags"
        }
      ]
    }
  }
}
```

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json");
        config.AddAwsSettings(context.HostingEnvironment.EnvironmentName);
    })
    .ConfigureServices((context, services) =>
    {
        // Configure your services using the loaded configuration
        services.Configure<DatabaseSettings>(
            context.Configuration.GetSection("Database")
        );
        services.Configure<ApiSettings>(
            context.Configuration.GetSection("Api")
        );
    })
    .Build();

await host.RunAsync();
```

## AWS Permissions Required

Ensure your application has the necessary AWS IAM permissions:

### Parameter Store

```json
{
  "Effect": "Allow",
  "Action": [
    "ssm:GetParameter",
    "ssm:GetParameters",
    "ssm:GetParametersByPath"
  ],
  "Resource": "arn:aws:ssm:*:*:parameter/myapp/*"
}
```

### Secrets Manager

```json
{
  "Effect": "Allow",
  "Action": [
    "secretsmanager:GetSecretValue",
    "secretsmanager:ListSecrets"
  ],
  "Resource": "arn:aws:secretsmanager:*:*:secret:*"
}
```

### AppConfig

```json
{
  "Effect": "Allow",
  "Action": [
    "appconfig:GetConfiguration",
    "appconfig:StartConfigurationSession"
  ],
  "Resource": "*"
}
```

## Best Practices

1. **Use Parameter Store paths for hierarchical configuration** - Organize your parameters by environment and service
2. **Use Secrets Manager for sensitive data** - Store passwords, API keys, and certificates in Secrets Manager
3. **Enable auto-reload for dynamic configuration** - Set appropriate `ReloadAfter` intervals for configuration that changes
4. **Use AppConfig for feature flags** - Leverage AppConfig for runtime feature toggles
5. **Set optional flags appropriately** - Mark non-critical configuration as optional to prevent startup failures
6. **Use environment-specific prefixes** - Organize parameters like `/myapp/prod/...` and `/myapp/dev/...`

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source. Please check the repository for license details.

## Links

- [GitHub Repository](https://github.com/olavopeixoto/opc.awssettings)
- [NuGet Package](https://www.nuget.org/packages/Opc.AwsSettings/)

# Opc.AwsSettings

An opinionated .NET Standard 2.0 library that provides support for all AWS settings options including Parameter Store, Secrets Manager, AppConfig Freeform and AppConfig Feature Flags.
