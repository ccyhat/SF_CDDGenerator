using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    [XmlRoot("device-model")]
    public class DeviceModel
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("ppengine-progid")] public string PpEngineProgid { get; set; } = string.Empty;
        [XmlAttribute("pp-template")] public string PpTemplate { get; set; } = string.Empty;
        [XmlAttribute("device-model-file")] public string DeviceModelFile { get; set; } = string.Empty;

        [XmlElement("ldevice")]
        public List<LDevice> LDevices { get; set; } = new List<LDevice>();
    }

    public class LDevice
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("index")] public string Index { get; set; } = string.Empty;

        [XmlElement("dataset")]
        public List<DeviceModelDataset> Datasets { get; set; } = new();
    }

    public class DeviceModelDataset
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("rw-attr")] public string RwAttr { get; set; } = string.Empty;

        [XmlElement("data")]
        public List<DeviceModelData> Datas { get; set; } = new();
    }

    public class DeviceModelData
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
        // 核心：原 Position 属性，仅反序列化、不序列化
        [XmlIgnore] // 关键：告诉 XmlSerializer 不序列化该属性
        public string Position { get; set; } = string.Empty;

        // 代理属性：仅用于接收 XML 中的 "position" 属性（反序列化）
        [XmlAttribute("position")] // 与 XML 中的属性名对应
        public string PositionForDeserialization
        {
            get
            {
                // 序列化时：返回 null 或不处理（因 XmlSerializer 序列化时会调用 get 方法，此处返回空不影响，因原属性已被 XmlIgnore）
                return null!;
            }
            set
            {
                // 反序列化时：将 XML 中的 "position" 值赋值给原 Position 属性
                Position = value;
            }
        }

        [XmlElement("value")]
        public List<DataValue> Values { get; set; } = new();
    }

    public class DataValue
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("value")] public string Value { get; set; } = string.Empty;
    }
}
