using System;

namespace SFP模块终测检查软件
{
    //===========================================================================
    // HardwareMap —— 硬件通道映射表
    //
    // 根据实际硬件连接定义4个模块对应的OTP12槽位和通道号。
    //
    // OTP12 槽位布局（从截图确认）：
    //   SLOT-01~04: BE2-03   （误码仪，通过加热台TCP访问）
    //   SLOT-05:    OPM-04   （光功率计，4通道共享仪器）
    //   SLOT-06:    ERM-04   （消光比仪，4通道共享仪器）
    //   SLOT-07:    VOA-02   （发射衰减器: ch1=模块1, ch2=模块2）
    //   SLOT-08:    VOA-02   （发射衰减器: ch1=模块3, ch2=模块4）
    //   SLOT-09:    VOA-02   （接收衰减器: ch1=模块1, ch2=模块2）
    //   SLOT-10:    VOA-02   （接收衰减器: ch1=模块3, ch2=模块4）
    //   SLOT-11:    SWD2-02  （光开关: 模块1用in1/out2, 模块2用in3/out4）
    //   SLOT-12:    SWD2-02  （光开关: 模块3用in1/out2, 模块4用in3/out4）
    //
    // 【重要】SLOT-05(OPM)和SLOT-06(ERM)是共享仪器！一次只能测一个模块，
    // 切光开关到哪个模块，就读哪个通道的数据。
    //===========================================================================

    /// <summary>VOA衰减器位置信息</summary>
    public class VoaLocation
    {
        public string Slot;   // OTP12槽位号，如 "07"
        public int Channel;   // VOA通道号 1或2
        public VoaLocation(string slot, int ch) { Slot = slot; Channel = ch; }
    }

    /// <summary>光开关位置信息</summary>
    public class SwitchLocation
    {
        public string Slot;   // OTP12槽位号，如 "11"
        public int InCh;      // 输入通道
        public int OutCh;     // 输出通道
        public SwitchLocation(string slot, int inCh, int outCh) { Slot = slot; InCh = inCh; OutCh = outCh; }
    }

    public static class HardwareMap
    {
        //=======================================================================
        // 模块索引说明：
        //   moduleIndex = 1~4 （对应加热台槽位1~4，也是ChannelIndex+1）
        //   本项目内部通道索引 channelIndex = 0~3 = moduleIndex - 1
        //=======================================================================

        /// <summary>
        /// 获取模块对应的加热台槽位号（1~4）
        /// 直接就是 moduleIndex，与 ChannelTester.SlotNumber 一致
        /// </summary>
        public static int GetHeaterSlot(int channelIndex)
        {
            return channelIndex + 1; // 0→1, 1→2, 2→3, 3→4
        }

        /// <summary>
        /// 获取模块对应"发射端VOA衰减器"的位置
        /// SLOT-07: ch1→模块1, ch2→模块2
        /// SLOT-08: ch1→模块3, ch2→模块4
        /// </summary>
        public static VoaLocation GetTxVoa(int channelIndex)
        {
            switch (channelIndex)
            {
                case 0: return new VoaLocation("07", 1);
                case 1: return new VoaLocation("07", 2);
                case 2: return new VoaLocation("08", 1);
                case 3: return new VoaLocation("08", 2);
                default: throw new ArgumentException("无效的通道索引: " + channelIndex);
            }
        }

        /// <summary>
        /// 获取模块对应"接收端VOA衰减器"的位置
        /// SLOT-09: ch1→模块1, ch2→模块2
        /// SLOT-10: ch1→模块3, ch2→模块4
        /// </summary>
        public static VoaLocation GetRxVoa(int channelIndex)
        {
            switch (channelIndex)
            {
                case 0: return new VoaLocation("09", 1);
                case 1: return new VoaLocation("09", 2);
                case 2: return new VoaLocation("10", 1);
                case 3: return new VoaLocation("10", 2);
                default: throw new ArgumentException("无效的通道索引: " + channelIndex);
            }
        }

        /// <summary>
        /// 获取模块对应"发射端光开关"的配置
        /// 光路方向: 模块Tx口 → 光开关 → OPM/ERM仪器
        /// SLOT-11: 模块1(in1→out2), 模块2(in3→out4)
        /// SLOT-12: 模块3(in1→out2), 模块4(in3→out4)
        /// </summary>
        public static SwitchLocation GetTxSwitch(int channelIndex)
        {
            switch (channelIndex)
            {
                case 0: return new SwitchLocation("11", 1, 2);
                case 1: return new SwitchLocation("11", 3, 4);
                case 2: return new SwitchLocation("12", 1, 2);
                case 3: return new SwitchLocation("12", 3, 4);
                default: throw new ArgumentException("无效的通道索引: " + channelIndex);
            }
        }

        /// <summary>
        /// 获取模块对应"接收端光开关"的配置
        /// 光路方向: 光源/衰减器 → 光开关 → 模块Rx口
        /// 与发射端方向相反（in/out对调）
        /// </summary>
        public static SwitchLocation GetRxSwitch(int channelIndex)
        {
            switch (channelIndex)
            {
                case 0: return new SwitchLocation("11", 2, 1);
                case 1: return new SwitchLocation("11", 4, 3);
                case 2: return new SwitchLocation("12", 2, 1);
                case 3: return new SwitchLocation("12", 4, 3);
                default: throw new ArgumentException("无效的通道索引: " + channelIndex);
            }
        }

        /// <summary>
        /// 获取OPM（光功率计）对应的槽位和通道
        /// OPM-04在SLOT-05，4个通道对应模块1~4
        /// 【注意】OPM是共享资源，读哪个通道不需要切光开关（电信号选通）
        /// </summary>
        public static void GetOpm(int channelIndex, out string slot, out int ch)
        {
            slot = "05";
            ch = channelIndex + 1; // 0→1, 1→2, 2→3, 3→4
        }

        /// <summary>
        /// 获取ERM（消光比仪）对应的槽位和通道
        /// ERM-04在SLOT-06，4个通道对应模块1~4
        /// 【注意】ERM是共享资源
        /// </summary>
        public static void GetErm(int channelIndex, out string slot, out int ch)
        {
            slot = "06";
            ch = channelIndex + 1; // 0→1, 1→2, 2→3, 3→4
        }

        /// <summary>
        /// 获取BE2（模块I2C控制板）对应的槽位
        /// BE2-03在SLOT-01~04，对应模块1~4
        /// 【注意】I2C通信通过加热台TCP，不经过OTP12的BE2板。
        /// 这个映射仅用于通过OTP12查询BE2板状态（如果需要）。
        /// </summary>
        public static string GetBe2Slot(int channelIndex)
        {
            // 0→"01", 1→"02", 2→"03", 3→"04"
            return (channelIndex + 1).ToString("D2");
        }
    }
}