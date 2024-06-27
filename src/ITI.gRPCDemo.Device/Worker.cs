using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ITI.gRPCDemo.Server.Protos;
using System.Text;

namespace ITI.gRPCDemo.Device
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly int _deviceId;
        private TrackingService.TrackingServiceClient _client;
        private TrackingService.TrackingServiceClient client
        {
            get
            {
                if (_client == null)
                {
                    var channel = GrpcChannel.ForAddress("https://localhost:7100");
                    _client = new TrackingService.TrackingServiceClient(channel);
                }
                return _client;
            }
        }
        public Worker(ILogger<Worker> logger, int deviceId)
        {
            _logger = logger;
            _deviceId = deviceId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random rnd = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                await SendMessage(rnd, stoppingToken);
            }

            await SubscribeNotification(stoppingToken);
            await KeepAlive(stoppingToken);

        }
        private async Task SendMessage(Random rnd, CancellationToken stoppingToken)
        {
            try
            {

                var request = new TrackingMessage
                {
                    DeviceId = _deviceId,
                    Speed = rnd.Next(0, 220),
                    Location = new Location { Lat = rnd.Next(0, 100), Long = rnd.Next(0, 100) },
                    Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
                };
                request.Sensor.Add(new Sensor { Key = "Khaled", Value = 1 });
                request.Sensor.Add(new Sensor { Key = "Mahmoud", Value = 2 });

                var plainText = "device:P@ssw0rd";
                
                var bytes = Encoding.UTF8.GetBytes(plainText);
                var token = Convert.ToBase64String(bytes);

                var meta = new Metadata
                {
                    { "Authorization",$"Basic {token}"}
                };

                var result = await client.SendMessageAsync(request, meta);

                _logger.LogInformation($"Response: {result.Success}");

                await Task.Delay(3000, stoppingToken);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        private async Task KeepAlive(CancellationToken stoppingToken)
        {
            var stream = client.KeepAlive();

            var alive = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await stream.RequestStream.WriteAsync(new PulseMessage
                    {
                        Status = ClientSatus.Working,
                        Det = new Details { Details_ = $"Details Id: {_deviceId} is working" },
                        Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                    Console.WriteLine("Done");
                    await Task.Delay(2000);
                }
            });

            await alive;

        }
        private async Task SubscribeNotification(CancellationToken cancellationToken)
        {

            var response = client.SubscribeNotification(new SubscribeRequest { DeviceId = _deviceId });

            var task = Task.Run(async () =>
             {
                 while (await response.ResponseStream.MoveNext(cancellationToken))
                 {
                     var msg = response.ResponseStream.Current;
                     _logger.LogInformation($"New Message Recieved Text: {msg.Text}");
                 }
             });
            await task;
        }
    }
}
