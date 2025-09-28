using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFTemplateGenerator.Helper.Paths
{
    public class PathSaver
    {
        private readonly string _configFilePath;

        // 配置数据
        private AppConfig _config;

        // 配置文件的JSON序列化选项
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // 确保枚举序列化为数值而不是字符串
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        // 单例实现
        private static readonly Lazy<PathSaver> _lazy = new(() => new PathSaver());
        public static PathSaver Instance => _lazy.Value;

        // 私有构造函数
        private PathSaver()
        {
            // 确定配置文件的保存位置（当前用户的应用数据目录）
            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? AppDomain.CurrentDomain.BaseDirectory;
            // 配置文件将保存在exe同级目录或指定子目录
            _configFilePath = Path.Combine(exeDirectory, "Config\\AppSetting.json");
            // 初始化配置（从文件加载或使用默认值）
            _config = LoadConfig();
        }

        /// <summary>
        /// 测试仪模式枚举
        /// </summary>
        public enum TESTER
        {
            /// <summary>
            /// 博电测试仪
            /// </summary>
            [Description("使用博电测试仪")]
            PONOVOTester = 0,
            /// <summary>
            /// 博电标准源
            /// </summary>
            [Description("使用博电标准源")]
            PONOVOStandardSource = 1,
            /// <summary>
            /// 昂立测试仪
            /// </summary>
            [Description("使用昂立测试仪")]
            ONLLYTester = 2,

        }

        /// <summary>
        /// 应用配置数据模型
        /// </summary>
        public class AppConfig
        {
            public string LastFilePath { get; set; } = null!;
            public string GBxmlFileSavePath { get; set; } = null!;//gbxml存储路径
            public string ConfigPath { get; set; } = null!;//总配置文件路径
            public string DIODefinePath { get; set; } = null!;//Port/Board配置文件路径
            public string DeviceModelPath { get; set; } = null!;//设备模型路径
            public TESTER Tester { get; set; } = TESTER.PONOVOTester; // 默认值为博电测试仪
        }

        /// <summary>
        /// 获取当前配置
        /// </summary>
        public AppConfig Config => _config;

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private AppConfig LoadConfig()
        {
            try
            {
                // 如果配置文件不存在，创建并返回默认配置
                if (!File.Exists(_configFilePath))
                {
                    var defaultConfig = CreateDefaultConfig();
                    SaveConfig(defaultConfig); // 保存默认配置
                    return defaultConfig;
                }

                // 读取并解析JSON配置文件
                var json = File.ReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<AppConfig>(json, _serializerOptions) ?? CreateDefaultConfig();
            }
            catch (Exception ex)
            {
                // 处理异常（例如记录日志）
                Console.WriteLine($"加载配置失败: {ex.Message}");

                // 返回默认配置并保存（覆盖可能损坏的文件）
                var defaultConfig = CreateDefaultConfig();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        private AppConfig CreateDefaultConfig()
        {
            return new AppConfig
            {
                // 设置应用程序内部的默认值，不依赖外部配置
                LastFilePath = AppDomain.CurrentDomain.BaseDirectory,
                GBxmlFileSavePath = AppDomain.CurrentDomain.BaseDirectory,
                ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "sf-template-local-db.xml"),
                DIODefinePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "DIOs"),
                DeviceModelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Library"),
                Tester = TESTER.PONOVOTester // 使用枚举默认值
            };
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        private void SaveConfig(AppConfig config)
        {
            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                // 序列化配置并写入文件
                var json = JsonSerializer.Serialize(config, _serializerOptions);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
                // 可以考虑实现重试机制或回滚操作
            }
        }

        /// <summary>
        /// 异步保存当前配置到文件
        /// </summary>
        public async Task SaveConfigAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                var json = JsonSerializer.Serialize(_config, _serializerOptions);
                await File.WriteAllTextAsync(_configFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步保存当前配置到文件
        /// </summary>
        public void SaveConfig()
        {
            SaveConfig(_config);
        }

    }
}
