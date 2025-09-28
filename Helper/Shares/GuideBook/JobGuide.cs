using System.Xml.Serialization;
namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    public class JobGuide
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;

        [XmlElement("script-init")]
        public ScriptInit ScriptInit { get; set; } = new();

        [XmlElement("script-name")]
        public ScriptName ScriptName { get; set; } = new();

        [XmlElement("script-result")]
        public ScriptResult ScriptResult { get; set; } = new();

        [XmlElement("rpt-map")]
        public RptMap RptMap { get; set; } = new();
    }

}

