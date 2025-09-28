using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor
{
    public interface IUpdateDCVoltage
    {
        Task UpdateDCVoltageAsync(SDL sdl, Items root);
    }
}
