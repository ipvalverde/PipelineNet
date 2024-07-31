# PipelineNet
[![Build status](https://github.com/ipvalverde/PipelineNet/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/ipvalverde/PipelineNet/actions/workflows/build.yml)

Pipeline net is a micro framework that helps you implement the pipeline and chain of responsibility patterns. With PipelineNet you can easily separate business logic and extend your application.
Pipelines can be used to execute a series of middleware sequentially without expecting a return, while chains of responsibilities do the same thing but expecting a return. And you can do it all asynchronously too.

You can obtain the package from this project through nuget:
```
Install-Package PipelineNet
```

Or if you're using dotnet CLI:
```
dotnet add package PipelineNet
```


<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**  *generated with [DocToc](https://github.com/thlorenz/doctoc)*

- [Simple example](#simple-example)
- [Pipeline vs Chain of responsibility](#pipeline-vs-chain-of-responsibility)
- [Middleware](#middleware)
- [Pipelines](#pipelines)
- [Chains of responsibility](#chains-of-responsibility)
- [Factories](#factories)
- [Middleware resolver](#middleware-resolver)
  - [ServiceProvider implementation](#serviceprovider-implementation)
  - [Unity implementation](#unity-implementation)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Simple example
Just to check how easy it is to use PipelineNet, here is an example of exception handling using a chain of responsibility:

First we define some middleware:
```C#
public class OutOfMemoryExceptionHandler : IMiddleware<Exception, bool>
{
    public bool Run(Exception parameter, Func<Exception, bool> next)
    {
        if (parameter is OutOfMemoryException)
        {
            // Handle somehow
            return true;
        }

        return next(parameter);
    }
}

public class ArgumentExceptionHandler : IMiddleware<Exception, bool>
{
    public bool Run(Exception parameter, Func<Exception, bool> next)
    {
        if (parameter is ArgumentException)
        {
            // Handle somehow
            return true;
        }

        return next(parameter);
    }
}
```
Now we create a chain of responsibility with the middleware:
```C#
var exceptionHandlersChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryExceptionHandler>() // The order of middleware being chained matters
    .Chain<ArgumentExceptionHandler>();
```
Now your instance of `ResponsibilityChain` can be executed as many times as you want:
```C#
// The following line will execute only the OutOfMemoryExceptionHandler, which is the first middleware.
var result = exceptionHandlersChain.Execute(new OutOfMemoryException()); // Result will be true

// This one will execute the OutOfMemoryExceptionHandler first, and then the ArgumentExceptionHandler gets executed.
result = exceptionHandlersChain.Execute(new ArgumentExceptionHandler()); // Result will be true

// If no middleware matches returns a value, the default of the return type is returned, which in the case of 'bool' is false.
result = exceptionHandlersChain.Execute(new InvalidOperationException()); // Result will be false
```
You can even define a fallback function that will be executed after your entire chain:
```C#
var exceptionHandlersChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryExceptionHandler>() // The order of middleware being chained matters
    .Chain<ArgumentExceptionHandler>()
    .Finally((parameter) =>
    {
        // Do something
        return true;
    });
```
Now if the same line gets executed:
```C#
var result = exceptionHandlersChain.Execute(new InvalidOperationException()); // Result will be true
```
The result will be true because of the function defined in the `Finally` method.

## Pipeline vs Chain of responsibility
Here is the difference between those two in PipelineNet:
- Chain of responsibility:
    - Returns a value;
    - Have a fallback function to execute at the end of the chain;
    - Used when you want that only one middleware to get executed based on an input, like the exception handling example;
- Pipeline:
    - Does not return a value;
    - Used when you want to execute various middleware over an input, like filterings over an image;

## Middleware
In PipelineNet the middleware is a definition of a piece of code that will be executed inside a pipeline or a chain of responsibility.

We have four interfaces defining middleware:
- The `IMiddleware<TParameter>` is used exclusively for pipelines, which does not have a return value.
- The `IAsyncMiddleware<TParameter>` the same as above but used for asynchronous pipelines. 
- The `IMiddleware<TParameter, TReturn>` is used exclusively for chains of responsibility, which does have a return value.
- The `IAsyncMiddleware<TParameter, TReturn>` the same as above but used for asynchronous chains of responsibility.

Besides the differences between those four kinds of middleware, they all have a similar structure, the definition of a method `Run`
in which the first parameter is the parameter passed to the Pipeline/Chain of responsibility beind executed and the second one
is an `Action` of `Func` to execute the next middleware in the flow. **It is important to remember to invoke the next middleware
by executing the `Action`/`Func` provided as the second parameter.** 

## Pipelines
The pipeline can be found in two flavours: `Pipeline<TParameter>` and `AsyncPipeline<TParameter>`. Both have the same functionaly,
aggregate and execute a series of middleware.

Here is an example of pipeline being configured with three middleware types:
```C#
var pipeline = new Pipeline<Bitmap>(new ActivatorMiddlewareResolver())
    .Add<RoudCornersMiddleware>()
    .Add<AddTransparencyMiddleware>()
    .Add<AddWatermarkMiddleware>();
```
From now on, the instance of pipeline can be used to perform the same operation over as many Bitmap instance as you like:
```C#
Bitmap image1 = (Bitmap) Image.FromFile("party-photo.png");
Bitmap image2 = (Bitmap) Image.FromFile("marriage-photo.png");
Bitmap image3 = (Bitmap) Image.FromFile("matrix-wallpaper.png");

pipeline.Execute(image1);
pipeline.Execute(image2);
pipeline.Execute(image3);
```
If you want to, you can use the asynchronous version, using asynchronous middleware. Changing the instantiation to:
```C#
var pipeline = new AsyncPipeline<Bitmap>(new ActivatorMiddlewareResolver())
    .Add<RoudCornersAsyncMiddleware>()
    .Add<AddTransparencyAsyncMiddleware>()
    .Add<AddWatermarkAsyncMiddleware>();
```
And the usage may be optimized:
```C#
Bitmap image1 = (Bitmap) Image.FromFile("party-photo.png");
Task task1 = pipeline.Execute(image1); // You can also simply use "await pipeline.Execute(image1);"

Bitmap image2 = (Bitmap) Image.FromFile("marriage-photo.png");
Task task2 = pipeline.Execute(image2);

Bitmap image3 = (Bitmap) Image.FromFile("matrix-wallpaper.png");
Task task3 = pipeline.Execute(image3);

Task.WaitAll(new Task[]{ task1, task2, task3 });
```

## Chains of responsibility
The chain of responsibility also has two implementations: `ResponsibilityChain<TParameter, TReturn>` and `AsyncResponsibilityChain<TParameter, TReturn>`.
Both have the same functionaly, aggregate and execute a series of middleware retrieving a return type.

One difference of chain responsibility when compared to pipeline is the fallback function that can be defined with
the `Finally` method. You can set one function for chain of responsibility, calling the method more than once
will replace the previous function defined.

As we already have an example of a chain of responsibility, here is an example using the asynchronous implementation:
If you want to, you can use the asynchronous version, using asynchronous middleware. Changing the instantiation to:
```C#
var exceptionHandlersChain = new AsyncResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryAsyncExceptionHandler>() // The order of middleware being chained matters
    .Chain<ArgumentAsyncExceptionHandler>()
    .Finally((ex) =>
    {
        ex.Source = ExceptionSource;
        return Task.FromResult(true);
    });
```
And here is the execution:
```C#
// The following line will execute only the OutOfMemoryExceptionHandler, which is the first middleware.
bool result = await exceptionHandlersChain.Execute(new OutOfMemoryException()); // Result will be true

// This one will execute the OutOfMemoryExceptionHandler first, and then the ArgumentExceptionHandler gets executed.
result = await exceptionHandlersChain.Execute(new ArgumentException()); // Result will be true

// If no middleware matches returns a value, the default of the return type is returned, which in the case of 'bool' is false.
result = await exceptionHandlersChain.Execute(new InvalidOperationException()); // Result will be false
```

## Factories
Instead of instantiating pipelines and chains of responsibility directly, you can use factories to instantiate them:
```C#
IAsyncPipelineFactory<Bitmap> pipelineFactory = new AsyncPipelineFactory<Bitmap>(new ActivatorMiddlewareResovler());

IAsyncPipeline<Bitmap> pipeline = pipelineFactory.Create()
    .Add<RoudCornersAsyncMiddleware>()
    .Add<AddTransparencyAsyncMiddleware>()
    .Add<AddWatermarkAsyncMiddleware>();
```

There are four types of factories:
- `PipelineFactory<TParamater>` for pipelines.
- `AsyncPipelineFactory<TParamater>` for asynchronous pipelines.
- `ResponsibilityChainFactory<TParameter, TReturn>` for chains of responsibility.
- `AsyncResponsibilityChainFactory<TParameter, TReturn>` for asynchronous chains of responsibility.

## Middleware resolver
You may be wondering what is all this `ActivatorMiddlewareResolver` class being passed to every instance of pipeline and chain of responsibility.
This is a default implementation of the `IMiddlewareResolver`, which is used to create instances of the middleware types.

When configuring a pipeline/chain of responsibility you define the types of the middleware, when the flow is executed those middleware
needs to be instantiated, so `IMiddlewareResolver` is responsible for that. Instantiated middleware are disposed automatically if they implement `IDisposable` or `IAsyncDisposable`. You can even create your own implementation, since the
`ActivatorMiddlewareResolver` only works for parametersless constructors.

### ServiceProvider implementation

An implementation of the middleware resolver for `IServiceProvider` was provided by [@mariusz96](https://github.com/mariusz96). It is tested against Microsoft.Extensions.DependencyInjection `8.X.X`, but should work with any dependency injection container that implements `IServiceProvider`.

You can grab it from nuget with:

```
Install-Package PipelineNet.ServiceProvider
```

Use it as follows:
```C#
public interface IMyService
{
    Task DoSomething();
}

public class MyService : IMyService
{
    private readonly IAsyncPipelineFactory<Bitmap> _pipelineFactory;

    public MyPipelineFactory(IAsyncPipelineFactory<Bitmap> pipelineFactory)
    {
        _pipelineFactory = pipelineFactory;
    }

    public Task DoSomething()
    {
        IAsyncPipeline<Bitmap> pipeline = _pipelineFactory.Create()
            .Add<RoudCornersAsyncMiddleware>()
            .Add<AddTransparencyAsyncMiddleware>()
            .Add<AddWatermarkAsyncMiddleware>();

        Bitmap image = (Bitmap) Image.FromFile("party-photo.png");

        await pipeline.Execute(image);
    }
}

public class RoudCornersAsyncMiddleware : IAsyncMiddleware<Bitmap>
{
    private readonly ILogger<RoudCornersAsyncMiddleware> _logger;

    // The following constructor arguments will be provided by IServiceProvider
    public RoudCornersAsyncMiddleware(ILogger<RoudCornersAsyncMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Run(Bitmap parameter, Func<Bitmap, Task> next)
    {
        _logger.LogInformation("Running RoudCornersAsyncMiddleware.");
        // Handle somehow
        await next(parameter);
    }
}
```

Register it using `IHttpContextAccessor` (recommended):
```C#
services.AddSingleton<IMyService, MyService>();

services.AddMiddlewareFromAssembly(typeof(RoudCornersAsyncMiddleware).Assembly, ServiceLifetime.Scoped); // Add all middleware from assembly

services.AddHttpContextAccessor();

services.TryAddSingleton<IMiddlewareResolver, HttpContextAccessorMiddlewareResolver>(); // Add middleware resolver

services.TryAddSingleton(typeof(IPipelineFactory<>), typeof(AsyncPipelineFactory<>)); // Add pipeline and chain of responsibility factories
services.TryAddSingleton(typeof(IAsyncPipelineFactory<>), typeof(AsyncPipelineFactory<>));
services.TryAddSingleton(typeof(IResponsibilityChainFactory<,>), typeof(ResponsibilityChainFactory<,>));
services.TryAddSingleton(typeof(IAsyncResponsibilityChainFactory<,>), typeof(AsyncResponsibilityChainFactory<,>));

public class HttpContextAccessorMiddlewareResolver : IMiddlewareResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAccessorMiddlewareResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException("httpContextAccessor",
            "An instance of IHttpContextAccessor must be provided.");
    }

    public MiddlewareResolverResult Resolve(Type type)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) throw new InvalidOperationException("HttpContext must not be null.");

        var middleware = httpContext.RequestServices.GetRequiredService(type);
        bool isDisposable = false;

        return new MiddlewareResolverResult
        {
            Middleware = middleware,
            IsDisposable = isDisposable
        };
    }
}
```

or `IServiceProvider`:
```C#
services.AddScoped<IMyService, MyService>();

services.AddMiddlewareFromAssembly(typeof(RoudCornersAsyncMiddleware).Assembly, ServiceLifetime.Scoped); // Add all middleware from assembly

services.TryAddScoped<IMiddlewareResolver, ServiceProviderMiddlewareResolver>(); // Add scoped middleware resolver so that scoped IServiceProvider is injected

services.TryAddScoped(typeof(IPipelineFactory<>), typeof(AsyncPipelineFactory<>)); // Add pipeline and chain of responsibility factories
services.TryAddScoped(typeof(IAsyncPipelineFactory<>), typeof(AsyncPipelineFactory<>));
services.TryAddScoped(typeof(IResponsibilityChainFactory<,>), typeof(ResponsibilityChainFactory<,>));
services.TryAddScoped(typeof(IAsyncResponsibilityChainFactory<,>), typeof(AsyncResponsibilityChainFactory<,>));
```

Note that `IServiceProvider` lifetime can vary based on the lifetime of the containing class. For example, if you resolve service from a scope, and it takes an `IServiceProvider`, it'll be a scoped instance.

For more information on dependency injection, see: [Dependency injection - .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection).

### Unity implementation

An implementation of the [middleware resolver for Unity](https://github.com/ShaneYu/PipelineNet.Unity) was kindly provided by [@ShaneYu](https://github.com/ShaneYu). It is tested against Unity.Container `5.X.X`, you can grab it from nuget with:

```
Install-Package PipelineNet.Unity
```

## License
This project is licensed under MIT. Please, feel free to contribute with code, issues or tips :)
