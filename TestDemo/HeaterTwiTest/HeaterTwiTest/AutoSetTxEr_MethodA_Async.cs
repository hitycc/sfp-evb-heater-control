#region  // 待测模块消光比自动调试   普通二分法  异步
        private async Task<bool> AutoSetTxEr_MethodA_Async()
        {
            UInt16 min = 0;
            UInt16 max = 0;
            UInt16 mod = 0;
            float er_target, result_err = 0;
            int looptime = 0;
            int millisecondsDelay = 2;

            switch (Dut)
            {
                case 1:
                    min = (UInt16)TestSet.txmod_Min;
                    max = (UInt16)TestSet.txmod_Max;
                    er_target = TestSet.txEr_target;
                    break;
                case 2:
                    min = (UInt16)TestSet2.txmod_Min;
                    max = (UInt16)TestSet2.txmod_Max;
                    er_target = TestSet2.txEr_target;
                    break;
                case 3:
                    min = (UInt16)TestSet3.txmod_Min;
                    max = (UInt16)TestSet3.txmod_Max;
                    er_target = TestSet3.txEr_target;
                    break;
                case 4:
                    min = (UInt16)TestSet4.txmod_Min;
                    max = (UInt16)TestSet4.txmod_Max;
                    er_target = TestSet4.txEr_target;
                    break;
                default:
                    min = (UInt16)TestSet.txmod_Min;
                    max = (UInt16)TestSet.txmod_Max;
                    er_target = TestSet.txEr_target;
                    break;
            }

            AddTestLog("erValMaxErr:" + erValMaxErr.ToString() + " er_target:" + er_target.ToString() + " MOdmin:" + min.ToString() + " Modmax:" + max.ToString());

            await switchSemaphore.WaitAsync();
            //lock (tx_lock)
            try
            {
                //光开关切换
                TestControl.opticalswitch.SetChannel(Dut);
                // 普通二分法查找
                do
                {
                    looptime++;

                    mod = (UInt16)((min + max) / 2);

                    if (mod < 3) return false; // 值异常

                    if (test.SetTxModBias(mod) == false) return false;
                    bool res = await Get_ERatio_DCA_Async(false);
                    if (res == false) return false;
                    //if (Get_ERatio_DCA(false) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            if (TestResult.txEr > 99)
                            {
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult.txEr - TestSet.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult.txEr.ToString() + " txEr_target:" + TestSet.txEr_target.ToString());
                            break;
                        case 2:
                            if (TestResult2.txEr > 99)
                            {
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult2.txEr - TestSet2.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult2.txEr.ToString() + " txEr_target:" + TestSet2.txEr_target.ToString());
                            break;
                        case 3:
                            if (TestResult3.txEr > 99)
                            {
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult3.txEr - TestSet3.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult3.txEr.ToString() + " txEr_target:" + TestSet3.txEr_target.ToString());
                            break;
                        case 4:
                            if (TestResult4.txEr > 99)
                            {
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult4.txEr - TestSet4.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult4.txEr.ToString() + " txEr_target:" + TestSet4.txEr_target.ToString());
                            break;
                        default:
                            break;
                    }

                    //
                    if (result_err > 0)
                    {
                        max = (UInt16)(mod - 1);
                    }
                    else
                    {
                        min = (UInt16)(mod + 1);
                    }
                    await Task.Delay(millisecondsDelay);
                } while ((Math.Abs(result_err) > erValMaxErr) && (max > min) && (looptime < 10));

                retutntxrxresult.mod = mod;
                switch (Dut)
                {
                    case 1:
                        TestResult.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txEr.ToString("F1"); // 界面显示
                        break;
                    case 2:
                        TestResult2.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet2.txEr_target.ToString("F1") + "/" + TestResult2.txEr.ToString("F1"); // 界面显示
                        break;
                    case 3:
                        TestResult3.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet3.txEr_target.ToString("F1") + "/" + TestResult3.txEr.ToString("F1"); // 界面显示
                        break;
                    case 4:
                        TestResult4.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet4.txEr_target.ToString("F1") + "/" + TestResult4.txEr.ToString("F1"); // 界面显示
                        break;
                    default:
                        break;
                }
                //
                if (Math.Abs(result_err) <= erValMaxErr) //(TestResult.txEr <= TestSet.txEr_Max) && (TestResult.txEr >= TestSet.txEr_Min)
                {
                    return true;
                }
                else
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (result_err > 0) retutntxrxresult.ErrorMessage += "ER：消光比大";
                        if (result_err < 0) retutntxrxresult.ErrorMessage += "ER：消光比小";
                    }
                    else
                    {
                        if (result_err > 0) retutntxrxresult.ErrorMessage += "ER: The extinction ratio is large";//ER：消光比大
                        if (result_err < 0) retutntxrxresult.ErrorMessage += "ER: The extinction ratio is small";//ER：消光比小
                    }
                    //
                    return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                //释放
                switchSemaphore.Release();
            }
        }
        #endregion