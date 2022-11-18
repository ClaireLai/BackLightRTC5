using BackLight.Laser.Define;
using BackLight.Laser.ErrCode;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackLight.Laser
{
    class Laser_CoherentMatrix : ILaser
    {
        ///////////////////////////////////////////////////////
        ///                     Para                        ///
        ///////////////////////////////////////////////////////
        #region Parameter
        private SerialPort serialPort;
        #endregion

        ///////////////////////////////////////////////////////
        ///                   Callback                      ///
        ///////////////////////////////////////////////////////
        #region Callback

        #endregion

        ///////////////////////////////////////////////////////
        ///                     Method                      ///
        ///////////////////////////////////////////////////////
        #region Method

        #region Implement
        public RetErr Open()
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;

            try
            {
                serialPort = new SerialPort();
                serialPort.PortName = "COM1"; //須由外部設定
                serialPort.BaudRate = 9600;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                //serialPort.ReadTimeout = 500;
                //serialPort.WriteTimeout = 500;
                //serialPort.NewLine = "\r";

                if (!serialPort.IsOpen)
                    serialPort.Open();

                //將雷射ANA模式設為Disable
                cmd = "ANA=0" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr CheckLaserReady()
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            try
            {
                cmd = "?STA" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (res.Contains(":0;"))
                {
                    return ret;
                }               
                else  //not ready or return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr Standby()
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            try
            {
                cmd = "?STA" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (res.Contains("11:0;") || res.Contains("8:0;")) //Ready
                {
                    return ret;
                }
                else if (res.Contains("0:0;"))   //RampUpOscillator
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (res.Contains("12:0;")) //WarmUp
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else  //not ready or return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr CheckLaserEnabled(out EEnabledState PEnabledState)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            PEnabledState = EEnabledState.esStandbyByFault;
            try
            {
                cmd = "?L" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (res.Contains("1:0;"))  //Enable
                {
                    PEnabledState = EEnabledState.esEnabled;
                    return ret;
                }
                else if (res.Contains("0:0;"))   //Disable
                {
                    if (CheckLaserReady().flag)     //Disable but ready
                    {
                        PEnabledState = EEnabledState.esStandby;
                    }
                    else                            //Disable and not ready, should check status
                    {
                        PEnabledState = EEnabledState.esStandbyByFault;
                    }
                }
                else  //not ready or return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        /// <summary>
        /// 模式:
        /// n=0 CW
        /// n = 1 Single shot
        /// n=2 Single shot external timing - Pulse Track
        /// n=3 Gated operation
        /// n=4 Burst operation
        /// n=5 Continuous pulsing
        /// n=6 Not use
        /// n=7 Single shot PulseEQ
        /// n = 8 Gated operation ThermEQ
        /// n = 9 Burst operation ThermEQ
        /// </summary>
        /// <param name="pMode"></param>
        /// <returns></returns>
        public RetErr SetMode(int pMode)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            try
            {
                //將雷射Disable，才可以設定模式
                cmd = "L=0" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }

                //確認狀態已經回到Standby
                cmd = "?STA" + Environment.NewLine;
                DateTime sendTime = DateTime.Now;
                do
                {
                    isTimeOut = !sendCmd(cmd, out res);
                    if(isTimeOut)
                    {
                        string funcName = MethodBase.GetCurrentMethod().Name;
                        ret.flag = false;
                        ret.Meg = funcName + " : Timeout, return => " + res;
                        ret.Num = Num._CheckLaserReady;
                    }

                    if ((DateTime.Now - sendTime) > TimeSpan.FromSeconds(10))
                        break;

                    Thread.Sleep(10);

                } while (!res.Contains("11:0;"));

                //設定模式
                if (pMode < 0 || pMode > 9)
                {

                }
                cmd = "QM=" + pMode.ToString() + Environment.NewLine;
                isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }

                //將雷射Enable
                cmd = "L=1" + Environment.NewLine;
                isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr OpenShutterAndDIODE(bool POpen)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            try
            {
                cmd = (POpen ? "L=1" : "L=0") + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._OpenShutterAndDIODE;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PPower">需搭配laser table 做轉換，這部分還沒實作</param>
        /// <param name="PFreq"></param>
        /// <returns></returns>
        public RetErr SetPowerAndFreq(double PPower, double PFreq)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            try
            {
                //Set power
                if (PPower < 0 || PPower > 100)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : power over range. power =>" + PPower.ToString();
                    ret.Num = Num._OpenShutterAndDIODE;
                }
                cmd = "ANAINT=" + PPower.ToString() + Environment.NewLine;  //這邊給的power需要有table轉換(要透過外部ini轉換，可以參考C++版本)
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._OpenShutterAndDIODE;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }

                //Set freq
                //這邊的上下線由外部給(ini檔)
                if (PFreq < 0 || PFreq > 100)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : power over range. power =>" + PPower.ToString();
                    ret.Num = Num._OpenShutterAndDIODE;
                }
                cmd = "RR=" + PFreq.ToString() + "000" + Environment.NewLine;
                isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._OpenShutterAndDIODE;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr HaveWarmUp(out bool isSup)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;

            isSup = false;

            try
            {
                cmd = "?W" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    isSup = false;

                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
                else if (res.Contains("0:0;"))   //Warm up
                {
                    isSup = false;
                }
                else if (res.Contains("1:0;"))   //Standby / Ready
                {
                    isSup = true;
                }
                else if (res.Contains("2:0;"))  //Operating within specification
                {
                    isSup = true;
                }
                else  //not ready or return error
                {
                    isSup = false;

                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Error, return => " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr CheckShutterAndDIODEOpened()
        {

            RetErr ret = new RetErr();
            string cmd;
            string res;

            bool shutter = false;
            bool diode = false;
            try
            {
                //確認Diode (laser enable)
                cmd = "?L" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (res.Contains("1:0;"))  //Enable
                {
                    diode = true;
                }
                else if (res.Contains("0:0;"))   //Disable
                {
                    diode = false;

                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else  //not ready or return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }

                //確認Shutter (gate)
                cmd = "?GATE" + Environment.NewLine;
                isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (res.Contains("1:0;"))  //Enable
                {
                    shutter = true;
                }
                else if (res.Contains("0:0;"))   //Disable
                {
                    shutter = false;

                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else  //not ready or return error
                {
                    shutter = false;

                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }

                if (shutter && diode)
                    return ret;
                else
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr SetGate(bool POpen)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            try
            {
                cmd = (POpen ? "GATE=1" : "GATE=0") + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._OpenShutterAndDIODE;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr Close()
        {
            RetErr ret = new RetErr();

            try
            {
                ret = this.Shutdown();
                serialPort.Close();
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr GetPowerVoltage(out double pPowerVoltage)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;

            pPowerVoltage = 0;
            try
            {
                //確認Diode (laser enable)
                cmd = "?P" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else
                {
                    pPowerVoltage = Convert.ToDouble(res.Split(':')[0]);
                }

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr GetTemperatrue(out double Temperature)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;

            Temperature = 0;
            try
            {
                //確認Diode (laser enable)
                cmd = "?T0" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else
                {
                    Temperature = Convert.ToDouble(res.Split(':')[0]);
                }

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr SetDiodeCurrent(double PDCurr)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;
            try
            {
                cmd = "CIP=" + PDCurr.ToString() + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._OpenShutterAndDIODE;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : " + res;
                    ret.Num = Num._CheckLaserReady;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr CheckHours()
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;

            double hours;

            try
            {
                //確認Diode (laser enable)
                cmd = "?OPH1" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else
                {
                    hours = Convert.ToDouble(res.Split(':')[0]);
                }

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr GetHours(out int pHours)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;

            pHours = 0;

            try
            {
                //確認Diode (laser enable)
                cmd = "?OPH1" + Environment.NewLine;
                bool isTimeOut = !sendCmd(cmd, out res);
                if (isTimeOut)
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else if (!res.Contains(":0;")) //return error
                {
                    string funcName = MethodBase.GetCurrentMethod().Name;
                    ret.flag = false;
                    ret.Meg = funcName + " : Timeout, return => " + res;
                    ret.Num = Num._CheckLaserReady;

                    return ret;
                }
                else
                {
                    pHours = (int)double.Parse(res.Split(':')[0]);
                }

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr IsWarmingUp(int pSecs, out bool isOK)
        {
            RetErr ret = new RetErr();
            string cmd;
            string res;

            isOK = false;

            try
            {
                DateTime dt = DateTime.Now;
                while (isOK == false)
                {
                    cmd = "?W" + Environment.NewLine;
                    bool isTimeOut = !sendCmd(cmd, out res);
                    if (isTimeOut)
                    {
                        isOK = false;

                        string funcName = MethodBase.GetCurrentMethod().Name;
                        ret.flag = false;
                        ret.Meg = funcName + " : Timeout, return => " + res;
                        ret.Num = Num._CheckLaserReady;
                    }
                    else if (res.Contains("0:0;"))   //Warm up
                    {
                        isOK = false;
                    }
                    else if (res.Contains("1:0;"))   //Standby / Ready
                    {
                        isOK = true;
                    }
                    else if (res.Contains("2:0;"))  //Operating within specification
                    {
                        isOK = true;
                    }
                    else  //not ready or return error
                    {
                        isOK = false;

                        string funcName = MethodBase.GetCurrentMethod().Name;
                        ret.flag = false;
                        ret.Meg = funcName + " : Error, return => " + res;
                        ret.Num = Num._CheckLaserReady;
                    }

                    if ((DateTime.Now - dt).Seconds > pSecs)
                        break;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }
            return ret;
        }

        public RetErr SetLaserPreset(double PPower, double PFreq, double PMove, double PBias, double PEpulse)
        {
            RetErr ret = new RetErr();

            try
            {
                ret = this.SetPowerAndFreq(PPower, PFreq);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr Shutdown()
        {
            RetErr ret = this.OpenShutterAndDIODE(false);
            return ret;
        }
        #endregion



        #region Not Implement        

        public RetErr CheckLaserFault()
        {
            RetErr ret = new RetErr();

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr GetBiasVoltage(out double pBiasVoltage)
        {
            RetErr ret = new RetErr();
            pBiasVoltage = 0;

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr GetPatBins(out char asBins)
        {
            RetErr ret = new RetErr();
            asBins = ' ';

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr GetSpot(out int pSpot)
        {
            RetErr ret = new RetErr();
            pSpot = 0;

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr HaveEPulse(out bool isSup)
        {
            RetErr ret = new RetErr();
            isSup = false;

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }        

        public RetErr SetBiasVoltage(double pBiasVoltage)
        {
            RetErr ret = new RetErr();

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr SetEPulse(double PEPulse)
        {
            RetErr ret = new RetErr();

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

        public RetErr SetETime(int etime)
        {
            RetErr ret = new RetErr();

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }        

        public RetErr SetPatternBin(int patbin)
        {
            RetErr ret = new RetErr();

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }

       

        
        #endregion

        private RetErr func()   //樣板
        {
            RetErr ret = new RetErr();

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CheckLaserEnabled;
            }

            return ret;
        }
        #endregion

        ///////////////////////////////////////////////////////
        ///                     MyFunc                      ///
        ///////////////////////////////////////////////////////
        #region My function


        public bool sendCmd(string cmd, out string recv)
        {
            //送出Command
            serialPort.Write(cmd);

            //等待接收資料
            recv = "";
            DateTime sendTime = DateTime.Now;
            while (!recv.Contains(";"))
            {
                recv += serialPort.ReadExisting();
                if ((DateTime.Now - sendTime) > TimeSpan.FromSeconds(5))
                {
                    return false;
                }
                Thread.Sleep(10);
            }
            return true;
        }
        #endregion
    }
}
