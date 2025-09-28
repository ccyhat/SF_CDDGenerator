using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection;

namespace SFTemplateGenerator.Processor.Moduels.FormatAnalogQuantityInspection
{
    public class UpdateRatedValue : IUpdateRatedValue
    {
        public Task UpdateRatedValueAsync(SDL sdl, Items root)
        {
            var safety = root.GetSafetys().Where(S => S.Name.Equals("更新额定值")).FirstOrDefault();
            var AC_Current = sdl.Cubicle.AlternatingCurrent ?? "1";
            string numbersOnly = new string(AC_Current.Where(c => char.IsDigit(c)).ToArray());
            int.TryParse(numbersOnly, out int result);
            safety.ScriptResult.CData =
                $"local vAcCurrent={result};\r\n\r\n" +
                "SetTestPara(\"MRIn\",vAcCurrent);\r\n\r\n" +
                "if (vAcCurrent==1) then\r\n\t" +
                "SetTestPara(\"XXDScale\",5);\r\n" +
                "else\r\n\t" +
                "SetTestPara(\"XXDScale\",2);\r\n" +
                "end";
            return Task.CompletedTask;
        }
    }
}
