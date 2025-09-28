using System.Windows.Input;

namespace SFTemplateGenerator.Helper.Controls
{
    public class NotifyPropertyListValueChangedCommand : ICommand
    {
        private readonly Action<object> _process;

        public event EventHandler CanExecuteChanged;

        public NotifyPropertyListValueChangedCommand(Action<object> process)
        {
            _process = process;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _process(parameter);
        }

        protected virtual void RasieCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }
    }
}
