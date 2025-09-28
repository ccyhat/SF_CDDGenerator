using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatTimeSynchronizationTest;

namespace SFTemplateGenerator.Processor.Moduels.FormatTimeSynchronizationTest
{
    public class FormatTimeSynchronizationTest : IFormatTimeSynchronizationTest
    {
        public Task FormatTimeSynchronizationTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {

            return Task.CompletedTask;
        }
    }
}
