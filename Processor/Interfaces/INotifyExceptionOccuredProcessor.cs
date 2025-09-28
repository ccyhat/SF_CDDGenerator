using SFTemplateGenerator.Processor.Moduels;

namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface INotifyExceptionOccuredProcessor
    {
        event NotifyExceptionOccuredHandler OnExceptionOccured;

        void RaiseException(object sender, string message, int? errorCode = null, Exception exception = null);
    }
}
