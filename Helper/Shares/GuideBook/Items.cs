using SFTemplateGenerator.Helper.Shares.TreeNode;
using System.Xml.Serialization;
namespace SFTemplateGenerator.Helper.Shares.GuideBook
{
    [XmlInclude(typeof(Safety))]
    [XmlInclude(typeof(MacroTest))]
    [XmlInclude(typeof(CommCMD))]
    [XmlInclude(typeof(Items))]
    // 声明为抽象类（纯虚类）
    public abstract class ItemBase
    {
        [XmlIgnore]
        public int OrderNum { get; set; } // 第一级菜单
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;

        // 通过序列化+反序列化实现深拷贝
        public virtual ItemBase Clone()
        {
            // 创建当前类型的XML序列化器
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            // 使用内存流进行序列化和反序列化
            using (MemoryStream stream = new MemoryStream())
            {
                // 序列化当前对象到内存流
                serializer.Serialize(stream, this);

                // 重置流位置，准备反序列化
                stream.Position = 0;

                // 反序列化为新对象（深拷贝）
                return (ItemBase)serializer.Deserialize(stream);
            }
        }

    }
    public class Items : ItemBase, IDisplayITem
    {

        [XmlAttribute("tkid")] public string TkId { get; set; } = string.Empty;
        [XmlAttribute("show")] public string Show { get; set; } = string.Empty;
        [XmlAttribute("enable")] public string Enable { get; set; } = string.Empty;
        [XmlAttribute("exectype")] public string ExecType { get; set; } = string.Empty;
        [XmlAttribute("batch-item")] public string BatchItem { get; set; } = string.Empty;
        [XmlAttribute("mdv-test-each")] public string MdvTestEach { get; set; } = string.Empty;
        [XmlAttribute("type")] public string Type { get; set; } = string.Empty;
        [XmlAttribute("characteristic-id")] public string CharacteristicId { get; set; } = string.Empty;
        [XmlAttribute("stxml")] public string StXml { get; set; } = string.Empty;
        [XmlAttribute("wzd-map")] public string WzdMap { get; set; } = string.Empty;
        [XmlElement("expr-script")] public ExprScript ExprScript { get; set; } = new();
        [XmlElement("script-init")] public ScriptInit ScriptInit { get; set; } = new();
        [XmlElement("script-name")] public ScriptName ScriptName { get; set; } = new();
        [XmlElement("script-result")] public ScriptResult ScriptResult { get; set; } = new();

        [XmlElement("safety", typeof(Safety))]
        [XmlElement("macrotest", typeof(MacroTest))]
        [XmlElement("items", typeof(Items))]
        [XmlElement("commcmd", typeof(CommCMD))]
        public List<ItemBase> ItemList { get; set; } = new List<ItemBase>();
        [XmlElement("rpt-map")] public RptMap RptMap { get; set; } = new();
        [XmlElement("reports")] public Reports Reports { get; set; } = new();
        public DllCall DllCall
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        public override Items Clone()
        {
            // 创建当前类型的XML序列化器
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            // 使用内存流进行序列化和反序列化
            using (MemoryStream stream = new MemoryStream())
            {
                // 序列化当前对象到内存流
                serializer.Serialize(stream, this);

                // 重置流位置，准备反序列化
                stream.Position = 0;

                // 反序列化为新对象（深拷贝）
                return (Items)serializer.Deserialize(stream);
            }
        }
        public List<Items> GetItems()
        {
            return ItemList.Where(I => I is Items).Select(I => (Items)I).ToList();
        }
        public List<MacroTest> GetMacroTests()
        {
            return ItemList.Where(I => I is MacroTest).Select(I => (MacroTest)I).ToList();
        }
        public List<Safety> GetSafetys()
        {
            return ItemList.Where(I => I is Safety).Select(I => (Safety)I).ToList();
        }
        public List<CommCMD> GetCommCMDs()
        {
            return ItemList.Where(I => I is CommCMD).Select(I => (CommCMD)I).ToList();
        }
        public void SortRecuresely()
        {
            var sortedItems = ItemList.OrderBy(item => item.OrderNum).ToList();
            // 递归排序子项（使用模式匹配简化类型判断和转换）
            foreach (var item in ItemList)
            {
                if (item is Items subItem)
                {
                    subItem.SortRecuresely();
                }
                if (item is MacroTest subItem1)
                {
                    subItem1.SortRecuresely();
                }
            }
            ItemList = sortedItems;
        }
        public void RefreshId()
        {
            var items = GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Id = $"Item{i + 1}";
                items[i].RefreshId();
            }
            var marcroTests = GetMacroTests();
            for (int i = 0; i < marcroTests.Count; i++)
            {
                marcroTests[i].Id = $"Macrotest{i + 1}";
                marcroTests[i].RefreshId();
            }
            var Safetys = GetSafetys();
            for (int i = 0; i < Safetys.Count; i++)
            {
                Safetys[i].Id = $"Safety{i + 1}";

            }
            var commCMDs = GetCommCMDs();
            for (int i = 0; i < commCMDs.Count; i++)
            {
                commCMDs[i].Id = $"CommCmd{i + 1}";
            }
        }
    }

    /* safety 及子节点 */
    public class Safety : ItemBase, IDisplayITem
    {
        [XmlAttribute("tkid")] public string TkId { get; set; } = string.Empty;
        [XmlAttribute("show")] public string Show { get; set; } = string.Empty;
        [XmlAttribute("enable")] public string Enable { get; set; } = string.Empty;
        [XmlAttribute("exectype")] public string ExecType { get; set; } = string.Empty;
        [XmlAttribute("batch-item")] public string BatchItem { get; set; } = string.Empty;
        [XmlAttribute("mdv-test-each")] public string MdvTestEach { get; set; } = string.Empty;
        [XmlAttribute("type")] public string Type { get; set; } = string.Empty;
        [XmlAttribute("sound-file")] public string SoundFile { get; set; } = string.Empty;
        [XmlAttribute("many-rpt-test-mode")] public string ManyRptTestMode { get; set; } = string.Empty;

        [XmlElement("script-init")] public ScriptInit ScriptInit { get; set; } = new();
        [XmlElement("script-name")] public ScriptName ScriptName { get; set; } = new();
        [XmlElement("script-result")] public ScriptResult ScriptResult { get; set; } = new();
        [XmlElement("script")] public Script Script { get; set; } = new();

        [XmlElement("datas")] public Datas Datas { get; set; } = new();
        [XmlElement("standard")] public Standard Standard { get; set; } = new();
        [XmlElement("dllcall")] public DllCall DllCall { get; set; } = new();
        [XmlElement("msg")] public MsgData Msg { get; set; } = null!;
        [XmlElement("rpt-map")] public RptMap RptMap { get; set; } = new();
        public override Safety Clone()
        {
            // 创建当前类型的XML序列化器
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            // 使用内存流进行序列化和反序列化
            using (MemoryStream stream = new MemoryStream())
            {
                // 序列化当前对象到内存流
                serializer.Serialize(stream, this);

                // 重置流位置，准备反序列化
                stream.Position = 0;

                // 反序列化为新对象（深拷贝）
                return (Safety)serializer.Deserialize(stream);
            }
        }

    }
    public class MacroTest : ItemBase, IDisplayITem
    {

        [XmlAttribute("tkid")] public string TkId { get; set; } = string.Empty;
        [XmlAttribute("show")] public string Show { get; set; } = string.Empty;
        [XmlAttribute("enable")] public string Enable { get; set; } = string.Empty;
        [XmlAttribute("exectype")] public string ExecType { get; set; } = string.Empty;
        [XmlAttribute("batch-item")] public string BatchItem { get; set; } = string.Empty;
        [XmlAttribute("mdv-test-each")] public string MdvTestEach { get; set; } = string.Empty;
        [XmlAttribute("type")] public string Type { get; set; } = string.Empty;
        [XmlAttribute("repeat-timers")] public string RepeatTimers { get; set; } = string.Empty;
        [XmlAttribute("cal-mode")] public string CalMode { get; set; } = string.Empty;
        [XmlAttribute("rpt-fill-no-repeat")] public string RptFillNoRepeat { get; set; } = string.Empty;

        [XmlElement("script-init")] public ScriptInit ScriptInit { get; set; } = new();
        [XmlElement("script-name")] public ScriptName ScriptName { get; set; } = new();
        [XmlElement("script-result")] public ScriptResult ScriptResult { get; set; } = new();

        [XmlElement("rpt-map")] public RptMap RptMap { get; set; } = new();

        [XmlElement("para")] public Para Para { get; set; } = new();


        [XmlElement("commcmd", typeof(CommCMD))]
        [XmlElement("safety", typeof(Safety))]
        public List<ItemBase> Safety_CommCMD_List { get; set; } = new List<ItemBase>();
        public DllCall DllCall
        {
            get
            {
                return null;
            }

            set
            {
            }
        }
        public override MacroTest Clone()
        {
            // 创建当前类型的XML序列化器
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            // 使用内存流进行序列化和反序列化
            using (MemoryStream stream = new MemoryStream())
            {
                // 序列化当前对象到内存流
                serializer.Serialize(stream, this);

                // 重置流位置，准备反序列化
                stream.Position = 0;

                // 反序列化为新对象（深拷贝）
                return (MacroTest)serializer.Deserialize(stream);
            }
        }

        public List<Safety> GetSafetys()
        {
            return Safety_CommCMD_List.Where(I => I is Safety).Select(I => (Safety)I).ToList();
        }
        public List<CommCMD> GetCommCMDs()
        {
            return Safety_CommCMD_List.Where(I => I is CommCMD).Select(I => (CommCMD)I).ToList();
        }
        public void SortRecuresely()
        {
            var sortedItems = Safety_CommCMD_List.OrderBy(item => item.OrderNum).ToList();
            Safety_CommCMD_List = sortedItems;
        }
        public void RefreshId()
        {
            var commCMDs = GetCommCMDs();
            for (int i = 0; i < commCMDs.Count; i++)
            {
                commCMDs[i].Id = $"CommCmd{i + 1}";
            }
            var safetys = GetSafetys();
            for (int i = 0; i < safetys.Count; i++)
            {
                safetys[i].Id = $"Safety{i + 1}";
            }
        }
    }
    public class CommCMD : ItemBase, IDisplayITem
    {
        [XmlAttribute("tkid")] public string TkId { get; set; } = string.Empty;
        [XmlAttribute("show")] public string Show { get; set; } = string.Empty;
        [XmlAttribute("enable")] public string Enable { get; set; } = string.Empty;
        [XmlAttribute("exectype")] public string ExecType { get; set; } = string.Empty;
        [XmlAttribute("batch-item")] public string BatchItem { get; set; } = string.Empty;
        [XmlAttribute("mdv-test-each")] public string MdvTestEach { get; set; } = string.Empty;
        [XmlAttribute("type")] public string Type { get; set; } = string.Empty;
        [XmlAttribute("rw-optr")] public string RwOptr { get; set; } = string.Empty;
        [XmlAttribute("sort-soe")] public string SortSoe { get; set; } = string.Empty;
        [XmlAttribute("dsv-run-after-rst")] public string DsvRunAfterRst { get; set; } = string.Empty;
        [XmlAttribute("mgbrpt-cmd-mode")] public string MgbRptCmdMode { get; set; } = string.Empty;
        [XmlElement("script-init")] public ScriptInit ScriptInit { get; set; } = new();
        [XmlElement("script-name")] public ScriptName ScriptName { get; set; } = new();
        [XmlElement("script-result")] public ScriptResult ScriptResult { get; set; } = new();
        [XmlElement("rpt-map")] public RptMap RptMap { get; set; } = new();
        [XmlElement("cmd")] public Cmd CMD { get; set; } = new();
        [XmlElement("datas")] public Datas Datas { get; set; } = new();
        [XmlElement("dsv-script")] public DsvScript DsvScript { get; set; } = new();
        public DllCall DllCall
        {
            get
            {
                return null;
            }
            set
            {

            }
        }
        public override CommCMD Clone()
        {
            // 创建当前类型的XML序列化器
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            // 使用内存流进行序列化和反序列化
            using (MemoryStream stream = new MemoryStream())
            {
                // 序列化当前对象到内存流
                serializer.Serialize(stream, this);

                // 重置流位置，准备反序列化
                stream.Position = 0;

                // 反序列化为新对象（深拷贝）
                return (CommCMD)serializer.Deserialize(stream);
            }
        }
    }


    /* 通用脚本/文本节点 */
    public class ExprScript
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlAttribute("is-ref")]
        public string IsRef { get; set; } = string.Empty;

        [XmlAttribute("macroid")]
        public string Macroid { get; set; } = string.Empty;

        [XmlAttribute("dataset")]
        public string Dataset { get; set; } = string.Empty;

        [XmlAttribute("timegap")]
        public string Timegap { get; set; } = string.Empty;

        [XmlAttribute("retrytimes")]
        public string RetryTimes { get; set; } = string.Empty;

        [XmlAttribute("time-ignore")]
        public string TimeIgnore { get; set; } = string.Empty;

    }


    public class Datas
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;

        [XmlElement("data")]
        public List<Data1> DataList { get; set; } = new List<Data1>();
    }

    public class Data1
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("unit")] public string Unit { get; set; } = string.Empty;
        [XmlAttribute("value")] public string Value { get; set; } = string.Empty;
        [XmlAttribute("format")] public string Format { get; set; } = string.Empty;
        [XmlAttribute("remark")] public string Remark { get; set; } = string.Empty;
        [XmlAttribute("default")] public string Default { get; set; } = string.Empty;
        [XmlAttribute("reserved")] public string Reserved { get; set; } = string.Empty;
        [XmlAttribute("time")] public string Time { get; set; } = string.Empty;
    }

    public class Standard { }

    public class DllCall : CDataXmlBase
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("para-type")] public string ParaType { get; set; } = string.Empty;
        [XmlAttribute("func-name")] public string FuncName { get; set; } = string.Empty;
        [XmlAttribute("result-file")] public string ResultFile { get; set; } = string.Empty;

    }



    public class Reports { }

    public class Para
    {
        [XmlAttribute("macroid")] public string Macroid { get; set; } = string.Empty;
        [XmlAttribute("testmode")] public string Testmode { get; set; } = string.Empty;

        [XmlElement("fparas")] public Fparas Fparas { get; set; } = new();
        [XmlElement("fparas-usr")] public FparasUsr FparasUsr { get; set; } = new();
        [XmlElement("script")] public Script Script { get; set; } = new();
    }

    public class Fparas
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;

        [XmlElement("para")] public List<ParaItem> Paras { get; set; } = new List<ParaItem>();
    }

    public class FparasUsr
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
    }

    public class ParaItem : CDataXmlBase
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;

    }

    public class Cmd
    {

        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("dataset-path")] public string DataSetPath { get; set; } = string.Empty;
        [XmlAttribute("delaybeforecmd")] public string DelayBeforeCmd { get; set; } = string.Empty;
        [XmlAttribute("delayaftercmd")] public string DelayAfterCmd { get; set; } = string.Empty;
        [XmlAttribute("delaybefortry")] public string DelayBeforeTry { get; set; } = string.Empty;
        [XmlAttribute("maxretrytimes")] public string MaxRetryTimes { get; set; } = string.Empty;
        [XmlAttribute("retrytimes")] public string RetryTimes { get; set; } = string.Empty;
        [XmlAttribute("rpt-fill-no-repeat")] public string RptFillNoRepeat { get; set; } = string.Empty;
        [XmlAttribute("cal-mode")] public string CalMode { get; set; } = string.Empty;
        [XmlAttribute("timelong")] public string TimeLong { get; set; } = string.Empty;
        [XmlAttribute("timegap")] public string TimeGap { get; set; } = string.Empty;
        [XmlAttribute("usecurrsetdata")] public string UseCurrSetData { get; set; } = string.Empty;
        [XmlAttribute("usedeviceex")] public string UseDeviceEx { get; set; } = string.Empty;
        [XmlAttribute("begin-mode")] public string BeginMode { get; set; } = string.Empty;
        [XmlAttribute("use-connect")] public string UseConnect { get; set; } = string.Empty;
        [XmlElement("value")] public List<CMDValue> Value { get; set; } = null!;
    }
    public class DsvScript
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("minocc")] public string MinOcc { get; set; } = string.Empty;
        [XmlAttribute("maxocc")] public string MaxOcc { get; set; } = string.Empty;
        [XmlAttribute("in-dataset")] public string InDataSet { get; set; } = string.Empty;
        [XmlAttribute("type")] public string Type { get; set; } = string.Empty;

        [XmlElement("eliminate")] public Eliminate Eliminate { get; set; } = new();
        [XmlElement("element")] public List<Element> Elements { get; set; } = new();
    }
    public class Eliminate
    {
        [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")] public string Id { get; set; } = string.Empty;
        [XmlAttribute("data-type")] public string DataType { get; set; } = string.Empty;
        [XmlAttribute("write-mode")] public string WriteMode { get; set; } = string.Empty;
        [XmlAttribute("index")] public string Index { get; set; } = string.Empty;
        [XmlElement("data")] public EliminateData Data { get; set; } = null!;
    }
    public class EliminateData
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlAttribute("data-type")]
        public string DataType { get; set; } = string.Empty;

        [XmlAttribute("value")]
        public string Value { get; set; } = string.Empty;

        [XmlAttribute("unit")]
        public string Unit { get; set; } = string.Empty;

        [XmlAttribute("min")]
        public string Min { get; set; } = string.Empty;

        [XmlAttribute("max")]
        public string Max { get; set; } = string.Empty;

        [XmlAttribute("format")]
        public string Format { get; set; } = string.Empty;

        [XmlAttribute("index")]
        public string Index { get; set; } = string.Empty;

        [XmlAttribute("time")]
        public string Time { get; set; } = string.Empty;

        [XmlAttribute("change")]
        public string Change { get; set; } = string.Empty;

        [XmlAttribute("step")]
        public string Step { get; set; } = string.Empty;
    }

    public class Element
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;


        [XmlAttribute("minocc")]
        public string MinOcc { get; set; } = string.Empty;


        [XmlAttribute("maxocc")]
        public int MaxOcc { get; set; } = 0;

        [XmlAttribute("mode")]
        public string Mode { get; set; } = string.Empty;

        [XmlElement("attr")]
        public List<Attr> Attrs { get; set; } = new List<Attr>();
    }

    public class Attr
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlAttribute("optr")]
        public string Optr { get; set; } = string.Empty;


        [XmlAttribute("value")]
        public string Value { get; set; } = string.Empty;


        [XmlAttribute("value2")]
        public string Value2 { get; set; } = string.Empty;

        [XmlAttribute("data-type")]
        public string DataType { get; set; } = string.Empty;

        [XmlAttribute("variable")]
        public string Variable { get; set; } = string.Empty;

    }
    public class CMDValue
    {


        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;
        [XmlAttribute("value")]
        public string Value { get; set; } = string.Empty;
    }
    public class MsgData : CDataXmlBase
    {
        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;
    }
}
