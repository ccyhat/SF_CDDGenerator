using SFTemplateGenerator.Helper.Paths;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using static SFTemplateGenerator.Helper.Paths.PathSaver;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class SelectTester : ISelectTester
    {
        public Task SelectTesterAsync(SDL sdl, Items root)
        {
            var config = PathSaver.Instance.Config;
            if (config.Tester == TESTER.PONOVOTester)
            {
                root.ItemList = root.ItemList.Where(I => !I.Name.Equals("选择标准源")).ToList();
            }
            else if (config.Tester == TESTER.PONOVOStandardSource)
            {
                root.ItemList = root.ItemList.Where(I => !I.Name.Equals("选择测试仪")).ToList();
            }
            else if (config.Tester == TESTER.ONLLYTester)
            {
                root.ItemList = root.ItemList.Where(I => !I.Name.Equals("选择测试仪")).ToList();
                root.ItemList = root.ItemList.Where(I => !I.Name.Equals("选择标准源")).ToList();
            }
            else
            {
                throw new NotSupportedException("不支持的模式");
            }


            return Task.CompletedTask;
        }


    }
}
