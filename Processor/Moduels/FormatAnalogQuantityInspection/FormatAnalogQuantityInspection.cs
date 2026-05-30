using Autofac;
using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;


namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{

    public class FormatAnalogQuantityInspection : IFormatAnalogQuantityInspection
    {

        private readonly IPreTestChecklist _preTestChecklist;
        private readonly ILifetimeScope _lifetimeScope;

        public FormatAnalogQuantityInspection(
            IPreTestChecklist preTestChecklist,
            ILifetimeScope lifetimeScope
            )
        {
            _preTestChecklist = preTestChecklist;
            _lifetimeScope = lifetimeScope;
        }
        private List<string> _nodename = new List<string>();
        public async Task FormatAnalogQuantityInspectionAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {

            Logger.Info($"模拟量检查");
            var boards = TargetDevice.Boards.Where(B => ACBORAD_REGEX.Any(R => R.IsMatch(B.Desc))).ToList();
            Logger.Info($"交流插件数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有交流插件，不进行模拟量检查");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("模拟量检查"));

            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("模拟量检查")).FirstOrDefault();
                await _preTestChecklist.PrepareAsync(sdl, root, _nodename);

                // 根据型号动态解析命名注册的 IVoltageCheck 实现
                IVoltageCheck? resolvedVoltageCheck = null;

                if (REGEX_6U.Any(R => R.IsMatch(TargetDevice?.Model ?? string.Empty)))
                {
                    // 优先尝试解析6U版本（注册时使用的名称）
                    if (!_lifetimeScope.TryResolveNamed<IVoltageCheck>("6U", out resolvedVoltageCheck))
                    {
                        // 回退到普通实现
                        _lifetimeScope.TryResolveNamed<IVoltageCheck>("Normal", out resolvedVoltageCheck);
                    }
                }
                else
                {
                    // 非6U型号优先普通实现
                    if (!_lifetimeScope.TryResolveNamed<IVoltageCheck>("Normal", out resolvedVoltageCheck))
                    {
                        _lifetimeScope.TryResolveNamed<IVoltageCheck>("6U", out resolvedVoltageCheck);
                    }
                }

                if (resolvedVoltageCheck != null)
                {
                    await resolvedVoltageCheck.VoltageCheckProcess(sdl, root, _nodename);
                }
                else
                {
                    Logger.Warning("未解析到 IVoltageCheck 实现，跳过电压检查。请确认已按名称注册：\"Normal\" 和/或 \"6U\"。");
                }

                //去掉多余节点              
                root.ItemList.RemoveAll(I => !_nodename.Contains(I.Name));
            }
        }
    }
}