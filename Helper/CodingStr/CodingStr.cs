namespace SFTemplateGenerator.Helper.CodingStr
{
    public class CodingStr
    {
        public static string SCRIPT_1 = "local vg_CLIErrorAbs = GetTestPara(\"g_CLIErrorAbs\"); --测量电流绝对误差，当电流通道时，CalAinError的第三个参数用它\r\n" +
            "local vg_CLIAngErrorAbs = GetTestPara(\"g_CLIAngErrorAbs\"); --测量电流角度绝对误差，测量电流判角度时，CalAinError的第三个参数用它\r\n" +
            "local vg_CLUErrorAbs = GetTestPara(\"g_CLUErrorAbs\"); --测量电压绝对误差，当电压通道基准值不是0时，CalAinError的第三个参数用它\r\n" +
            "local vg_CLUAngErrorAbs = GetTestPara(\"g_CLUAngErrorAbs\"); --测量电压角度绝对误差，测量电压判角度时，CalAinError的第三个参数用它\r\n\r\n" +
            "local vMRIn = GetTestPara(\"MRIn\"); --模入电流额定CT值，非测量电流通道时，CalAinError的第二个参数填它\r\n" +
            "local vCLIn = GetTestPara(\"CLIn\"); --测量电流额定CT值，测量电流通道时，CalAinError的第二个参数填它\r\n\r\n" +
            "local vg_MRUErrorAbs = GetTestPara(\"g_MRUErrorAbs\"); --模入电压绝对误差，非测量电压且基准值不为0时，CalAinError的第三个参数用它\r\n" +
            "local vg_MRUAngErrorAbs = GetTestPara(\"g_MRUAngErrorAbs\"); --模入电压角度绝对误差，非测量电压判角度时，CalAinError的第三个参数用它\r\n" +
            "local vg_MRIErrorRel = GetTestPara(\"g_MRIErrorRel\"); --模入电流绝对误差，非测量电流时，CalAinError的第三个参数用它\r\n" +
            "local vg_MRIAngErrorAbs = GetTestPara(\"g_MRIAngErrorAbs\"); --模入电流角度绝对误差，非测量电流判角度时，CalAinError的第三个参数用它\r\n" +
            "local vg_U1VErrorAbs = GetTestPara(\"g_U1VErrorAbs\"); --电压1V绝对误差，当通道为电压而且基准值是0时，CalAinError的第三个参数用它\r\n\r\n" +
            "local v_Uc = GetPara(\"..\\\\\", \"_Uc\"); --Uc幅值\r\n" +
            "local v_Ub = GetPara(\"..\\\\\", \"_Ub\"); --Ub幅值\r\n" +
            "local v_Ua = GetPara(\"..\\\\\", \"_Ua\"); --Ua幅值\r\n" +
            "local v_Ic = GetPara(\"..\\\\\", \"_Ic\"); --Ic幅值\r\n" +
            "local v_Ib = GetPara(\"..\\\\\", \"_Ib\"); --Ib幅值\r\n" +
            "local v_Ia = GetPara(\"..\\\\\", \"_Ia\"); --Ia幅值\r\n\r\n" +
            "local nRsltJdg = 0;\r\n\r\n";
        public static string SCRIPT_1_END =
            "   SetRsltJdg(\"\", 1);\r\n" +
            "else\r\n" +
            "	SetRsltJdg(\"\", 0);\r\n" +
            "end;";
        public static string SCRIPT_2 =
            "local vg_MRIAngErrorAbs = GetTestPara(\"g_MRIAngErrorAbs\"); --模入电流角度绝对误差\r\n" +
            "local vg_MRUAngErrorAbs = GetTestPara(\"g_MRUAngErrorAbs\"); --模入电压角度绝对误差\r\n" +
            "local vMRIn = GetTestPara(\"MRIn\"); --模入额定CT值\r\n" +
            "local vg_MRIErrorRel = GetTestPara(\"g_MRIErrorRel\"); --模入电流相对误差\r\n" +
            "local vg_MRUErrorRel = GetTestPara(\"g_MRUErrorRel\"); --模入电压相对误差\r\n" +
            "local vg_MRUErrorAbs = GetTestPara(\"g_MRUErrorAbs\"); --模入电压绝对误差\r\n" +
            "local vg_U1VErrorAbs = GetTestPara(\"g_U1VErrorAbs\"); --电压1V绝对误差\r\n\r\n\r\n" +
            "local v_Uc = GetPara(\"..\\\\\", \"_Uc\"); --Uc幅值\r\n" +
            "local v_Ub = GetPara(\"..\\\\\", \"_Ub\"); --Ub幅值\r\n" +
            "local v_Ua = GetPara(\"..\\\\\", \"_Ua\"); --Ua幅值\r\n" +
            "local v_Ic = GetPara(\"..\\\\\", \"_Ic\"); --Ic幅值\r\n" +
            "local v_Ib = GetPara(\"..\\\\\", \"_Ib\"); --Ib幅值\r\n" +
            "local v_Ia = GetPara(\"..\\\\\", \"_Ia\"); --Ia幅值\r\n" +
            "local v_I0 = vMRIn*0.02;\r\n\r\n" +
            "local nRsltJdg = 0;\r\n";
        public static string SCRIPT_2_END =
            "local vTempU = v_Ua + v_Ub + v_Uc;\r\n" +
            "local vTempI = v_Ia + v_Ib + v_Ic;\r\n" +
            "if (vTempU < 150) then\r\n" +
            "\tnRsltJdg = 0;\r\n" +
            "end;\r\n" +
            "if (vTempI < 2.4) then\r\n" +
            "\tnRsltJdg = 0;\r\n" +
            "end;\r\n\r\n" +
            "strshow = string.format(\"nRsltJdg=%d\", nRsltJdg);\r\n" +
            "ShowMsg(strshow);";
        public static string SCRIPT_2_TAIL =
            "   SetRsltJdg(\"\", 1);\r\n" +
            "else\r\n" +
            "	SetRsltJdg(\"\", 0);\r\n" +
            "end;";
        public static string SCRIPT_4_HEAD =
            "local vg_MRUErrorRel = GetTestPara(\"g_MRUErrorRel\"); --模入电压相对误差\r\n" +
            "local vg_CLIErrorRel = GetTestPara(\"g_CLIErrorRel\"); --测量电流相对误差，当是测量电流通道时，CalAinError的第四个参数用它\r\n" +
            "local vg_CLIAngErrorAbs = GetTestPara(\"g_CLIAngErrorAbs\"); --测量电流角度绝对误差，测量电流判角度时，CalAinError的第三个参数用它\r\n" +
            "local vg_CLUErrorAbs = GetTestPara(\"g_CLUErrorAbs\"); --测量电压绝对误差，当电压通道基准值不是0时，CalAinError的第三个参数用它\r\n" +
            "local vg_CLUAngErrorAbs = GetTestPara(\"g_CLUAngErrorAbs\"); --测量电压角度绝对误差，测量电压判角度时，CalAinError的第三个参数用它\r\n\r\n" +
            "local vMRIn = GetTestPara(\"MRIn\"); --模入电流额定CT值，非测量电流通道时，CalAinError的第二个参数填它\r\n" +
            "local vCLIn = GetTestPara(\"CLIn\"); --测量电流额定CT值，测量电流通道时，CalAinError的第二个参数填它\r\n\r\n\r\n" +
            "local vg_MRUErrorRel = GetTestPara(\"g_MRUErrorRel\"); --模入电压相对误差，非测量电压且基准值不为0时，CalAinError的第三个参数用它\r\n" +
            "local vg_MRUAngErrorAbs = GetTestPara(\"g_MRUAngErrorAbs\"); --模入电压角度绝对误差，非测量电压判角度时，CalAinError的第三个参数用它\r\n" +
            "local vg_MRIErrorRel = GetTestPara(\"g_MRIErrorRel\"); --模入电流相对误差，非测量电流时，CalAinError的第四个参数用它\r\n" +
            "local vg_MRIAngErrorAbs = GetTestPara(\"g_MRIAngErrorAbs\"); --模入电流角度绝对误差，非测量电流判角度时，CalAinError的第三个参数用它\r\n" +
            "local vg_U1VErrorAbs = GetTestPara(\"g_U1VErrorAbs\"); --电压1V绝对误差，当通道为电压而且基准值是0时，CalAinError的第三个参数用它\r\n\r\n" +
            "local v_I0 = vMRIn*1.414;\r\n" +
            "local v_IcError = vMRIn*0.02;\r\n\r\n" +
            "local v_Uc = GetPara(\"..\\\\\", \"_Uc\"); --Uc幅值\r\n" +
            "local v_Ub = GetPara(\"..\\\\\", \"_Ub\"); --Ub幅值\r\n" +
            "local v_Ua = GetPara(\"..\\\\\", \"_Ua\"); --Ua幅值\r\n" +
            "local v_Ic = GetPara(\"..\\\\\", \"_Ic\"); --Ic幅值\r\n" +
            "local v_Ib = GetPara(\"..\\\\\", \"_Ib\"); --Ib幅值\r\n" +
            "local v_Ia = GetPara(\"..\\\\\", \"_Ia\"); --Ia幅值\r\n\r\n" +
            "local nRsltJdg = 0;\r\n";
        public static string SCRIPT_4_END =
            "\tSetRsltJdg(\"\", 1); \r\n" +
            "else \r\n\t" +
            "SetRsltJdg(\"\", 0); \r\n" +
            "end;";

    }
}
