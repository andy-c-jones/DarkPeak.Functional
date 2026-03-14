using System.Diagnostics.CodeAnalysis;

namespace DarkPeak.Functional.Mediator;

/// <summary>
/// Defines a self-validating message. Implement this on commands, queries, or requests
/// to enable automatic validation via <see cref="ResultValidationBehavior{TMessage, T}"/>.
/// </summary>
public interface IValidate
{
    /// <summary>
    /// Validates the message and returns whether it is valid.
    /// </summary>
    /// <param name="error">
    /// When this method returns <c>false</c>, contains the <see cref="ValidationError"/> describing
    /// the validation failure; otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if the message is valid; <c>false</c> otherwise.</returns>
    bool IsValid([NotNullWhen(false)] out ValidationError? error);
}
