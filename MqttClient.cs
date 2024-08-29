using System;
using System.Data;
using MQTTnet.Formatter;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Net;

namespace Frigate_Helper;

public class MqttClient
{
    MqttFactory? mqttFactory;
    MQTTnet.Client.IMqttClient? mqttClient;
    MqttClientOptions? mqttClientOptions;

    public delegate void EventHandler(Event e);
    public event EventHandler? Event;

    public MqttClientConnectResult? Connect()
    {
        return ConnectAsync().Result;
    }

    public async Task<MqttClientConnectResult?> ConnectAsync()
    {
        mqttFactory = new MqttFactory();

        if(mqttClient!=null)throw new Exception("Connected");

        mqttClient = mqttFactory.CreateMqttClient();


        mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var message = new Event(e.ApplicationMessage.ConvertPayloadToString());
                //Console.WriteLine(message.ID);
                Event?.Invoke(message);


                return Task.CompletedTask;
            };

        mqttClient.DisconnectedAsync += async e =>
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await InternalConnect();
            
        };

        mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("home-assistant.home")
            .WithCredentials("mqtt","mqtt")
            .Build();

        var response = await InternalConnect();

        

        

        return response;
    }

    async Task<MqttClientConnectResult?> InternalConnect()
    {
        MqttClientConnectResult? response = null;
        try
        {
            if(mqttClient != null)
            {
                response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None); // Since 3.0.5 with CancellationToken
                Console.WriteLine("Connected");

                Console.WriteLine("The MQTT client is connected.");
        
                response.DumpToConsole();

                var mqttSubscribeOptions = mqttFactory?.CreateSubscribeOptionsBuilder().WithTopicFilter("frigate/events").Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic.");

                return response;
            }
            
        }
        catch
        {
            Console.WriteLine("### RECONNECTING FAILED ###");
        }

        
        return response;
    }

    const string mqttBaseTopic = "frigate-helper/";

    internal void Publish(Statistic<int> s)
    {
        string topic = mqttBaseTopic + s.Topic;

        var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(s.Value.ToString())
                .Build();

        mqttClient!.PublishAsync(applicationMessage, CancellationToken.None);
    }

    internal void Publish(string topic, string payload)
    {
        string fullTopic = mqttBaseTopic + topic;

        var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(fullTopic)
                .WithPayload(payload)
                .Build();

        mqttClient!.PublishAsync(applicationMessage, CancellationToken.None);
    }

    public void Disconnect()
    {
        // This will send the DISCONNECT packet. Calling _Dispose_ without DisconnectAsync the
        // connection is closed in a "not clean" way. See MQTT specification for more details.
        mqttClient?.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
    }
}
