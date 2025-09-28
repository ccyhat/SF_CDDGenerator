using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDO;
using System.Text.RegularExpressions;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDO
{
    public class FormatExecuteDOTest : IFormatExecuteDOTest
    {
        private readonly IPrepareExecuteDO _prepareExecuteDO;
        private readonly IExecuteDOProcess _executeDOProcess;
        private List<string> _nodename = new List<string>();
        public static Regex r1 = new Regex(@"合.*?压板不闭合测试");
        public static Regex r2 = new Regex(@"合.*?压板闭合测试");
        public static Regex r3 = new Regex(@"分.*?压板闭合测试");
        public static Regex r4 = new Regex(@"分.*?压板不闭合测试");
        public FormatExecuteDOTest(
            IPrepareExecuteDO prepareExecuteDO,
            IExecuteDOProcess executeDOProcess)
        {
            _prepareExecuteDO = prepareExecuteDO;
            _executeDOProcess = executeDOProcess;
        }
        public async Task FormatExecuteDOAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {
            Logger.Info($"开出传动测试");
            var boards = TargetDevice.Boards.Where(B => DOBORAD_REGEX.IsMatch(B.Desc)|| POWERBORAD_REGEX.IsMatch(B.Desc)).ToList();
            Logger.Info($"开出插件和电源插件总数量：{boards.Count()}");
            if (boards.Count() == 0)
            {
                Logger.Info($"没有开出插件，不进行开出传动测试");
                guideBook.Device.Items.RemoveAll(I => I.Name.Equals("开出传动测试"));
            }
            else
            {
                var root = guideBook.Device.Items.Where(I => I.Name.Equals("开出传动测试")).FirstOrDefault()!;
                await _prepareExecuteDO.PrepareExecuteDOAsync(_nodename);
                await _executeDOProcess.ExecuteDOProcessAsync(sdl, root, _nodename);
                if (TargetDevice.Model.Contains("200F"))
                {
                    var dicOrder = root.ItemList
                        .Select(item => item.Name)
                        .OrderBy(name => name)
                        .Aggregate(new Dictionary<string, int>(), (dict, name) =>
                        {
                            string key = name.Substring(0, 3);
                            if (!dict.ContainsKey(key))
                            {
                                dict.Add(key, dict.Count);
                            }
                            return dict;
                        });
                    foreach (var item in root.ItemList)
                    {
                        item.OrderNum = firstWeight(dicOrder, item.Name) * 1000 + secondeWeight(item.Name);
                    }
                }
                else
                {
                    var reset = root.ItemList.FirstOrDefault(I => I.Name.Equals("装置复归（总）"))!;
                    reset.OrderNum = int.MaxValue;
                }
                _nodename.Add("装置复归（总）");
                //去掉多余节点
                root.ItemList.RemoveAll(I => !_nodename.Contains(I.Name));
            }
        }
        private int secondeWeight(string input)
        {
            if (input.Contains("传动测试")) return 1;
            if (r1.IsMatch(input)) return 2;
            if (r2.IsMatch(input)) return 3;
            if (r3.IsMatch(input)) return 4;
            if (r4.IsMatch(input)) return 5;
            return 0; // 默认值
        }
        private int firstWeight(Dictionary<string, int> dic, string input)
        {
            if (input.Contains("测试前准备")) return 0;
            if (input.Contains("把手测试")) return 1;
            if (input.Contains("装置告警")) return 98;
            if (input.Contains("装置故障")) return 98;
            if (input.Contains("装置闭锁")) return 98;
            if (input.Contains("装置复归")) return 99;
            if (dic.ContainsKey(input.Substring(0, 3)))
            {
                return dic[input.Substring(0, 3)];
            }
            else
            {
                return 99; // 如果不存在，返回默认值
            }
        }
    }
}
