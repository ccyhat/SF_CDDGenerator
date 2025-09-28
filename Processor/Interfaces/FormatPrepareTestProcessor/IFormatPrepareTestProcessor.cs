using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor
{
    public interface IFormatPrepareTestProcessor
    {
        Task FormatPrepareTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
