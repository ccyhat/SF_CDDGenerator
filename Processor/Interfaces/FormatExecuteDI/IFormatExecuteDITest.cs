using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatExecuteDI
{
    public interface IFormatExecuteDITest
    {
        public Task FormatExecuteDIAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
