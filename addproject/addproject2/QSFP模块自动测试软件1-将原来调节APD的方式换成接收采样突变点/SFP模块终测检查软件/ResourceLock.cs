using System.Threading;

namespace SFP模块终测检查软件
{
    //===========================================================================
    // ResourceLock —— 多线程共享资源锁对象集合
    //
    // 【为什么需要这个？】
    // 多线程并行测试时，有些硬件设备是4个通道共享的（比如DCA眼图仪只有1台），
    // 同一时刻只能一个通道使用。C#的lock关键字需要一个object作为"锁令牌"，
    // 这里集中定义所有共享资源的锁对象。
    //
    // 【使用方法】
    //   lock (ResourceLock.DcaLock) {
    //       // 只有拿到锁的线程才能进入这里，其他线程会在lock处等待
    //       DoDcaMeasurement();
    //   }
    //   // 出了大括号自动释放锁，下一个等待的线程才能进入
    //
    // 【哪些设备需要加锁？】
    //   - DcaLock:      DCA眼图仪（GPIB连接，全局1台，必须锁）
    //   - DbLock:       SQL数据库连接（全局1个连接，必须锁）
    //
    // 【哪些设备不需要手动加锁？】
    //   - I2C/加热台:   SFP_EVB_Heater类内部已经有lock(this)，自动串行化
    //   - OTP12(VISA):  OTP12Driver.SendScpiToSlot()内部已有lock，自动串行化
    //   - 各通道独立VOA: 每个通道有自己的VOA通道，操作时通过SendScpiToSlot原子
    //                   发送命令，不需要额外加锁
    //===========================================================================

    public static class ResourceLock
    {
        /// <summary>DCA眼图仪锁（GPIB连接的Agilent 86100D，全局1台，测量眼图时必须锁）</summary>
        public static readonly object DcaLock = new object();

        /// <summary>数据库锁（SQL Server连接，写入记录时必须锁）</summary>
        public static readonly object DbLock = new object();

        /// <summary>
        /// 波形仪/波长计锁（Keysight 86120C等GPIB仪器，如果程序中有使用）
        /// 如果波长计是4通道共享的，使用前也需要加此锁。
        /// </summary>
        public static readonly object WavemeterLock = new object();
    }
}