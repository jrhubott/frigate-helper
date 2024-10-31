using Frigate_Helper;

// Create a new application builder with the provided command-line arguments
var builder = Host.CreateApplicationBuilder(args);

// Register the Worker service as a hosted service
builder.Services.AddHostedService<Worker>();

// Build the host from the configured builder
var host = builder.Build();

// Run the application host

Console.WriteLine("************************************");
Console.WriteLine(VersionInfo.Version);
Console.WriteLine("************************************");

host.Run();
