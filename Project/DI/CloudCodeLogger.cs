using Microsoft.Extensions.Logging;
using System;
using Unity.Services.CloudCode.Core;

// Define logger interface to match Microsoft.Extensions.Logging
public interface ICloudLogger : ILogger
{
    void LogDebug(string message);
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogError(string message, Exception exception);
}

// Implementation for Cloud Code
public class CloudCodeLogger : ICloudLogger
{
    // You could also implement a logger that uses Debug.Log via the IGameInstanceLogger
    // if provided in your context

    public void LogDebug(string message)
    {
        Console.WriteLine($"[DEBUG] {message}");
    }

    public void LogInformation(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING] {message}");
    }

    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public void LogError(string message, Exception exception)
    {
        Console.WriteLine($"[ERROR] {message}. Exception: {exception.Message}");
        if (exception.StackTrace != null)
        {
            Console.WriteLine($"Stack trace: {exception.StackTrace}");
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {        
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}