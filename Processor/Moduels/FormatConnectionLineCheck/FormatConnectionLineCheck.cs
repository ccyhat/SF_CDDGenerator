using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatConnectionLineCheck;
using System.Text.RegularExpressions;

namespace SFTemplateGenerator.Processor.Moduels.FormatConnectionLineCheck
{
    public class FormatConnectionLineCheck : IFormatConnectionLineCheck
    {
        private static readonly List<Regex> REGEX_TD = new List<Regex>()
        {
            new Regex(@"(\d)?Q(\d)?D"),
            new Regex(@"UD"),
            new Regex(@"ZD"),
            new Regex(@"GD"),
            new Regex(@"ID"),
            new Regex(@"RD"),
        };
        public Task FormatConnectionLineCheckAsync(Device TargetDevice, SDL sdl, GuideBook guideBook)
        {
            var cores = sdl.Cubicle.Cores;
            var devices = sdl.Cubicle.Devices.Where(D => D.Class == "TD").ToList();
            var target_cores = cores.Where(C => C.Class == "导线")
                .Where(C => C.DeviceA != C.DeviceB)
                .Where(C => devices.Any(D => D.Name == C.DeviceA) && devices.Any(D => D.Name == C.DeviceB)).ToList();
            target_cores = target_cores.Where(C =>
                REGEX_TD.All(R => !R.IsMatch(C.DeviceA)) && // 同时满足DeviceA不匹配所有R
                REGEX_TD.All(R => !R.IsMatch(C.DeviceB))    // 和DeviceB不匹配所有R
            ).ToList();
            if (target_cores.Count > 0)
            {
                var root = guideBook.Device.Items.FirstOrDefault(I => I.Name.StartsWith("连接线检测"));
                if (root != null)
                {
                    foreach (var target in target_cores)
                    {
                        var item = root.GetItems().FirstOrDefault(I => I.Name.Equals("导通测试"))!.Clone();
                        if (item != null)
                        {
                            //找到线两端的device
                            var A = devices.Where(D => D.Name == target.DeviceA).FirstOrDefault()!;
                            var B = devices.Where(D => D.Name == target.DeviceB).FirstOrDefault()!;
                            //找到线两端的board
                            var AA = A.Boards.Where(b => b.Name == target.BoardA).FirstOrDefault()!;
                            var BB = B.Boards.Where(b => b.Name == target.BoardB).FirstOrDefault()!;
                            var Speaking = $"量{B?.Name}{BB?.Name}和{A?.Name}{AA?.Name}";
                            var safety = item.GetSafetys().FirstOrDefault(I => I.Name.Equals("连接线测试（示例）"))!;
                            safety.Name = Speaking;
                            item.Name = Speaking;
                            safety.DllCall.CData = $"SpeakString={Speaking};ExpectString=是否完成;";
                        }
                        root.ItemList.Add(item);
                    }
                    root.ItemList.RemoveAll(I => I.Name.Equals("导通测试"));
                    if (root.ItemList.Count == 0)
                    {
                        guideBook.Device.Items.RemoveAll(I => I.Name.StartsWith("连接线检测"));
                    }
                }
            }
            else
            {
                guideBook.Device.Items.RemoveAll(I => I.Name.StartsWith("连接线检测"));
            }
            return Task.CompletedTask;
        }

    }

}
