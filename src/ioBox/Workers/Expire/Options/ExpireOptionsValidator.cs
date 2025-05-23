using IOBox.TaskExecution.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Expire.Options;

class ExpireOptionsValidator : TaskExecutionOptionsValidator<ExpireOptions>
{
    public override ValidateOptionsResult Validate(string? name, ExpireOptions options)
    {
        var result = base.Validate(name, options);

        if (result.Failed)
        {
            return result;
        }

        if (options.NewMessageTtl <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(ExpireOptions)}." +
                $"{nameof(options.NewMessageTtl)} must be null or greater than 0.");
        }

        if (options.FailedMessageTtl <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(ExpireOptions)}." +
                $"{nameof(options.FailedMessageTtl)} must be null or greater than 0.");
        }

        return ValidateOptionsResult.Success;
    }
}
