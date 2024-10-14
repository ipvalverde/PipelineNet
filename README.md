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
- [Cancellation tokens](#cancellation-tokens)
- [Middleware resolver](#middleware-resolver)
  - [ServiceProvider implementation](#serviceprovider-implementation)
  - [Unity implementation](#unity-implementation)
- [Migrate from PipelineNet 0.10 to 0.20](#migrate-from-pipelinenet-010-to-020)
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
You can even define a fallback that will be executed after your entire chain:
```C#
var exceptionHandlersChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryExceptionHandler>() // The order of middleware being chained matters
    .Chain<ArgumentExceptionHandler>()
    .Finally<FinallyDoSomething>();

public class FinallyDoSomething : IFinally<Exception, bool>
{
    public bool Finally(Exception parameter)
    {
        // Do something
        return true;
    }
}
```
Now if the same line gets executed:
```C#
var result = exceptionHandlersChain.Execute(new InvalidOperationException()); // Result will be true
```
The result will be true because of the type used in the `Finally` method.

You can also choose to throw an exception in the `Finally` method instead of returning a value:
```C#
var exceptionHandlersChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryExceptionHandler>()
    .Chain<ArgumentExceptionHandler>()
    .Finally<ThrowInvalidOperationException>();

public class ThrowInvalidOperationException : IFinally<Exception, bool>
{
    public bool Finally(Exception parameter)
    {
        throw new InvalidOperationException("End of the chain of responsibility reached. No middleware matches returned a value.");
    }
}
```
Now if the end of the chain was reached and no middleware matches returned a value, the `InvalidOperationException` will be thrown.

## Pipeline vs Chain of responsibility
Here is the difference between those two in PipelineNet:
- Chain of responsibility:
    - Returns a value;
    - Have a fallback to execute at the end of the chain;
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

One difference of chain responsibility when compared to pipeline is the fallback that can be defined with
the `Finally` method. You can set one finally for chain of responsibility, calling the method more than once
will replace the previous type used.

As we already have an example of a chain of responsibility, here is an example using the asynchronous implementation:
If you want to, you can use the asynchronous version, using asynchronous middleware. Changing the instantiation to:
```C#
var exceptionHandlersChain = new AsyncResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryAsyncExceptionHandler>() // The order of middleware being chained matters
    .Chain<ArgumentAsyncExceptionHandler>()
    .Finally<ExceptionHandlerFallback>();

public class ExceptionHandlerFallback : IAsyncFinally<Exception, bool>
{
    public Task<bool> Finally(Exception parameter)
    {
        parameter.Data.Add("MoreExtraInfo", "More information about the exception.");
        return Task.FromResult(true);
    }
}
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

## Cancellation tokens
If you want to pass the cancellation token to your asynchronous pipeline middleware, you can do so by implementing the `ICancellableAsyncMiddleware<TParameter>` interface
and passing the cancellation token argument to the `IAsyncPipeline<TParameter>.Execute` method:
```C#
var pipeline = new AsyncPipeline<Bitmap>(new ActivatorMiddlewareResolver())
    .AddCancellable<RoudCornersCancellableAsyncMiddleware>()
    .Add<AddTransparencyAsyncMiddleware>() // You can mix both kinds of asynchronous middleware
    .AddCancellable<AddWatermarkCancellableAsyncMiddleware>();

Bitmap image = (Bitmap) Image.FromFile("party-photo.png");
CancellationToken cancellationToken = CancellationToken.None;
await pipeline.Execute(image, cancellationToken);

public class RoudCornersCancellableAsyncMiddleware : ICancellableAsyncMiddleware<Bitmap>
{
    public async Task Run(Bitmap parameter, Func<Bitmap, Task> next, CancellationToken cancellationToken)
    {
        await RoundCournersAsync(parameter, cancellationToken);
        await next(parameter);
    }

    private async Task RoudCournersAsync(Bitmap bitmap, CancellationToken cancellationToken)
    {
        // Handle somehow
        await Task.CompletedTask;
    }
}
```
And to pass the cancellation token to your asynchronous chain of responsibility middleware, you can implement the `ICancellableAsyncMiddleware<TParameter, TReturn>` interface
and pass the cancellation token argument to the `IAsynchChainOfResponsibility<TParamete, TReturnr>.Execute` method.

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

Use it with dependency injection:
```C#
services.AddMiddlewareFromAssembly(typeof(RoudCornersAsyncMiddleware).Assembly);
services.AddScoped<IAsyncPipeline<Bitmap>>(serviceProvider =>
{
    return new AsyncPipeline<Bitmap>(new ServiceProviderMiddlewareResolver(serviceProvider))
        .Add<RoudCornersAsyncMiddleware>()
        .Add<AddTransparencyAsyncMiddleware>()
        .Add<AddWatermarkAsyncMiddleware>();
});
services.AddScoped<IMyService, MyService>();

public interface IMyService
{
    Task DoSomething();
}

public class MyService : IMyService
{
    private readonly IAsyncPipeline<Bitmap> _pipeline;

    public MyService(IAsyncPipeline<Bitmap> pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task DoSomething()
    {
        Bitmap image = (Bitmap) Image.FromFile("party-photo.png");
        await _pipeline.Execute(image);
    }
}

public class RoudCornersAsyncMiddleware : IAsyncMiddleware<Bitmap>
{
    private readonly ILogger<RoudCornersAsyncMiddleware> _logger;

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

Or instantiate pipeline/chain of responsibility directly:
```C#
services.AddMiddlewareFromAssembly(typeof(RoudCornersAsyncMiddleware).Assembly);

public class MyService : IMyService
{
    public async Task DoSomething()
    {
        IServiceProvider serviceProvider = GetServiceProvider();

        IAsyncPipeline<Bitmap> pipeline = new AsyncPipeline<Bitmap>(new ServiceProviderMiddlewareResolver(serviceProvider))
            .Add<RoudCornersAsyncMiddleware>()
            .Add<AddTransparencyAsyncMiddleware>()
            .Add<AddWatermarkAsyncMiddleware>();

        Bitmap image = (Bitmap) Image.FromFile("party-photo.png");
        await pipeline.Execute(image);
    }

    private IServiceProvider GetServiceProvider() => // Get service provider somehow
}
```

Note that `IServiceProvider` lifetime can vary based on the lifetime of the containing class. For example, if you resolve service from a scope, and it takes an `IServiceProvider`, it'll be a scoped instance.

For more information on dependency injection, see: [Dependency injection - .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection).

### Unity implementation

An implementation of the [middleware resolver for Unity](https://github.com/ShaneYu/PipelineNet.Unity) was kindly provided by [@ShaneYu](https://github.com/ShaneYu). It is tested against Unity.Container `5.X.X`, you can grab it from nuget with:

```
Install-Package PipelineNet.Unity
```

## Migrate from PipelineNet 0.10 to 0.20
In PipelineNet 0.20, `Finally` overloads that use `Func` have been made obsolete. This will be removed in the next major version.

To migrate replace:
```C#
var exceptionHandlersChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryExceptionHandler>()
    .Chain<ArgumentExceptionHandler>()
    .Finally((parameter) =>
    {
        // Do something
        return true;
    });
```
With:
```C#
var exceptionHandlersChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
    .Chain<OutOfMemoryExceptionHandler>()
    .Chain<ArgumentExceptionHandler>()
    .Finally<FinallyDoSomething>();

public class FinallyDoSomething : IFinally<Exception, bool>
{
    public bool Finally(Exception parameter)
    {
        // Do something
        return true;
    }
}
```

## License
This project is licensed under MIT. Please, feel free to contribute with code, issues or tips :)
