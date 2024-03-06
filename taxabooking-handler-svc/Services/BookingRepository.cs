using Booking.Models;
namespace TaxaBookingHandler.Services;

public interface IBookingRepository : IDisposable
{
    void Put(BookingDTO dto);
    IEnumerable<BookingDTO> GetBookingsByRequestTime();
}

/// <summary>
/// Implementation of the repository-pattern as described here:
/// <link>https://learn.microsoft.com/en-us/previous-versions/msp-n-p/ff649690(v=pandp.10)?redirectedfrom=MSDN</link>
/// 
/// Part of this article:
/// <link>https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application</link>
/// </summary>
public class BookingRepository : IBookingRepository
{

    //private readonly ILogger<BookingRepository> _logger;
    private List<BookingDTO> _bookingsList;

    public BookingRepository(/*ILogger<BookingRepository> logger*/)
    {
        //_logger = logger;
        _bookingsList = new List<BookingDTO>();
    }

    public void Put(BookingDTO item)
    {
        _bookingsList.Add(item);
    }

    public IEnumerable<BookingDTO> GetBookingsByRequestTime()
    {
        var sortedDates = from booking in _bookingsList orderby booking.RequestedStartTime select booking;
        return sortedDates;
    }

# region IDisposable

    private bool disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                _bookingsList.Clear();
            }
        }
        this.disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

#endregion

}