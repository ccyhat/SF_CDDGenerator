

namespace SFTemplateGenerator.Helper.UtilityTools
{
    public class DeviceModelCache
    {
        private Dictionary<string, string> _dicdevicesDes = new Dictionary<string, string>();
        private Dictionary<string, string> _dicdevicesId = new Dictionary<string, string>();
        public void CreateDes(string positions, string descName)
        {
            if (!string.IsNullOrEmpty(positions))
            {
                foreach (var position in positions.Split(';'))
                {
                    _dicdevicesDes[position] = descName;
                }
            }
        }
        public void CreateId(string positions, string descName, string Id)
        {
            if (!string.IsNullOrEmpty(positions))
            {
                foreach (var position in positions.Split(';'))
                {
                    _dicdevicesId[position + "-" + descName] = Id;
                }
            }
        }
        public bool ContainsDescName(string boardName, string portName)
        {
            return _dicdevicesDes.ContainsKey(boardName + "-" + portName);
        }
        public string this[string boardName, string portName, string desc]
        {
            get
            {
                if (ContainsDescName(boardName, portName))
                {
                    return _dicdevicesDes[boardName + "-" + portName];
                }
                Logger.Logger.Warning($"_dicdevicesDes字典未找到{boardName}{portName}对应的值");
                return desc;
            }
        }
        public bool IsMatch(string boardName, string portName)
        {
            var key = boardName + "-" + portName;
            if (_dicdevicesId.Keys.Any(K => K.StartsWith(key)))
            {
                var realKey = _dicdevicesId.Keys.Where(K => K.StartsWith(key)).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(realKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public string GetDataSetId(string boardName, string portName, string desc)
        {
            //// 2025年1月3日，因为deviceModel中的Name与cdd中Port的desc不对应导致不能正确判断dsAin而做的修改。我觉得这个本身就是一个风险。    许阳
            var key = boardName + "-" + portName;
            if (_dicdevicesId.Keys.Any(K => K.StartsWith(key)))
            {
                var realKey = _dicdevicesId.Keys.Where(K => K.StartsWith(key)).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(realKey))
                {
                    return _dicdevicesId[realKey];
                }
                else
                {
                    return "";
                }
            }
            Logger.Logger.Warning($"GetDataSetId未找到{boardName}{portName}");
            return "";
        }
        public bool Has3I0()
        {
            bool hasMatchingKey = _dicdevicesDes.Keys.Any(key => key.StartsWith("3I0"));
            return hasMatchingKey;
        }
        public bool Has3U0()
        {
            bool hasMatchingKey = _dicdevicesDes.Keys.Any(key => key.StartsWith("3U0"));
            return hasMatchingKey;
        }
        public string Get3I0(string str_3I0)
        {
            if (_dicdevicesDes.TryGetValue(str_3I0, out string value1))
            {
                return value1;
            }
            return _dicdevicesDes["3I0"];
        }
        public string Get3U0(string str_3U0)
        {
            if (_dicdevicesDes.TryGetValue(str_3U0, out string value1))
            {
                return value1;
            }
            return _dicdevicesDes["3U0"];
        }
    }
}
