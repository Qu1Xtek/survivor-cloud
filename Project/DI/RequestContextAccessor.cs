using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.DI
{
    /// <summary>
    /// Ambient per-request context for Cloud Code execution.
    /// Uses AsyncLocal so concurrent requests don't overwrite each other.
    /// </summary>
    public sealed class RequestContextAccessor
    {
        private static readonly AsyncLocal<ILogger?> _currentLogger = new();
        private static readonly AsyncLocal<ILogger?> _previousLogger = new();
        private static readonly AsyncLocal<IExecutionContext?> _currentContext = new();

        public ILogger Logger => _currentLogger.Value ?? NullLogger.Instance;
        public IExecutionContext? ExecutionContext => _currentContext.Value;

        public void PushLogger(ILogger logger)
        {
            _previousLogger.Value = _currentLogger.Value;
            _currentLogger.Value = logger;
        }

        public void PopLogger()
        {
            _currentLogger.Value = _previousLogger.Value;
            _previousLogger.Value = null;
        }

        public void PushContext(IExecutionContext? context)
        {
            _currentContext.Value = context;
        }
    }
}

