using IOBox.TaskExecution.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.RetryPoll.Options;

class RetryPollOptionsValidator : TaskExecutionOptionsValidator<RetryPollOptions>
{
    public override ValidateOptionsResult Validate(string? name, RetryPollOptions options)
    {
        var result = base.Validate(name, options);

        if (result.Failed)
        {
            return result;
        }

        if (options.Limit < 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(RetryPollOptions)}." +
                $"{nameof(options.Limit)} must be greater than or equal to 0.");
        }

        return ValidateOptionsResult.Success;
    }
}
