using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Config
{
    [XmlRoot("DeviceType")]
    public class DeviceType
    {

        [XmlElement("Model")]
        public List<Model> Models { get; set; } = new List<Model>();
    }


    public class Model
    {

        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;

        [XmlAttribute("portfile")]
        public string Portfile { get; set; } = string.Empty;
    }

}
