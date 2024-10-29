using System;
using System.Data;
using MQTTnet.Formatter;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Net;

namespace Frigate_Helper
{
    /// <summary>
    /// Represents an MQTT client for connecting to and interacting with an MQTT broker.
    /// </summary>
    public class MqttClient
    {
        // Factory for creating MQTT clients.
        private MqttFactory? mqttFactory;
        
        // The MQTT client instance.
        private MQTTnet.Client.IMqttClient? mqttClient;
        
        // Options for configuring the MQTT client.
        private MqttClientOptions? mqttClientOptions;

        /// <summary>
        /// Event handler for application messages received from the MQTT broker.
        /// </summary>
        public delegate void EventHandler(Event e);
        public event EventHandler? Event;
        
        // Base topic for MQTT messages.
        private string mqttBaseTopic;
        
        public MqttClient()
        {
            mqttFactory = new MqttFactory();
            mqttBaseTopic = Environment.GetEnvironmentVariable("MQTT_BASE_TOPIC");
            mqttBaseTopic ??= "frigate-helper/";
            Console.WriteLine($"MQTT Base Topic: {mqttBaseTopic}");
        }

        /// <summary>
        /// Connects to the MQTT broker synchronously.
        /// </summary>
        /// <returns>The result of the connection attempt.</returns>
        public MqttClientConnectResult? Connect()
        {
            return ConnectAsync().Result;
        }

        /// <summary>
        /// Asynchronously connects to the MQTT broker.
        /// </summary>
        /// <returns>The result of the connection attempt.</returns>
        public async Task<MqttClientConnectResult?> ConnectAsync()
        {

            // Ensure the client is not already connected.
            if (mqttClient != null) throw new Exception("Connected");

            // Create a new MQTT client.
            mqttClient = mqttFactory.CreateMqttClient();

            // Handle incoming messages from the broker.
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var message = new Event(e.ApplicationMessage.ConvertPayloadToString());
                //Console.WriteLine(message.ID);
                Event?.Invoke(message);

                return Task.CompletedTask;
            };

            // Handle disconnection and attempt reconnection.
            mqttClient.DisconnectedAsync += async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                await InternalConnect();
            };

            // Retrieve MQTT connection details from environment variables.
            string? mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST");
            mqttHost ??= "home-assistant.home";

            string? user = Environment.GetEnvironmentVariable("MQTT_USER");
            user ??= "mqtt";

            string? password = Environment.GetEnvironmentVariable("MQTT_PASSWORD");
            password ??= "mqtt";

            // Build the MQTT client options.
            mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttHost)
                .WithCredentials(user, password)
                .Build();

            // Attempt to connect to the broker.
            var response = await InternalConnect();

            return response;
        }

        /// <summary>
        /// Asynchronously reconnects to the MQTT broker.
        /// </summary>
        /// <returns>The result of the reconnection attempt.</returns>
        private async Task<MqttClientConnectResult?> InternalConnect()
        {
            MqttClientConnectResult? response = null;
            try
            {
                if (mqttClient != null)
                {
                    // Connect to the broker with the specified options.
                    response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None); // Since 3.0.5 with CancellationToken
                    Console.WriteLine("Connected");

                    Console.WriteLine("The MQTT client is connected.");
                    response.DumpToConsole();

                    // Subscribe to the specified topic.
                    var mqttSubscribeOptions = mqttFactory?.CreateSubscribeOptionsBuilder().WithTopicFilter("frigate/events").Build();

                    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                    Console.WriteLine("MQTT client subscribed to topic.");

                    return response;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("### RECONNECTING FAILED {0} ###",ex.Message);
            }

            return response;
        }

        
        /// <summary>
        /// Publishes a statistic to the MQTT broker.
        /// </summary>
        /// <param name="s">The statistic to publish.</param>
        internal void Publish(Statistic<int> s)
        {
            string topic = mqttBaseTopic + s.Topic;

            // Build the application message with the statistic value.
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(s.Value.ToString())
                .Build();

            // Publish the message to the broker.
            mqttClient!.PublishAsync(applicationMessage, CancellationToken.None);
        }

        /// <summary>
        /// Publishes a message to the MQTT broker.
        /// </summary>
        /// <param name="topic">The topic to publish to.</param>
        /// <param name="payload">The payload to publish.</param>
        internal void Publish(string topic, string payload)
        {
            string fullTopic = mqttBaseTopic + topic;

            // Build the application message with the specified payload.
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(fullTopic)
                .WithPayload(payload)
                .Build();

            // Publish the message to the broker.
            mqttClient!.PublishAsync(applicationMessage, CancellationToken.None);
        }

        /// <summary>
        /// Disconnects from the MQTT broker.
        /// </summary>
        public void Disconnect()
        {
            // This will send the DISCONNECT packet. Calling _Dispose_ without DisconnectAsync the
            // connection is closed in a "not clean" way. See MQTT specification for more details.
            mqttClient?.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
        }
    }
}
