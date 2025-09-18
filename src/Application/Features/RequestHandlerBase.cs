using Agrovet.Application.Helpers;
using FluentValidation;

namespace Agrovet.Application.Features;

public abstract class ValidatorBase<T> : AbstractValidator<T> where T : class
{
    protected Func<string, bool> ContainsCode(IEnumerable<string> validCodes) => validCodes.Contains;
    protected Func<string, bool> NotContainsCode(IEnumerable<string> validCodes) => x => !validCodes.Contains(x);
}

public class RequestHandlerBase : Disposable
{

}