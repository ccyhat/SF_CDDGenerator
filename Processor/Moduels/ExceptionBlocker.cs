using SFTemplateGenerator.Processor.Interfaces;

namespace SFTemplateGenerator.Processor.Moduels
{
    public class ExceptionBlocker : IExceptionBlocker
    {
        private bool _signal = true;
        private bool _terminationFlag;

        public Task ContinueAsync()
        {
            _signal = true;
            return Task.CompletedTask;
        }

        public Task ResetAsync(bool flag = true)
        {
            _signal = !flag;
            _terminationFlag = false;
            return Task.CompletedTask;
        }

        public Task TerminateAsync()
        {
            _signal = true;
            _terminationFlag = true;

            return Task.CompletedTask;
        }

        public Task<bool> WaitAsync()
        {
            while (!_signal)
            {
            }

            return Task.FromResult<bool>(!_terminationFlag);
        }
    }
}
