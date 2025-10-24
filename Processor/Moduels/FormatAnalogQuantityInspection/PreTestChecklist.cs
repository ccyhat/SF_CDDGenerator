using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;
using System.Text.RegularExpressions;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class PreTestChecklist : IPreTestChecklist
    {
        private readonly IUpdateRatedValue _updateRatedValue;
        private readonly IConnectCircuitBreaker _connectCircuitBreaker;
        private readonly ISelectTester _selectTester;
        private readonly static Regex REGEX_ZSD= new Regex(@"^ZSD-5$");
        public PreTestChecklist(
            IUpdateRatedValue updateRatedValue,
            IConnectCircuitBreaker connectCircuitBreaker,
            ISelectTester selectTester

            )
        {
            _updateRatedValue = updateRatedValue;
            _connectCircuitBreaker = connectCircuitBreaker;
            _selectTester = selectTester;
        }

        public async Task PrepareAsync(SDL sdl, Items root, List<string> _nodename)
        {
            var originalItem = root.ItemList.FirstOrDefault(item => item.Name.Equals("测试前准备(示例)"));
            if (originalItem != null)
            {
                var prepare = originalItem.Clone() as Items;
                await _updateRatedValue.UpdateRatedValueAsync(sdl, prepare!);
                await _connectCircuitBreaker.ConnectCircuitBreakerAsync(sdl, prepare!);
                await _selectTester.SelectTesterAsync(sdl, prepare!);
                var zsd = sdl.Cubicle.Devices.Where(d => REGEX_ZSD.IsMatch(d.Model));
                if(zsd.Any())
                {
                    var zsdItem = prepare!.GetSafetys().FirstOrDefault(i => i.Name.Equals("保留"));
                    zsdItem.Name= "电流试验端子回路";
                    var Speaking = $"SpeakString=调试员自行测试电流试验端子回路;ExpectString=是否完成;";
                    zsdItem.DllCall.CData = Speaking;
                   

                }

                prepare!.Name = "测试前准备";
                _nodename.Add("测试前准备");
                root.ItemList.Add(prepare);
            }
        }
    }
}
