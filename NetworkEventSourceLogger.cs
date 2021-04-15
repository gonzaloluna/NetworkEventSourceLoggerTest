using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Tracing;
using System.Text;

namespace WebApplication1
{
    public class NetworkEventSourceLogger : EventListener
    {
        private readonly StringBuilder _messageBuilder = new StringBuilder();
        private readonly ILogger<NetworkEventSourceLogger> _logger;
        public NetworkEventSourceLogger(ILogger<NetworkEventSourceLogger> logger)
        {
            _logger = logger;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);
            if (eventSource.Name.Contains("Net"))
                EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            string message;
            lock (_messageBuilder)
            {
                _messageBuilder.Append("<- Event ");
                _messageBuilder.Append(eventData.EventSource.Name);
                _messageBuilder.Append(" - ");
                _messageBuilder.Append(eventData.EventName);
                _messageBuilder.Append(" : ");
                _messageBuilder.Append(string.Join(",", eventData.Payload));
                _messageBuilder.AppendLine(" ->");
                message = _messageBuilder.ToString();
                _messageBuilder.Clear();
            }
            _logger.LogDebug(message);
        }
    }

    /// <summary>
    /// Enables network logging if Logging:NetworkEventSourceLogger = true on appSettings.json
    /// </summary>
    public static class NetworkEventSourceExtensions
    {
        public static IApplicationBuilder UseNetworkEventSourceLogger(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetService(typeof(ILogger<NetworkEventSourceLogger>)) as ILogger<NetworkEventSourceLogger>;

            var configuration = app.ApplicationServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var value = configuration.GetSection("Logging:NetworkEventSourceLogger").Value;
            bool enabled = false;

            if (bool.TryParse(value, out enabled) && enabled) new NetworkEventSourceLogger(logger);
            return app;
        }
    }
}
