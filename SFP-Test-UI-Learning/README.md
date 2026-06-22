# SFP-Test-UI-Learning
C# WinForm 光模块(SFP/SFP+/SFP28)测试上位机 分步学习工程仓库
适配 VS2008 / .NET Framework 3.5

## 学习任务目录
1. 【01_Layout_Container_Demo】上位机基础布局专项
    知识点覆盖：
    - Dock 停靠、Anchor 锚定自适应原理
    - 五大布局容器：Panel / GroupBox / SplitContainer / TableLayoutPanel / FlowLayoutPanel
    - 上位机经典整体框架搭建（顶部按钮栏、左右分栏、参数表格、日志窗口、底部状态栏）
    - 日志自动滚屏封装、按钮交互基础逻辑
2. 【02_SerialPort_LearnDemo】串口通信核心专项（业务核心，上位机灵魂）
    项目简介：
    基于原生System.IO.Ports.SerialPort实现串口收发，无第三方库，适配SFP光模块测试仪RS232串口通信；
    搭配VSPD虚拟串口COM3<->COM4闭环自测，不需要真实硬件设备即可完整调试收发、报文解析。

    核心知识点全覆盖：
    1. 串口基础五参数配置：端口、波特率、数据位、停止位、校验位
    2. SerialPort完整生命周期：Open/Close打开释放端口、WriteLine发送指令
    3. DataReceived异步接收事件（后台子线程机制）
    4. Invoke跨线程UI更新（解决串口经典报错：跨线程操作无效）
    5. 字符串报文分割、截取、解析（模拟SFP电压/温度数据提取）
    6. 全局统一日志封装：时间戳、自动滚动到底部
    7. 异常捕获：端口占用、打开失败容错处理

    界面布局复用任务1知识点：
    FlowLayoutPanel顶部配置区、SplitContainer上下分栏、TableLayoutPanel参数面板、RichTextBox日志窗口，Dock+Anchor自适应缩放。

    测试流程：
    1. VSPD创建COM3 <-> COM4虚拟串口配对
    2. 双开VS2019，两份程序分别绑定COM3、COM4
    3. COM3下发读取指令 READ_SFP
    4. COM4回复标准报文 VOLT=3.28,TEMP=26.5
    5. COM3自动解析报文，填充电压、温度文本框，实时打印收发日志
后续规划：
3. 串口报文进阶：16进制收发、CRC校验、协议分包处理
4. SFP完整业务测试流程：自动循环测试、阈值判断
5. 本地日志保存、CSV测试报告导出
6. TCP Socket网络通信上位机（远程设备通信）

## 开发环境
IDE: Visual Studio 2019
Framework: .NET Framework 4.17.5