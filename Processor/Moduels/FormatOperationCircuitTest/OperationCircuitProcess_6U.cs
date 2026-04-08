using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest;

namespace SFTemplateGenerator.Processor.Moduels.FormatOperationCircuitTest
{
    public class OperationCircuitProcess_6U : IOperationCirucuitProcess
    {
        public Task OperationCirucuitProcessAsync(SDL sdl, Items root, List<string> NodeName)
        {
            // 克隆模板节点创建自测提示
            var item = root.ItemList.FirstOrDefault(I => I.Name.Equals("断路器合位（YD）"))?.Clone() as Items;
            if (item != null)
            {
                item.Name = "操作插件自测";

                var safety = item.ItemList.FirstOrDefault(I => I.Name.Equals("提示接入表笔")) as Safety;
                if (safety != null)
                {
                    safety.Name = "操作插件由调试人员自测";
                    safety.DllCall.CData = "SpeakString=操作插件由调试人员自测;ExpectString=是否完成;";

                    // 只保留必要的提示节点
                    item.ItemList = item.ItemList.Where(I => I.Name.Equals("操作插件由调试人员自测")).ToList();
                }

                root.ItemList.Add(item);
                NodeName.Add(item.Name);
            }

            return Task.CompletedTask;
        }
    }
}
