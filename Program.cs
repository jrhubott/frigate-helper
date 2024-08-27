using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;



namespace Frigate_Helper;
internal class Program
{
    static MqttClient? mqtt;
    private static async Task Main(string[] args)
    {
        mqtt = new();
        mqtt.Connect();
        
        var counter = 0;
        var max = args.Length is not 0 ? Convert.ToInt32(args[0]) : -1;
        while (max is -1 || counter < max)
        {
            Console.WriteLine($"Counter: {++counter}");
            await Task.Delay(TimeSpan.FromMilliseconds(1_000));
        }
    }
}