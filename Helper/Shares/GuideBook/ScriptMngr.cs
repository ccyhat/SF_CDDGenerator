using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    public class ScriptMngr
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;

        [XmlElement("script")]
        public List<Script> Scripts { get; set; } = new();
    }

    public class Script : CDataXmlBase
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("is-ref")] public string IsRef { get; set; } = string.Empty;


    }
}
