using System.Reactive.Linq;
using System.Reactive.Subjects;
using GraphQL;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using Tsundoku.Clients;
using MsLogger = Microsoft.Extensions.Logging.ILogger;

namespace Tsundoku.Services;

/// <summary>
/// Defines a service that periodically checks the health of external APIs.
/// </summary>
public interface IApiHealthCheckService : IDisposable
{
    /// <summary>Observable that emits the current AniList API availability.</summary>
    IObservable<bool> IsAniListAvailable { get; }

    /// <summary>Observable that emits the current MangaDex API availability.</summary>
    IObservable<bool> IsMangaDexAvailable { get; }

    /// <summary>Gets whether AniList is currently available.</summary>
    bool IsAniListUp { get; }

    /// <summary>Starts the periodic health check timer.</summary>
    void Start();

    /// <summary>Stops the periodic health check timer.</summary>
    void Stop();

    /// <summary>Runs a manual health check immediately and returns (AniList, MangaDex) availability.</summary>
    Task<(bool AniList, bool MangaDex)> CheckNowAsync();
}

/// <summary>
/// Periodically checks the AniList and MangaDex APIs for availability
/// and exposes reactive observables for their status.
/// </summary>
public sealed class ApiHealthCheckService : IApiHealthCheckService
{
    private readonly MsLogger _logger;
    private readonly AniListGraphQLClient _aniListClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BehaviorSubject<bool> _aniListStatus = new(true);
    private readonly BehaviorSubject<bool> _mangaDexStatus = new(true);
    private Timer? _timer;

    private const int CheckIntervalMinutes = 10;

    public IObservable<bool> IsAniListAvailable => _aniListStatus.DistinctUntilChanged();
    public IObservable<bool> IsMangaDexAvailable => _mangaDexStatus.DistinctUntilChanged();
    public bool IsAniListUp => _aniListStatus.Value;

    public ApiHealthCheckService(AniListGraphQLClient aniListClient, IHttpClientFactory httpClientFactory, ILogger<ApiHealthCheckService> logger)
    {
        _aniListClient = aniListClient;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void Start()
    {
        // Run immediately then every 10 minutes
        _timer = new Timer(_ => _ = CheckAllSafeAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(CheckIntervalMinutes));
        _logger.ServiceStarted(CheckIntervalMinutes);
    }

    public async Task<(bool AniList, bool MangaDex)> CheckNowAsync()
    {
        await CheckAllSafeAsync();
        return (_aniListStatus.Value, _mangaDexStatus.Value);
    }

    private async Task CheckAllSafeAsync()
    {
        try
        {
            await CheckAllAsync();
        }
        catch (Exception ex)
        {
            _logger.UnhandledCheckException(ex);
        }
    }

    public void Stop()
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _timer?.Dispose();
        _timer = null;
        _logger.ServiceStopped();
    }

    private async Task CheckAllAsync()
    {
        await Task.WhenAll(CheckAniListAsync(), CheckMangaDexAsync());
    }

    private async Task CheckAniListAsync()
    {
        try
        {
            GraphQLRequest request = new GraphQLRequest
            {
                Query = "{ Page(page:1,perPage:1) { media(type:MANGA) { id } } }"
            };

            GraphQLResponse<object> response = await _aniListClient.SendQueryAsync<object>(request);
            bool isAvailable = response.Errors is null || response.Errors.Length == 0;
            _aniListStatus.OnNext(isAvailable);

            if (!isAvailable)
            {
                string errorMsg = string.Join("; ", response.Errors!.Select(e => e.Message));
                _logger.AniListCheckFailedResponse(errorMsg);
            }
            else
            {
                _logger.AniListCheckPassed();
            }
        }
        catch (Exception ex)
        {
            _aniListStatus.OnNext(false);
            _logger.AniListCheckFailedException(ex);
        }
    }

    private async Task CheckMangaDexAsync()
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient("MangaDexClient");
            using HttpResponseMessage response = await client.GetAsync("ping");
            bool isAvailable = response.IsSuccessStatusCode;
            _mangaDexStatus.OnNext(isAvailable);

            if (!isAvailable)
            {
                _logger.MangaDexCheckFailedResponse(response.StatusCode);
            }
            else
            {
                _logger.MangaDexCheckPassed();
            }
        }
        catch (Exception ex)
        {
            _mangaDexStatus.OnNext(false);
            _logger.MangaDexCheckFailedException(ex);
        }
    }

    public void Dispose()
    {
        Stop();
        _aniListStatus.Dispose();
        _mangaDexStatus.Dispose();
    }
}
