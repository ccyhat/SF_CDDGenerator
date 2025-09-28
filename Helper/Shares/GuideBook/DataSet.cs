using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    public class GuideBookDataSet
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("write-mode")] public string WriteMode { get; set; } = string.Empty;
        [XmlAttribute("index")] public string Index { get; set; } = string.Empty;

        [XmlElement("data")]
        public List<GuideBookData> Datas { get; set; } = new();
    }

    public class GuideBookData
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("value")] public string Value { get; set; } = string.Empty;
        [XmlAttribute("unit")] public string Unit { get; set; } = string.Empty;
        [XmlAttribute("min")] public string Min { get; set; } = string.Empty;
        [XmlAttribute("max")] public string Max { get; set; } = string.Empty;
        [XmlAttribute("format")] public string Format { get; set; } = string.Empty;
        [XmlAttribute("index")] public string Index { get; set; } = string.Empty;
        [XmlAttribute("time")] public string Time { get; set; } = string.Empty;
        [XmlAttribute("change")] public string Change { get; set; } = string.Empty;
        [XmlAttribute("step")] public string Step { get; set; } = string.Empty;

        [XmlElement("value")]
        public List<Value> Values { get; set; } = new();
    }

    public class Value
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("value")] public string Val { get; set; } = string.Empty;
    }
}
