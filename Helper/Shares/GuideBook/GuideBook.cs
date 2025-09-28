using System.Xml.Serialization;
namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    [XmlRoot("guidebook")]
    public class GuideBook
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlAttribute("test-type-file")]
        public string TestTypeFile { get; set; } = string.Empty;

        [XmlAttribute("macro-file")]
        public string MacroFile { get; set; } = string.Empty;

        [XmlAttribute("device-id")]
        public string DeviceId { get; set; } = string.Empty;

        [XmlAttribute("dev-by")]
        public string DevBy { get; set; } = string.Empty;

        [XmlAttribute("version")]
        public string Version { get; set; } = string.Empty;

        [XmlAttribute("comm-cmd-config")]
        public string CommCmdConfig { get; set; } = string.Empty;

        [XmlAttribute("pp-template")]
        public string PpTemplate { get; set; } = string.Empty;

        [XmlAttribute("script-library")]
        public string ScriptLibrary { get; set; } = string.Empty;

        [XmlAttribute("ppengine-progid")]
        public string PpEngineProgId { get; set; } = string.Empty;

        [XmlAttribute("dvm-file")]
        public string DvmFile { get; set; } = string.Empty;

        [XmlAttribute("stand-file")]
        public string StandFile { get; set; } = string.Empty;

        [XmlAttribute("device-model-file")]
        public string DeviceModelFile { get; set; } = string.Empty;

        [XmlAttribute("test-control-mode")]
        public string TestControlMode { get; set; } = string.Empty;

        [XmlAttribute("expand-config-file")]
        public string ExpandConfigFile { get; set; } = string.Empty;

        [XmlAttribute("iecfg-file")]
        public string IecfgFile { get; set; } = string.Empty;

        [XmlAttribute("read-only")]
        public string ReadOnly { get; set; } = string.Empty;

        [XmlAttribute("save-rpt-to-db")]
        public string SaveRptToDb { get; set; } = string.Empty;
        [XmlElement("dataset")]
        public List<GuideBookDataSet> Datasets { get; set; } = new();
        [XmlElement("job-guide")]
        public JobGuide JobGuides { get; set; } = new();
        [XmlElement("expr-script-mngr")]
        public ExprScriptMngr ExprScriptMNGR { get; set; } = new();
        [XmlElement("scriptmngr")]
        public ScriptMngr ScriptMngr { get; set; } = new();
        [XmlElement("script-init")]
        public ScriptInit ScriptInit { get; set; } = new();
        [XmlElement("script-name")]
        public ScriptName ScriptName { get; set; } = new();
        [XmlElement("script-result")]
        public ScriptResult ScriptResult { get; set; } = new();
        [XmlElement("rpt-map")]
        public RptMap RptMap { get; set; }
        [XmlElement("device")]
        public TestDevice Device { get; set; } = new();
    }
}
