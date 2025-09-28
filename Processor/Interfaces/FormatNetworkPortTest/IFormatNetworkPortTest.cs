using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatNetworkPortTest
{
    public interface IFormatNetworkPortTest
    {
        public Task FormatNetworkPortTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
