using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Config
{
    // 完善DeviceTemplate类以匹配XML结构
    public class DeviceTemplate
    {
        [XmlAttribute("dvinfcode")]
        public string Dvinfcode { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("template")]
        public string Template { get; set; }

        [XmlAttribute("device-type")]
        public string DeviceType { get; set; }

        [XmlAttribute("dvmfile")]
        public string Dvmfile { get; set; }

        [XmlAttribute("unnormal")]
        public string Unnormal { get; set; }
    }

    // 已完善的Local_DB类
    [XmlRoot("sf-template-local-db", Namespace = null)]
    public class Local_DB
    {
        [XmlAttribute("is-ref")]
        public string IsRef { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("device-template")]
        public List<DeviceTemplate> DeviceTemplates { get; set; } = new List<DeviceTemplate>();
    }
}
