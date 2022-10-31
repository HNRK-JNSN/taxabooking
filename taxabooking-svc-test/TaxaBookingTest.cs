using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using TaxaService.Controllers;
using Booking.Models;
namespace TaxaServiceTest;

public class TaxaBookingTests
{
    private ILogger<BookingController>? _logger;
    private IConfiguration? _configuration;

    [SetUp]
    public void Setup()
    {
         _logger = new Mock<ILogger<BookingController>>().Object;

        var myConfiguration = new Dictionary<string, string>
        {
            {"TaxaBookingBrokerHost", "http://testhost.local"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    [Test]
    public async Task TestBookingEndpoint_valid_dto()
    {
        // Arrange
        var requestTime = new DateTime(2023,11,22, 14, 22, 32);
        var bookingDTO = CreateBooking(requestTime);
        var mockRepo = new Mock<IBookingService>();
        mockRepo.Setup(svc => svc.AddBooking(bookingDTO)).Returns(Task.CompletedTask);
        var controller = new BookingController(_logger, _configuration, mockRepo.Object);

        // Act
        BookingController.ResetRequestCounter();
        var result = await controller.PostBooking(bookingDTO);

        // Assert
        Assert.IsInstanceOf(typeof(BookingDTO), result);
        Assert.AreEqual(requestTime, result?.RequestedStartTime);
        Assert.AreEqual(1, result?.BookingID);
        Assert.AreEqual(DateTime.UtcNow.Date, result?.BookingSubmitTime.Value.Date);
        Assert.AreEqual(DateTime.UtcNow.ToShortTimeString(), 
                        result?.BookingSubmitTime.Value.ToShortTimeString());
    }

    [Test]
    public async Task TestBookingEndpoint_next_counter()
    {
        // Arrange
        var requestTime = new DateTime(2023,11,22, 14, 22, 32);
        var bookingDTO = CreateBooking(requestTime);
        var mockRepo = new Mock<IBookingService>();
        mockRepo.Setup(svc => svc.AddBooking(bookingDTO)).Returns(Task.CompletedTask);

        var controller1 = new BookingController(_logger, _configuration, mockRepo.Object);
        var controller2 = new BookingController(_logger, _configuration, mockRepo.Object);

        // Act
        BookingController.ResetRequestCounter();
        var res1 = await controller1.PostBooking(bookingDTO);
        var bookingId1 = res1.BookingID;
        var res2 = await controller2.PostBooking(bookingDTO);
        var bookingId2 = res2.BookingID;

        // Assert
        Assert.AreEqual(1, bookingId1);
        Assert.AreEqual(2, bookingId2);
    }

    [Test]
    public async Task TestBookingEndpoint_failure_posting()
    {
        // Arrange
        var bookingDTO = CreateBooking(new DateTime(2023,11,22, 14, 22, 32));
        var mockRepo = new Mock<IBookingService>();
        mockRepo.Setup(svc => svc.AddBooking(bookingDTO)).Returns(Task.FromException(new Exception()));
        var controller = new BookingController(_logger, _configuration, mockRepo.Object);

        // Act        
        var result = await controller.PostBooking(bookingDTO);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Helper method for creating BookingDTO instance.
    /// </summary>
    /// <param name="requestTime"></param>
    /// <returns></returns>
    private BookingDTO CreateBooking(DateTime requestTime)
    {
        var bookingDTO = new BookingDTO()
        {
            BookingID = null,
            BookingSubmitTime = null,
            CustomerName ="Test Customer",
            StartAdresse ="Test Address",
            DestinationsAdresse = "Some place, not far",
            RequestedStartTime = requestTime,
        };
        return bookingDTO;
    }
}

