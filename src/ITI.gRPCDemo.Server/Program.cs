using ITI.gRPCDemo.Server;
using ITI.gRPCDemo.Server.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("BasicAuth")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuth", options => { });
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();

//app.MapControllers();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<TelemetryTrackingService>();

    if(app.Environment.IsDevelopment())
        endpoints.MapGrpcReflectionService();

    endpoints.MapControllers();
});

app.Run();
