using Assignmate.AssignmentService;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Register services HERE
builder.Services.AddControllers();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["ServiceBus:ConnectionString"];
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<ServiceBusClient>();
    var config = sp.GetRequiredService<IConfiguration>();
    var topicName = config["ServiceBus:TopicName"];
    return client.CreateSender(topicName);
});

builder.Services.AddSingleton<EventGridPublisher>();

// 2️⃣ THEN build
var app = builder.Build();

// 3️⃣ Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();