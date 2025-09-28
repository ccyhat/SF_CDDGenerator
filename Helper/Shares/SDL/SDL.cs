using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.Shares.SDL
{
    [XmlRoot("SDL", Namespace = "http://www.csg.cn/2023/SDL")]
    public class SDL
    {

        [XmlElement("Header")]
        public Header Header { get; set; } = new Header();

        [XmlElement("Cubicle")]
        public Cubicle Cubicle { get; set; } = new Cubicle();
    }
    // Header元素对应的类
    public class Header
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("revision")]
        public string Revision { get; set; }

        [XmlAttribute("toolID")]
        public string ToolId { get; set; }

        [XmlAttribute("fileCRC")]
        public string FileCrc { get; set; }

        [XmlElement("History")]
        public History History { get; set; } = new History();
    }
    // History元素对应的类
    public class History
    {
        [XmlElement("Hitem")]
        public List<HItem> Items { get; set; } = new List<HItem>();
    }
    // Hitem元素对应的类
    public class HItem
    {
        [XmlAttribute("revision")]
        public string Revision { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("when")]
        public string When { get; set; }

        [XmlAttribute("who")]
        public string Who { get; set; }

        [XmlAttribute("what")]
        public string What { get; set; }

        [XmlAttribute("why")]
        public string Why { get; set; }

        [XmlAttribute("checker")]
        public string Checker { get; set; }

        [XmlAttribute("reviewer")]
        public string Reviewer { get; set; }

        [XmlAttribute("approver")]
        public string Approver { get; set; }
    }
    // Cubicle元素对应的类
    public class Cubicle
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("desc")]
        public string Desc { get; set; }

        [XmlAttribute("inst")]
        public string Inst { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("style")]
        public string Style { get; set; }

        [XmlAttribute("configVersion")]
        public string ConfigVersion { get; set; }

        [XmlAttribute("designer")]
        public string Designer { get; set; }

        [XmlAttribute("graphId")]
        public string GraphId { get; set; }

        [XmlAttribute("height")]
        public string Height { get; set; }

        [XmlAttribute("width")]
        public string Width { get; set; }

        [XmlAttribute("depth")]
        public string Depth { get; set; }

        [XmlAttribute("cabinetsNumber")]
        public string CabinetsNumber { get; set; }

        [XmlAttribute("cabinetBodyNature")]
        public string CabinetBodyNature { get; set; }

        [XmlAttribute("cabinetPanelBelong")]
        public string CabinetPanelBelong { get; set; }

        [XmlAttribute("DCPowerSupplyVoltage")]
        public string DcPowerSupplyVoltage { get; set; }

        [XmlAttribute("tripCurrent")]
        public string TripCurrent { get; set; }

        [XmlAttribute("ACPowerSupplyVoltage")]
        public string AcPowerSupplyVoltage { get; set; }

        [XmlAttribute("closingCurrent")]
        public string ClosingCurrent { get; set; }

        [XmlAttribute("alternatingCurrent")]
        public string AlternatingCurrent { get; set; }

        [XmlAttribute("protocol")]
        public string Protocol { get; set; }

        [XmlAttribute("DescriptionCustomizedFunctions")]
        public string DescriptionCustomizedFunctions { get; set; }

        [XmlAttribute("cabinetMode")]
        public string CabinetMode { get; set; }

        [XmlAttribute("cabineColor")]
        public string CabineColor { get; set; }

        [XmlAttribute("cabinetsManufacturer")]
        public string CabinetsManufacturer { get; set; }

        [XmlAttribute("cabinetDoorStructure")]
        public string CabinetDoorStructure { get; set; }

        [XmlAttribute("faceDirectionDoorHinge")]
        public string FaceDirectionDoorHinge { get; set; }

        [XmlAttribute("installationMethodBusbars")]
        public string InstallationMethodBusbars { get; set; }

        [XmlAttribute("installDustProof")]
        public string InstallDustProof { get; set; }

        [XmlAttribute("installRequirementsGrounding")]
        public string InstallRequirementsGrounding { get; set; }

        [XmlAttribute("cabinetsMaterialCode")]
        public string CabinetsMaterialCode { get; set; }

        [XmlAttribute("powerGridCodingPlate")]
        public string PowerGridCodingPlate { get; set; }

        [XmlAttribute("NstandardKKScoding")]
        public string NstandardKKScoding { get; set; }

        [XmlAttribute("PreviousContractNumber")]
        public string PreviousContractNumber { get; set; }

        [XmlAttribute("KKSCodingContent")]
        public string KKSCodingContent { get; set; }

        [XmlAttribute("StandardDrawingNumber")]
        public string StandardDrawingNumber { get; set; }

        [XmlAttribute("StandardDrawingVersion")]
        public string StandardDrawingVersion { get; set; }

        [XmlElement("Device")]
        public List<Device> Devices { get; set; } = new List<Device>();
        [XmlElement("Core")]
        public List<Core> Cores { get; set; } = new List<Core>();
    }

    // Device元素对应的类
    public class Device
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("desc")]
        public string Desc { get; set; }

        [XmlAttribute("iedName")]
        public string IedName { get; set; }

        [XmlAttribute("model")]
        public string Model { get; set; }

        [XmlAttribute("vendor")]
        public string Vendor { get; set; }

        [XmlAttribute("catalog")]
        public string Catalog { get; set; }

        [XmlAttribute("configVersion")]
        public string ConfigVersion { get; set; }

        [XmlAttribute("graphId")]
        public string GraphId { get; set; }

        [XmlAttribute("height")]
        public string Height { get; set; }

        [XmlAttribute("width")]
        public string Width { get; set; }

        [XmlAttribute("depth")]
        public string Depth { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlElement("Board")]
        public List<Board> Boards { get; set; } = null!;

        [XmlElement("EAI")]
        public List<EAI> EAIs { get; set; } = new List<EAI>();
        [XmlElement("Port")]
        public List<Port> Ports { get; set; } = null!;
    }
    public class Board
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("desc")]
        public string Desc { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("graphId")]
        public string GraphId { get; set; }

        /// <summary>
        /// 包含多个<Port>子元素
        /// </summary>
        [XmlElement("Port")]
        public List<Port> Ports { get; set; } = new List<Port>();
    }
    public class Port
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("desc")]
        public string Desc { get; set; }

        [XmlAttribute("icdRef")]
        public string IcdRef { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlAttribute("graphId")]
        public string GraphId { get; set; }

        [XmlAttribute("side")]
        public string Side { get; set; }

        [XmlAttribute("netport")]
        public string Netport { get; set; }

        [XmlElement("PAI")]
        public List<PAI> PAIs { get; set; } = new List<PAI>();
        [XmlIgnore]
        public List<string> PortPair { get; set; } = new List<string>();
    }
    public class PAI
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("desc")]
        public string Desc { get; set; }

        [XmlElement("Val")]
        public string Val { get; set; }

    }

    // Core元素对应的类
    public class Core
    {
        [XmlAttribute("boardA")]
        public string BoardA { get; set; }

        [XmlAttribute("boardB")]
        public string BoardB { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlAttribute("color")]
        public string Color { get; set; }

        [XmlAttribute("crossSection")]
        public string CrossSection { get; set; }

        [XmlAttribute("desc")]
        public string Desc { get; set; }

        [XmlAttribute("enddesc")]
        public string EndDesc { get; set; }

        [XmlAttribute("startdesc")]
        public string StartDesc { get; set; }

        [XmlAttribute("designation")]
        public string Designation { get; set; }

        [XmlAttribute("deviceA")]
        public string DeviceA { get; set; }

        [XmlAttribute("deviceB")]
        public string DeviceB { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("portA")]
        public string PortA { get; set; }

        [XmlAttribute("portB")]
        public string PortB { get; set; }
    }

    // EAI元素对应的类
    public class EAI
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("desc")]
        public string Desc { get; set; }

        [XmlElement("Val")]
        public string Val { get; set; }
    }


}
