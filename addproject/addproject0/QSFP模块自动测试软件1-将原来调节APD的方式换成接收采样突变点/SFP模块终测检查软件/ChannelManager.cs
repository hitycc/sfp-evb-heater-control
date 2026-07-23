using System;
using System.Threading;

namespace SFP模块终测检查软件
{
    //===========================================================================
    // ChannelManager —— 4通道管理器
    //
    // 集中管理4个通道的ChannelContext对象。
    // UI线程和测试线程都通过这里获取各通道的数据。
    //
    // 使用示例：
    //   ChannelContext ch0 = ChannelManager.GetChannel(0);  // 获取通道1的数据
    //   int count = ChannelManager.ChannelCount;            // = 4
    //   ChannelManager.ResetAll();                          // 重置所有通道
    //===========================================================================

    public static class ChannelManager
    {
        /// <summary>并行测试通道总数（4通道光模块测试平台）</summary>
        public const int ChannelCount = 4;

        private static readonly ChannelContext[] _channels;

        // 静态构造函数：程序启动时初始化4个通道上下文
        static ChannelManager()
        {
            _channels = new ChannelContext[ChannelCount];
            for (int i = 0; i < ChannelCount; i++)
            {
                _channels[i] = new ChannelContext(i);
            }
        }

        /// <summary>
        /// 显式初始化通道管理器
        /// 在Main_Form_Load或Program.Main中调用，确保4个通道上下文已创建，
        /// 并为每个通道设置UI线程同步上下文（用于跨线程安全更新UI）。
        /// </summary>
        public static void Initialize()
        {
            // 静态构造函数已经创建了4个ChannelContext实例，
            // 这里只需要确保UI同步上下文被设置
            SynchronizationContext uiCtx = SynchronizationContext.Current;
            if (uiCtx != null)
            {
                for (int i = 0; i < ChannelCount; i++)
                {
                    _channels[i].UISyncContext = uiCtx;
                }
            }
        }

        /// <summary>获取指定通道的上下文（index: 0~3）</summary>
        public static ChannelContext GetChannel(int index)
        {
            if (index < 0 || index >= ChannelCount)
                throw new ArgumentOutOfRangeException("index",
                    string.Format("通道索引必须在0~{0}之间，当前值: {1}", ChannelCount - 1, index));
            return _channels[index];
        }

        /// <summary>获取所有通道的上下文数组</summary>
        public static ChannelContext[] GetAllChannels()
        {
            return _channels;
        }

        /// <summary>重置所有通道数据（"开始测试"按钮点击时调用）</summary>
        public static void ResetAll()
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                _channels[i].Reset();
            }
        }

        /// <summary>停止所有正在运行的测试</summary>
        public static void StopAll(ChannelTester[] testers)
        {
            if (testers == null) return;
            for (int i = 0; i < testers.Length; i++)
            {
                if (testers[i] != null)
                {
                    testers[i].StopTest();
                }
            }
        }

        /// <summary>检查是否有任意通道正在测试</summary>
        public static bool IsAnyRunning(ChannelTester[] testers)
        {
            if (testers == null) return false;
            for (int i = 0; i < testers.Length; i++)
            {
                if (testers[i] != null && testers[i].IsRunning)
                    return true;
            }
            return false;
        }

        /// <summary>获取当前正在测试的通道数量</summary>
        public static int GetRunningCount(ChannelTester[] testers)
        {
            if (testers == null) return 0;
            int count = 0;
            for (int i = 0; i < testers.Length; i++)
            {
                if (testers[i] != null && testers[i].IsRunning)
                    count++;
            }
            return count;
        }
    }
}