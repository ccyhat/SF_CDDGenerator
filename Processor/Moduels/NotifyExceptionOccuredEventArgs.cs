namespace SFTemplateGenerator.Processor.Moduels
{
    public class NotifyExceptionOccuredEventArgs
    {
        public int? ErrorCode { get; set; }
        public string ExceptionMessage { get; set; }
        public Exception Exception { get; set; }
    }
}
