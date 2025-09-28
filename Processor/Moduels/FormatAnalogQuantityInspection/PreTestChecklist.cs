using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class PreTestChecklist : IPreTestChecklist
    {
        private readonly IUpdateRatedValue _updateRatedValue;
        private readonly IConnectCircuitBreaker _connectCircuitBreaker;
        private readonly ISelectTester _selectTester;
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
                prepare!.Name = "测试前准备";
                _nodename.Add("测试前准备");
                root.ItemList.Add(prepare);
            }
        }
    }
}
