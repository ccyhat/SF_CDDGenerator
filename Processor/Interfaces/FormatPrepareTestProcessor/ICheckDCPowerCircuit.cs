using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor
{
    public interface ICheckDCPowerCircuit
    {
        Task CheckDCPowerCircuitAsync(SDL sdl, Items root);
    }
}
