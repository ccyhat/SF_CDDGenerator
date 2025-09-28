
using Autofac;
using Castle.DynamicProxy;
using SFTemplateGenerator.Processor.Interfaces;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using SFTemplateGenerator.Processor.Interfaces.FormatConnectionLineCheck;
using SFTemplateGenerator.Processor.Interfaces.FormatDirectCurrentTest;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDI;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDO;
using SFTemplateGenerator.Processor.Interfaces.FormatNetworkPortTest;
using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;
using SFTemplateGenerator.Processor.Interfaces.FormatPowerPluginTest;
using SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor;
using SFTemplateGenerator.Processor.Interfaces.FormatProgramVersionCheck;
using SFTemplateGenerator.Processor.Interfaces.FormatTestEndPrompt;
using SFTemplateGenerator.Processor.Interfaces.FormatTimeSynchronizationTest;
using SFTemplateGenerator.Processor.Interfaces.FormatVoltageSwitchCircuitTest;
using SFTemplateGenerator.Processor.Moduels;
using SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection;
using SFTemplateGenerator.Processor.Moduels.FormatConnectionLineCheck;
using SFTemplateGenerator.Processor.Moduels.FormatDirectCurrentTest;
using SFTemplateGenerator.Processor.Moduels.FormatExecuteDI;
using SFTemplateGenerator.Processor.Moduels.FormatExecuteDO;
using SFTemplateGenerator.Processor.Moduels.FormatNetworkPortTest;
using SFTemplateGenerator.Processor.Moduels.FormatOperationCircuitTest;
using SFTemplateGenerator.Processor.Moduels.FormatPowerPluginTest;
using SFTemplateGenerator.Processor.Moduels.FormatPrepareTestProcessor;
using SFTemplateGenerator.Processor.Moduels.FormatProgramVersionCheck;
using SFTemplateGenerator.Processor.Moduels.FormatTestEndPrompt;
using SFTemplateGenerator.Processor.Moduels.FormatTimeSynchronizationTest;
using SFTemplateGenerator.Processor.Moduels.FormatVoltageSwitchCircuitTest;

namespace SFTemplateGenerator.Processor
{
    public class ProcessorModuel : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {


            builder.RegisterType<ProxyGenerator>().As<IProxyGenerator>().InstancePerDependency();
            // зЂВс
            builder.RegisterType<SDLKeeper>()
                   .As<ISDLKeeper>()
                   .SingleInstance();
            builder.RegisterType<TargetDeviceKeeper>()
                   .As<ITargetDeviceKeeper>()
                   .SingleInstance();
            builder.RegisterType<DeviceModelKeeper>()
                   .As<IDeviceModelKeeper>()
                   .SingleInstance();
            builder.RegisterType<PraseCDDProcessor>()
                  .As<IPraseCDDProcessor>()
                  .SingleInstance();
            builder.RegisterType<ConfigProcessor>()
                  .As<IConfigProcessor>()
                  .SingleInstance();
            // .EnableInterfaceInterceptors().InterceptedBy(typeof(ExceptionInterceptor));

            builder.RegisterType<IEDFilter>()
                  .As<IIEDFilter>()
                  .SingleInstance();
            builder.RegisterType<GbXmlProcessor>()
                .As<IGbXmlProcessor>()
                .SingleInstance();

            // зЂВс ExceptionInterceptor
            builder.RegisterType<ExceptionInterceptor>()
                .AsSelf().
                SingleInstance();
            builder.RegisterType<ExceptionBlocker>()
              .As<IExceptionBlocker>()
              .SingleInstance();
            builder.RegisterType<NotifyExceptionOccuredProcessor>()
                .As<INotifyExceptionOccuredProcessor>()
                .SingleInstance();
            LoadPrepareTestProcessor(builder);
            LoadAnalogQuantityInspection(builder);
            LoadDirectCurrentTest(builder);
            LoadProgramVersionCheck(builder);
            LoadExcuteDO(builder);
            LoadExcuteDI(builder);
            LoadConnectionLineCheck(builder);
            LoadVoltageSwitchCircuitTest(builder);
            LoadOperationCircuitTest(builder);
            LoadTimeSynchronizationTest(builder);
            LoadPowerPluginTest(builder);
            LoadFormatNetworkPortTest(builder);
            LoadFormatTestEndPrompt(builder);
        }
        private void LoadPrepareTestProcessor(ContainerBuilder builder)
        {
            builder.RegisterType<CheckDCPowerCircuit>()
                .As<ICheckDCPowerCircuit>()
                .InstancePerDependency();
            builder.RegisterType<CheckTerminalConnector>()
                .As<ICheckTerminalConnector>()
                .InstancePerDependency();
            builder.RegisterType<ConnectMaintenanceSignal>()
                .As<IConnectMaintenanceSignal>()
                .InstancePerDependency();
            builder.RegisterType<ConnectTimeSynchronizationLine>()
                .As<IConnectTimeSynchronizationLine>()
                .InstancePerDependency();
            builder.RegisterType<FormatPrepareTestProcessor>()
                .As<IFormatPrepareTestProcessor>()
                .InstancePerDependency();
            builder.RegisterType<UpdateDCVoltage>()
                .As<IUpdateDCVoltage>()
                .InstancePerDependency();
        }
        private void LoadAnalogQuantityInspection(ContainerBuilder builder)
        {
            builder.RegisterType<FormatAnalogQuantityInspection>()
                .As<IFormatAnalogQuantityInspection>()
                .InstancePerDependency();
            builder.RegisterType<PreTestChecklist>()
                .As<IPreTestChecklist>()
                .InstancePerDependency();
            builder.RegisterType<SwitchTest>()
                .As<ISwitchTest>()
                .InstancePerDependency();
            builder.RegisterType<VoltageCheck>()
                .As<IVoltageCheck>()
                .InstancePerDependency();
            builder.RegisterType<UpdateRatedValue>()
                .As<IUpdateRatedValue>()
                .InstancePerDependency();
            builder.RegisterType<ConnectCircuitBreaker>()
                .As<IConnectCircuitBreaker>()
                .InstancePerDependency();
            builder.RegisterType<SelectTester>()
                .As<ISelectTester>()
                .InstancePerDependency();
            builder.RegisterType<ZeroSequenceVoltageCurrentTest>()
                .As<IZeroSequenceVoltageCurrentTest>()
                .InstancePerDependency();
            builder.RegisterType<SwitchTest_dsAnAin>()
                .As<ISwitchTest_dsAnAin>()
                .InstancePerDependency();
        }
        private void LoadDirectCurrentTest(ContainerBuilder builder)
        {
            builder.RegisterType<FormatDirectCurrentTest>()
                .As<IFormatDirectCurrentTest>()
                .InstancePerDependency();
        }
        private void LoadProgramVersionCheck(ContainerBuilder builder)
        {
            builder.RegisterType<FormatProgramVersionCheck>()
                .As<IFormatProgramVersionCheck>()
                .InstancePerDependency();
        }
        private void LoadExcuteDO(ContainerBuilder builder)
        {
            builder.RegisterType<FormatExecuteDOTest>()
                .As<IFormatExecuteDOTest>()
                .InstancePerDependency();
            builder.RegisterType<PrepareExecuteDO>()
               .As<IPrepareExecuteDO>()
               .InstancePerDependency();
            builder.RegisterType<ExecuteDOProcess>()
               .As<IExecuteDOProcess>()
               .InstancePerDependency();
        }
        private void LoadExcuteDI(ContainerBuilder builder)
        {

            builder.RegisterType<FormatExecuteDITest>()
                .As<IFormatExecuteDITest>()
                .InstancePerDependency();
            builder.RegisterType<PrepareExecuteDI>()
              .As<IPrepareExecuteDI>()
              .InstancePerDependency();
            builder.RegisterType<ExecuteDIProcess>()
               .As<IExecuteDIProcess>()
               .InstancePerDependency();
        }
        private void LoadConnectionLineCheck(ContainerBuilder builder)
        {
            builder.RegisterType<FormatConnectionLineCheck>()
                .As<IFormatConnectionLineCheck>()
                .InstancePerDependency();
        }
        private void LoadVoltageSwitchCircuitTest(ContainerBuilder builder)
        {
            builder.RegisterType<FormatVoltageSwitchCircuitTest>()
               .As<IFormatVoltageSwitchCircuitTest>()
               .InstancePerDependency();
            builder.RegisterType<SwitchCirucuitProcess>()
                .As<ISwitchCirucuitProcess>()
                .InstancePerDependency();
        }
        private void LoadOperationCircuitTest(ContainerBuilder builder)
        {
            builder.RegisterType<FormatOperationCircuitTest>()
              .As<IFormatOperationCircuitTest>()
              .InstancePerDependency();
            builder.RegisterType<OperationCirucuitProcess>()
                .As<IOperationCirucuitProcess>()
                .InstancePerDependency();
        }
        private void LoadTimeSynchronizationTest(ContainerBuilder builder)
        {
            builder.RegisterType<FormatTimeSynchronizationTest>()
              .As<IFormatTimeSynchronizationTest>()
              .InstancePerDependency();
        }
        private void LoadPowerPluginTest(ContainerBuilder builder)
        {
            builder.RegisterType<FormatPowerPluginTest>()
              .As<IFormatPowerPluginTest>()
              .InstancePerDependency();
            builder.RegisterType<FailAlarmTest>()
                .As<IFailAlarmTest>()
              .InstancePerDependency();


        }
        private void LoadFormatNetworkPortTest(ContainerBuilder builder)
        {
            builder.RegisterType<FormatNetworkPortTest>()
              .As<IFormatNetworkPortTest>()
              .InstancePerDependency();
        }
        private void LoadFormatTestEndPrompt(ContainerBuilder builder)
        {
            builder.RegisterType<FormatTestEndPrompt>()
             .As<IFormatTestEndPrompt>()
             .InstancePerDependency();
        }
    }
}
