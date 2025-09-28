using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    public class ExprScriptMngr
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
    }
}
