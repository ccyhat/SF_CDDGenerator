namespace SFTemplateGenerator.Helper.Return
{
    public enum CustomeErrorCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 200,

        /// <summary>
        /// 失败
        /// </summary>
        Error = 100500,

        /// <summary>
        /// 未找到
        /// </summary>
        NoFind = 100404


    }
    public class ReturnCode
    {
        /// <summary>
        /// 200 成功 100500 错误
        /// </summary>
        public CustomeErrorCode Code { get; set; }

        /// <summary>
        /// 错误时候返回的信息
        /// </summary>
        public string ErrorMsg { get; set; }

    }

    public class ReturnCodeEntity<T> : ReturnCode
    {
        public T Entity { get; set; }

        public int Count { get; set; }
    }

    public class ReturnCodeItemList<T> : ReturnCode
    {
        /// <summary>
        /// 返回要查询的列表
        /// </summary>
        public List<T> Entity { get; set; }

    }

    public class ReturnCodeMoreItemList<T1, T2> : ReturnCode
    {
        /// <summary>
        /// 返回要查询的列表
        /// </summary>
        public List<T1> Entity { get; set; }

        /// <summary>
        /// 返回要查询的列表
        /// </summary>
        public List<T2> EntityTwo { get; set; }

    }
}
