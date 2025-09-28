using System.Runtime.CompilerServices;

namespace SFTemplateGenerator.Helper.Logger
{
    public static class Logger
    {
        // 日志级别枚举
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Fatal = 4
        }

        // 当前日志级别（默认为Info，只记录Info及以上级别的日志）
        private static LogLevel CurrentLevel { get; set; } = LogLevel.Debug;

        // 日志文件路径
        private static string LogFilePath { get; set; } = "application.log";

        // 静态构造函数
        static Logger()
        {
            // 确保日志文件所在目录存在
            string directory = Path.GetDirectoryName(LogFilePath)!;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // 设置日志文件路径
        public static void SetLogFilePath(string filePath)
        {
            LogFilePath = filePath;
        }

        // 设置日志级别
        public static void SetLogLevel(LogLevel level)
        {
            CurrentLevel = level;
        }

        // 记录Debug日志（带代码位置信息）
        public static void Debug(string message, Exception exception = null!,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Debug, message, exception, filePath, lineNumber, memberName);
        }

        // 记录Info日志（带代码位置信息）
        public static void Info(string message, Exception exception = null!,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Info, message, exception, filePath, lineNumber, memberName);
        }

        // 记录Warning日志（带代码位置信息）
        public static void Warning(string message, Exception exception = null!,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Warning, message, exception, filePath, lineNumber, memberName);
        }

        // 记录Error日志（带代码位置信息）
        public static void Error(string message, Exception exception = null!,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Error, message, exception, filePath, lineNumber, memberName);
        }

        // 记录Fatal日志（带代码位置信息）
        public static void Fatal(string message, Exception exception = null!,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Fatal, message, exception, filePath, lineNumber, memberName);
        }

        public static void ClearLog()
        {
            try
            {
                lock (typeof(Logger))
                {
                    // 检查文件是否存在
                    if (File.Exists(LogFilePath))
                    {
                        // 清空文件内容（创建新的空文件）
                        File.WriteAllText(LogFilePath, string.Empty);
                        Info("日志文件已清空");
                    }
                    else
                    {
                        Warning($"尝试清空不存在的日志文件: {LogFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error($"清空日志文件失败: {ex.Message}", ex);
            }
        }

        // 核心日志记录方法（带代码位置信息）
        private static void Log(LogLevel level, string message, Exception exception = null!,
            string filePath = "", int lineNumber = 0, string memberName = "")
        {
            // 检查日志级别是否需要记录
            if (level < CurrentLevel)
                return;

            try
            {
                // 获取文件名（仅保留文件名，去掉完整路径）
                string fileName = Path.GetFileName(filePath);

                // 构建包含代码位置的日志消息
                string codeLocation = string.IsNullOrEmpty(fileName)
                    ? ""
                    : $"[{fileName}:{lineNumber}({memberName})] ";

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] [{Thread.CurrentThread.ManagedThreadId}] {codeLocation}{message}";

                // 添加异常信息（如果有）
                if (exception != null)
                {
                    logMessage += Environment.NewLine +
                                 $"[Exception] {exception.Message}" + Environment.NewLine +
                                 $"[StackTrace] {exception.StackTrace}";

                    // 记录内部异常（如果有）
                    Exception innerEx = exception.InnerException!;
                    while (innerEx != null)
                    {
                        logMessage += Environment.NewLine +
                                     $"[InnerException] {innerEx.Message}" + Environment.NewLine +
                                     $"[StackTrace] {innerEx.StackTrace}";
                        innerEx = innerEx.InnerException!;
                    }
                }

                // 追加到日志文件
                lock (typeof(Logger))
                {
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // 日志记录失败时的处理（避免影响主程序运行）
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}
