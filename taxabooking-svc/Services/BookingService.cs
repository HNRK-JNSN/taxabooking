using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Booking.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

public interface IBookingService
{
    Task<BookingDTO?> AddBooking(BookingDTO theBooking);

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
    private readonly string _mqHostname;
    private static Int32 NextId { get; set; }

    /// <summary>
    /// Create an instance of the BookingService.
    /// </summary>
    /// <param name="logger">The global logging mechanism instance.</param>
    /// <param name="configuration">System configuration instance.</param>
    public BookingService(ILogger<BookingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration;

        _mqHostname = configuration["TaxaBookingBrokerHost"]!;

        if (String.IsNullOrEmpty(_mqHostname))
        {
            _mqHostname = "localhost";
        }

        _logger.LogInformation($"Using host at {_mqHostname} for message broker");

    }

    /// <summary>
    /// Submit a booking to the message queue, for the remote booking handler service.
    /// </summary>
    /// <param name="theBooking">The booking request to submit</param>
    /// <returns>A guid with the request id</returns>
    public async Task<BookingDTO?> AddBooking(BookingDTO theBooking)
    {
        Task t;
        try
        {
            t = Task.Run(() => 
            {
                var factory = new ConnectionFactory() { HostName = _mqHostname };

                using (var channel = factory.CreateConnection().CreateModel())
                {
                    theBooking.BookingID = ++NextId;
                    theBooking.BookingSubmitTime = DateTime.UtcNow;

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
                    channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
                }
            });

            await t; 
        }
        catch (OperationInterruptedException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }

        return theBooking;
    }

    /// <summary>
    /// Reset the booking ID counter.
    /// </summary>
    public static void ResetRequestCounter()
    {
        NextId = 0;
    }

}
