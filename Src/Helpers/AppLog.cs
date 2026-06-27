using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MsLogger = Microsoft.Extensions.Logging.ILogger;

namespace Tsundoku.Helpers;

/// <summary>
/// Static accessor for <see cref="ILoggerFactory"/> so that static classes that can't
/// receive DI (e.g. parsers, static helpers, model schema migrations) can still emit
/// source-generated log messages.
/// </summary>
public static class AppLog
{
    private static ILoggerFactory _factory = NullLoggerFactory.Instance;

    /// <summary>Called once after the DI container is built.</summary>
    public static void SetFactory(ILoggerFactory factory) => _factory = factory;

    public static MsLogger CreateLogger<T>() => _factory.CreateLogger<T>();

    public static MsLogger CreateLogger(string categoryName) => _factory.CreateLogger(categoryName);
}
