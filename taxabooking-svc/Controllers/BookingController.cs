using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IBookingService _bookingservice;
    private static Int32 NextId { get; set; }

    /// <summary>
    /// Inject a logger service into the controller on creation.
    /// </summary>
    /// <param name="logger">The logger service.</param>
    public BookingController(ILogger<BookingController> logger, IConfiguration configuration, IBookingService bookingService)
    {
        _logger = logger;
        _bookingservice = bookingService;
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
    public async Task<BookingDTO?> PostBooking(BookingDTO theBooking)
    {
        // TODO: Sequencing and datestamp should be performed by 
        //       booking consumer service!
        theBooking.BookingID = ++NextId;
        theBooking.BookingSubmitTime = DateTime.UtcNow;

        var res = _bookingservice.AddBooking(theBooking);

        if (res.IsFaulted)
        {
            return null;
        }

        return theBooking;
    }

    public static void ResetRequestCounter()
    {
        NextId = 0;
    }
}
