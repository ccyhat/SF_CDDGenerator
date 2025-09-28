using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor;


namespace SFTemplateGenerator.Processor.Moduels.FormatPrepareTestProcessor
{
    public class FormatPrepareTestProcessor : IFormatPrepareTestProcessor
    {
        private readonly IUpdateDCVoltage _updateDCVoltage;
        private readonly ICheckTerminalConnector _checkTerminalConnector;
        private readonly ICheckDCPowerCircuit _checkDCPowerCircuit;
        private readonly IConnectMaintenanceSignal _connectMaintenanceSignal;
        private readonly IConnectTimeSynchronizationLine _connectTimeSynchronizationLine;
        public FormatPrepareTestProcessor(
             IUpdateDCVoltage updateDCVoltage,
             ICheckTerminalConnector checkTerminalConnector,
                ICheckDCPowerCircuit checkDCPowerCircuit,
                IConnectMaintenanceSignal connectMaintenanceSignal,
                IConnectTimeSynchronizationLine connectTimeSynchronizationLine

            )
        {
            _updateDCVoltage = updateDCVoltage;
            _checkTerminalConnector = checkTerminalConnector;
            _checkDCPowerCircuit = checkDCPowerCircuit;
            _connectMaintenanceSignal = connectMaintenanceSignal;
            _connectTimeSynchronizationLine = connectTimeSynchronizationLine;

        }
        public async Task FormatPrepareTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {

            Logger.Info($"测试前检测");
            var root = guideBook.Device.Items.Where(I => I.Name.Equals("测试前检查")).FirstOrDefault()!;
            await _updateDCVoltage.UpdateDCVoltageAsync(sdl, root);//更新直流电压
            await _checkTerminalConnector.CheckTerminalConnectorAsync(sdl, root);//检查端子连片
            await _checkDCPowerCircuit.CheckDCPowerCircuitAsync(sdl, root);//检查直流电源回路
            await _connectMaintenanceSignal.ConnectMaintenanceSignalAsync(sdl, root);//接入检修信号
            await _connectTimeSynchronizationLine.ConnectTimeSynchronizationLineAsync(TargetDevice, sdl, root);//接入对时线
        }
    }
}
