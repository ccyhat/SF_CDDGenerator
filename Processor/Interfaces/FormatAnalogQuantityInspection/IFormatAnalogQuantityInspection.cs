using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;


namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface IFormatAnalogQuantityInspection
    {
        Task FormatAnalogQuantityInspectionAsync(Device TargetDevice, SDL sdl, GuideBook guideBook);
    }
}
