# __Unit test af .NET MVC Controller__

> Version 0.9 - 16-05-2023

<img align="right" src="micro-logo.png" width="150" />

I denne guide beskrives hvordan en .NET Core Controller unit-testes med brug af et NUnit-test projekt og et Moq-frameworket.

---

> __Nyttige links:__
>  
> - Microsoft dokumentation for [best-practices i unit-tests](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
> - [Unit testing C# with NUnit and .NET Core](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit?source=recommendations)
> - [What are fakes in unit testing: mocks vs stubs](https://canro91.github.io/2021/05/24/WhatAreFakesInTesting/)
>  
> .

---

## __Opret Unit Test subprojekt__

Hvis du har fulgt guiden [Opret ny microservice løsning](Howto-new-service.md) er
dette trin ikke nødvendigt da det er en del af den nævnte guide.

Der antages at der allerede er oprettet en __.NET Visual Studio solution__ (løsning) med et _microservice-projekt_ som skal unit-testes. Der antages ligeledes at dette projekt indeholder en __C# HTTP-controller__ (herefter navngivet _MyServiceController_) som er den klasse  som skal testes. Constructor'en af _MyServiceController_ forventes at modtage et _logger_ og en _configurations_ instans.

1) Fra en terminal i roden af _løsningen_ opret et nyt C# NUnit testprojekt:
  
   ``` bash
    ~$ dotnet new nunit -o mit-projekt-test
   ```
  
2) Tilføj en reference i løsningen imellem  microservice-projektet og unit-test projektet:

   ``` bash
    ~$ dotnet sln add ./mit-projekt-test/mit-projekt-test.csproj
    ~$ dotnet add ./mit-projekt-test/mit-projekt-test.csproj reference ./min-service/min-service.csproj
   ```

   > __Note:__
   >
   > I Visual Studio Code er en solution-reference en henvisning til en løsning (solution) i et projekt. En solution-reference bruges til at definere afhængigheder mellem projekter og at organisere koden i en større løsning.
   >
   > _

---

## __Tilpas NUnit projektet__

Inden vi går i gang med at teste skal vi tilføje lidt ekstra værktøjer og omdøbe en af de auto-genererede filer fra NUnit-templaten.

1) Fra en terminal i _test-projektet_, tilføj _nuget_ for MOQ Framework:

   ``` bash
   ~$ dotnet add package moq
   ```

2) Omdøb filen UnitTest1.cs til at reflektere navnet på den C#-klasse som skal testes - _MyServiceControllerTest.cs_.

> __Note:__
>
> I unit testing regi refererer en "stub" til en type testdouble, som er en erstatning for en ægte komponent eller en afhængighed i et testscenario. En stub bruges til at simulere eller efterligne opførslen af den rigtige komponent eller afhængighed, så testen kan udføres isoleret og forudsigeligt.
>
> En stub er normalt oprettet af testudvikleren og bruges til at erstatte eksterne komponenter eller afhængigheder, såsom databaseadgang, webtjenesteopkald eller filsysteminteraktioner, som ikke er direkte relateret til den specifikke enhed (unit), der testes. Formålet med at erstatte disse komponenter er at isolere enheden under test og fokusere på at teste dens adfærd uden at være afhængig af de faktiske implementeringer af de eksterne komponenter.
>  
> _Moq-frameworket_, skelner ikke imellem om et _object_ er en stub, mock eller fake!
> _

---

## __Forbered Test-klassen__

Åben din solution med Visual Studio Code og find unit-test-filen (_MyServiceControllerTest.cs_) fra trin 2 ovenover. Vi starter med at mock'e de klasse-instanser som bliver _injected_ ind i vores konstruktor i _MyServiceController_-klassen.

1) Start med at omdøbe _Tests_-klassen til _MyServiceControllerTests_
  
2) Tilføj følgende biblioteks-referencer i toppen af filen (nederste reference skal selvfølgelig matche det namespace som _MyServiceController_ ligger i):
  
   ```C#
   using NUnit.Framework;
   using Moq;
   using <MyServiceNamespace.Controllers>
   ```
  
3) Tilføj en _Mock_ for logger-instansen
  
   Tilføj en __global variabel__ til _logger:
  
   ```C#
   private ILogger<BookingController> _logger = null;
   ```
  
   I Testklassens Setup-metode, tilføj:
  
   ```C#
    _logger = new Mock<ILogger<CatalogController>>().Object;
   ```

4) Tilføj endnu en _Mock_ for configurations-instansen

   Tilføj en _global variabel_ til _configuration:
  
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

Det er god praktis at tilføje hjælpe metoder, til f.eks. oprettelse af DTO-instanser, da denne logik ofte gentages en del i testene. Et eksempel på en hjælper metode, vist herunder, opretter en standard dummy DTO som kan anvendes i forskellige test. BookingDTO-klassen er fra TaxaBooking projektet.

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

## __Unit testing med Mocks__

I _TaxaBooking_-klassen, hvori vores kontroller bor, anvendes en hjælper-klasse kaldet __BookingService__ (med _IBookingService_ som interface). Denne klasse har til formål at tilgå en _message-queue_ med brug af eksterne bibliotekker. Da vi __IKKE__ ønsker at logikken fra __BookingService__ skal indgå i vores unit-tests (ellers ville de være integrationstests), skal vi abstrahere anvendelsen af denne klasse væk med brug af et mock-instans.

Koden som skal testes vises herunder:

   ```C#
    public IActionResult PostBooking(BookingDTO theBooking)
    {
        var res = _bookingservice.AddBooking(theBooking);

        if (res.IsFaulted)
        {
            return BadRequest();
        }

        return CreatedAtAction("Get", new { id = theBooking.BookingID }, theBooking);
    }
   ```

I vores unit-test herunder, ønsker vi at teste det tilfælde at der opstår en fejl i __BookingService.AddBooking__-metoden når vi udfører et HTTP post til vores __BookingController__.  Vi vil altså gerne teste at der returneres et __null__-object fra vores __PostBooking__-metode.

   ```C#
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
   ```

Bemærk at __Unit-testen__ er delt op i 3 sektioner:

### __Arrange__

I denne sektion forbereder vi testen, vores DTO, vores mock af __BookingService.AddBooking__, samt oprettelse af en instans af den klasse som skal testes - __BookingController__. Bemærk at vi sender alle vores 3 mock-instanser (\_logger, \_configuration og MockRepo.Object) med i constructor'en.

### __Act__

Her eksekvere vi den logik som skal testes, altså, vi kalder __PostBooking__ med vores DTO og får et svar tilbage i __res__ variablen.

### __Assert__

Her undersøger vi om vores svar er som forventes, altså om testen består eller fejler.

> Bemærk:
>  
> Vi kalder vores _PostBooking-metode_ asynkront, så derfor skal selve testmetoden (_TestBookingEndpoint_failure_posting_) returnere en Task som skal præfikses med _async_ definitionen.
>  
> .

---

## __Kør unit-testene__

For at eksekvere alle testene i dit test-projekt skal du køre følgende kommando fra en terminal i roden af løsningen:

``` bash
dotnet test --logger trx
```

Som producerer følgende output med __Passed: 3__ (altså ingen fejl i testene):

```bash
 Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 148 ms - taxabooking-svc-test.dll (net6.0)
```

> Den sidste parameter i dotnet test kommandoen (--logger trx), gemmer outputet i en XML-fil i en folder kaldet _TestResults_.

---

## __Test af enkelte test__

Under udvikling kan du f.eks. have behov for at kun at køre en enkelt test eller liste de tilgængelige test i projektet. Det gør du med kommandoerne

### __List af test__

```bash
~$ dotnet test -l
```

### __Kør enkel test__

```bash
~$ dotnet test --filter TestBookingEndpoint_failure_posting
```

Du kan finde mere information om test filtrering på: [Run selected unit tests](https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=nunit)

---
