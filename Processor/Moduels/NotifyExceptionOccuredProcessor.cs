using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Processor.Interfaces;

namespace SFTemplateGenerator.Processor.Moduels
{
    public class NotifyExceptionOccuredProcessor : INotifyExceptionOccuredProcessor
    {
        public event NotifyExceptionOccuredHandler OnExceptionOccured;

        public void RaiseException(object sender, string message, int? errorCode = null, Exception exception = null)
        {

            if (OnExceptionOccured != null)
            {
                Logger.Info(message);
                OnExceptionOccured(sender, new NotifyExceptionOccuredEventArgs
                {
                    ErrorCode = errorCode,
                    ExceptionMessage = message,
                    Exception = exception

                });
            }
        }
    }
}
