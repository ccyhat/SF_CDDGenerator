using Autofac;
using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatOperationCircuitTest
{
    public class FormatOperationCircuitTest : IFormatOperationCircuitTest
    {
        private readonly ILifetimeScope _lifetimeScope;
        private List<string> _nodename = new List<string>();
        public FormatOperationCircuitTest(
            ILifetimeScope lifetimeScope
            )
        {
            _lifetimeScope = lifetimeScope;
        }
        public Task FormatOperationCircuitTestAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {

            Logger.Info($"操作回路测试");
            var boards = TargetDevice.Boards.Where(B => OPBORAD_REGEX.Any(R => R.IsMatch(B.Desc))).ToList();
            Logger.Info($"操作插件数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有操作插件，不进行操作回路测试");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("操作回路测试"));
            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("操作回路测试")).FirstOrDefault();

                // 根据型号动态解析命名注册的 IOperationCirucuitProcess 实现
                IOperationCirucuitProcess? resolvedProcessor = null;

                if (REGEX_6U.Any(R => R.IsMatch(TargetDevice?.Model ?? string.Empty)))
                {
                    // 优先尝试解析 6U 版本
                    if (!_lifetimeScope.TryResolveNamed<IOperationCirucuitProcess>("6U", out resolvedProcessor))
                    {
                        // 回退到普通实现
                        _lifetimeScope.TryResolveNamed<IOperationCirucuitProcess>("Normal", out resolvedProcessor);
                    }
                }
                else
                {
                    // 非 6U 型号优先普通实现
                    if (!_lifetimeScope.TryResolveNamed<IOperationCirucuitProcess>("Normal", out resolvedProcessor))
                    {
                        _lifetimeScope.TryResolveNamed<IOperationCirucuitProcess>("6U", out resolvedProcessor);
                    }
                }

                if (resolvedProcessor != null)
                {
                    resolvedProcessor.OperationCirucuitProcessAsync(sdl, root, _nodename);
                }
                else
                {
                    Logger.Warning("未解析到 IOperationCirucuitProcess 实现，跳过操作回路测试。");
                }

                //去掉多余节点
                root.ItemList.RemoveAll(I => !_nodename.Contains(I.Name));
            }

            return Task.CompletedTask;
        }
    }
}
