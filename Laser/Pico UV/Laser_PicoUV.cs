using System;
using BackLight.Laser.ErrCode;
using BackLight.Laser.Define;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackLight.Laser.Pico_UV
{
    class Laser_PicoUV:ILaser
    {
        #region Method
        //task: open
        public override RetErr Open()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: close
        public override RetErr Close()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Open Shutter & DIODE
        public override RetErr OpenShutterAndDIODE(bool POpen)
        {
            RetErr ret = new RetErr();

            return ret;
        }


        //task: Set Power(%) & Freq(KHz) & Bias(V) & Mode(0-based) & SpotDelay(us)
        //note: 會設定ThermaTrack
        public override RetErr SetLaserPreset(double PPower, double PFreq,
                                                double PMove, double PBias,
                                                double PEpulse)
        {
            RetErr ret = new RetErr();

            return ret;
        }


        //task: Set Power(%) & Freq(KHz)
        //note: 會設定ThermaTrack
        public override RetErr SetPowerAndFreq(double PPower, double PFreq)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Check Laser Ready
        //note: 會確認All Temp Reached & All Servo Locked
        //retn: ready : OperOk
        //      Not Ready : AVIA355_LaserNotReady
        public override RetErr CheckLaserReady()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Check Shutter & DIODE Opened
        //retn: ready : OperOk
        //      Not Open : AVIA355_ShutterOrDIODENotOpened
        public override RetErr CheckShutterAndDIODEOpened()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Check Laser Enabled

        public override RetErr CheckLaserEnabled(out EEnabledState PEnabledState)
        {
            RetErr ret = new RetErr();
            PEnabledState = EEnabledState.esStandby;
            return ret;
        }

        //task: Check Laser Fault
        //retn: 有錯誤時回傳 AVIA355_LaserHardwearFault
        public override RetErr CheckLaserFault()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Set Mode
        //note: Mode 的定義隨機型不同
        public override RetErr SetMode(int pMode)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Set Bias Voltage
        //note: Bias Voltage 定義隨機型不同
        public override RetErr SetBiasVoltage(double pBiasVoltage)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Get Power Voltage
        public override RetErr GetPowerVoltage(out double pPowerVoltage)
        {
            RetErr ret = new RetErr();
            pPowerVoltage = -1;
            return ret;
        }

        //task: Get Bias Voltage
        public override RetErr GetBiasVoltage(out double pBiasVoltage)
        {
            RetErr ret = new RetErr();
            pBiasVoltage = -1;
            return ret;
        }

        //task: Set EPulse, in khz
        public override RetErr SetEPulse(double PEPulse)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: check if support EPulse
        public override RetErr HaveEPulse(out bool isSup)
        {
            RetErr ret = new RetErr();
            isSup = false;
            return ret;
        }

        //task: Set Etime
        public override RetErr SetETime(int etime)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Set patternbit
        public override RetErr SetPatternBin(int patbin)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: check if support warm-up
        public override RetErr HaveWarmUp(out bool isSup)
        {
            RetErr ret = new RetErr();
            isSup = false;
            return ret;
        }

        //task: check if warming up
        public override RetErr IsWarmingUp(int pSecs, out bool isOK)
        {
            RetErr ret = new RetErr();
            isOK = true;
            return ret;
        }

        //task: Standby after open
        public override RetErr Standby()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Shutdown after open
        public override RetErr Shutdown()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Set Gate
        public override RetErr SetGate(bool POpen)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Set Diode Current
        public override RetErr SetDiodeCurrent(double PDCurr)
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Get Hours until now
        public override RetErr GetHours(out int pHours)
        {
            RetErr ret = new RetErr();
            pHours = 0;
            return ret;
        }

        public override RetErr GetPatBins(out char asBins)
        {
            RetErr ret = new RetErr();
            asBins = '0';
            return ret;
        }

        //task: Check Hours Status
        public override RetErr CheckHours()
        {
            RetErr ret = new RetErr();

            return ret;
        }

        //task: Get Spot
        public override RetErr GetSpot(out int pSpot)
        {
            RetErr ret = new RetErr();
            pSpot = 0;
            return ret;
        }

        // task: Get Temperature

        public override RetErr GetTemperatrue(out double Temperature)
        {
            RetErr ret = new RetErr();
            Temperature = 0;
            return ret;
        }
        #endregion
    }
}
