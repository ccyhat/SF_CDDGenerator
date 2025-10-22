namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    internal class BreakerMatcher
    {
        public static readonly Dictionary<string, string> notShortCircuited = new Dictionary<string, string>
    {
        // 可以在这里初始化字典内容
        {"HQa", "A相合闸"},        
        {"HQb", "B相合闸"},
        {"HQc", "C相合闸"},
        {"TQa", "A项跳闸"},
        {"TQb", "B项跳闸"},
        {"TQc", "C项跳闸"},

    };
        public static readonly Dictionary<string, string> isShortCircuited = new Dictionary<string, string>
    {
        // 可以在这里初始化字典内容

        {"TQa", "A项跳闸"},
        {"TQb", "B项跳闸"},
        {"TQc", "C项跳闸"},
        {"1TQa", "第1组A项跳闸"},
        {"1TQb", "第1组B项跳闸"},
        {"1TQc", "第1组C项跳闸"},
        {"2TQa", "第2组A项跳闸"},
        {"2TQb", "第2组B项跳闸"},
        {"2TQc", "第2组C项跳闸"},
    };
        public static string GetShortCircuitedDesc(string key)
        {
            return isShortCircuited.TryGetValue(key, out var val) ? val : key;
        }

        public static string GetNotShortCircuitedDesc(string key)
        {
            return notShortCircuited.TryGetValue(key, out var val) ? val : key;
        }
    }
}
