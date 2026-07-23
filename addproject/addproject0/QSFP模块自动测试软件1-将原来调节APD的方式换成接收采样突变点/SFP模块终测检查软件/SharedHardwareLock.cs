using System;

namespace FibertopTest_Common
{
    /// <summary>
    /// 共享硬件资源的线程同步锁管理器
    /// 
    /// 【为什么需要这个类？】
    /// 在多线程测试中，4个模块线程会同时运行，但有些硬件设备只有1个：
    ///   - 光开关（opticalSwitch串口）— 1个，4个模块争抢
    ///   - 波长计（Keysight 86120C）— 1个，4个模块争抢  
    ///   - 示波器（DCAX-86100眼图仪）— 1个，4个模块争抢
    /// 
    /// 如果不加锁，4个线程同时往同一个串口发命令，命令会混在一起导致设备报错。
    /// 加锁后，同一时刻只有1个线程能操作该设备，其他线程排队等待。
    /// 
    /// 【使用方法】
    /// 在需要操作共享设备的地方，用 lock 包住：
    /// <code>
    /// lock (SharedHardwareLocks.OpticalSwitchLock)
    /// {
    ///     GlobalVarFun.opticalSwitch.WriteLine(command);
    ///     string response = GlobalVarFun.opticalSwitch.ReadLine();
    /// }
    /// </code>
    /// </summary>
    public static class SharedHardwareLocks
    {
        /// <summary>
        /// 光开关串口的锁对象
        /// 所有操作 opticalSwitch 的代码都要用这个锁包住
        /// </summary>
        public static readonly object OpticalSwitchLock = new object();

        /// <summary>
        /// 波长计（Keysight 86120C）的锁对象
        /// 所有操作 kt86120c 的代码都要用这个锁包住
        /// </summary>
        public static readonly object WavelengthMeterLock = new object();

        /// <summary>
        /// 示波器/眼图仪（DCAX-86100）的锁对象
        /// 所有操作眼图仪的代码都要用这个锁包住
        /// </summary>
        public static readonly object OscilloscopeLock = new object();

        /// <summary>
        /// 光功率计/衰减器（OTP12Driver）的锁对象
        /// 如果OTP12Driver不支持多通道并发操作，需要用这个锁
        /// </summary>
        public static readonly object OtpDriverLock = new object();

        /// <summary>
        /// 日志写入锁 — 防止多线程同时写日志导致文本交错
        /// </summary>
        public static readonly object LogLock = new object();

        /// <summary>
        /// SQL数据库写入锁 — 防止多线程同时写数据库导致连接冲突
        /// </summary>
        public static readonly object SqlLock = new object();

        /// <summary>
        /// 全局状态访问锁 — 用于保护需要临时读写的全局变量
        /// （主要用于非ThreadStatic的全局变量访问）
        /// </summary>
        public static readonly object GlobalStateLock = new object();

        /// <summary>
        /// I2C总线锁 — SFP/QSFP模块的I2C读写是共享总线，同一时刻只能一个线程访问
        /// </summary>
        public static readonly object I2CLock = new object();
    }
}
