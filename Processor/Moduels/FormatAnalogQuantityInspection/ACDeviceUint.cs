using SFTemplateGenerator.Helper.Shares.SDL;
using System.Text.RegularExpressions;
namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class ACDeviceUint
    {
        public List<List<Core>> Group1 { get; set; } = new List<List<Core>>(); // Un+Ubph
        public List<List<Core>> Group2 { get; set; } = new List<List<Core>>(); // Ux+Us电源电压
        public List<List<Core>> Group3 { get; set; } = new List<List<Core>>(); // In
        public List<List<Core>> Group4 { get; set; } = new List<List<Core>>(); // I0+Is电源电流
        public List<List<Core>> Group5 { get; set; } = new List<List<Core>>(); // IJ
        public List<List<Core>> Group6 { get; set; } = new List<List<Core>>(); // Ibph
        public List<List<Core>> Group7 { get; set; } = new List<List<Core>>(); // IL
        public List<List<Core>> Group8 { get; set; } = new List<List<Core>>(); // IH
        public List<string> KK_BYQ_List1 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List2 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List3 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List4 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List5 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List6 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List7 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List8 { get; set; } = new List<string>(); // KK_BYQ
        public void Add(ACDeviceUint obj)
        {
            this.Group1.AddRange(obj.Group1);
            this.Group2.AddRange(obj.Group2);
            this.Group3.AddRange(obj.Group3);
            this.Group4.AddRange(obj.Group4);
            this.Group5.AddRange(obj.Group5);
            this.Group6.AddRange(obj.Group6);
            this.Group6.AddRange(obj.Group7);
            this.Group6.AddRange(obj.Group8);
            this.KK_BYQ_List1.AddRange(obj.KK_BYQ_List1);
            this.KK_BYQ_List2.AddRange(obj.KK_BYQ_List2);
            this.KK_BYQ_List3.AddRange(obj.KK_BYQ_List3);
            this.KK_BYQ_List4.AddRange(obj.KK_BYQ_List4);
            this.KK_BYQ_List5.AddRange(obj.KK_BYQ_List5);
            this.KK_BYQ_List6.AddRange(obj.KK_BYQ_List6);
            this.KK_BYQ_List6.AddRange(obj.KK_BYQ_List7);
            this.KK_BYQ_List6.AddRange(obj.KK_BYQ_List8);
        }
        public List<string> GetSeperateKK()
        {
            Regex regex = new Regex(@".*ZKK.*(a|b|c)", RegexOptions.IgnoreCase);
            return KK_BYQ_List1.Where(KK => regex.IsMatch(KK)).ToList();
        }

        public List<string> IPortName { get; set; } = new List<string>();//存储端口号
        public List<string> UPortName { get; set; } = new List<string>();
    };
    public class ACPortsGroup//可能是3项+n，也可能是2项
    {
      
        List<ACPortsUint> ACPortsUintList = new List<ACPortsUint>();
    };
    public class ACPortsUint//可能是3项+n，也可能是2项
    {
        string UintName;
        int Count=0;
        List<ACPorts> ACPortsList = new List<ACPorts>();
        public ACPortsUint(string UintName)
        {
            this.UintName = UintName;
        }
        public void Add(ACPorts obj)
        {
            this.ACPortsList.Add(obj);
            Count++;
        }
    };
    public class ACPorts
    {
        public string StartPort;
        public List<string> GroupName;
        public List<Core> Line { get; set; } = new List<Core>(); // 存储分组数据
        public ACPorts(List<Core> Line)
        {
            this.Line=Line;
        }


    }
    
}
