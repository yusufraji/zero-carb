# Getting Started

This SDK has several entry points:

- You can run the application using the [CLI](./src/CarbonAware.CLI).

- You can build a container containing the [WebAPI](./src/CarbonAware.WebApi) and connect via REST requests.

- (Future) You can install the Nuget package and make requests directly. ([tracked here](https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues/40))

Each of these has configuration requirements which are detailed below. You can also visit the [quickstart.md](docs/quickstart.md) guide for a step-by-step process for running the CLI locally, deploying the Web API locally, polling the API via HTTP requests or generating and using client libraries (Python example).

## Pre-requisites

Make sure you have installed the following pre-requisites:

- dotnet core SDK [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download)
- WattTime account - See [instruction on WattTime](https://www.watttime.org/api-documentation/#register-new-user) for details (or use our python samples as described [here](samples/watttime-registration/readme.md)).

## Data Sources

We intend to support multiple data sources for carbon data.  At this time, only a JSON file and [WattTime](https://www.watttime.org/) are supported.  To use WattTime data, you'll need to acquire a license from them and set the appropriate configuration information.

## Configuration

This project uses standard [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) mechanisms.

The WebAPI project uses standard configuration sources provided by [ASPNetCore](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/).  Please review this link to understand how configuration is loaded and the priority of that configuration.

Please note that configuration is hierarchical.  The last configuration source loaded that contains a configuration value will be the value that's used.  This means that if the same configuration value is found in both appsettings.json and as an environment variable, the value from the environment variable will be the value that's applied.

### Configuration options

#### Environment variables
When adding values via environment variables, we recommend that you use the double underscore form, rather than the colon form.  Colons won't work in non-windows environment.  For example:

```bash
  CarbonAwareVars__CarbonIntensityDataSource="WattTime"
```

Note that double underscores are used to represent dotted notation or child elements that you see in the JSON below.  For example, to set proxy information using environment variables, you'd do this:

```bash
  CarbonAwareVars__Proxy__UseProxy
```

#### Local project settings

You have the possibility to use an untracked local settings file to override the project settings (that is loaded after the environement variables and will therefore superseed any variables of the same name).

Todo so, rename a copy of the local template called `appsettings.local.json.template` to `appsettings.local.json`.
Remove the first line of (invalid comments) and update the variables accordingly.

### CarbonAwareSDK Specific Configuration

#### CarbonAwareVars

Used to configure specific values that affect how the application gets data and the routes exposed.  The configuration looks like this:

```json
{
    "carbonAwareVars": {
        "carbonIntensityDataSource": "",
        "webApiRoutePrefix": "",
        "proxy": {
            "useProxy": false,
            "url": "",
            "username": "",
            "password": ""
        }
    }
}
```

##### carbonIntensityDataSource

Must be one of the following: `None, JSON, WattTime`.  

If set to `WattTime`, WattTime configuration must also be supplied.

`None` is the default, and if this value is supplied, an exception will be thrown at startup.

`JSON` will result in the data being loaded from a [json file](./src/CarbonAware.DataSources.Json/test-data-azure-emissions.json) compiled into the project.  You should not use these values in production, since they are static and don't represent carbon intensity accurately.

##### webApiRoutePrefix

Used to add a prefix to all routes in the WebApi project.  Must start with a `/`.  Invalid paths will cause an exception to be thrown at startup.

By default, all controllers are off of the root path.  For example:

```bash
http://localhost/emissions
```

If this prefix is set, it will allow calls to controllers using the prefix, which can be helpful for cross cluster calls, or when proxies strip out information from headers.  For example, if this value is set to:

```bash
/mydepartment/myapp
```

Then calls can be made that look like this:

```bash
http://localhost/mydepartment/myapp/emissions
```

Note that the controllers still respond off of the root path.

##### proxy

This value is used to set proxy information in situations where internet egress requires a proxy.  For proxy values to be used `useProxy` must be set to `true`.  Other values should be set as needed for your environment.

### WattTime Configuration

If using the WattTime datasource, WattTime configuration is required.

```json
{
    "wattTimeClient":{
        "username": "",
        "password": "",
        "baseUrl": "https://api2.watttime.org/v2/"
    }
}
```
> **Sign up for a test account:** To create an account, follow these steps : https://www.watttime.org/api-documentation/#best-practices-for-api-usage

#### username

The username you receive from WattTime.  This value is required when using a WattTime datasource.

#### password

The WattTime password for the username supplied.  This value is required when using a WattTime datasource.

#### baseUrl

The url to use when connecting to WattTime.  Defaults to [https://api2.watttime.org/v2/](https://api2.watttime.org/v2/).

In normal use, you shouldn't need to set this value, but this value can be used to enable integration testing scenarios or if the WattTime url should change in the future.

### Logging Configuration

This project is using standard [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line).  To configure different log levels, please see the documentation at this link.

### Tracing and Monitoring Configuration
Application monitoring and tracing can be configured using the `TelemetryProvider` variable in the application configuration.  

```bash
CarbonAwareVars__TelemetryProvider="ApplicationInsights"
```
This application is integrated with Application Insights for monitoring purposes. The telemetry collected in the app is pushed to AppInsights and can be tracked for logs, exceptions, traces and more. To connect to your Application Insights instance, configure the `ApplicationInsights_Connection_String` variable.

```bash
ApplicationInsights_Connection_String="AppInsightsConnectionString"
```

You can alternatively configure using Instrumentation Key by setting the `AppInsights_InstrumentationKey` variable. However, Microsoft is ending technical support for instrumentation key–based configuration of the Application Insights feature soon. ConnectionString-based configuration should be used over InstrumentationKey. For more details, please refer to https://docs.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string?tabs=net. 

```bash
AppInsights_InstrumentationKey="AppInsightsInstrumentationKey"
```

### Verbosity 
You can configure the verbosity of the application error messages by setting the 'VerboseApi' enviroment variable. Typically, you would set this value to 'true' in the development or staging regions. When set to 'true', a detailed stack trace would be presented for any errors in the request. 
```bash
CarbonAwareVars__VerboseApi="true"
```

### Sample Environment Variable Configuration Using WattTime

```bash
CarbonAwareVars__CarbonIntensityDataSource="WattTime"
CarbonAwareVars__WebApiRoutePrefix="/microsoft/cse/fsi"
CarbonAwareVars__Proxy__UseProxy=true
CarbonAwareVars__Proxy__Url="http://10.10.10.1"
CarbonAwareVars__Proxy__Username="proxyUsername"
CarbonAwareVars__Proxy__Password="proxyPassword"
WattTimeClient__Username="wattTimeUsername"
WattTimeClient__Password="wattTimePassword"
```

### Sample Json Configuration Using WattTime

```json
{
    "carbonAwareVars": {
        "carbonIntensityDataSource": "WattTime",
        "webApiRoutePrefix": "/microsoft/cse/fsi",
        "proxy": {
            "useProxy": true,
            "url": "http://10.10.10.1",
            "username": "proxyUsername",
            "password": "proxyPassword"
        }
    },
    "wattTimeClient":{
        "username": "wattTimeUsername",
        "password": "wattTimePassword",
    }
}
```

## Publish WebAPI with container

You can publish Web API for Carbon Aware SDK with container. This instruction shows how to build / run container image with [Podman](https://podman.io/).

### Build container image

Following commands build the container which named to `carbon-aware-sdk-webapi` from sources.

```bash
$ cd src
$ podman build -t carbon-aware-sdk-webapi -f CarbonAware.WebApi/src/Dockerfile .
```

### Run Web API container

Carbon Aware SDK Web API publishes the service on Port 80, so you need to map it to local port. Following commands maps it to Port 8080.

You also need to configure the SDK with environment variables. They are minimum set when you use WattTime as a data source.

```bash
$ podman run -it --rm -p 8080:80 \
    -e CarbonAwareVars__CarbonIntensityDataSource="WattTime" \
    -e WattTimeClient__Username="wattTimeUsername" \
    -e WattTimeClient__Password="wattTimePassword" \
  carbon-aware-sdk-webapi
```

When you success to run the container, you can access it via HTTP client.

```bash
$ curl -s http://localhost:8080/emissions/forecasts/current?location=westus2 | jq
[
  {
    "generatedAt": "2022-08-10T14:10:00+00:00",
    "optimalDataPoint": {
      "location": "GCPD",
      "timestamp": "2022-08-10T20:40:00+00:00",
      "duration": 5,
      "value": 440.4361702590741
    },
            :
```
