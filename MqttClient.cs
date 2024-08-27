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
    public void Connect()
    {
        var mqttFactory = new MqttFactory();

        mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("home-assistant.home").WithCredentials(new MqttClientCredentials("mqtt",Encoding.ASCII.GetBytes("mqtt"))).Build();
        //await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var response = mqttClient?.ConnectAsync(mqttClientOptions, CancellationToken.None);

        Console.WriteLine("The MQTT client is connected.");

        response.DumpToConsole();
    }

    public void Disconnect()
    {
        // This will send the DISCONNECT packet. Calling _Dispose_ without DisconnectAsync the
        // connection is closed in a "not clean" way. See MQTT specification for more details.
        mqttClient?.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
    }
}
