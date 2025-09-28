using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatTimeSynchronizationTest
{
    public interface IFormatTimeSynchronizationTest
    {
        public Task FormatTimeSynchronizationTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
