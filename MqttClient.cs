using System;
using System.Data;
using MQTTnet.Formatter;
using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace Frigate_Helper;

public class MqttClient
{
    MQTTnet.Client.IMqttClient? mqttClient;
    MqttClientOptions? mqttClientOptions;

    public delegate void EventHandler(Event e);
    public event EventHandler? Event;

    public MqttClientConnectResult Connect()
    {
        return ConnectAsync().Result;
    }

    public async Task<MqttClientConnectResult> ConnectAsync()
    {
        var mqttFactory = new MqttFactory();

        if(mqttClient!=null)throw new Exception("Connected");

        mqttClient = mqttFactory.CreateMqttClient();


        mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Received application message.");
                //e.DumpToConsole();
                
                var message = new Event(e.ApplicationMessage.ConvertPayloadToString());
                //Console.WriteLine(message.ID);
                Event?.Invoke(message);


                return Task.CompletedTask;
            };

        mqttClient.DisconnectedAsync += async e =>
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None); // Since 3.0.5 with CancellationToken
            }
            catch
            {
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        };

        mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("home-assistant.home")
            .WithCredentials("mqtt","mqtt")
            .Build();

        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        Console.WriteLine("The MQTT client is connected.");
        
        response.DumpToConsole();

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter("frigate/events").Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        Console.WriteLine("MQTT client subscribed to topic.");

        return response;
    }

    const string mqttBaseTopic = "frigate-helper/";

    internal void Publish(Statistic s)
    {
        string topic = mqttBaseTopic + "statistic/" + s.Name + "/";

        var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic + "moving")
                .WithPayload(s.Moving.ToString())
                .Build();

        mqttClient!.PublishAsync(applicationMessage, CancellationToken.None);

        applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic + "stationary")
                .WithPayload(s.Stationary.ToString())
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
