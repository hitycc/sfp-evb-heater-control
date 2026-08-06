#region RxSenBitErrorCheck_Async
/// <summary>
/// 接收灵敏度、饱和光功率误码测试：DUT对应OTP仪器槽位映射
/// DUT1、2 → 槽位"09"；DUT3、4 → 槽位"10"
/// </summary>
public static Dictionary<int, string> RxBitErrDutToOtpSlot = new Dictionary<int, string>
{
    { 1, "09" },
    { 2, "09" },
    { 3, "10" },
    { 4, "10" }
};
private async Task<bool> RxSenBitErrorCheck_Async()
{
    string errmsg = "";
    string Status = ""; // BERT返回的原始状态字符串
    double berThreshold = 5e-5;
    ReturnReuslt result = new ReturnReuslt();

    // VOA初始化 选择当前Dut对应的OTP-12槽位
    string slotStr = RxBitErrDutToOtpSlot[Dut];
    otp12.SetSlot(slotStr);
    otp12.VOA_SetAlcState(Dut, "OFF"); // 关闭自动光功率控制（ALC）
    otp12.VOA_SetMode(Dut, "ATTenuation");
    otp12.VOA_SetApMode(Dut, "ABSolute");
    otp12.VOA_SetOutputState(Dut, "ON");
    // 切换光开关到接收方向
    otp12.SW_SetRouteForModule(Dut, false);
    await Task.Delay(500); // 等待500ms让硬件稳定

    // 配置BERT速率和码型（10G31 = 10.3125Gbps, PRBS31）
    otp12.BERT_SetRate("10G31");
    otp12.BERT_SetPattern(31);

    // 设置到饱和点
    switch (Dut)
    {
        case 1:
            otp12.VOA_SetAttenuation(Dut, DOA.rxOverLoadAtt); //饱和点
            break;
        case 2:
            otp12.VOA_SetAttenuation(Dut, DOA2.rxOverLoadAtt); //饱和点
            break;
        case 3:
            otp12.VOA_SetAttenuation(Dut, DOA3.rxOverLoadAtt); //饱和点
            break;
        case 4:
            otp12.VOA_SetAttenuation(Dut, DOA4.rxOverLoadAtt); //饱和点
            break;
    }

    await Task.Delay(500);
    if (GlobalVarFun.setup.bert_connect && GlobalVarFun.setup.rx_sen_test)
    {
        try
        {
            otp12.BERT_ClearAllErr();//// 清除BERT误码计数器
            Status = otp12.BERT_GetErrData(Dut);// 读取误码数据，格式："errBits totalBits lockFlag"
            string error = "";
            try
            {
                // 解析误码数据...
                string[] parts = Status.Trim().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    long errBits = long.Parse(parts[0]);// 误码比特数
                    long totalBits = long.Parse(parts[1]);// 总传输比特数
                    int lockFlag = int.Parse(parts[2]);// 时钟锁定标志(1=锁定,0=失步)

                    if (lockFlag == 1) // 锁定
                    {
                        double ber = totalBits > 0 ? (double)errBits / totalBits : 0; //计算实际误码率
                        if (errBits == 0 || ber <= berThreshold)
                        {
                            result.message = "饱和光功率测试PASS: " + Status + " BER=" + ber.ToString("E6");
                            ModListBoxShow(this, result);
                            //饱和光功率测试PASS
                        }
                        else
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                errmsg += "饱和光功率测试失败：\r\n";
                            }
                            else
                            {
                                errmsg += "Saturation optical power test failed：\r\n";
                            }
                            errmsg += Status + " BER=" + ber.ToString("E6") + "\r\n";
                            result.message = "饱和光功率测试失败：误码率=" + ber.ToString("E6");
                            ModListBoxShow(this, result);
                        }
                    }
                    else // lockFlag == 0 失步
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg += "饱和光功率测试失败(失步)：\r\n";
                        }
                        else
                        {
                            errmsg += "Saturation optical power test failed (no lock)：\r\n";
                        }
                        errmsg += Status + "\r\n";
                        result.message = "饱和光功率测试：失步 " + Status;
                        ModListBoxShow(this, result);
                    }
                }
                else
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg += "饱和光功率测试失败,误码数据异常：\r\n";
                    }
                    else
                    {
                        errmsg += "Saturation light power test failed,Bit error exception：\r\n";
                    }
                    errmsg += Status + "\r\n";
                }
            }
            catch
            {
                errmsg += "饱和光功率测试失败,误码数据解析异常：\r\n";
                errmsg += Status + "\r\n";
            }
            if (errmsg == "") errmsg = "";
            result.message = "饱和光功率测试：" + (errmsg == "" ? "PASS" : errmsg);
            ModListBoxShow(this, result);
        }
        catch
        {
            errmsg += "饱和光功率测试失败,误码率获取异常：\r\n";
            result.message = "饱和光功率测试：" + errmsg;
            ModListBoxShow(this, result);
        }
    }
    //将衰减器调到灵敏度点（衰减最大，入射光最弱），测试最小接收光功率下的误码率。
    switch (Dut)
    {
        case 1:
            otp12.VOA_SetAttenuation(Dut, DOA.rxSenAtt);      //灵敏度点
            break;
        case 2:
            otp12.VOA_SetAttenuation(Dut, DOA2.rxSenAtt);      //灵敏度点
            break;
        case 3:
            otp12.VOA_SetAttenuation(Dut, DOA3.rxSenAtt);      //灵敏度点
            break;
        case 4:
            otp12.VOA_SetAttenuation(Dut, DOA4.rxSenAtt);      //灵敏度点
            break;
    }

    otp12.BERT_ClearAllErr();//清除误码率     
    await Task.Delay(500);
    errmsg += sencheck(Dut);

    if (GlobalVarFun.setup.bert_connect && GlobalVarFun.setup.rx_sen_test)
    {
        otp12.BERT_ClearAllErr();//清除误码率              
        errmsg += sencheck(Dut);
        switch (Dut)
        {
            case 1:
                otp12.VOA_SetAttenuation(Dut, DOA.rxSenAtt - 2);      //灵敏度点-2dB
                break;
            case 2:
                otp12.VOA_SetAttenuation(Dut, DOA2.rxSenAtt - 2);      //灵敏度点-2dB
                break;
            case 3:
                otp12.VOA_SetAttenuation(Dut, DOA3.rxSenAtt - 2);      //灵敏度点-2dB
                break;
            case 4:
                otp12.VOA_SetAttenuation(Dut, DOA4.rxSenAtt - 2);      //灵敏度点-2dB
                break;
        }

        otp12.BERT_ClearAllErr();//清除误码率     
        await Task.Delay(500);
        errmsg += sencheck(Dut);

        switch (Dut)
        {
            case 1:
                otp12.VOA_SetAttenuation(Dut, DOA.rxOverLoadAtt);  //光饱和点
                break;
            case 2:
                otp12.VOA_SetAttenuation(Dut, DOA2.rxOverLoadAtt);  //光饱和点
                break;
            case 3:
                otp12.VOA_SetAttenuation(Dut, DOA3.rxOverLoadAtt);  //光饱和点
                break;
            case 4:
                otp12.VOA_SetAttenuation(Dut, DOA4.rxOverLoadAtt);  //光饱和点
                break;
        }

        otp12.BERT_ClearAllErr();//清除误码率     
        await Task.Delay(500);
        errmsg += sencheck(Dut);
        switch (Dut)
        {
            case 1:
                otp12.VOA_SetAttenuation(Dut, DOA.rxOverLoadAtt + 3);  //光饱和点+3dB
                break;
            case 2:
                otp12.VOA_SetAttenuation(Dut, DOA2.rxOverLoadAtt + 3);  //光饱和点+3dB
                break;
            case 3:
                otp12.VOA_SetAttenuation(Dut, DOA3.rxOverLoadAtt + 3);  //光饱和点+3dB
                break;
            case 4:
                otp12.VOA_SetAttenuation(Dut, DOA4.rxOverLoadAtt + 3);  //光饱和点+3dB
                break;
        }

        otp12.BERT_ClearAllErr();//清除误码率     
        await Task.Delay(500);
        errmsg += sencheck(Dut);
        switch (Dut)
        {
            case 1:
                otp12.VOA_SetAttenuation(Dut, DOA.rxSenAtt);      //回到灵敏度点
                break;
            case 2:
                otp12.VOA_SetAttenuation(Dut, DOA2.rxSenAtt);      //回到灵敏度点
                break;
            case 3:
                otp12.VOA_SetAttenuation(Dut, DOA3.rxSenAtt);      //回到灵敏度点
                break;
            case 4:
                otp12.VOA_SetAttenuation(Dut, DOA4.rxSenAtt);      //回到灵敏度点
                break;
        }

        otp12.BERT_ClearAllErr();//清除误码率     
        await Task.Delay(1000);
        errmsg += sencheck(Dut);
        retutntxrxresult.ErrorMessage = errmsg;

        if (errmsg != "")
        {
            return false;
        }
    }
    return true;
}
//
private string sencheck(int ch)
{
    string error = "";
    string Status = "";
    double berThreshold = 5e-5;
    // 用于界面日志显示的结果对象
    ReturnReuslt result = new ReturnReuslt();
    try
    {
        //// ch=Dut编号(1-4)，读取该通道误码数据 `"errBits totalBits lockFlag"`
        Status = otp12.BERT_GetErrData(ch);
        string fpn = "";
        switch (Dut)
        {
            case 1:
                fpn = TestResult.fibertop_pn;
                break;
            case 2:
                fpn = TestResult2.fibertop_pn;
                break;
            case 3:
                fpn = TestResult3.fibertop_pn;
                break;
            case 4:
                fpn = TestResult4.fibertop_pn;
                break;
            default:
                break;
        }

        try
        {
            string[] parts = Status.Trim().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                long errBits = long.Parse(parts[0]);
                long totalBits = long.Parse(parts[1]);
                int lockFlag = int.Parse(parts[2]);

                if (lockFlag == 1) // 同步锁定
                {
                    double ber = totalBits > 0 ? (double)errBits / totalBits : 0;
                    result.message = "误码率 : " + Status + " BER=" + ber.ToString("E6");
                    ModListBoxShow(this, result);

                    if (errBits == 0 || ber <= berThreshold)
                    {
                        result.message = "灵敏度测试PASS: " + Status + " BER=" + ber.ToString("E6");
                        ModListBoxShow(this, result);
                        //灵敏度测试PASS
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            error += "灵敏度测试失败：\r\n";
                        }
                        else
                        {
                            error += "Sensitivity test failure：\r\n";
                        }
                        error += Status + " BER=" + ber.ToString("E6") + "\r\n";
                    }
                }
                else // lockFlag == 0 失步
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        error += "灵敏度测试失败(失步)：\r\n";
                    }
                    else
                    {
                        error += "Sensitivity test failure (no lock)：\r\n";
                    }
                    error += Status + "\r\n";
                }
            }
            else
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    error += "灵敏度测试失败,误码数据格式异常：\r\n";
                }
                else
                {
                    error += "Sensitivity test failure,Bit error exception：\r\n";
                }
                error += Status + "\r\n";
            }
        }
        catch (System.Exception parseEx)
        {
            if (GlobalVarFun.Language == "Chinese")
            {
                error += "灵敏度测试失败,误码数据解析异常：\r\n";
            }
            else
            {
                error += "Sensitivity test failure,Bit error parse exception：\r\n";
            }
            error += Status + " " + parseEx.Message + "\r\n";
        }
    }
    catch
    {
        error += "灵敏度测试失败,获取误码异常：\r\n";
        error += Status + "\r\n";
    }
    if (error == "") error = "PASS";
    result.message = "灵敏度测试：" + error;
    ModListBoxShow(this, result);

    return error == "PASS" ? "" : error;
}
#endregion