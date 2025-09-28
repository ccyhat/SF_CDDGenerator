using SFTemplateGenerator.Helper.Shares.TreeNode;
using System.Xml.Serialization;
namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    public class TestDevice : IDisplayITem
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("factory")] public string Factory { get; set; } = string.Empty;
        [XmlAttribute("type")] public string Type { get; set; } = string.Empty;
        [XmlAttribute("is-time-sets-unit-s")] public string IsTimeSetsUnitS { get; set; } = string.Empty;

        [XmlAttribute("src-device-model-file")]
        public string SrcDeviceModelFile { get; set; } = string.Empty;

        [XmlAttribute("dsv-text-with-value")]
        public string DsvTextWithValue { get; set; } = string.Empty;

        [XmlAttribute("dsv-text-with-utctime")]
        public string DsvTextWithUtcTime { get; set; } = string.Empty;
        [XmlElement("sys-paras")]
        public SysParas SysParas { get; set; } = new();
        [XmlElement("device-model")]
        public DeviceModel DeviceModel { get; set; } = new();
        [XmlElement("characteristics")]
        public Characteristics Characteristics { get; set; } = new();
        [XmlElement("script-init")]
        public ScriptInit ScriptInit { get; set; } = new();

        [XmlElement("script-name")]
        public ScriptName ScriptName { get; set; } = new();

        [XmlElement("script-result")]
        public ScriptResult ScriptResult { get; set; } = new();

        [XmlElement("rpt-map")]
        public RptMap RptMap { get; set; } = new();
        [XmlElement("items")]
        public List<Items> Items { get; set; } = new();

        public void SortRecuresely()
        {
            var sortedItems = Items.OrderBy(item => item.OrderNum).ToList();
            foreach (var item in sortedItems)
            {
                item.SortRecuresely();
            }
            Items = sortedItems;
        }
        public void RefreshId()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Id = $"Items{i + 1}";
                Items[i].RefreshId();
            }
        }

        public DllCall DllCall
        {
            get
            {
                return null!;
            }

            set
            {
            }
        }
    }
    public class Characteristics
    {
        // 没有任何属性或子元素
    }
}
