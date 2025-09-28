using System.Text.RegularExpressions;

namespace SFTemplateGenerator.Helper.UtilityTools
{
    public class RegexProcess
    {
        public static List<Regex> GetModifiedRegexList(string input, List<Regex> ListRegex)
        {
            // 匹配前缀数字（如"1-"中的"1"）
            Regex regex = new Regex(@"^(\d)-\w*");
            var match = regex.Match(input);
            // 如果匹配失败，返回原始列表
            if (!match.Success)
            {
                return ListRegex;
            }
            // 获取捕获的数字（group[1]）
            string prefix = match.Groups[1].Value;
            // 创建新的正则列表，添加前缀
            List<Regex> modifiedRegexList = new List<Regex>();
            foreach (var originalRegex in ListRegex)
            {
                // 构建新的正则模式（添加前缀）
                string newPattern = $@"{prefix}-{originalRegex.ToString()}";
                modifiedRegexList.Add(new Regex(newPattern));
            }
            return modifiedRegexList;
        }
        public static List<string> GetModifiedRegexList(string input, List<string> ListRegex)
        {
            // 匹配前缀数字（如"1-"中的"1"）
            Regex regex = new Regex(@"^(\d)-\w*");
            var match = regex.Match(input);
            // 如果匹配失败，返回原始列表
            if (!match.Success)
            {
                return ListRegex;
            }
            // 获取捕获的数字（group[1]）
            string prefix = match.Groups[1].Value;
            // 创建新的正则列表，添加前缀
            List<string> modifiedRegexList = new List<string>();
            foreach (var originalRegex in ListRegex)
            {
                // 构建新的正则模式（添加前缀）
                string newPattern = $@"{prefix}-{originalRegex.ToString()}";
                modifiedRegexList.Add(newPattern);
            }
            return modifiedRegexList;
        }
    }
}
