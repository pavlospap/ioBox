using Microsoft.Extensions.Options;

namespace IOBox.TaskExecution.Options;

abstract class TaskExecutionOptionsValidator<TOptions> : IValidateOptions<TOptions>
    where TOptions : class, ITaskExecutionOptions
{
    public virtual ValidateOptionsResult Validate(string? name, TOptions options)
    {
        var type = typeof(TOptions).Name;

        if (options.Delay <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {type}.{nameof(options.Delay)} must be greater than 0.");
        }

        if (options.Timeout <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {type}.{nameof(options.Timeout)} must be null or greater than 0.");
        }

        if (options.BatchSize <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {type}.{nameof(options.BatchSize)} must be greater than 0.");
        }

        return ValidateOptionsResult.Success;
    }
}
