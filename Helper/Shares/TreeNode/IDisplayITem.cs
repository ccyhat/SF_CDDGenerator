using SFTemplateGenerator.Helper.Shares.GuideBook;

namespace SFTemplateGenerator.Helper.Shares.TreeNode
{
    public interface IDisplayITem
    {
        string Name { get; set; }
        ScriptInit ScriptInit { get; set; }
        ScriptName ScriptName { get; set; }
        ScriptResult ScriptResult { get; set; }
        RptMap RptMap { get; set; }
        DllCall DllCall { get; set; }
    }
}
