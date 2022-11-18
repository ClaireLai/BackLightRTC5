using System;
using GalvoScan.Hardware.Laser.ErrCode;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalvoScan.Hardware.Laser.YVO4
{
    class Laser_YVO4
    {
        #region Method

        public   RetErr Open()
        {
            RetErr ret = new RetErr();
            ret.Meg = "Laser Open";
            return ret;
        }
        public   RetErr Close()
        {
            RetErr ret = new RetErr();
            ret.Meg = "Laser Close";
            return ret;
        }
        public   RetErr OpenShutterAndDIODE(Boolean flag)
        {
            RetErr ret = new RetErr();
            ret.Meg = "Laser Open Shutter And DIODE";
            return ret;
        }
        public   RetErr SetPowerFreq(double Power, double Freq)
        {
            RetErr ret = new RetErr();
            ret.Meg = "Laser Set Power And Frequence";
            return ret;
        }

        public   RetErr SetLaserPara(double Power, double Freq, int mode, double bias, double epulse)
        {
            RetErr ret = new RetErr();
            ret.Meg = "Laser Set LaserPara";
            return ret;
        }
        public   RetErr CheckLaserReady(out Boolean ReadyOrNot)
        {
            RetErr ret = new RetErr();
            ReadyOrNot = true;
            ret.Meg = "Laser Check is Ready or Not";
            return ret;
        }
        public   RetErr CheckShutterAndDiodeOpened(out Boolean OpenOrNot)
        {
            RetErr ret = new RetErr();
            OpenOrNot = true;
            ret.Meg = "Laser Check Shtter and DIODE is Open or Not";
            return ret;
        }
        public   RetErr CheckLaserEnable(out Boolean Enable)
        {
            RetErr ret = new RetErr();
            Enable = true;
            ret.Meg = "Laser Check Laser is Enable or Not";
            return ret;
        }
        public   RetErr CheckLaserFault(out string FaultMeg)
        {
            RetErr ret = new RetErr();
            FaultMeg = "";
            ret.Meg = "Laser Check Laser Fault Meg";
            return ret;
        }

        #endregion
    }
}
