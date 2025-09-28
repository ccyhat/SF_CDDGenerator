using System.Xml.Serialization;
namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    [XmlRoot("sys-paras")]
    public class SysParas
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("rw-attr")] public string RWAttr { get; set; } = string.Empty;
        [XmlElement("data")]
        public List<Data> Datas { get; set; } = new List<Data>();
    }

    public class Data
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("unit")] public string Unit { get; set; } = string.Empty;
        [XmlAttribute("value")] public string Value { get; set; } = string.Empty;
        [XmlAttribute("default-value")] public string DefaultValue { get; set; } = string.Empty;
        [XmlAttribute("min")] public string Min { get; set; } = string.Empty;
        [XmlAttribute("max")] public string Max { get; set; } = string.Empty;
        [XmlAttribute("data-index")] public string DataIndex { get; set; } = string.Empty;
        [XmlAttribute("step")] public string Step { get; set; } = string.Empty;
    }
}
