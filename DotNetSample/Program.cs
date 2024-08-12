using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var factory = new ConnectionFactory()
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "weatherforecast_request", durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.QueueDeclare(queue: "weatherforecast_response", durable: false, exclusive: false, autoDelete: false, arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var request = JsonSerializer.Deserialize<WeatherForecastRequest>(message);

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    var responseMessage = JsonSerializer.Serialize(forecast);
    var responseBytes = Encoding.UTF8.GetBytes(responseMessage);

    channel.BasicPublish(exchange: "", routingKey: "weatherforecast_response", basicProperties: null, body: responseBytes);
};

channel.BasicConsume(queue: "weatherforecast_request", autoAck: true, consumer: consumer);

app.MapGet("/weatherforecast", () =>
{
    var request = new WeatherForecastRequest(Guid.NewGuid().ToString());
    var message = JsonSerializer.Serialize(request);
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "", routingKey: "weatherforecast_request", basicProperties: null, body: body);

    // Simulate waiting for the response
    var responseConsumer = new EventingBasicConsumer(channel);
    var tcs = new TaskCompletionSource<WeatherForecast[]>();

    responseConsumer.Received += (model, ea) =>
    {
        if (!tcs.Task.IsCompleted)
        {
            var responseBody = ea.Body.ToArray();
            var responseMessage = Encoding.UTF8.GetString(responseBody);
            var response = JsonSerializer.Deserialize<WeatherForecast[]>(responseMessage);
            tcs.SetResult(response);
        }
    };


    channel.BasicConsume(queue: "weatherforecast_response", autoAck: true, consumer: responseConsumer);

    return tcs.Task.Result;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();