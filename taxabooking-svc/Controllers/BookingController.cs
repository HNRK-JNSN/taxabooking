using Microsoft.AspNetCore.Mvc;
using Booking.Models;
using System.Diagnostics;

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
    public async Task<Dictionary<string,string>> GetVersion()
    {
        var properties = new Dictionary<string, string>();
        var assembly = typeof(Program).Assembly;

        properties.Add("service", "Booking");
        var ver = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion;
        properties.Add("version", ver!);

        try {
            var hostName = System.Net.Dns.GetHostName();
            var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
            var ipa = ips.First().MapToIPv4().ToString();
            properties.Add("hosted-at-address", ipa);
        } catch (Exception ex) {
            _logger.LogError(ex.Message);
            properties.Add("hosted-at-address", "Could not resolve IP-address");
        }

        return properties;
    }

    /// <summary>
    /// Endpoint for receiving taxa booking messages.
    /// </summary>
    /// <param name="theBooking">A booking object</param>
    /// <returns>On success - the booking object with booking id and received date.</returns>
    [HttpPost("Booking")]
    public IActionResult PostBooking(BookingDTO theBooking)
    {
        var res = _bookingservice.AddBooking(theBooking);

        if (res.IsFaulted)
        {
            return BadRequest();
        }

        return CreatedAtAction("Get", new { id = theBooking.BookingID }, theBooking);
    }

}
