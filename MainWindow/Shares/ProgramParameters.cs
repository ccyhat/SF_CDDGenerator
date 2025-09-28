namespace SFTemplateGenerator.MainWindow.Shares
{
    public class ProgramParameters
    {
        private static readonly object SYNC_ROOT = new object();
        private static ProgramParameters _parameters = null!;
        private string[] _args = null!;

        private ProgramParameters()
        {
        }

        public static ProgramParameters Instance
        {
            get
            {
                if (_parameters == null)
                {
                    lock (SYNC_ROOT)
                    {
                        if (_parameters == null)
                        {
                            _parameters = new ProgramParameters();
                        }
                    }
                }

                return _parameters;
            }
        }

        public string[] Args => _args;

        public void SetArgs(params string[] args)
        {
            _args = args;
        }
    }
}
