using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDI
{
    internal class ExtensionBoardMatcher
    {
        public static readonly List<string> SwitchPosition = new List<string>()
        {
            "边",
            "中"
        };
        public static readonly Dictionary<ValueTuple<string,string>, string> ExtensionPortDesc = new Dictionary<ValueTuple<string, string>, string>
    {
        // 可以在这里初始化字典内容
        {(SwitchPosition[0],"分相跳闸位置TWJa"), "接点输入4"},
        {(SwitchPosition[1],"分相跳闸位置TWJa"), "接点输入1"},
        {(SwitchPosition[0],"分相跳闸位置TWJb"), "接点输入5"},
        {(SwitchPosition[1],"分相跳闸位置TWJb"), "接点输入2"},
        {(SwitchPosition[0],"分相跳闸位置TWJc"), "接点输入6"},
        {(SwitchPosition[1],"分相跳闸位置TWJc"), "接点输入3"},
    };
    }
}
