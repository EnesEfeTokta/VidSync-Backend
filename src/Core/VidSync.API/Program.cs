using System.Net;
using VidSync.Persistence;
using VidSync.Signaling.Hubs;
using VidSync.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://192.168.1.157:5173",
                "https://192.168.1.157:5173",
                "https://vidsync-front.enesefetokta.shop"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Servisleri mantıksal gruplar halinde kaydediyoruz
builder.Services.AddInfrastructureServices(configuration);
builder.Services.AddIdentityServices(configuration);
builder.Services.AddSignalingServices();

// Web API katmanına özgü servisler
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kestrel sunucu yapılandırması
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5123);
    serverOptions.Listen(IPAddress.Any, 7123, listenOptions =>
    {
        listenOptions.UseHttps("localhost+3.p12", "changeit");
    });
});

var app = builder.Build();

// Middleware pipeline'ı yapılandırma
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub endpoint'leri
app.MapHub<CommunicationHub>("/communicationhub");
app.MapHub<TranscriptionHub>("/transcriptionhub");

app.Run();