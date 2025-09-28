

namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface IExceptionBlocker
    {
        Task ContinueAsync();
        Task<bool> WaitAsync();
        Task TerminateAsync();
        Task ResetAsync(bool flag = true);
    }
}
