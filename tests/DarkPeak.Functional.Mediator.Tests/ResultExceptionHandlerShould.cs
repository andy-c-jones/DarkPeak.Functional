using Mediator;

namespace DarkPeak.Functional.Mediator.Tests;

public sealed class ResultExceptionHandlerShould
{
    private readonly ResultExceptionHandler<TestCommand, string> _sut = new();

    [Test]
    public async Task Convert_exception_to_failure_with_internal_error()
    {
        var command = new TestCommand();
        MessageHandlerDelegate<TestCommand, Result<string, Error>> next =
            (_, _) => throw new InvalidOperationException("Something went wrong");

        var result = await _sut.Handle(command, next, CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();

        var failure = (Failure<string, Error>)result;
        await Assert.That(failure.Error).IsTypeOf<InternalError>();

        var internalError = (InternalError)failure.Error;
        await Assert.That(internalError.Message).IsEqualTo("Something went wrong");
        await Assert.That(internalError.ExceptionType).IsEqualTo("InvalidOperationException");
    }

    [Test]
    public async Task Preserve_exception_type_name_for_custom_exceptions()
    {
        var command = new TestCommand();
        MessageHandlerDelegate<TestCommand, Result<string, Error>> next =
            (_, _) => throw new CustomTestException("Custom failure");

        var result = await _sut.Handle(command, next, CancellationToken.None);

        var failure = (Failure<string, Error>)result;
        var internalError = (InternalError)failure.Error;
        await Assert.That(internalError.ExceptionType).IsEqualTo("CustomTestException");
        await Assert.That(internalError.Message).IsEqualTo("Custom failure");
    }

    [Test]
    public async Task Pass_through_successful_results()
    {
        var command = new TestCommand();
        MessageHandlerDelegate<TestCommand, Result<string, Error>> next =
            (_, _) => new ValueTask<Result<string, Error>>(Result.Success<string, Error>("success"));

        var result = await _sut.Handle(command, next, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        var success = (Success<string, Error>)result;
        await Assert.That(success.Value).IsEqualTo("success");
    }

    [Test]
    public async Task Pass_through_failure_results()
    {
        var command = new TestCommand();
        var expectedError = new NotFoundError { Message = "Not found" };
        MessageHandlerDelegate<TestCommand, Result<string, Error>> next =
            (_, _) => new ValueTask<Result<string, Error>>(Result.Failure<string, Error>(expectedError));

        var result = await _sut.Handle(command, next, CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        var failure = (Failure<string, Error>)result;
        await Assert.That(failure.Error).IsTypeOf<NotFoundError>();
        await Assert.That(failure.Error.Message).IsEqualTo("Not found");
    }

    public sealed record TestCommand : IResultCommand<string>;

    private sealed class CustomTestException(string message) : Exception(message);
}
