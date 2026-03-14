using Mediator;

namespace DarkPeak.Functional.Mediator.Tests;

public sealed class SenderExtensionsShould
{
    [Test]
    public async Task Send_result_command_with_value()
    {
        var expected = Result.Success<string, Error>("ok");
        var sender = new FakeSender(expected);

        var result = await sender.SendResult(new TestResultCommand<string>());

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(((Success<string, Error>)result).Value).IsEqualTo("ok");
    }

    [Test]
    public async Task Send_result_command_with_unit()
    {
        var expected = Result.Success<Unit, Error>(Unit.Value);
        var sender = new FakeSender(expected);

        var result = await sender.SendResult(new TestUnitCommand());

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Send_result_query()
    {
        var expected = Result.Success<int, Error>(42);
        var sender = new FakeSender(expected);

        var result = await sender.SendResult(new TestResultQuery());

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(((Success<int, Error>)result).Value).IsEqualTo(42);
    }

    [Test]
    public async Task Send_result_request_with_value()
    {
        var expected = Result.Success<string, Error>("requested");
        var sender = new FakeSender(expected);

        var result = await sender.SendResult(new TestResultRequest());

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(((Success<string, Error>)result).Value).IsEqualTo("requested");
    }

    [Test]
    public async Task Send_result_request_with_unit()
    {
        var expected = Result.Success<Unit, Error>(Unit.Value);
        var sender = new FakeSender(expected);

        var result = await sender.SendResult(new TestUnitRequest());

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Forward_cancellation_token()
    {
        using var cts = new CancellationTokenSource();
        var sender = new FakeSender(Result.Success<string, Error>("ok"));

        await sender.SendResult(new TestResultCommand<string>(), cts.Token);

        await Assert.That(sender.LastCancellationToken).IsEqualTo(cts.Token);
    }

    public sealed record TestResultCommand<T> : IResultCommand<T>;
    public sealed record TestUnitCommand : IResultCommand;
    public sealed record TestResultQuery : IResultQuery<int>;
    public sealed record TestResultRequest : IResultRequest<string>;
    public sealed record TestUnitRequest : IResultRequest;

    private sealed class FakeSender(object response) : ISender
    {
        public CancellationToken LastCancellationToken { get; private set; }

        public ValueTask<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return new ValueTask<TResponse>((TResponse)response);
        }

        public ValueTask<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return new ValueTask<TResponse>((TResponse)response);
        }

        public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return new ValueTask<TResponse>((TResponse)response);
        }

        public ValueTask<object?> Send(object message, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return new ValueTask<object?>(response);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamQuery<TResponse> query, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamCommand<TResponse> command, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IAsyncEnumerable<object?> CreateStream(object message, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
