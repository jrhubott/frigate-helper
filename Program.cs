using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Formatter;
using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace Frigate_Helper;
internal class Program
{
    private static async Task Main(string[] args)
    {

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("home-assistant.home").WithCredentials(new MqttClientCredentials("mqtt",Encoding.ASCII.GetBytes("mqtt"))).Build();
            //await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            Console.WriteLine("The MQTT client is connected.");

            response.DumpToConsole();

            // This will send the DISCONNECT packet. Calling _Dispose_ without DisconnectAsync the
            // connection is closed in a "not clean" way. See MQTT specification for more details.
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
        }


        var counter = 0;
        var max = args.Length is not 0 ? Convert.ToInt32(args[0]) : -1;
        while (max is -1 || counter < max)
        {
            Console.WriteLine($"Counter: {++counter}");
            await Task.Delay(TimeSpan.FromMilliseconds(1_000));
        }
    }
}