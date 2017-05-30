# MassTransit.Hosting.Extensions

This library provides extensions for [MassTransit] to support:
* Dependency Injection with [Microsoft.Extensions.DependencyInjection]
* Configuration with [Microsoft.Extensions.Options]

This library was designed to make no assumptions how to host MassTransit consumers and provides the developer with flexible hosting implementation options.

* `MassTransit.Hosting.Extensions`
  * Provides extensions common to all MassTransit transports

* `MassTransit.RabbitMqTransport.Hosting.Extensions`
  * Provides extensions specific to RabbitMq

## Problem Statement

MassTransit currently provides the following interfaces to load configuration and settings:

* [IConfigurationProvider]
* [ISettingsProvider]

The core MassTransit library only provides abstractions and no implementation unless the [MassTransit.Host] package is also used. Unfortunatly the `MassTransit.Host` package makes many assumptions and forces the developer with many potentially unwanted conventions such as:

* Autofac as the DI container
  * No ability to modify the registrations in ContainerBuilder
* A prebuilt Windows Service executable using Topshelf
  * No ability to modify the Topshelf service configuration
* log4net as the logging provider
* Configuration settings from assembly config files
  * web.config is not supported
 
None of the items mentioned above are bad or wrong, just potentially not intended to be used in every host implementation. The individual libraries such as Autofac, log4net and Topshelf are in fact extremly helpful, just not always needed in every implementation.

Also the `MassTransit.Host` is not usable in other hosting environments such as Web Applications, Console Applications, etc since it provides a prebuilt Topshelf executable. Instead it would be convenient to use the same style of design for MassTransit consumers agnostic of their host environment.

## Solution
This library provides _just enough_ adapter classes to use any DI framework and an extensible configuration model. Other than using [Microsoft.Extensions.Options] and [Microsoft.Extensions.DependencyInjection], this library makes no assumptions on DI containers, logging providers, configuration providers, and the hosting environment.

## Dependency Injection
MassTransit actually provides additional DI specific packages such as:
* MassTransit.AutoFacIntegration
* MassTransit.StructureMapIntegration
* MassTransit.UnityIntegration
* (and others...)

When it really doesn't need to. Instead the developer should be able to choose any DI framework outside the concern of MassTransit. This is now possible with Microsoft's recent addition to the DI ecosystem with their new [Microsoft.Extensions.DependencyInjection] abstraction layer.

Specifically this library provides the following:
* An implementation of `IConsumerFactory` that creates child scopes and resolves consumer instances from the DI container
* An implementation of `IBusServiceConfigurator` that resolves configurators for service specifications, endpoint specifications, and bus observers from the DI container
* An implementation of `ISettingsProvider` that resolves generic settings providers from the DI container
* An implementation of `IConfigurationManagerProvider` that loads configuration settings from app/web.config

See the examples below for how to register services into the DI container.

### Configuration Settings
This library provides multiple mechanisms to retreive MassTransit settings:

* Resolve Settings
* Options Framework
* App/Web.config File

#### Resolve Settings
This library provides the following interface so that any MassTransit `ISettings` instance may be resolved from the DI container:

```csharp
public interface ISettingsProvider<T>
    where T : ISettings
{
    bool TryGetSettings(out T settings);

    bool TryGetSettings(string prefix, out T settings);
}
```

The developer can then register their own implementations of `ISettingsProvider<T>` that retreive the settings by any means they choose.

Since MassTransit only provides interfaces for settings such as `RabbitMqSettings`, this library also provides a simple POCO that has public get and set properties:

```csharp
public class RabbitMqOptions : RabbitMqSettings
{
    public string Username { get; set; }
    public string Password { get; set; }
    public ushort? Heartbeat { get; set; }
    public string Host { get; set; }
    public int? Port { get; set; }
    public string VirtualHost { get; set; }
    public string ClusterMembers { get; set; }
}
```

Here is an example settings provider that can be registered with the DI container:

```csharp
public class MySettingsProvider : ISettingsProvider<RabbitMqSettings>
{
    public bool TryGetSettings(out RabbitMqSettings settings)
    {
        return TryGetSettings(null, out settings);
    }

    public bool TryGetSettings(string prefix, out RabbitMqSettings settings)
    {
        // you can use prefix or not...
        settings = new RabbitMqOptions
        {
            // load, initialize, etc from somewhere...
            Host = "...",
            Username = "..."
        };
        // return true/false if the settings where loaded successfully or not...
        return true;
    }
}
```

#### Options Framework
Using Microsoft's new [Microsoft.Extensions.Options] framework, MassTransit settings may also be explicitly loaded from any configured configuration source.

A generic implementation of `ISettingsProvider<T>` is also provided by this library to configure any settings using the `IOptions` pattern. When using the new `IOptions` model, simply register the options like so:

```csharp
IServiceProvider ConfigureServices(IServiceCollection services)
{
    // uses an explicit options configuration...
    services.AddOptionsSettingsProvider<RabbitMqSettings, RabbitMqOptions>(options =>
    {
        options.Host = "...";
    });

    // or load options from configuration sources...
    services.AddOptionsSettingsProvider<RabbitMqSettings, RabbitMqOptions>();
    services.Configure<RabbitMqOptions>(Configuration);
}
void LoadConfiguration()
{
    // Set up configuration sources...
    var builder = new ConfigurationBuilder()
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json");
        // add other configuration sources...

    Configuration = builder.Build();
}
```

#### App/Web.config File
An implementation of `IConfigurationProvider` is provided that loads `appSettings` and `connectionStrings` using `System.ConfigurationManager` from the usual `web/app.config` files in the .NET Framework.

Also, another `ISettingsProvider` is provided that when the the generic `ISettingProvider<T>` didn't retreive settings, then those settings will be loaded by using the `IConfigurationProvider` from the `web/app.config` file. The registration of `ConfigurationSettingsProvider<T>` is done as an open generic so that [closed generic] registrations of `ISettingProvider<T>` take precedence.

## Full Example Usage
```csharp
IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMassTransit();
    services.AddRabbitMq(options =>
    {
        options.Host = "localhost";
        // (configure other options...)
    });

    services.AddScoped<IConsumer<IMyData>, MyDataConsumer>();
    services.AddScoped<IServiceSpecification, MyServiceSpecification>();
    services.AddScoped<IEndpointSpecification, MyEndpointSpecification>();
    services.AddScoped<IBusObserver, MyBusObserver>();

    // (add other services registrations...)

/*  (example using Autofac...)

    var builder = new ContainerBuilder();
    builder.RegisterType<MyType>().As<IMyType>();
    builder.Populate(services);
    return new AutofacServiceProvider(builder.Build());
*/

    return services.BuildServiceProvider();
}

void Run(IServiceProvider provider)
{
    var hostBusFactory = provider.GetService<IHostBusFactory>();
    var busServiceConfigurator = provider.GetService<IBusServiceConfigurator>();
    var busControl = hostBusFactory.CreateBus(busServiceConfigurator, "MyService");

    busControl.Start();
    // ...
    busControl.Stop();
}

public interface IMyData
{
    Guid Id { get; }
    DateTimeOffset Timestamp { get; }
}

public class MyDataConsumer : IConsumer<IMyData>
{
    public async Task Consume(ConsumeContext<IMyData> context)
    {
        await Console.Out.WriteAsync($"Data with id {context.Message.Id}");

        await Task.Delay(TimeSpan.FromSeconds(3));

        await Console.Out.WriteAsync($"Data with timestamp {context.Message.Timestamp}");
    }
}

public class MyServiceSpecification : IServiceSpecification
{
    public void Configure(IServiceConfigurator configurator)
    {
        configurator.UseRetry(r => r.Immediate(5));
        // (configure other options...)
    }
}

public class MyEndpointSpecification : IEndpointSpecification
{
    private readonly IConsumerFactory<IConsumer<IMyData>> _consumerFactory;

    public MyEndpointSpecification(IConsumerFactory<IConsumer<IMyData>> consumerFactory)
    {
        _consumerFactory = consumerFactory;
    }

    public string QueueName { get; } = "my-data";
    public int ConsumerLimit { get; } = Environment.ProcessorCount;

    public void Configure(IReceiveEndpointConfigurator endpointConfigurator)
    {
        endpointConfigurator.Consumer(_consumerFactory);
        // (configure other options...)
    }
}
````

# Feedback
Please provide any feedback, comments, or issues to this GitHub project [here][issues].

[MassTransit]: http://masstransit-project.com/
[Microsoft.Extensions.DependencyInjection]: https://github.com/aspnet/DependencyInjection
[Microsoft.Extensions.Options]: https://github.com/aspnet/Options
[IConfigurationProvider]: https://github.com/MassTransit/MassTransit/blob/develop/src/MassTransit/Hosting/IConfigurationProvider.cs
[ISettingsProvider]: https://github.com/MassTransit/MassTransit/blob/develop/src/MassTransit/Hosting/ISettingsProvider.cs
[MassTransit.Host]: https://github.com/MassTransit/MassTransit/tree/develop/src/MassTransit.Host
[closed generic]: https://stackoverflow.com/questions/2173107/what-exactly-is-an-open-generic-type-in-net
[issues]: https://github.com/NCodeGroup/MassTransit.Hosting.Extensions/issues
