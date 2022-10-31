using System;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Booking.Models;

namespace TaxaService.Controllers;

/// <summary>
/// This controller exposes a microservices HTTP interface for receiving
/// taxa booking messages. Data is relayed to a common message-broker for
/// further processing.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BookingController : ControllerBase
{
    private readonly ILogger<BookingController> _logger;
    private readonly IConnection _connection;
    private Int32 NextId { get; set; }

    /// <summary>
    /// Inject a logger service into the controller on creation.
    /// </summary>
    /// <param name="logger">The logger service.</param>
    public BookingController(ILogger<BookingController> logger, IConfiguration configuration)
    {
        _logger = logger;

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
    /// Service version endpoint. 
    /// Fetches metadata information, through reflection from the service assembly.
    /// </summary>
    /// <returns>All metadata attributes from assembly in text string</returns>
    [HttpGet("version")]
    public IEnumerable<string> Get()
    {
        var properties = new List<string>();
        var assembly = typeof(Program).Assembly;
        foreach (var attribute in assembly.GetCustomAttributesData())
        {
            properties.Add($"{attribute.AttributeType.Name} - {attribute.ToString()}");
        }
        return properties;
    }

    /// <summary>
    /// Endpoint for recieving taxa booking messages.
    /// </summary>
    /// <param name="theBooking">A booking object</param>
    /// <returns>On success - the booking object with booking id and received date.</returns>
    [HttpPost("Booking")]
    public BookingDTO? Post(BookingDTO theBooking)
    {
        // TODO: Sequencing and datestamp should be performed by 
        //       booking consumer service!
        theBooking.BookingID = NextId++;
        theBooking.BookingSubmitTime = DateTime.UtcNow;

        try {
            using(var channel = _connection.CreateModel())
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
        } catch {
            return null;
        }

        return theBooking;

    }
}
