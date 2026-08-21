using System;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace XFP模块测试程序
{
    public enum OpticalPowerMeterProtocol
    {
        PssAscii,
        HandheldBinary
    }

    public delegate double BerStatusParser(string status);
    public delegate bool EyeDataReader(out double tdecq, out double outerEr);

    // Adapts the two power-meter protocols used by the existing QSFP software.
    public sealed class SerialPowerMeterAdapter : IOpticalPowerMeter
    {
        private readonly SerialPort port;
        private readonly OpticalPowerMeterProtocol protocol;
        private readonly int channel;
        private readonly int delayMilliseconds;

        public SerialPowerMeterAdapter(SerialPort port,
            OpticalPowerMeterProtocol protocol, int channel, int delayMilliseconds)
        {
            this.port = port;
            this.protocol = protocol;
            this.channel = channel;
            this.delayMilliseconds = delayMilliseconds;
        }

        public double ReadPowerDbm()
        {
            if (port == null || !port.IsOpen)
                throw new InvalidOperationException("光功率计串口未打开。");

            if (protocol == OpticalPowerMeterProtocol.HandheldBinary)
                return ReadHandheldPower();
            return ReadPssPower();
        }

        private double ReadPssPower()
        {
            port.DiscardInBuffer();
            port.WriteLine("Read:Power Channel" + channel.ToString(CultureInfo.InvariantCulture));
            Thread.Sleep(delayMilliseconds);

            string response = port.ReadLine().Trim();
            double value;
            if (!double.TryParse(response, NumberStyles.Float,
                CultureInfo.InvariantCulture, out value))
                throw new FormatException("光功率计返回值无法解析: " + response);
            return value;
        }

        private double ReadHandheldPower()
        {
            byte[] command = new byte[] { 0xef, 0xef, 0x04, 0x04, 0x60, 0x06, 0x4c };
            byte[] response = new byte[14];
            port.DiscardInBuffer();
            port.Write(command, 0, command.Length);
            Thread.Sleep(delayMilliseconds);
            ReadExactly(response, 0, response.Length);

            if (response[0] != 0xed || response[1] != 0xfa)
                throw new InvalidOperationException("手持光功率计返回帧头错误。");

            int scale = 1;
            switch (response[9] & 0x30)
            {
                case 0x30: scale = 1000; break;
                case 0x20: scale = 100; break;
                case 0x10: scale = 10; break;
            }

            double raw = (response[7] * 256.0 + response[8]) / scale;
            switch (response[9] & 0x07)
            {
                case 1: return 10.0 * Math.Log10(raw + 1e-6);
                case 2: return 10.0 * Math.Log10(raw / 1000.0 + 1e-6);
                case 3: return 10.0 * Math.Log10(raw / 1000000.0 + 1e-6);
                case 4: return (raw - 9000.0) / 100.0;
                default: throw new InvalidOperationException("手持光功率计返回单位未知。");
            }
        }

        private void ReadExactly(byte[] buffer, int offset, int count)
        {
            int read = 0;
            while (read < count)
            {
                int n = port.Read(buffer, offset + read, count - read);
                if (n <= 0)
                    throw new TimeoutException("读取光功率计数据超时。");
                read += n;
            }
        }
    }

    public sealed class SerialAttenuatorAdapter : IOpticalAttenuator
    {
        private readonly SerialPort port;
        private readonly double delayPerDbMilliseconds;
        private double currentAttenuationDb;

        public SerialAttenuatorAdapter(SerialPort port,
            double delayPerDbMilliseconds, double initialAttenuationDb)
        {
            this.port = port;
            this.delayPerDbMilliseconds = delayPerDbMilliseconds;
            currentAttenuationDb = initialAttenuationDb;
        }

        public bool SetAttenuation(double value)
        {
            if (port == null || !port.IsOpen)
                return false;

            // TestQSFPER1 currently passes -6/-8. The old DOA driver expects
            // a positive attenuation value and sends a negative SCPI value.
            double attenuationDb = Math.Abs(value);
            if (attenuationDb > 60.0)
                return false;
            if (attenuationDb > 40.0)
                attenuationDb = 40.0;

            string command = "Configure:Atten channel1 -" +
                attenuationDb.ToString("F1", CultureInfo.InvariantCulture);
            port.WriteLine(command);
            int wait = (int)(delayPerDbMilliseconds *
                Math.Abs(currentAttenuationDb - attenuationDb)) + 200;
            Thread.Sleep(wait);
            currentAttenuationDb = attenuationDb;
            return true;
        }
    }

    public sealed class PssBerAnalyzer : IBerAnalyzer
    {
        private readonly SerialPort port;
        private readonly string channel;
        private readonly int delayMilliseconds;
        private readonly BerStatusParser parser;

        public string LastStatus { get; private set; }

        public PssBerAnalyzer(SerialPort port, string channel,
            int delayMilliseconds, BerStatusParser parser)
        {
            this.port = port;
            this.channel = channel;
            this.delayMilliseconds = delayMilliseconds;
            this.parser = parser;
        }

        public double ReadBer()
        {
            if (port == null || !port.IsOpen)
                throw new InvalidOperationException("BER 仪串口未打开。");
            if (parser == null)
                throw new InvalidOperationException("尚未配置 BER 返回值解析器。");

            port.WriteLine("Sense:Clear " + channel);
            Thread.Sleep(100);
            port.WriteLine("Status:Result? " + channel);
            Thread.Sleep(delayMilliseconds);
            LastStatus = port.ReadLine();
            return parser(LastStatus);
        }
    }

    public sealed class DelegateEyeAnalyzer : IEyeAnalyzer
    {
        private readonly EyeDataReader reader;

        public DelegateEyeAnalyzer(EyeDataReader reader)
        {
            this.reader = reader;
        }

        public bool ReadEyeData(out double tdecq, out double outerEr)
        {
            if (reader == null)
            {
                tdecq = double.NaN;
                outerEr = double.NaN;
                return false;
            }
            return reader(out tdecq, out outerEr);
        }
    }
}
