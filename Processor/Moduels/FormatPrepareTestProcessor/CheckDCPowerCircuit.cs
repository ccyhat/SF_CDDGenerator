using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor;

namespace SFTemplateGenerator.Processor.Moduels.FormatPrepareTestProcessor
{

    public class CheckDCPowerCircuit : ICheckDCPowerCircuit
    {


        public Task CheckDCPowerCircuitAsync(SDL sdl, Items root)
        {

            var safety = root.GetSafetys().FirstOrDefault(S => S.Name.StartsWith("检查直流电源回路"))!;

            var DC_Voltage = sdl.Cubicle.DcPowerSupplyVoltage;
            safety.Name = $"检查直流电源回路 {DC_Voltage}";
            safety.DllCall.CData = $"SpeakString=电压{DC_Voltage};ExpectString=是否合格;";
            return Task.CompletedTask;
        }
    }
}
