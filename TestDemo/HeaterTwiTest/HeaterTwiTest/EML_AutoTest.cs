#region //EML_AutoTest
/// <summary>
/// EML自动测试：DUT编号 → OTP板卡槽位字符串
/// 全部4个通道统一使用OTP槽位06
/// </summary>
public static Dictionary<int, string> EmlDutToOtpSlot = new Dictionary<int, string>()
{
    {1, "05"},
    {2, "05"},
    {3, "05"},
    {4, "05"}
};
private bool EML_AutoTest(UInt16 emlvalmin, UInt16 emlvalmax)
{
    int looptime = 0;           // 二分法循环计数器
    UInt16 emlval = 0;          // 当前设置的TEC温度值（DAC值）
    Double wavelenth = 0;       // 波长计读取的实际波长
    Double result_err = 0;      // 实际波长与目标波长的误差
    // OTP12初始化 - 切换光开关到发射方向（模块→波长计）
    string slotStr = EmlDutToOtpSlot[Dut];//等待500ms让光开关切换稳定
    otp12.SetSlot(slotStr);
    /*otp12.SW_SetRouteForModule(Dut, true); // true=发射方向 系统应该已经自动测试好*/
    Thread.Sleep(500);
    // 按Dut选择目标波长
    double wLengthTarget = 0;
    //根据DUT编号（1-4），从对应的TestSet配置中读&#x53D6;__&#x76EE;标波长__（单位nm，如1550.12nm等DWDM通道波长）
    switch (Dut)
    {
        case 1: wLengthTarget = TestSet.wLength_target; break;
        case 2: wLengthTarget = TestSet2.wLength_target; break;
        case 3: wLengthTarget = TestSet3.wLength_target; break;
        case 4: wLengthTarget = TestSet4.wLength_target; break;
        default: wLengthTarget = TestSet.wLength_target; break;
    }
    // 普通二分法查找
    //如果没有外接波长计（kt86120c），直接取 `(最小值+最大值)/2` 的中间值设置，__不做反馈调节__。
    if (GlobalVarFun.setup.otp12.IsConnected == false)
    {
        emlval = (UInt16)((emlvalmin + emlvalmax) / 2);
        if (test.setWaveLength(emlval) == false) return false;
    }
    else
    {
        do
        {
            looptime++;

            emlval = (UInt16)((emlvalmin + emlvalmax) / 2); // 取中间值作为试探点

            if (emlval < 2) return false; // 值太小 Error
            if (emlval < 830) // 下限钳位（对应约830nm）
            {
                emlval = 830;
            }
            if (emlval > 1830) // 上限钳位（对应约1830nm）
            {
                emlval = 1830;
            }
            test.setWaveLength(emlval); // 设置TEC温度到emlval
            try
            {
                // 新代码 - 先设置OPM波长（可能需要设置后读取），然后读取
                string waveStr = otp12.OPM_GetWaveLength(opmCh);
                // OPM返回可能是米科学计数（如 1.550000E-06），需要转换为nm
                // 1.550000E-06 m = 1550 nm
                wavelenth = double.Parse(waveStr) * 1e9; // 米转纳米（如果返回是米的话）
                                                         // 或者如果返回是nm（如 1.550000E+03），则直接解析
            }
            catch {
                // 新代码 - 先设置OPM波长（可能需要设置后读取），然后读取
                string waveStr = otp12.OPM_GetWaveLength(opmCh);
                // OPM返回可能是米科学计数（如 1.550000E-06），需要转换为nm
                // 1.550000E-06 m = 1550 nm
                wavelenth = double.Parse(waveStr) * 1e9; // 米转纳米（如果返回是米的话）
                                                         // 或者如果返回是nm（如 1.550000E+03），则直接解析
            }
            Thread.Sleep(20);// 计算误差
            if (wavelenth <= 0) return false;
            result_err = wavelenth - wLengthTarget; //wLengthTarget目标波长
            //
            if (result_err < 0)
            {
                emlvalmax = (UInt16)(emlval - 1); // 波长偏短 → 需要降低温度值，下调上界
            }
            else
            {
                emlvalmin = (UInt16)(emlval + 1); // 波长偏长 → 需要升高温度值，上调下界
            }
        } while ((Math.Abs(result_err) > wLengthMaxErr) && (emlvalmax > emlvalmin) && (looptime < 100));
        //}
        //catch
        //{
        //    errorMessage += "波长计读取异常";
        //    return false;
        //}
        if ((Math.Abs(result_err) > wLengthMaxErr))
        {
            if (GlobalVarFun.Language == "Chinese")
            {
                retutntxrxresult.ErrorMessage += "波长调试失败，与目标波长不符";
            }
            else
            {
                retutntxrxresult.ErrorMessage += "The wavelength debugging failed and does not match the target wavelength";
            }
            return false;
        }
    }
    switch (Dut)
    {
        case 1:
            TestResult.txtosaTemp = emlval;
            break;
        case 2:
            TestResult2.txtosaTemp = emlval;
            break;
        case 3:
            TestResult3.txtosaTemp = emlval;
            break;
        case 4:
            TestResult4.txtosaTemp = emlval;
            break;
        default:
            break;
    }

    return true;
}
#endregion