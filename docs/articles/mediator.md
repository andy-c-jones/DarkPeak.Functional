# Mediator

The `DarkPeak.Functional.Mediator` package integrates DarkPeak.Functional with [Mediator](https://github.com/martinothamar/Mediator) — the source-generated CQRS library by Martin Othamar. This is **not** MediatR; Mediator uses source generators for zero-reflection, high-performance dispatch. This package provides convenience interfaces, pipeline behaviors, and extensions for using `Result<T, Error>` throughout the Mediator pipeline.

## Installation

```bash
dotnet add package DarkPeak.Functional.Mediator
```

You also need the Mediator source generator package itself:

```bash
dotnet add package Mediator
```

## Basic Usage

Define a command, implement a handler, and send it through the pipeline:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Mediator;
using Mediator;

// Define a command
public record CreateOrder(string Product, int Quantity) : IResultCommand<Order>;

// Implement the handler
public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Result<Order, Error>>
{
    public ValueTask<Result<Order, Error>> Handle(
        CreateOrder command, CancellationToken cancellationToken)
    {
        var order = new Order(Guid.NewGuid(), command.Product, command.Quantity);
        return new ValueTask<Result<Order, Error>>(Result.Success<Order, Error>(order));
    }
}

// Send it
var result = await sender.SendResult(new CreateOrder("Widget", 5));

result.Match(
    success: order => Console.WriteLine($"Created order {order.Id}"),
    failure: error => Console.WriteLine($"Failed: {error.Message}"));
```

## Marker Interfaces

The marker interfaces eliminate the need to write `ICommand<Result<T, Error>>` everywhere:

| Interface | Extends | Use Case |
|-----------|---------|----------|
| `IResultCommand<T>` | `ICommand<Result<T, Error>>` | Commands returning a value |
| `IResultCommand` | `ICommand<Result<Unit, Error>>` | Commands with no return value |
| `IResultQuery<T>` | `IQuery<Result<T, Error>>` | Queries |
| `IResultRequest<T>` | `IRequest<Result<T, Error>>` | Generic requests returning a value |
| `IResultRequest` | `IRequest<Result<Unit, Error>>` | Generic requests with no return value |

```csharp
// Without marker interfaces
public record GetUser(Guid Id) : IQuery<Result<User, Error>>;

// With marker interfaces
public record GetUser(Guid Id) : IResultQuery<User>;
```

Use `IResultCommand` / `IResultCommand<T>` for write operations, `IResultQuery<T>` for read operations, and `IResultRequest` / `IResultRequest<T>` when the distinction doesn't apply.

## Validation

Implement `IValidate` on a message to enable automatic validation before the handler runs:

```csharp
using System.Diagnostics.CodeAnalysis;
using DarkPeak.Functional;
using DarkPeak.Functional.Mediator;

public record CreateOrder(string Product, int Quantity) : IResultCommand<Order>, IValidate
{
    public bool IsValid([NotNullWhen(false)] out ValidationError? error)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Product))
            errors["product"] = ["Product is required"];

        if (Quantity <= 0)
            errors["quantity"] = ["Quantity must be greater than zero"];

        if (errors.Count > 0)
        {
            error = new ValidationError
            {
                Message = "Invalid order",
                Errors = errors
            };
            return false;
        }

        error = null;
        return true;
    }
}
```

`ResultValidationBehavior` runs in the pipeline before the handler. If `IsValid` returns `false`, the pipeline short-circuits and returns a `Failure` containing the `ValidationError` — the handler is never invoked.

## Exception Handling

`ResultExceptionHandler` catches any unhandled exception thrown during message handling and converts it to a `Failure<T, Error>` containing an `InternalError`:

```csharp
// If a handler throws...
public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Result<Order, Error>>
{
    public ValueTask<Result<Order, Error>> Handle(
        CreateOrder command, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Database connection lost");
    }
}

// ...the caller receives a Result, not an exception
var result = await sender.SendResult(new CreateOrder("Widget", 5));

result.Match(
    success: order => Console.WriteLine($"Created {order.Id}"),
    failure: error => Console.WriteLine($"Error: {error.Message}"));
    // Error: Database connection lost
```

The `InternalError` includes the exception `Message` and `ExceptionType` (e.g. `"InvalidOperationException"`). No exceptions leak out of the pipeline — every response is a `Result<T, Error>`.

## Registration

Register the Mediator source generator and DarkPeak pipeline behaviors together:

```csharp
using DarkPeak.Functional.Mediator;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator();                      // Mediator source generator
builder.Services.AddDarkPeakMediatorBehaviors();     // Exception handler + validation

var app = builder.Build();
```

`AddDarkPeakMediatorBehaviors` registers behaviors in the correct order:

1. **`ResultExceptionHandler`** — outermost, catches unhandled exceptions
2. **`ResultValidationBehavior`** — innermost, short-circuits invalid messages

## Sending Messages

`SenderExtensions` provides typed `SendResult` overloads on `ISender`:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Mediator;
using Mediator;

public class OrderEndpoints
{
    private readonly ISender _sender;

    public OrderEndpoints(ISender sender) => _sender = sender;

    public async Task HandleAsync(Guid orderId)
    {
        // Command with return value
        Result<Order, Error> created = await _sender.SendResult(new CreateOrder("Widget", 5));

        // Command with no return value
        Result<Unit, Error> archived = await _sender.SendResult(new ArchiveOrder(orderId));

        // Query
        Result<Order, Error> found = await _sender.SendResult(new GetOrderById(orderId));

        // With cancellation token
        using var cts = new CancellationTokenSource();
        var result = await _sender.SendResult(new GetOrderById(orderId), cts.Token);
    }
}
```

## Full Example

A complete minimal API with a validated command, handler, DI setup, and endpoint:

```csharp
using System.Diagnostics.CodeAnalysis;
using DarkPeak.Functional;
using DarkPeak.Functional.AspNet;
using DarkPeak.Functional.Mediator;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator();
builder.Services.AddDarkPeakMediatorBehaviors();

var app = builder.Build();

app.MapPost("/orders", async (CreateOrderRequest request, ISender sender) =>
    (await sender.SendResult(
        new CreateOrder(request.Product, request.Quantity)))
        .ToCreatedResult(order => $"/orders/{order.Id}"));

app.Run();
```

### Messages

```csharp
public record CreateOrderRequest(string Product, int Quantity);

public record CreateOrder(string Product, int Quantity) : IResultCommand<Order>, IValidate
{
    public bool IsValid([NotNullWhen(false)] out ValidationError? error)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Product))
            errors["product"] = ["Product is required"];

        if (Quantity <= 0)
            errors["quantity"] = ["Quantity must be greater than zero"];

        if (errors.Count > 0)
        {
            error = new ValidationError
            {
                Message = "Invalid order",
                Errors = errors
            };
            return false;
        }

        error = null;
        return true;
    }
}

public record Order(Guid Id, string Product, int Quantity);
```

### Handler

```csharp
public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Result<Order, Error>>
{
    public ValueTask<Result<Order, Error>> Handle(
        CreateOrder command, CancellationToken cancellationToken)
    {
        var order = new Order(Guid.NewGuid(), command.Product, command.Quantity);
        return new ValueTask<Result<Order, Error>>(Result.Success<Order, Error>(order));
    }
}
```

### HTTP Responses

**Success — 201 Created:**

```http
POST /orders
Content-Type: application/json

{ "product": "Widget", "quantity": 5 }
```

```http
HTTP/1.1 201 Created
Location: /orders/3fa85f64-5717-4562-b3fc-2c963f66afa6

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "product": "Widget",
  "quantity": 5
}
```

**Validation Failure — 422 Unprocessable Entity:**

```http
POST /orders
Content-Type: application/json

{ "product": "", "quantity": -1 }
```

```http
HTTP/1.1 422 Unprocessable Entity

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "errors": {
    "product": ["Product is required"],
    "quantity": ["Quantity must be greater than zero"]
  }
}
```

**Unhandled Exception — 500 Internal Server Error:**

If the handler throws, `ResultExceptionHandler` converts it to an `InternalError`, which the AspNet extensions map to a `500` ProblemDetails response.

## API Reference

| Type | Kind | Description |
|------|------|-------------|
| `IResultCommand<T>` | Interface | Command returning `Result<T, Error>` |
| `IResultCommand` | Interface | Command returning `Result<Unit, Error>` |
| `IResultQuery<T>` | Interface | Query returning `Result<T, Error>` |
| `IResultRequest<T>` | Interface | Request returning `Result<T, Error>` |
| `IResultRequest` | Interface | Request returning `Result<Unit, Error>` |
| `IValidate` | Interface | Self-validating message contract with `IsValid(out ValidationError?)` |
| `ResultValidationBehavior<TMessage, T>` | Pipeline Behavior | Auto-validates messages implementing `IValidate` before handler execution |
| `ResultExceptionHandler<TMessage, T>` | Exception Handler | Converts unhandled exceptions to `Failure` with `InternalError` |
| `ServiceCollectionExtensions.AddDarkPeakMediatorBehaviors()` | Extension Method | Registers both pipeline behaviors in correct order |
| `SenderExtensions.SendResult()` | Extension Method | Typed overloads on `ISender` for sending `IResultCommand`, `IResultQuery`, and `IResultRequest` |
