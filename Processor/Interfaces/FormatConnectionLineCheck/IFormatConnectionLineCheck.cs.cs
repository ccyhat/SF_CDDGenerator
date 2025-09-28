using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatConnectionLineCheck
{
    public interface IFormatConnectionLineCheck
    {
        public Task FormatConnectionLineCheckAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
