using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor
{
    public interface IConnectMaintenanceSignal
    {
        Task ConnectMaintenanceSignalAsync(SDL sdl, Items root);
    }
}
