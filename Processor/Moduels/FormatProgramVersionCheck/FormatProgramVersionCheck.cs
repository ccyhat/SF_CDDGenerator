using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatProgramVersionCheck;

namespace SFTemplateGenerator.Processor.Moduels.FormatProgramVersionCheck
{
    public class FormatProgramVersionCheck : IFormatProgramVersionCheck
    {
        public Task FormatProgramVersionCheckAsync(Device targetDevice, SDL sdl, GuideBook guideBook)
        {
            return Task.CompletedTask;
        }
    }
}
