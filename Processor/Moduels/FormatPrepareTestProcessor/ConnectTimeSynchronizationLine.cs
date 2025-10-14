using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor;
using System.Text;
using static SFTemplateGenerator.Helper.Constants.CDDRegex;

namespace SFTemplateGenerator.Processor.Moduels.FormatPrepareTestProcessor
{
    public class ConnectTimeSynchronizationLine : IConnectTimeSynchronizationLine
    {
        private readonly List<string> timeSyncTypes = new List<string>
        {
            "GPS",
            "GPSGND",
            "差分B码IN+",
            "差分B码IN-",
            "SYN+",
            "SYN-",
            "RS485A-1A/B+",
            "RS485A-1A/B-",
            "485+",
            "485-",
            "IN1",
            "IN2",
            "RS485A-1A/B+/P+",
            "RS485A-1A/B-/P-",
            "IRIG-B",
            "B+",
            "B-",
            "B码对时+",
            "B码对时-"
        };
        public Task ConnectTimeSynchronizationLineAsync(Device TargetDevice, SDL sdl, Items root)
        {
            var boards = TargetDevice.Boards.Where(B => MANAGEBORAD_REGEX.Any(R=>R.IsMatch(B.Desc)));
            foreach(var board in boards)
            {
                var ports = board.Ports.Where(P => timeSyncTypes.Any(T => T.Equals(P.Desc)));
                if(ports.Count()>=2)
                {
                    var tupleA = FindNearestPort(sdl, TargetDevice, board, ports.FirstOrDefault());
                    var tupleB = FindNearestPort(sdl, TargetDevice, board, ports.LastOrDefault());
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SpeakString=对时接入");
                    sb.Append($"{tupleA.Item1}{tupleA.Item2}和{tupleB.Item2};ExpectString=是否完成;");
                    var template = root.GetSafetys().FirstOrDefault(S => S.Name.StartsWith("接入对时线"));
                    if (template != null)
                    {
                        template.Name = "接入对时线";                    
                        template.DllCall.CData = sb.ToString();                        
                    }
                }
            }                      
            return Task.CompletedTask;
        }
        private Tuple<string, string, string> FindNearestPort(SDL sdl, Device device, Board board, Port port)
        {
            var core = sdl.Cubicle.Cores.FirstOrDefault(C =>
                (C.DeviceA == device.Name && C.BoardA == board.Name && C.PortA == port.Name) ||
                (C.DeviceB == device.Name && C.BoardB == board.Name && C.PortB == port.Name)
            );
            if (core != null)
            {
                var otherDeviceName = core.DeviceA == device.Name ? core.DeviceB : core.DeviceA;
                var otherBoardName = core.BoardA == board.Name ? core.BoardB : core.BoardA;
                var otherPortName = core.PortA == port.Name ? core.PortB : core.PortA;
                return new Tuple<string, string, string>(otherDeviceName, otherBoardName, otherPortName);
            }
            return new Tuple<string, string, string>("", "", "");
        }
    }
}
