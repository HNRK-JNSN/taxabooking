# __Unit test af .NET MVC Controller__

<img align="right" src="micro-logo.png" width="150" />

I denne guide beskrives hvordan en .NET Core Controller kan test med brug af et NUtnit test projekt og et MOQ-frameworket.

> _Note:_
>  
> - Microsoft dokumentation for best-practices i unit-tests [Link1](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
> - [Unit testing C# with NUnit and .NET Core](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit?source=recommendations)
>  
> _

---

## __Opret Unit Test subproject__

Fra en terminal i test projektet:

``` bash
  dotnet new utest -o mit-projekt-test
```

TODO: tilføj reference i løsningen til projektet som testes.

## __Tilføj MOQ Framework nuget__

Fra en terminal i test projektet:

``` bash
  dotnet add package moq
```

> TODO: Beskriv hvad er og hvorfor man bruger en mock


## __Tilføj Mock for Ilogger__

Tilføj global variabel til ilogger:

```C#
    private ILogger<BookingController> _logger = null;
```

I Testklassens Setup-metode:

```C#
    _logger = new Mock<ILogger<CatalogController>>().Object;

    var myConfiguration = new Dictionary<string, string>
    {
        {"Key1", "Value1"},
        {"Nested:Key1", "NestedValue1"},
        {"Nested:Key2", "NestedValue2"}
    };

    _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(myConfiguration)
        .Build();
```

## __Tilføj Mock for IConfiguration__

Tilføj global variabel til iconfiguration:

```C#
    private IConfiguration _configuration = null;
```

I Testklassens Setup-metode:

```C#
    _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(myConfiguration)
        .Build();
```

---

## __Tilføj hjælper metoder__

I Testfilen tilføj metoder til at oprette f.eks. test DTO'er.

```C#
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
```

---

## __Tilføj unit test med Mock__

```C#
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
```

---

## __Kør unit-testene__

Fra en terminal i roden af løsningen

``` bash
dotnet test --loger trx
```
