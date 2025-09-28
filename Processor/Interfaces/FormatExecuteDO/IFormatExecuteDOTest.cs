using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatExecuteDO
{
    public interface IFormatExecuteDOTest
    {
        public Task FormatExecuteDOAsync(Device targetDevice, SDL sdl, GuideBook guideBook);
    }
}
