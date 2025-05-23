using IOBox.TaskExecution.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Unlock.Options;

class UnlockOptionsValidator : TaskExecutionOptionsValidator<UnlockOptions>
{
    public override ValidateOptionsResult Validate(string? name, UnlockOptions options)
    {
        var result = base.Validate(name, options);

        if (result.Failed)
        {
            return result;
        }

        if (options.LockDuration <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(UnlockOptions)}." +
                $"{nameof(options.LockDuration)} must be greater than 0.");
        }

        return ValidateOptionsResult.Success;
    }
}
