using System;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Booking.Models;

namespace TaxaService.Controllers;

[ApiController]
[Route("[controller]")]
public class BookingController : ControllerBase
{

    private static List<BookingDTO> Bookings = new List<BookingDTO>();

    private readonly ILogger<BookingController> _logger;

    private readonly IConnection _connection;

    private Int32 NextId { get; set; }

    public BookingController(ILogger<BookingController> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
    }

    [HttpGet("GetBooking")]
    public IEnumerable<BookingDTO> Get(int id)
    {
        return Bookings;
    }

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

    [HttpPost("Booking")]
    public void Post(BookingDTO theBooking)
    {
        theBooking.BookingID = NextId++;
        theBooking.BookingSubmitTime = DateTime.UtcNow;

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
    }
}
