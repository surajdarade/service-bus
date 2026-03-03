using Azure.Messaging.ServiceBus;
using Assignmate.AIService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["ServiceBus:ConnectionString"];
    return new ServiceBusClient(connectionString);
});

builder.Services.AddHostedService<AIMessageProcessor>();

var host = builder.Build();
host.Run();