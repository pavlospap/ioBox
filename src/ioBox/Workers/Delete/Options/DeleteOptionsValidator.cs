using IOBox.TaskExecution.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Delete.Options;

class DeleteOptionsValidator : TaskExecutionOptionsValidator<DeleteOptions>
{
    public override ValidateOptionsResult Validate(string? name, DeleteOptions options)
    {
        var result = base.Validate(name, options);

        if (result.Failed)
        {
            return result;
        }

        if (options.ProcessedMessageTtl <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DeleteOptions)}." +
                $"{nameof(options.ProcessedMessageTtl)} must be null or greater than 0.");
        }

        if (options.ExpiredMessageTtl <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DeleteOptions)}." +
                $"{nameof(options.ExpiredMessageTtl)} must be null or greater than 0.");
        }

        return ValidateOptionsResult.Success;
    }
}
