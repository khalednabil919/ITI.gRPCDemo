using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ITI.gRPCDemo.Server.Protos;
using Microsoft.AspNetCore.Authorization;

namespace ITI.gRPCDemo.Server.Services
{
    public class TelemetryTrackingService:TrackingService.TrackingServiceBase
    {
        ILogger<TelemetryTrackingService> _logger;
        public TelemetryTrackingService(ILogger<TelemetryTrackingService> logger)
        {
            _logger = logger;
        }

        //Unary One Request one Response
        [Authorize(AuthenticationSchemes ="BasicAuth",Roles ="Device")]
        public override Task<TrackingResponse> SendMessage(TrackingMessage request, ServerCallContext context)
        {
            if(request.DeviceId == 0 )
            {
                throw new RpcException ( new Status ( StatusCode.InvalidArgument,"Device id can be zero" ) );
            }
            _logger.LogInformation($"New Message: DeviceId:{request.DeviceId} Location: ({request.Location.Lat}, {request.Location.Long}), SensorCount:{request.Sensor.Count}");
            return Task.FromResult(new TrackingResponse { Success = true });
        }
        
        //Client Stream
        //Stream Request and one Response
        // Many Request and one response
        public override async Task<Empty> KeepAlive(IAsyncStreamReader<PulseMessage> requestStream, ServerCallContext context)
        {
            await Task.Run(async () =>
            {
                await foreach(var item in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"{nameof(KeepAlive)}; Status:{item.Status} Details:{item.Det.Details_}");
                }
            });
            return new Empty();
        }
        
        //Server Stream
        //One Request and many Response
        //Request and Stream Response
        public override async Task SubscribeNotification(SubscribeRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            var task = Task.Run(async () =>
            {
                while(!context.CancellationToken.IsCancellationRequested)
                {
                    await responseStream.WriteAsync(new Notification
                    {
                        Text = $"New Notification from Device {request.DeviceId}",
                        Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                    await Task.Delay(3000);
                }
            });
            await task;
        }
    }
}
