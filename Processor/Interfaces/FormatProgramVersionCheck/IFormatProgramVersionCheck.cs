using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatProgramVersionCheck
{
    public interface IFormatProgramVersionCheck
    {
        Task FormatProgramVersionCheckAsync(Device targetDevice, SDL sdl, GuideBook guideBook);
    }
}
