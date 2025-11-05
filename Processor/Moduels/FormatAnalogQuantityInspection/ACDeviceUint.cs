using SFTemplateGenerator.Helper.Shares.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class ACDeviceUint
    {
        public List<List<Core>> Group1 { get; set; } = new List<List<Core>>(); // Un
        public List<List<Core>> Group2 { get; set; } = new List<List<Core>>(); // Ux
        public List<List<Core>> Group3 { get; set; } = new List<List<Core>>(); // In
        public List<List<Core>> Group4 { get; set; } = new List<List<Core>>(); // I0
        public List<List<Core>> Group5 { get; set; } = new List<List<Core>>(); // IJ
        public List<string> KK_BYQ_List1 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List2 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List3 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List4 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List5 { get; set; } = new List<string>(); // KK_BYQ
        public void Add(ACDeviceUint obj)
        {
            this.Group1.AddRange(obj.Group1);
            this.Group2.AddRange(obj.Group2);
            this.Group3.AddRange(obj.Group3);
            this.Group4.AddRange(obj.Group4);
            this.Group5.AddRange(obj.Group5);
            this.KK_BYQ_List1.AddRange(obj.KK_BYQ_List1);
            this.KK_BYQ_List2.AddRange(obj.KK_BYQ_List2);
            this.KK_BYQ_List3.AddRange(obj.KK_BYQ_List3);
            this.KK_BYQ_List4.AddRange(obj.KK_BYQ_List4);
            this.KK_BYQ_List5.AddRange(obj.KK_BYQ_List5);
        }
        public List<string> GetSeperateKK()
        {
            Regex regex = new Regex(@".*ZKK.*(a|b|c)");
            return KK_BYQ_List1.Where(KK => regex.IsMatch(KK)).ToList();           
        }
    };
}
