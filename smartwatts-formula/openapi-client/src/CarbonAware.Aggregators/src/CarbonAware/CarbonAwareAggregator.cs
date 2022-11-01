using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareAggregator : ICarbonAwareAggregator
{
    private static readonly ActivitySource Activity = new ActivitySource(nameof(CarbonAwareAggregator));
    private readonly ILogger<CarbonAwareAggregator> _logger;
    private readonly ICarbonIntensityDataSource _dataSource;

    /// <summary>
    /// Creates a new instance of the <see cref="CarbonAwareAggregator"/> class.
    /// </summary>
    /// <param name="logger">The logger for the aggregator</param>
    /// <param name="dataSource">An <see cref="ICarbonIntensityDataSource"> data source.</param>
    public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonIntensityDataSource dataSource)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._dataSource = dataSource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        { 
            var (start, end) = this.GetAndValidateLocationTimeWindow(props);
            return await this._dataSource.GetCarbonIntensityAsync(GetMutlipleLocationsOrThrow(props), start, end);
        }
    }

    public (DateTimeOffset, DateTimeOffset, Boolean) GetAndValidateLocationTimeWindow2(IDictionary props)
    {
        return (DateTimeOffset.Now, DateTimeOffset.Now, true);
    }


    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(IDictionary props)
    {
        var results = await GetEmissionsDataAsync(props);
        return GetOptimalEmissions(results);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            _logger.LogInformation("Aggregator getting carbon intensity forecast from data source");

            var forecasts = new List<EmissionsForecast>();
            foreach (var location in GetMutlipleLocationsOrThrow(props))
            {
                var forecast = await this._dataSource.GetCurrentCarbonIntensityForecastAsync(location);
                var emissionsForecast = ProcessAndValidateForecast(forecast, props);
                forecasts.Add(emissionsForecast);
            }

            return forecasts;
        }
    }

    /// <inheritdoc />
    public async Task<double> CalculateAverageCarbonIntensityAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            var start = GetOffsetOrThrow(props, CarbonAwareConstants.Start);
            var end = GetOffsetOrThrow(props, CarbonAwareConstants.End);
            var location = GetSingleLocationOrThrow(props);
            ValidateDateInput(start, end);
            _logger.LogInformation("Aggregator getting average carbon intensity from data source");
            var emissionData = await this._dataSource.GetCarbonIntensityAsync(location, start, end);
            var value = emissionData.AverageOverPeriod(start, end);
            _logger.LogInformation($"Carbon Intensity Average: {value}");

            return value;
        }
    }

    public async Task<EmissionsForecast> GetForecastDataAsync(IDictionary props)
    {
        EmissionsForecast forecast;
        using (var activity = Activity.StartActivity())
        {
            (var location, var forecastRequestedAt) = GetAndValidateForecastInput(props);
            _logger.LogDebug($"Aggregator getting carbon intensity forecast from data source for location {location} and requestedAt {forecastRequestedAt}");

            forecast = await this._dataSource.GetCarbonIntensityForecastAsync(location, forecastRequestedAt);
            var emissionsForecast = ProcessAndValidateForecast(forecast, props);
            return emissionsForecast;
        }
    }

    /// <summary>
    /// Parses provided start and end times for missing and invalid results. 
    /// </summary>
    /// <param name="props"></param>
    /// <returns>A start and end DateTimeOffset validated based on potential missing user input on start and end values.
    /// Also returns flag if either or both start and end times are missing.
    /// </returns>
    private (DateTimeOffset, DateTimeOffset) GetAndValidateLocationTimeWindow(IDictionary props)
    {
        var startProp = props[CarbonAwareConstants.Start];
        var endProp = props[CarbonAwareConstants.End];

        DateTimeOffset windowStart;
        DateTimeOffset windowEnd;

        windowStart = this.GetOffsetOrDefault(props, CarbonAwareConstants.Start, DateTimeOffset.UtcNow);
            
        if(endProp is null)
        {
            windowEnd = windowStart;
        } 
        else
        {
            if (startProp is null)
            {
                Exception ex = new ArgumentException("'time is required if 'toTime' is passed");
                _logger.LogError("argument exception", ex);
                throw ex;
            }

            windowEnd = this.GetOffsetOrDefault(props, CarbonAwareConstants.End, DateTimeOffset.UtcNow);
        }

        return (windowStart, windowEnd);
    }

    /// <summary>
    /// Limits results set to 1 element when only time or toTime or neither are passed.
    /// </summary>
    /// <param name="emissionsData"></param>
    /// <returns>New List with only the lastest element returned by Time</returns>
    private IEnumerable<EmissionsData> EnsureSingleResult(IEnumerable<EmissionsData> emissionsData)
    {
        if (emissionsData.Count() == 0) return new List<EmissionsData>();
        return new List<EmissionsData>() { emissionsData.OrderByDescending(x => x.Time).First() };
    } 

    private (Location, DateTimeOffset) GetAndValidateForecastInput(IDictionary props)
    {
        var error = new ArgumentException("Invalid EmissionsForecast request");
        Location location = new ();
        DateTimeOffset requestedAt = new ();
        try {
            location = GetSingleLocationOrThrow(props);
        } catch (ArgumentException e) {
            error.Data["location"] = e.Message;
        }
        try {
            requestedAt = GetOffsetOrThrow(props, CarbonAwareConstants.ForecastRequestedAt);
        } 
        catch (ArgumentException e) {
            error.Data["requestedAt"] = e.Message;
        }
        if (error.Data.Count > 0)
        {
            throw error;
        }
        return (location, requestedAt);
    }

    private EmissionsForecast ProcessAndValidateForecast(EmissionsForecast forecast, IDictionary props)
    {
        var windowSize = GetDurationOrDefault(props);
        var firstDataPoint = forecast.ForecastData.First();
        var lastDataPoint = forecast.ForecastData.Last();
        forecast.DataStartAt = GetOffsetOrDefault(props, CarbonAwareConstants.Start, firstDataPoint.Time);
        forecast.DataEndAt = GetOffsetOrDefault(props, CarbonAwareConstants.End, lastDataPoint.Time + lastDataPoint.Duration);
        forecast.Validate();
        forecast.ForecastData = IntervalHelper.FilterByDuration(forecast.ForecastData, forecast.DataStartAt, forecast.DataEndAt);
        forecast.ForecastData = forecast.ForecastData.RollingAverage(windowSize, forecast.DataStartAt, forecast.DataEndAt);
        forecast.OptimalDataPoints = GetOptimalEmissions(forecast.ForecastData);
        if (forecast.ForecastData.Any())
        {
            forecast.WindowSize = forecast.ForecastData.First().Duration;
        }
        return forecast;
    }

    private IEnumerable<EmissionsData> GetOptimalEmissions(IEnumerable<EmissionsData> emissionsData)
    {
        if (!emissionsData.Any())
        {
            return Array.Empty<EmissionsData>();
        }

        var bestResult = emissionsData.MinBy(x => x.Rating);

        IEnumerable<EmissionsData> results = Array.Empty<EmissionsData>();

        if(bestResult != null)
        {
            results = emissionsData.Where(x => x.Rating == bestResult.Rating);
        }

        return results;
    }

    /// <summary>
    /// Extracts the given offset prop and converts to DateTimeOffset. If prop is not defined, defaults to provided default
    /// </summary>
    /// <param name="props"></param>
    /// <returns>DateTimeOffset representing end period of carbon aware data search. </returns>
    /// <exception cref="ArgumentException">Throws exception if prop isn't a valid DateTimeOffset. </exception>
    private DateTimeOffset GetOffsetOrDefault(IDictionary props, string field, DateTimeOffset defaultValue)
    {
        // Default if null
        var dateTimeOffset = props[field] ?? defaultValue;
        DateTimeOffset outValue;
        // If fail to parse property, throw error
        if (!DateTimeOffset.TryParse(dateTimeOffset.ToString(), null, DateTimeStyles.AssumeUniversal, out outValue))
        {
            Exception ex = new ArgumentException("Failed to parse" + field + "field. Must be a valid DateTimeOffset");
            _logger.LogError("argument exception", ex);
            throw ex;
        }

        return outValue;
    }

    /// <summary>
    /// Extracts the given offset prop and converts to DateTimeOffset. If prop is not defined, throws
    /// </summary>
    /// <param name="props"></param>
    /// <returns>DateTimeOffset representing end period of carbon aware data search. </returns>
    /// <exception cref="ArgumentException">Throws exception if prop isn't found or isn't a valid DateTimeOffset. </exception>
    private DateTimeOffset GetOffsetOrThrow(IDictionary props, string field)
    {
        if (props[field] != null)
        {
            return GetOffsetOrDefault(props, field, DateTimeOffset.MinValue);
        }

        Exception ex = new ArgumentException("Failed to find" + field + "field. Must be a valid DateTimeOffset");
        _logger.LogError("argument exception", ex);
        throw ex;
    }

    private void ValidateDateInput(DateTimeOffset start, DateTimeOffset end)
    {
        if (start >= end)
        {
            throw new ArgumentException($"Invalid start and end. Start time must come before end time. start is {start}, end is {end}");
        }
    }

    private IEnumerable<Location> GetMutlipleLocationsOrThrow(IDictionary props)
    {
        if (props[CarbonAwareConstants.MultipleLocations] is IEnumerable<Location> locations)
        {
            return locations;
        }
        Exception ex = new ArgumentException("locations parameter must be provided and be non empty");
        _logger.LogError("argument exception", ex);
        throw ex;
    }

    private Location GetSingleLocationOrThrow(IDictionary props)
    {        
        if (props[CarbonAwareConstants.SingleLocation] is Location location)
        {
            return location;
        }
        Exception ex = new ArgumentException("location parameter must be provided");
        _logger.LogError("argument exception", ex);
        throw ex;
    }

    private TimeSpan GetDurationOrDefault(IDictionary props, TimeSpan defaultValue = default)
    {
        if (props[CarbonAwareConstants.Duration] is int duration)
        {
            return TimeSpan.FromMinutes(duration);
        }
        return defaultValue;
    }
}
