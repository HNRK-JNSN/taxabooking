using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Booking.Models;

namespace TaxaBookingHandler.Services;

/// <summary>
/// Consumes messages from the common message queue.
/// </summary>
public class BookingWorker : BackgroundService
{
    private readonly ILogger<BookingWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IBookingRepository _repository;
    private int _nextID;

    /// <summary>
    /// Create a worker service that receives a ilogger and 
    /// environment configuration instance.
    /// <link>https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0</link>
    /// </summary>
    /// <param name="logger"></param>
    public BookingWorker(ILogger<BookingWorker> logger, IConfiguration configuration, IBookingRepository repository)
    {
        _logger = logger;
        _configuration = configuration;
        _repository = repository;

        var mqhostname = configuration["TaxaBookingBrokerHost"];

        if (String.IsNullOrEmpty(mqhostname))
        {
            mqhostname = "localhost";
        }

        var factory = new ConnectionFactory() { HostName = mqhostname };
        _connection = factory.CreateConnection();

        _logger.LogInformation($"Booking worker listening on host at {mqhostname}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();
        channel.QueueDeclare(queue: "taxabooking",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            BookingDTO? dto = JsonSerializer.Deserialize<BookingDTO>(message);
            if (dto != null)
            {
                dto.BookingID = _nextID++;
                _logger.LogInformation("Processing booking {id} from {customer} ", dto.BookingID, dto.CustomerName);

                _repository.Put(dto);
                
            } else {
                _logger.LogWarning($"Could not deserialize message with body: {message}");
            }

        };

        channel.BasicConsume(queue: "taxabooking",
                            autoAck: true,
                            consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}