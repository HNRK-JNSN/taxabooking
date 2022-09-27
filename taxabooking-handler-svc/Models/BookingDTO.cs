namespace Booking.Models
{
    public class BookingDTO
    {
        public int? BookingID { get; set; }

        public DateTime? BookingSubmitTime { get; set; }

        public String? CustomerName { get; set; }

        public string? StartAdresse { get; set; }

        public string? DestinationsAdresse { get; set; }

        public DateTime? RequestedStartTime { get; set; }

    }
}