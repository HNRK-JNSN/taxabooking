using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using TaxaService.Controllers;
using Booking.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Exceptions;

namespace TaxaServiceTest;

/// <summary>
/// Unit test for TaxaBooking Service
/// </summary>
public class TaxaBookingTests
{
    private ILogger<BookingController> _logger = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public void Setup()
    {
         _logger = new Mock<ILogger<BookingController>>().Object;

        var myConfiguration = new Dictionary<string, string?>
        {
            {"TaxaBookingBrokerHost", "http://testhost.local"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    [Test]
    public void TestBookingEndpoint_valid_dto()
    {
        // Arrange
        var requestTime = new DateTime(2023,11,22, 14, 22, 32);
        var bookingDTO = CreateBooking(requestTime);
        var stubRepo = new Mock<IBookingService>();
        stubRepo.Setup(svc => svc.AddBooking(bookingDTO))
            .Returns(Task.FromResult<BookingDTO?>(bookingDTO));
        var controller = new BookingController(_logger, _configuration, stubRepo.Object);

        // Act
        //BookingController.ResetRequestCounter();
        var result = controller.PostBooking(bookingDTO);

        // Assert
        Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
        Assert.That((result as CreatedAtActionResult)?.Value, Is.TypeOf<BookingDTO>());

    }

    [Test]
    public void TestBookingEndpoint_failure_posting()
    {
        // Arrange
        var bookingDTO = CreateBooking(new DateTime(2023,11,22, 14, 22, 32));
        var stubRepo = new Mock<IBookingService>();
        stubRepo.Setup(svc => svc.AddBooking(bookingDTO))
            .Returns(Task.FromException<BookingDTO?>(new Exception() ));
        var controller = new BookingController(_logger, _configuration, stubRepo.Object);

        // Act        
        var result = controller.PostBooking(bookingDTO);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestResult>());
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
