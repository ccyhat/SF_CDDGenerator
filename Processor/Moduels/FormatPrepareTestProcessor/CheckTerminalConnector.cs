using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatPrepareTestProcessor;
using System.Data;
using System.Text;

namespace SFTemplateGenerator.Processor.Moduels.FormatPrepareTestProcessor
{
    public class CheckTerminalConnector : ICheckTerminalConnector
    {
        private const int SIZE = 256;
        public Task CheckTerminalConnectorAsync(SDL sdl, Items root)
        {
            var tdDevices = sdl.Cubicle.Devices.Where(D => D.Class == "TD").ToList();
            var devices = OrderDevices(tdDevices);
            StringBuilder sb = new StringBuilder();
            int testItemCount = 1;
            foreach (var device in devices)
            {
                List<List<Core>> lines = new();
                var BoardssortedByNumber = SortBoards(device.Boards);
                GetAllTDLines(sdl, device, BoardssortedByNumber.ToList(), lines);
                sb.Append($"{device.Name}: ");
                var sortedByNumber = BoardssortedByNumber.Select(B => B.Name).ToList();
                foreach (var line in lines)
                {
                    // 1. 获取当前线路中所有匹配的端口索引（去重）
                    var matchingBoardIndices = GetMatchingBoardIndices(line, sortedByNumber);

                    // 2. 对索引排序并分组为连续序列
                    var continuousBoardGroups = GroupContinuousIndices(matchingBoardIndices);
                    for (int i = 0; i < continuousBoardGroups.Count; i++)
                    {
                        var currentGroup = continuousBoardGroups[i];
                        if (currentGroup.Count == 0 || currentGroup.Count == 1)
                        {
                            continue;
                        }
                        else
                        {
                            var firstBoard = sortedByNumber[currentGroup.First()];
                            var lastBoard = sortedByNumber[currentGroup.Last()];
                            sb.Append($"{firstBoard}到{lastBoard}. ");
                            if (i < continuousBoardGroups.Count - 1)
                            {
                                var nextGroupFirstPort = sortedByNumber[continuousBoardGroups[i + 1].First()];
                                sb.Append($"{lastBoard}跳{nextGroupFirstPort}. ");
                            }
                        }
                    }
                    var duplicatesWithCount = FindRepetitionBoard(line);
                    if (duplicatesWithCount.Any())
                    {
                        foreach (var dup in duplicatesWithCount)
                        {
                            sb.Append($"{dup.Item2 + 1}个{dup.Item1}短连. ");
                        }
                    }
                }
                sb.Append($"共{sortedByNumber.LastOrDefault()}个端子. ");//这里是邹易龙要求播报最后一个端子号，而不是播报端子总数
                if (sb.Length > SIZE)
                {
                    var safety = root.GetSafetys().FirstOrDefault(I => I.Name.Equals("检查端子连片（合格）"))!.Clone();
                    safety.Name = $"检查端子连片 - {testItemCount}";
                    safety.DllCall.CData = $"SpeakString={sb};ExpectString=是否合格;";
                    root.ItemList.Add(safety);
                    sb.Clear();
                    testItemCount++;
                }
            }
            if (sb.Length > 0)
            {
                var safety = root.GetSafetys().FirstOrDefault(I => I.Name.Equals("检查端子连片（合格）"))!.Clone();
                safety.Name = $"检查端子连片 - {testItemCount}";
                safety.DllCall.CData = $"SpeakString={sb};ExpectString=是否合格;";
                root.ItemList.Add(safety);
            }
            root.ItemList.RemoveAll(I => I.Name.Equals("检查端子连片（合格）"));
            return Task.CompletedTask;
        }
        private Tuple<string, string, string> GetAnotherPort(SDL sdl, Core core, string deviceName, string boardName)
        {
            string AnotherDevice = "";
            string AnotherBoard = "";
            string AnotherPort = "";
            if (core.DeviceA == deviceName && core.BoardA == boardName)
            {
                AnotherDevice = core.DeviceB;
                AnotherBoard = core.BoardB;
                AnotherPort = core.PortB;
            }
            else
            {
                AnotherDevice = core.DeviceA;
                AnotherBoard = core.BoardA;
                AnotherPort = core.PortA;
            }

            return new Tuple<string, string, string>(AnotherDevice, AnotherBoard, AnotherPort);
        }
        private void GetAllCores(SDL sdl, Device target, Board board, List<Core> cores)
        {
            GetAllCores(sdl, target.Name, board.Name, cores);
        }
        private void GetAllCores(SDL sdl, string deviceName, string boardName, List<Core> cores)
        {
            var totalCores = sdl.Cubicle.Cores.Where(C => C.DeviceA == C.DeviceB);
            var device = sdl.Cubicle.Devices.FirstOrDefault(d => d.Name == deviceName)!;
            List<Core> nextCores = null!;
            if (device.Class.Equals("TD"))
            {
                nextCores = totalCores.Except(cores).Where(c =>
               ((c.DeviceA == deviceName && c.BoardA == boardName) ||
               (c.DeviceB == deviceName && c.BoardB == boardName))).ToList();
            }
            if (nextCores.Count == 0)
            {
                return;
            }
            foreach (var core in nextCores)
            {
                if(!cores.Any(c=>c==core))
                {
                    cores.Add(core);
                }                
                Tuple<string, string, string> anotherPort = GetAnotherPort(sdl, core, deviceName, boardName);
                GetAllCores(sdl, anotherPort.Item1, anotherPort.Item2, cores);
            }
        }
        /// <summary>
        /// 此方法区别于其他模块的getalllines
        /// </summary>
        /// <param name="sdl"></param>
        /// <param name="deviceName"></param>
        /// <param name="boardName"></param>
        /// <param name="cores"></param>
        private void GetAllTDLines(SDL sdl, Device device, List<Board> boards,
        List<List<Core>> lines)
        {
#if DEBUG
            if(device.Name=="5QD")
            {
                int a = 0;
            }
#endif
            var board = boards.FirstOrDefault();
            if (board != null)
            {
                boards.RemoveAll(B => B.Name == board.Name);
                List<Core> cores = new List<Core>();
                GetAllCores(sdl, device, board, cores);
                if (cores.Count > 0)
                {
                    lines.Add(cores);
                }
                List<string> boardNames = getBoardNames(cores);
                boards.RemoveAll(B => boardNames.Any(N => B.Name == N));
                GetAllTDLines(sdl, device, boards, lines);
            }
            else
            {
                return;
            }
        }
        private List<string> getBoardNames(List<Core> cores)
        {
            List<string> BoardNameList = new List<string>();
            foreach (var core in cores)
            {
                if (!BoardNameList.Any(B => B == core.BoardA))
                {
                    BoardNameList.Add(core.BoardA);
                }
                if (!BoardNameList.Any(B => B == core.BoardB))
                {
                    BoardNameList.Add(core.BoardB);
                }
            }
            return BoardNameList;
        }

        private List<Tuple<string, int>> FindRepetitionBoard(List<Core> cores)
        {
            // 处理空列表情况
            if (cores == null)
            {
                return new List<Tuple<string, int>>();
            }
            var duplicates = cores.Where(C => C.BoardA == C.BoardB).GroupBy(G => G.BoardA).Select(S => new Tuple<string, int>(S.Key, S.Count())).ToList();
            return duplicates;
        }
        // 收集当前线路中所有匹配的端口索引（去重）
        private List<int> GetMatchingBoardIndices(List<Core> line, List<string> portNames)
        {
            var matchingIndices = new List<int>();

            foreach (var core in line)
            {
                for (int i = 0; i < portNames.Count; i++)
                {
                    // 匹配BoardA或BoardB且索引未添加过
                    if ((portNames[i] == core.BoardA || portNames[i] == core.BoardB) &&
                        !matchingIndices.Contains(i))
                    {
                        matchingIndices.Add(i);
                    }
                }
            }

            matchingIndices.Sort(); // 排序确保连续判断有效
            return matchingIndices;
        }

        // 将排序后的索引分组为连续序列（如[1,2,3,5,6] → [[1,2,3], [5,6]]）
        private List<List<int>> GroupContinuousIndices(List<int> sortedIndices)
        {
            var continuousGroups = new List<List<int>>();
            if (sortedIndices.Count == 0)
                return continuousGroups;

            // 初始化第一个连续组
            var currentGroup = new List<int> { sortedIndices[0] };

            // 遍历剩余索引判断连续性
            for (int i = 1; i < sortedIndices.Count; i++)
            {
                if (sortedIndices[i] == sortedIndices[i - 1] + 1)
                {
                    currentGroup.Add(sortedIndices[i]); // 连续则加入当前组
                }
                else
                {
                    continuousGroups.Add(currentGroup); // 不连续则保存当前组
                    currentGroup = new List<int> { sortedIndices[i] }; // 开启新组
                }
            }

            continuousGroups.Add(currentGroup); // 添加最后一个组
            return continuousGroups;
        }

        private List<Device> OrderDevices(List<Device> tdDevices)
        {
            List<Device> orderedDevices = new List<Device>();
            // 按"ablut"的Val分组，并对分组结果按"左在前、右在后"排序
            var groupedDevices = tdDevices
                .GroupBy(device => device.EAIs.FirstOrDefault(eai => eai.Name.Equals("ablut"))?.Val)
                .OrderBy(group =>
                    group.Key == "左" ? 0    // "左"分组优先级最高
                    : group.Key == "右" ? 1  // "右"分组优先级次之
                    : 2)                     // 其他分组放最后
                .ToList();
            foreach (var deviceGroup in groupedDevices)
            {
                // 每组内按"position"的数值排序
                var orderedDevicesInGroup = deviceGroup.OrderBy(device =>
                {
                    // 获取当前设备的"position"属性
                    var positionEai = device.EAIs.FirstOrDefault(eai => eai.Name.Equals("position"));
                    // 处理空值或转换失败的情况
                    if (positionEai == null || string.IsNullOrWhiteSpace(positionEai.Val))
                    {
                        return 0; // 可根据业务需求调整默认排序值
                    }
                    // 转换为整数进行数值排序
                    if (int.TryParse(positionEai.Val, out int positionValue))
                    {
                        return positionValue;
                    }
                    else
                    {
                        return 0; // 转换失败时的默认值
                    }
                }).ToList();
                // 将排序后的组内设备添加到结果列表
                orderedDevices.AddRange(orderedDevicesInGroup);
            }
            return orderedDevices;
        }
        private List<Board> SortBoards(List<Board> boards)
        {

            var originalOrder = boards.Select(b => b.Name).ToList();
            var BoardssortedByNumber = boards
                // 主要排序：按名称中的数字部分升序
                .OrderBy(B =>
                {
                    // 提取名称中的所有数字字符
                    char[] digits = B.Name.Where(c => char.IsDigit(c)).ToArray();
                    string numbersOnly = new string(digits);
                    // 解析数字（无法解析时用int.MaxValue排在最后）
                    int.TryParse(numbersOnly, out int num);
                    return numbersOnly.Length > 0 ? num : int.MaxValue;
                })
                // 次要排序：相同数字时，纯数字项排在带字母项前面
                .ThenBy(B =>
                {
                    // 提取数字部分
                    char[] digits = B.Name.Where(c => char.IsDigit(c)).ToArray();
                    string numbersOnly = new string(digits);
                    // 纯数字项（名称完全由数字组成）标记为0，带字母项标记为1
                    return B.Name == numbersOnly ? 0 : 1;
                })
                .ToList();

            // 检查排序后顺序是否发生变化
            var sortedOrder = BoardssortedByNumber.Select(b => b.Name).ToList();
            bool orderChanged = !originalOrder.SequenceEqual(sortedOrder);
            // 若顺序变化则记录日志
            if (orderChanged)
            {
                var original = string.Join(", ", originalOrder);
                var sorted = string.Join(", ", sortedOrder);
                Logger.Warning($"Board排序顺序发生变化 - 原始顺序: [{original}], 新顺序: [{sorted}]");
            }
            return BoardssortedByNumber;
        }

    }
}

