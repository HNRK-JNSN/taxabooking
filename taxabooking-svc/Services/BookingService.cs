using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Booking.Models;
using RabbitMQ.Client;

public interface IBookingService
{
    Task AddBooking(BookingDTO theBooking);

}

/// <summary>
/// Repository class for handling booking request.
/// The BookingService uses a RabbitMQ message queue to forward requests
/// to the booking handling service.
/// </summary>
public class BookingService : IBookingService
{
    private ILogger<BookingService> _logger;
    private IConfiguration _config;
    private readonly IConnection _connection;

    /// <summary>
    /// Create an instance of the BookingService.
    /// </summary>
    /// <param name="logger">The global logging mechanism instance.</param>
    /// <param name="configuration">System configuration instance.</param>
    public BookingService(ILogger<BookingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration;

        var mqhostname = configuration["TaxaBookingBrokerHost"];

        if (String.IsNullOrEmpty(mqhostname))
        {
            mqhostname = "localhost";
        }

        _logger.LogInformation($"Using host at {mqhostname} for message broker");

        var factory = new ConnectionFactory() { HostName = mqhostname };
        _connection = factory.CreateConnection();
    }

    /// <summary>
    /// Submit a booking to the message queue, for the remote booking handler service.
    /// </summary>
    /// <param name="theBooking">The booking request to submit</param>
    /// <returns>A guid with the request id</returns>
    public Task AddBooking(BookingDTO theBooking)
    {
        try
        {
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(queue: "taxabooking",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                var body = JsonSerializer.SerializeToUtf8Bytes(theBooking);

                channel.BasicPublish(exchange: "",
                                    routingKey: "taxabooking",
                                    basicProperties: null,
                                    body: body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return Task.FromException(ex);
        }

        return Task.CompletedTask;
    }

}