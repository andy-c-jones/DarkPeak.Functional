using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for <see cref="Unit"/> covering equality, default value, and type parameter usage.
/// </summary>
public class UnitShould
{
    [Test]
    public async Task Unit_value_is_default()
    {
        var unit = Unit.Value;

        await Assert.That(unit).IsEqualTo(default(Unit));
    }

    [Test]
    public async Task Unit_values_are_equal()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        await Assert.That(unit1).IsEqualTo(unit2);
    }

    [Test]
    public async Task Unit_can_be_used_as_result_success_type()
    {
        var result = Result.Success<Unit, Error>(Unit.Value);

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => default);
        await Assert.That(value).IsEqualTo(Unit.Value);
    }

    [Test]
    public async Task Unit_result_failure_propagates_error()
    {
        var error = new InternalError { Message = "Something failed" };
        var result = Result.Failure<Unit, Error>(error);

        await Assert.That(result.IsFailure).IsTrue();
        var err = result.Match(_ => null!, e => e);
        await Assert.That(err.Message).IsEqualTo("Something failed");
    }
}
