using Microsoft.AspNetCore.Mvc;
using TaxaBookingHandler.Services;
using Booking.Models;

namespace TaxaBookingHandler.Service.Controllers;

/// <summary>
/// Service interface for exposing processed taxa booking data.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BookingHandlerController : ControllerBase
{
    private readonly ILogger<BookingHandlerController> _logger;
    private readonly IBookingRepository _repository;

    /// <summary>
    /// Create an instance with a logger service injected.
    /// </summary>
    /// <param name="logger"></param>
    public BookingHandlerController(ILogger<BookingHandlerController> logger, IBookingRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// API endpoint for fetching time ordered list of bookings.
    /// </summary>
    /// <returns>A list of chronological ordered bookings.</returns>
    [HttpGet("bookinglist")]
    public IEnumerable<BookingDTO> GetBookingsList()
    {
        return _repository.GetBookingsByRequestTime();
    }
}
