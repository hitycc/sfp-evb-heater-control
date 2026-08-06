private async Task <bool> Get_ERatio_DCA_Async(bool autoScale)
        {
            float tx_er = 0;
            await dcaSemaphore.WaitAsync();
            try
            {
                /*// 切换光开关到发射方向（模块Tx → ERM仪器）
                otp12.SW_SetRouteForModule(Dut, true);*/
                // 设置到ERM消光比模块槽位
                otp12.SetSlot("06");
                // 设置ERM信号速率（10G模块）
                otp12.ERM_SetRate(Dut, "10G");
                // 等待信号稳定
                await Task.Delay(2000);

                // 多次读取消光比，取有效值
                for (int i = 0; i < 5; i++)
                {
                    string erData = otp12.ERM_ReadERData(Dut);
                    if (!string.IsNullOrEmpty(erData))
                    {
                        // 返回格式: "power,er" 例如 "-9.001,12.001"
                        string[] parts = erData.Split(',');
                        if (parts.Length >= 2)
                        {
                            if (float.TryParse(parts[1].Trim(), out tx_er))
                            {
                                if (tx_er > 0 && tx_er <= 50)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    await Task.Delay(500);
                }

                // 异常值重试一次
                if ((tx_er > 50) || (tx_er < 0.5))
                {
                    await Task.Delay(500);
                    string erData = otp12.ERM_ReadERData(Dut);
                    if (!string.IsNullOrEmpty(erData))
                    {
                        string[] parts = erData.Split(',');
                        if (parts.Length >= 2)
                        {
                            float.TryParse(parts[1].Trim(), out tx_er);
                        }
                    }
                }

                // 加设备偏差值
                tx_er += (float)(GlobalVarFun.setup.dca_er_err);
                switch (Dut)
                {
                    case 1:
                        TestResult.txEr = tx_er;
                        break;
                    case 2:
                        TestResult2.txEr = tx_er;
                        break;
                    case 3:
                        TestResult3.txEr = tx_er;
                        break;
                    case 4:
                        TestResult4.txEr = tx_er;
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception exp)
            {
                AddTestLog("ERM消光比读取错误！" + exp.Message);
                switch (Dut)
                {
                    case 1:
                        TestResult.txEr = 0;
                        break;
                    case 2:
                        TestResult2.txEr = 0;
                        break;
                    case 3:
                        TestResult3.txEr = 0;
                        break;
                    case 4:
                        TestResult4.txEr = 0;
                        break;
                    default:
                        break;
                }
                return false;
            }
            finally
            {
                dcaSemaphore.Release();
            }
        }
        #endregion