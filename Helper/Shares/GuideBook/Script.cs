using System.Xml;
using System.Xml.Serialization;
namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    public class ScriptInit : CDataXmlBase
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("is-ref")] public string IsRef { get; set; } = string.Empty;


    }

    public class ScriptName : CDataXmlBase
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("is-ref")] public string IsRef { get; set; } = string.Empty;

    }

    public class ScriptResult : CDataXmlBase
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("is-ref")] public string IsRef { get; set; } = string.Empty;


    }

    public class RptMap
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlElement("data")]
        public List<RptMapData> RptMapDatas { get; set; } = new();
        [XmlElement("Area")]
        public RptMapArea Area { get; set; } = null!;
    }
    public class RptMapData
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;
        [XmlAttribute("precision")]
        public string Precision { get; set; } = string.Empty;
        [XmlElement("bkmk")]
        public List<BkmkElement> Bookmarks { get; set; } = new List<BkmkElement>();
    }
    public class RptMapArea
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;

        [XmlElement("bkmk")]
        public List<BkmkElement> Bkmks { get; set; } = new();
    }
    public class BkmkElement
    {
        [XmlAttribute("attr-id")]
        public string AttrId { get; set; } = string.Empty;

        [XmlAttribute("process")]
        public string Process { get; set; } = string.Empty;

        [XmlAttribute("fill-mode")]
        public string FillMode { get; set; } = string.Empty;

        [XmlText]
        public string Value { get; set; } = string.Empty;
    }
}
