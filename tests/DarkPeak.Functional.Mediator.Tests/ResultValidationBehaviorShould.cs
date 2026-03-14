using System.Diagnostics.CodeAnalysis;
using Mediator;

namespace DarkPeak.Functional.Mediator.Tests;

public sealed class ResultValidationBehaviorShould
{
    private readonly ResultValidationBehavior<ValidatableCommand, string> _sut = new();

    [Test]
    public async Task Short_circuit_with_failure_when_message_is_invalid()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required"]
        };
        var command = new ValidatableCommand(IsMessageValid: false, errors);

        var result = await _sut.Handle(command, NextHandler, CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result).IsTypeOf<Failure<string, Error>>();

        var failure = (Failure<string, Error>)result;
        await Assert.That(failure.Error).IsTypeOf<ValidationError>();

        var validationError = (ValidationError)failure.Error;
        await Assert.That(validationError.Message).IsEqualTo("Validation failed");
        await Assert.That(validationError.Errors).IsNotNull();
        await Assert.That(validationError.Errors!["Name"][0]).IsEqualTo("Name is required");
    }

    [Test]
    public async Task Delegate_to_next_handler_when_message_is_valid()
    {
        var command = new ValidatableCommand(IsMessageValid: true, null);

        var result = await _sut.Handle(command, NextHandler, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result).IsTypeOf<Success<string, Error>>();

        var success = (Success<string, Error>)result;
        await Assert.That(success.Value).IsEqualTo("handled");
    }

    [Test]
    public async Task Not_call_handler_when_validation_fails()
    {
        var handlerCalled = false;
        var command = new ValidatableCommand(IsMessageValid: false, new Dictionary<string, string[]>());

        MessageHandlerDelegate<ValidatableCommand, Result<string, Error>> next =
            (_, _) =>
            {
                handlerCalled = true;
                return new ValueTask<Result<string, Error>>(Result.Success<string, Error>("handled"));
            };

        await _sut.Handle(command, next, CancellationToken.None);

        await Assert.That(handlerCalled).IsFalse();
    }

    [Test]
    public async Task Pass_cancellation_token_to_next_handler()
    {
        using var cts = new CancellationTokenSource();
        var capturedToken = CancellationToken.None;
        var command = new ValidatableCommand(IsMessageValid: true, null);

        MessageHandlerDelegate<ValidatableCommand, Result<string, Error>> next =
            (_, ct) =>
            {
                capturedToken = ct;
                return new ValueTask<Result<string, Error>>(Result.Success<string, Error>("handled"));
            };

        await _sut.Handle(command, next, cts.Token);

        await Assert.That(capturedToken).IsEqualTo(cts.Token);
    }

    private static ValueTask<Result<string, Error>> NextHandler(
        ValidatableCommand message, CancellationToken cancellationToken) =>
        new(Result.Success<string, Error>("handled"));

    public sealed record ValidatableCommand(
        bool IsMessageValid,
        Dictionary<string, string[]>? ValidationErrors) : IResultCommand<string>, IValidate
    {
        public bool IsValid([NotNullWhen(false)] out ValidationError? error)
        {
            if (IsMessageValid)
            {
                error = null;
                return true;
            }

            error = new ValidationError
            {
                Message = "Validation failed",
                Errors = ValidationErrors
            };
            return false;
        }
    }
}
