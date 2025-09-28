using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Shares.DIO
{
    [XmlRoot("PortDefine")]
    public class PortDefine
    {
        [XmlElement("Port")]
        public List<DIOPort> Ports = new List<DIOPort>();

        // 生成带类型前缀的集合标识（DO_1, DI_1等）
        public void GenerateTypedCollectionIds()
        {
            // 1. 按类型分组（DO和DI）
            var doPorts = Ports.Where(p => p.PortType?.Contains("DO", StringComparison.OrdinalIgnoreCase) == true).ToList();//会有tripDO
            var diPorts = Ports.Where(p => p.PortType?.Equals("DI", StringComparison.OrdinalIgnoreCase) == true).ToList();

            // 2. 为DO类型生成集合标识
            var doCollections = IdentifyCollections(doPorts);
            AssignCollectionIds(doCollections);

            // 3. 为DI类型生成集合标识
            var diCollections = IdentifyCollections(diPorts);
            AssignCollectionIds(diCollections);
        }
        // 识别同一类型内的关联集合（基于port与match的引用关系）
        //<Port board = "14" port="a2" type="DO" match="a4,a6"/>
        //<Port board = "14" port="a4" type="DO" match="a2"/>
        //<Port board = "14" port="a6" type="DO" match="a2"/>
        //==========================>>>>>>>>
        //board = "14" port="a2"=> board = "14" port="a4"
        //board = "14" port="a2"=> board = "14" port="a6"
        private List<List<DIOPort>> IdentifyCollections(List<DIOPort> portsOfType)
        {
            // 筛选出match属性不包含逗号的端口（简单匹配关系）
            var filteredPorts = portsOfType.Where(port => !port.match.Contains(",")).ToList();
            List<DIOPort> newPorts = new List<DIOPort>();

            // 遍历筛选后的端口副本，移除满足特定条件的端口
            foreach (var currentPort in filteredPorts.ToList())
            {
                // 检查是否存在同board下port与当前端口的match匹配的端口
                if (filteredPorts.Any(otherPort =>
                    otherPort.port == currentPort.match &&
                    otherPort.board == currentPort.board))
                {
                    filteredPorts.Remove(currentPort);
                    newPorts.Add(currentPort);
                }
            }

            for (int i = 0; i < filteredPorts.Count; i++)
            {
                var port = newPorts.FirstOrDefault(P => P.board == filteredPorts[i].board && P.match == filteredPorts[i].port && P.port == filteredPorts[i].match);
                if (port != null)
                {
                    filteredPorts[i] = port;
                }

            }

            // 生成最终的端口集合列表
            var finalPortCollections = new List<List<DIOPort>>();
            foreach (var filteredPort in filteredPorts)
            {
                var portSet = new List<DIOPort>();
                foreach (var port in portsOfType)
                {
                    // 匹配同board且port相等的端口
                    if (filteredPort.board == port.board && port.port == filteredPort.port)
                    {
                        portSet.Add(port);
                    }
                    var matchs = port.match.Split(',');
                    // 匹配同board且match包含当前port的端口
                    if (filteredPort.board == port.board && matchs.Any(M => M.Equals(filteredPort.port)))
                    {
                        portSet.Add(port);
                    }
                }
                finalPortCollections.Add(portSet);
            }

            return finalPortCollections;
        }


        // 为集合分配带类型前缀的标识（如DO_1, DI_2）
        private void AssignCollectionIds(List<List<DIOPort>> collections)
        {
            for (int i = 0; i < collections.Count; i++)
            {
                var count = 0;
                foreach (var port in collections[i])
                {
                    port.CollectionId.Add($"{port.PortType}_{i + 1}_{count}");
                    count++;
                }
            }
        }
    }
    public class DIOPort
    {
        [XmlAttribute("board")]
        public string board { get; set; }
        [XmlAttribute("port")]
        public string port { get; set; }
        [XmlAttribute("type")]
        public string PortType { get; set; }
        [XmlAttribute("match")]
        public string match { get; set; }
        // 新增：集合标识（格式如DO_1, DO_2），同一集合内的port和match共享此标识
        [XmlIgnore] // 不序列化到XML
        public List<string> CollectionId { get; set; } = new List<string>();
    }

}
