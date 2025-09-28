class FolderHierarchyCollector
{
    static void Main(string[] args)
    {
        // 1. 替换为你的根文件夹路径
        string rootFolderPath = @"D:\YourTargetRootFolder";
        // 总列表：存储多个子 List，每个子 List 是一条完整路径的层级拆分
        List<List<string>> allFilePathHierarchies = new List<List<string>>();

        try
        {
            // 2. 初始化根路径的层级列表，作为递归起点
            List<string> rootHierarchy = new List<string> { rootFolderPath };
            // 递归收集所有路径层级
            CollectFilePathHierarchies(rootFolderPath, rootHierarchy, allFilePathHierarchies);

            // 3. 输出结果（按层级展示每条路径）
            Console.WriteLine($"找到 {allFilePathHierarchies.Count} 个 .txt 文件的路径层级：");
            for (int i = 0; i < allFilePathHierarchies.Count; i++)
            {
                Console.WriteLine($"第 {i + 1} 条路径：");
                foreach (var folderOrFile in allFilePathHierarchies[i])
                {
                    Console.WriteLine($"  - {folderOrFile}");
                }
                Console.WriteLine(); // 空行分隔，增强可读性
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行出错：{ex.Message}");
        }
    }

    /// <summary>
    /// 递归收集从根目录到 .txt 文件的完整路径层级
    /// </summary>
    /// <param name="currentFolder">当前遍历的文件夹路径</param>
    /// <param name="currentHierarchy">当前已积累的路径层级列表（如：[根目录, 子目录1, 子目录2]）</param>
    /// <param name="totalHierarchies">总列表：存储所有完整路径层级</param>
    private static void CollectFilePathHierarchies(string currentFolder,
                                                  List<string> currentHierarchy,
                                                  List<List<string>> totalHierarchies)
    {
        // 步骤1：先处理当前文件夹下的 .txt 文件（找到文件即完成一条路径）
        foreach (string txtFile in Directory.EnumerateFiles(currentFolder, "*.txt"))
        {
            // 复制当前层级列表（避免引用冲突），并添加最终的 .txt 文件路径
            List<string> completeHierarchy = new List<string>(currentHierarchy);
            completeHierarchy.Add(txtFile);
            // 将完整路径层级加入总列表
            totalHierarchies.Add(completeHierarchy);
        }

        // 步骤2：递归遍历当前文件夹的子文件夹，延续路径层级
        foreach (string subFolder in Directory.EnumerateDirectories(currentFolder))
        {
            // 复制当前层级列表，添加新的子文件夹路径
            List<string> newHierarchy = new List<string>(currentHierarchy);
            newHierarchy.Add(subFolder);
            // 递归进入子文件夹，继续收集路径
            CollectFilePathHierarchies(subFolder, newHierarchy, totalHierarchies);
        }
    }
}