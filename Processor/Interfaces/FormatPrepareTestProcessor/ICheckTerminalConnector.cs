using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor
{
    public interface ICheckTerminalConnector
    {
        Task CheckTerminalConnectorAsync(SDL sdl, Items root);
    }
}
