using ITI.gRPCDemo.Device;

int deviceId = int.Parse(Console.ReadLine());
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>(p =>
{
    var logger = p.GetService<ILogger<Worker>>();
    return new Worker(logger, deviceId);
});

var host = builder.Build();
host.Run();
