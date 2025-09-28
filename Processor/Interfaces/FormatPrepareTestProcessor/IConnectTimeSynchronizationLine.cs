using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor
{
    public interface IConnectTimeSynchronizationLine
    {
        Task ConnectTimeSynchronizationLineAsync(Device TargetDevice, SDL sdl, Items root);
    }
}
