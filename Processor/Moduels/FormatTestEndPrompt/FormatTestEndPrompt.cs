using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatTestEndPrompt;

namespace SFTemplateGenerator.Processor.Moduels.FormatTestEndPrompt
{
    public class FormatTestEndPrompt : IFormatTestEndPrompt
    {
        public Task FormatTestEndPromptAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {
            return Task.CompletedTask;
        }
    }
}
