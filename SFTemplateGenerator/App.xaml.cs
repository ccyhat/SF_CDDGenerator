using SFTemplateGenerator.MainWindow.Shares;
using System.Windows;
namespace SFTemplateGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ProgramParameters.Instance.SetArgs(e.Args);
            base.OnStartup(e);
        }
    }

}
