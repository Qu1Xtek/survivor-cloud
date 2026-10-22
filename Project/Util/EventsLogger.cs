using CloudLogic.DI;
using Microsoft.Extensions.Logging;
using System;

namespace CloudLogic.Util
{
    public class EventsLogger
    {
        private ILogger _logger;
        private RequestContextAccessor _context;

        public EventsLogger(ILogger logger, RequestContextAccessor context)
        {
            _logger = logger;
            _context = context;
        }

        public void LogWithEx(string eventName, Exception ex)
        {
            _logger.LogError(ex, $"{eventName} errored, " +
                $"for player {_context?.ExecutionContext?.PlayerId}, " +
                $"with exception {ex.Message}, " +
                $"correlationId - {_context?.ExecutionContext?.CorrelationId} "+
                $"at {ex}");
        }

        public void LogWithCustom(string eventName, string errorDescription)
        {
            _logger.LogError($"{eventName} errored, " +
                $"for player {_context?.ExecutionContext?.PlayerId}, " +
                $"reason {errorDescription}, " +
                $"correlationId - {_context?.ExecutionContext?.CorrelationId}");
        }
    }
}
