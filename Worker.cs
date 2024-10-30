namespace Frigate_Helper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private MqttClient mqtt = new();
        private readonly EventHandler eventHandler = new();

        // Constructor to initialize the Worker class with an ILogger instance
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            // Subscribe to the MQTT event to handle incoming messages
            mqtt.Event += e =>
            {
                _logger.LogInformation("Received MQTT event: {event}", e);
                eventHandler.Handle(e); // Handle the event using the event handler
            };

            // Subscribe to the StatisticReady event to publish data when statistics are ready
            StatisticHelper.StatisticReady += e =>
            {
                _logger.LogInformation("Publishing statistics to {topic}", e.Topic);
                mqtt.Publish(e.Topic, e.ToPayload()); // Publish the payload to the specified topic
            };

            // Attempt to connect to the MQTT broker and log the result
            try
            {
                mqtt.Connect();
                _logger.LogInformation("Connected to MQTT broker successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker."); // Log error details if connection fails
            }
        }

        // This method is called when the service is started
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Optional: Log every minute when the worker is running
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                // Wait for 60 seconds or until a cancellation is requested
                await Task.Delay(60000, stoppingToken);

                // Perform periodic updates
                Console.WriteLine("====Periodic Update All====");
                eventHandler.GenerateStatistics(true); // Generate and publish statistics
                Console.WriteLine("===============");
            }

            _logger.LogInformation("Worker is stopping.");
        }
    }
}