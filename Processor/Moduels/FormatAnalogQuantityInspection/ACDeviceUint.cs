using SFTemplateGenerator.Helper.Shares.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class ACDeviceUint
    {
        public List<List<Core>> Group1 { get; set; } = new List<List<Core>>(); // Un
        public List<List<Core>> Group2 { get; set; } = new List<List<Core>>(); // Ux
        public List<List<Core>> Group3 { get; set; } = new List<List<Core>>(); // In
        public List<List<Core>> Group4 { get; set; } = new List<List<Core>>(); // I0
        public List<string> KK_BYQ_List1 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List2 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List3 { get; set; } = new List<string>(); // KK_BYQ
        public List<string> KK_BYQ_List4 { get; set; } = new List<string>(); // KK_BYQ
    }
}
