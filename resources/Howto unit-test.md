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

- dotnet new utest -o mit-projekt-test
- tilføj reference i løsningen

## __Tilføj MOQ Framework nuget__

- dotnet add package moq

## TODO: Beskriv hvad og hvorfor er en mock


## __ Tilføj Mock for Ilogger

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

## __ Tilføj Mock for IConfiguration

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
