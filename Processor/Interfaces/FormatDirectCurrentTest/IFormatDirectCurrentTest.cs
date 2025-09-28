using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatDirectCurrentTest
{
    public interface IFormatDirectCurrentTest
    {
        public Task FormatDirectCurrentTestAsync(Device targetDevice, SDL sdl, GuideBook guideBook);
    }
}
