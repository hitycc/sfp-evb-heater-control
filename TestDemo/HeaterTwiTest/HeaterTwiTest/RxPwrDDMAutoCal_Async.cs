private async Task<bool> RxPwrDDMAutoCal_Async()
        {

            ReturnReuslt result = new ReturnReuslt();
            string slotStr= VOArxDutToSlot[Dut];
            otp12.SetSlot(slotStr);
            //先关闭ALC自动功率跟踪（ALC开启时会自动调节衰减，手动设置会被覆盖）
            otp12.VOA_SetAlcState(Dut, "OFF");
            //设置工作模式为衰减模式（而非功率模式POWer）
            otp12.VOA_SetMode(Dut, "ATTenuation");
            //设置操作模式为绝对值模式ABSolute（而非参考值模式REFerence）
            otp12.VOA_SetApMode(Dut, "ABSolute");
            //打开输出光路（确保光路上有输出）
            otp12.VOA_SetOutputState(VoaChannel, "ON");
            //设置TXSFP光源1
            await Task.Delay(waittimes);
            switch (Dut)
            {
                case 1:
                    //设置衰减值DOA.rxCalAtt[0]
                    otp12.VOA_SetAttenuation(Dut,DOA.rxCalAtt[0]);
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    retutntxrxresult.RxRealPowers[0] = TestSet.rxPwr_Cal[0];
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut,DOA2.rxCalAtt[0]);
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA2.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    retutntxrxresult.RxRealPowers[0] = TestSet2.rxPwr_Cal[0];
                    break;
                case 3:
                    //
                    otp12.VOA_SetAttenuation(Dut,DOA3.rxCalAtt[0]);
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    retutntxrxresult.RxRealPowers[0] = TestSet.rxPwr_Cal[0];
                    break;
                case 4:
                    //
                    otp12.VOA_SetAttenuation(Dut,DOA4.rxCalAtt[0]);
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    retutntxrxresult.RxRealPowers[0] = TestSet.rxPwr_Cal[0];
                    break;
            }
            
            rxAdc[0] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[0] = rxAdc[0];

            ModListBoxShow(this, result);
            await Task.Delay(waittimes);
            //设置TXSFP光源2
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut,DOA.rxCalAtt[1]);
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut,DOA2.rxCalAtt[1]);
                    retutntxrxresult.RxRealPowers[1] = TestSet2.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA2.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut,DOA3.rxCalAtt[1]);
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut,DOA4.rxCalAtt[1]);
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
            }
            
            rxAdc[1] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[1] = rxAdc[1];
            retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Cal[1];
            result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
            ModListBoxShow(this, result);

            //设置TXSFP光源3
            await Task.Delay(waittimes);
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut,DOA.rxCalAtt[2]);
                    result.message = "设置TXSFP光源3：Success" + " AttVal" + DOA.rxCalAtt[2].ToString() + " rxAdc:" + rxAdc[2].ToString();
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut,DOA2.rxCalAtt[2]);
                    result.message = "设置TXSFP光源3：Success" + " AttVal" + DOA2.rxCalAtt[2].ToString() + " rxAdc:" + rxAdc[2].ToString();
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut,DOA3.rxCalAtt[2]);
                    result.message = "设置TXSFP光源3：Success" + " AttVal" + DOA.rxCalAtt[2].ToString() + " rxAdc:" + rxAdc[2].ToString();
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut,DOA4.rxCalAtt[2]);
                    result.message = "设置TXSFP光源3：Success" + " AttVal" + DOA.rxCalAtt[2].ToString() + " rxAdc:" + rxAdc[2].ToString();
                    break;
            }
            opticaldoaatt.SetAttenuation(DOA.rxCalAtt[2]);
            rxAdc[2] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[2] = rxAdc[2];
           
            ModListBoxShow(this, result);
            await Task.Delay(waittimes);
            if (GlobalVarFun.setup.rx_apd_cal) // APD 检查后面2个点
            {
                //设置TXSFP光源4
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut,DOA.rxCalAtt[3]);
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut,DOA2.rxCalAtt[3]);
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut,DOA3.rxCalAtt[3]);
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut,DOA4.rxCalAtt[3]);
                        break;
                }
                
                rxAdc[3] = test.GetRxADC();
                retutntxrxresult.RxddmPowers[3] = rxAdc[3];

                //设置TXSFP光源5
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxCalAtt[4]);
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxCalAtt[4]);
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxCalAtt[4]);
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxCalAtt[4]);
                        break;
                }
                
                rxAdc[4] = test.GetRxADC();
                retutntxrxresult.RxddmPowers[4] = rxAdc[4];
            }

            //设置TXSFP光源 为无光状态
            await Task.Delay(waittimes);
            opticaldoaatt.SetAttenuation(60);
            rxAdc[5] = test.GetRxADC();
            rxAdc[5] += 3; //加大 预防跳动问题
            retutntxrxresult.RxddmPowers[5] = rxAdc[5];
            result.message = "设置TXSFP光源 为无光状态：Success" + " AttVal" + "60" + " rxAdc:" + rxAdc[5].ToString();

            if (rxAdc[5] > 63) // 最大63
            {
                rxAdc[5] = 63;
                retutntxrxresult.ErrorMessage += "++" + "无光采样值超出最大限制";
                retutntxrxresult.RxddmPowers[0] = rxAdc[5];
                result.message = "设置TXSFP光源 为无光状态：Fail " + retutntxrxresult.ErrorMessage;
            }
            switch (Dut)
            {
                case 1:
                    TestResult.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                case 2:
                    TestResult2.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                case 3:
                    TestResult3.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                case 4:
                    TestResult4.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                default:
                    break;
            }
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
            //计算校准参数
            if (CulRxCalPar() == false)
            {
                result.message = "计算校准参数：Fail";
                ModListBoxShow(this, result);
                return false;
            }
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
            // 写入校准参数到模块
            if (test.WriteRxCalData() == false)
            {
                result.message = "写入校准参数到模块：Fail";
                ModListBoxShow(this, result);
                return false;
            }
            return true;
        }