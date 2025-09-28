using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor;

namespace SFTemplateGenerator.Processor.Moduels.FormatPrepareTestProcessor
{
    public class ConnectMaintenanceSignal : IConnectMaintenanceSignal
    {
        public Task ConnectMaintenanceSignalAsync(SDL sdl, Items root)
        {
            return Task.CompletedTask;
        }
    }
}
