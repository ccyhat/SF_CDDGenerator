using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest
{
    public interface IFormatOperationCircuitTest
    {
        public Task FormatOperationCircuitTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
