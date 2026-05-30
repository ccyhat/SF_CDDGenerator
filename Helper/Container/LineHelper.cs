using SFTemplateGenerator.Helper.Shares.SDL;
using System.Collections;

namespace SFTemplateGenerator.Helper.Container
{
    public enum Side
    {
        A,
        B
    }

    public static class LineHelper
    {
        static public void GetAllLines(SDL sdl, Device target, Board board, Port port,  List<List<Core>> totalLines)
        {
            var line = new List<Core>();
            GetAllLines(sdl, target.Name, board.Name, port.Name, line, totalLines);
        }
        static public void GetAllLines(SDL sdl, string target, string board, string port, List<List<Core>> totalLines)
        {
            var line = new List<Core>();
            GetAllLines(sdl, target, board, port, line, totalLines);
        }

        static public void GetAllLines(SDL sdl, string deviceName,
        string boardName,
        string portName,
        List<Core> currentLine,
        List<List<Core>> lines)
        {
            var allCores = sdl.Cubicle.Cores.ToList();
            var currentDevice = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == deviceName);

            // 设备不存在时，终止递归
            if (currentDevice == null)
            {
                if (currentLine.Count > 0)
                {
                    lines.Add(new List<Core>(currentLine));
                }
                return;
            }

            var nextCores = GetNextCores(allCores, currentDevice, boardName, portName, currentLine);

            // 无后续线缆或到达IED设备时，终止递归
            if (!nextCores.Any() )
            {
                if (currentLine.Any())
                {
                    lines.Add(new List<Core>(currentLine));
                }
                return;
            }

            foreach (var (core, side) in nextCores)
            {
                var anotherPort = GetAnotherPort(sdl, core, side);
                var targetDevice = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == anotherPort.Device);

            
                if (targetDevice == null || targetDevice.Class.Equals("IED"))
                {
                    lines.Add(new List<Core>(currentLine));
                }
                else
                {
                    currentLine.Add(core);
                    GetAllLines(sdl, anotherPort.Device, anotherPort.Board, anotherPort.Port, currentLine, lines);
                }
            }
        }

        static public void GetAllLinesBFS(SDL sdl, string deviceName,
        string boardName,
        string portName,
        List<Core> currentLine,
        List<List<Core>> lines)
        {
            var allCores = sdl.Cubicle.Cores.ToList();
            var queue = new Queue<(string device, string board, string port, List<Core> path)>();
            queue.Enqueue((deviceName, boardName, portName, new List<Core>(currentLine)));

            while (queue.Count > 0)
            {
                var (dev, board, port, path) = queue.Dequeue();
                var currentDevice = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == dev);

                // 设备不存在时，记录当前路径
                if (currentDevice == null)
                {
                    if (path.Count > 0)
                    {
                        lines.Add(new List<Core>(path));
                    }
                    continue;
                }

                var nextCores = GetNextCores(allCores, currentDevice,  board, port, path);

                // 无后续线缆或到达IED设备时，记录当前路径
                if (nextCores.Count == 0 || currentDevice.Class.Equals("IED"))
                {
                    if (path.Count > 0)
                    {
                        lines.Add(new List<Core>(path));
                    }
                    continue;
                }

                // 将所有可能的下一跳加入队列
                foreach (var item in nextCores)
                {
                    var (core, side) = item;
                    var anotherPort = GetAnotherPort(sdl, core, side);
                    var targetDevice = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == anotherPort.Device);

                    var newPath = new List<Core>(path);
                    newPath.Add(core);

                    if (targetDevice == null || targetDevice.Class.Equals("IED"))
                    {
                        lines.Add(new List<Core>(newPath));
                    }
                    else
                    {
                        queue.Enqueue((anotherPort.Device, anotherPort.Board, anotherPort.Port, newPath));
                    }
                }
            }
        }

        static private List<(Core core, Side side)> GetNextCores(List<Core> allCores, Device currentDevice, string boardName, string portName, List<Core> currentLine)
        {
            if (currentDevice.Class.Equals("TD"))
            {
                return allCores.Except(currentLine)
                    .Where(c => ((c.DeviceA == currentDevice.Name && c.BoardA == boardName) ||
                                (c.DeviceB == currentDevice.Name && c.BoardB == boardName)) &&
                                !(c.DeviceA == c.DeviceB && c.BoardA == c.BoardB))
                    .Select(c => (c, c.DeviceA == currentDevice.Name && c.BoardA == boardName ? Side.A : Side.B))
                    .ToList();
            }
            else
            {
                return allCores.Except(currentLine)
                    .Where(c => (c.DeviceA == currentDevice.Name && c.BoardA == boardName && c.PortA == portName) ||
                                (c.DeviceB == currentDevice.Name && c.BoardB == boardName && c.PortB == portName))
                    .Select(c => (c, c.DeviceA == currentDevice.Name && c.BoardA == boardName && c.PortA == portName ? Side.A : Side.B))
                    .ToList();
            }
        }
        static public (string Device, string Board, string Port) GetAnotherPort(SDL sdl, Core core, Side side)
        {
            string anotherDevice, anotherBoard, anotherPort;
            if (side == Side.A)
            {
                anotherDevice = core.DeviceB;
                anotherBoard = core.BoardB;
                anotherPort = core.PortB;
            }
            else
            {
                anotherDevice = core.DeviceA;
                anotherBoard = core.BoardA;
                anotherPort = core.PortA;
            }
            //检查下一个是不是短连片
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == anotherDevice);
            if (device != null && device.Class.Equals("KK"))
            {
                if (int.TryParse(anotherPort, out int portNumber))
                {
                    anotherPort =  (portNumber - 1).ToString() ;
                }
            }

            return (anotherDevice, anotherBoard, anotherPort);
        }
        static public List<Core> GetTargetLine(SDL sdl, List<List<Core>> lines)
        {
           
            foreach (var line in lines)
            {
                foreach (var core in line)
                {
                    var deviceA = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == core.DeviceA);
                    var deviceB = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == core.DeviceB);
                    if (deviceA != null && deviceB != null)
                    {
                        if (deviceA.Class.Equals("YB") || deviceB.Class.Equals("YB"))
                        {
                            if (LastDeviceIsTD(sdl, line))
                            {
                                return line;
                            }
                            else
                            {
                                return line.GetRange(0, line.Count - 1);
                            }

                        }
                    }
                }
            }
            if (lines.FirstOrDefault() != null)
            {
                return lines.FirstOrDefault().ToList();
            }
            return null;
        }
        static bool LastDeviceIsTD(SDL sdl, List<Core> cores)
        {
            if (cores.Count == 1)
            {
                return true;
            }
            if (cores.Count > 1)
            {
                var lastCore = cores.LastOrDefault()!;
                var SecondlastCore = cores[cores.Count - 2];

                if (lastCore.DeviceA == SecondlastCore.DeviceA)
                {
                    var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == lastCore.DeviceB);
                    if (device != null && device.Class.Equals("TD"))
                    {
                        return true;
                    }
                }
                if (lastCore.DeviceA == SecondlastCore.DeviceB)
                {
                    var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == lastCore.DeviceB);
                    if (device != null && device.Class.Equals("TD"))
                    {
                        return true;
                    }
                }
                if (lastCore.DeviceB == SecondlastCore.DeviceA)
                {
                    var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == lastCore.DeviceA);
                    if (device != null && device.Class.Equals("TD"))
                    {
                        return true;
                    }
                }
                if (lastCore.DeviceB == SecondlastCore.DeviceB)
                {
                    var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == lastCore.DeviceA);
                    if (device != null && device.Class.Equals("TD"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    

}
