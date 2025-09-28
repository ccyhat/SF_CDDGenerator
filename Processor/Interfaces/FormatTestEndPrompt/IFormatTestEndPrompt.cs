using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatTestEndPrompt
{
    public interface IFormatTestEndPrompt
    {
        public Task FormatTestEndPromptAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
