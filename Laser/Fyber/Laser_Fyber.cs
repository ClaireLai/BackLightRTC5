#define Log
using System;
using GalvoScan.Hardware.Laser.ErrCode;
using GalvoScan.Hardware.Laser.Define;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GalvoScan.Hardware.Laser.Fyber
{
    class Laser_Fyber:ILaser
    {
        ///////////////////////////////////////////////
        ///                 Para                    ///
        ///////////////////////////////////////////////

        #region Para

        // Rs232
        SerialPort gPort;
        Define.Info gInfo;

        // Recive Data
        string gRecData = "";
        bool isRecDone = false;

        // Math
        Software.Math.Laser.IMathLaser IMath = new Software.Math.Laser.Fyber.Math_Laser_Fyber();

        // stopwathc
        Stopwatch Timer = new Stopwatch();

        #endregion

        ///////////////////////////////////////////////
        ///                 SubRouting              ///
        ///////////////////////////////////////////////

        #region Subrouting

        void ReciveData(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Byte[] buffer = new Byte[gPort.BytesToRead];
            gPort.Read(buffer, 0, gPort.BytesToRead);

            gRecData = gRecData + Encoding.ASCII.GetString(buffer);
            isRecDone = true;
        }

        void SendData(string cmd)
        {
            if (!gPort.IsOpen)
                return;
            gPort.Write(cmd);
            gRecData = "";
            isRecDone = false;
        }

        bool Cmd(string cmd, out string Rec)
        {
            Rec = "";
            try
            {
                gRecData = "";
                // Set
                string _cmd = cmd ;
                gPort.Write(_cmd);
                Thread.Sleep(10);
                Log.Pushlist("", _cmd);
                Timer.Restart();
                while (!isRecDone) 
                { 
                    Application.DoEvents();
                    Timer.Stop();
                    if (Timer.ElapsedMilliseconds > 5000)   // 超過五秒 段開
                        break;
                }
                Rec = gRecData.Replace("\r\n", "");
            }
            catch(Exception ee)
            {
                return false;
            }
            return true;
        }

        #endregion


        ///////////////////////////////////////////////
        ///                 Method                  ///
        ///////////////////////////////////////////////
        #region Method

        //task: open
        public override RetErr Open()
        {
            RetErr ret = new RetErr();
            try
            {
                #region ReadData

                string path = Application.StartupPath + "\\SetFile\\PL_SPL.ini";
                if (!System.IO.File.Exists(path))
                {
                    // 建立檔案
                    gInfo = new Define.Info();
                    string s = JsonConvert.SerializeObject(gInfo, Formatting.Indented);
                    System.IO.StreamWriter strw = new System.IO.StreamWriter(path, false, Encoding.Default);
                    strw.Write(s);
                    strw.Close();
                    throw new Exception("Can't Find file : " + path);
                }
                System.IO.StreamReader strr = new System.IO.StreamReader(path, Encoding.Default);
                string ori = strr.ReadToEnd();
                strr.Close();

                gInfo = JsonConvert.DeserializeObject<Define.Info>(ori);

                #endregion

                #region PortSetup

                gPort = new SerialPort();
                gPort.BaudRate = gInfo.Baud;
                gPort.PortName = "COM" + gInfo.Port.ToString();
                gPort.DataBits = 8;
                gPort.StopBits = StopBits.One;

                gPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(ReciveData);

                #endregion

                #region Connect Port


                gPort.Open();
                System.Threading.Thread.Sleep(100);
                int connetTimes = 0;
                while (!gPort.IsOpen)
                {
                    gPort.Open();
                    System.Threading.Thread.Sleep(100);
                    if (gPort.IsOpen)
                        break;

                    if (connetTimes == 10)
                        throw new Exception("Can not Connect SPI Laser Serial Port ");
                    connetTimes++;
                }

                #endregion

                #region Set interFaceMode 5:hardwareMode, 0:softwareMode

                #region ControlMode

                string ControlMode =  gInfo.IntFacMode < 8 && gInfo.IntFacMode >= 0 ? 
                                        gInfo.IntFacMode.ToString() : "0";
                if (gInfo.IntFacMode == 0)
                    return ret;

                //// Set
                //string cmd = "SM "+ ControlMode + "\r\n";
                //bool r = Cmd(cmd, out string rec) ? true : throw new Exception("Cmd fail:" + cmd);
                //if (rec.Contains("E"))
                //    throw new Exception("Cmd fail:" + cmd);
                //Log.Pushlist("Cmd", cmd);

                //// chk
                //cmd = "GM\r\n";
                //r = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                //if (rec.Contains("E"))
                //    throw new Exception("Cmd fail:" + cmd);
                //if (rec != ControlMode)
                //    throw new Exception("Set Laser ControlMode Fail ");
                //Log.Pushlist("Cmd", cmd);

                #endregion

                #endregion
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._open,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber Connect Open fail!\r\n" + ee.Message;
                ret.Num = Num._open;
                return ret;
            }
            return ret;
        }

        //task: close
        public override RetErr Close()
        {
            RetErr ret = new RetErr();
            try
            {
                if(gPort.IsOpen)
                    gPort.Close();
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._close,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber Power Convert fail!\r\n" + ee.Message;
                ret.Num = Num._close;
                return ret;
            }
            return ret;
        }

        //task: Open Shutter & DIODE
        public override RetErr OpenShutterAndDIODE(bool POpen)
        {
            RetErr ret = new RetErr();
            try
            {
                if (gInfo.IntFacMode != 0) return ret;
                string cmd = "GS\r\n";
                bool rt = Cmd(cmd, out string rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                cmd = "SS 0\r\n";
                rt = Cmd(cmd, out rec)? true: throw new Exception("Cmd:" + cmd);
                Log.Pushlist("Cmd", cmd);


            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._OpenShutterAndDIODE,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber OpenShutterAndDIODE fail! " + ee.Message;
                ret.Num = Num._OpenShutterAndDIODE;
                return ret;
            }
            return ret;
        }


        //task: Set Power(W) & Freq(KHz) & Bias(V) & Mode(0-based) & SpotDelay(us)
        //note: 會設定ThermaTrack
        public override RetErr SetLaserPreset(double PPower, double PFreq,
                                                double PMode, double PBias,
                                                double PEpulse)
        {
            RetErr ret = new RetErr();
            string cmd = "";

            if (gInfo.IntFacMode == 0)
                return ret;
            try
            {
                cmd = "sc 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);

                #region Power
                
                // Set
                IMath.PowerConvert(PPower, out double _Power);
                cmd = "si " + (_Power * 10).ToString("f0") + "\r\n";
                bool rt = Cmd(cmd, out string rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gi\r\n";
                rt = Cmd(cmd, out  rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                #endregion

                #region Freq

                // Set
                cmd = "sr " + (PFreq * 1000).ToString() + "\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gr\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);
                if (rec != (PFreq * 1000).ToString())
                    throw new Exception("Set Freq Fail ");

                #endregion

                #region Mode

                // Set
                cmd = "sw " + PMode.ToString() + "\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gw\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);
                if (rec != PMode.ToString())
                    throw new Exception("Set Mode Fail ");

                #endregion

                #region Bias

                // Set
                cmd = "sh " + (PBias*10).ToString() + "\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gh\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);
                if (rec != (PBias * 10).ToString())
                    throw new Exception("Set Bias Fail ");

                #endregion

                cmd = "ss 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);


                cmd = "ss 1" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);

            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._SetLaserPreset,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber SetLaserPreset fail!" + ee.Message;
                ret.Num = Num._SetLaserPreset;
                return ret;
            }
            return ret;
        }


        //task: Set Power(%) & Freq(KHz)
        //note: 會設定ThermaTrack
        public override RetErr SetPowerAndFreq(double PPower, double PFreq)
        {
            RetErr ret = new RetErr();
            string cmd = "";
            if (gInfo.IntFacMode == 0)
                return ret;
            try
            {
                cmd = "sc 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);

                #region Power

                // Set
                //IMath.PowerConvert(PPower, out double _Power);
                cmd = "si " + (PPower * 10).ToString() + "\r\n";
                bool rt = Cmd(cmd, out string rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gi\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                #endregion

                #region Freq

                // Set
                cmd = "sr " + (PFreq * 1000).ToString() + "\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gr\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);
                if (rec != (PFreq * 1000).ToString())
                    throw new Exception("Set Freq Fail ");

                #endregion

                cmd = "ss 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);


                cmd = "ss 1" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._SetPowerAndFreq,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber SetPowerAndFreq fail!" + ee.Message;
                ret.Num = Num._SetPowerAndFreq;
                return ret;
            }
            return ret;
        }

        //task: Check Laser Ready
        //note: 會確認All Temp Reached & All Servo Locked
        //retn: ready : OperOk
        //      Not Ready : AVIA355_LaserNotReady
        public override RetErr CheckLaserReady()
        {
            RetErr ret = new RetErr();
            string tamp = "";
            try
            {
                // 通道是否連線
                if (!gPort.IsOpen)
                    throw new Exception("not connect");
                // chk Laser status
                string cmd = "QS\r\n";
                gPort.Write(cmd);
                while (!isRecDone) Application.DoEvents();
                bool work = int.TryParse(gRecData, out int status) ? true : throw new Exception("Rec Data not int");
                for(int i = 10; i <= 15; i++)
                {
                    bool ramp = false;
                    switch (i)
                    {
                        case 10: // alarm
                            ramp = (status >> i & 1) == 1 ? 
                                    true : throw new Exception("Laser has not been deactivated"); 
                            break;
                        case 11:    //System_Fault
                            ramp = (status >> i & 1) == 1 ?
                                    true : throw new Exception("one or more Laser power supplies are not in range");
                            break;
                        case 12:    //Beam_Delivery
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("temperature alarm or monitor detected");
                            break;
                        case 13:    //Laser_Temperature
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("beam delivery alarm or monitor detected");
                            break;
                        case 14:    //Laser_Emission_Warning
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("system fault detected");
                            break;
                        case 15:    //Laser_Deactivated
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("alarm condition detected");
                            break;
                    }
                }
               
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._CheckLaserReady,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber CheckLaserReady fail!" + ee.Message;
                ret.Num = Num._CheckLaserReady;
                return ret;
            }
            return ret;
        }

        //task: Check Shutter & DIODE Opened
        //retn: ready : OperOk
        //      Not Open : AVIA355_ShutterOrDIODENotOpened
        public override RetErr CheckShutterAndDIODEOpened()
        {
            RetErr ret = new RetErr();
            try
            {
                // chk
                string cmd = "gs\r\n";
                bool rt = Cmd(cmd, out string rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._CheckShutterAndDIODEOpened,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber CheckShutterAndDIODEOpened fail!" + ee.Message;
                ret.Num = Num._CheckShutterAndDIODEOpened;
                return ret;
            }
            return ret;
        }

        //task: Check Laser Enabled

        public override RetErr CheckLaserEnabled(out EEnabledState PEnabledState)
        {
            RetErr ret = new RetErr();
            EEnabledState status = EEnabledState.esStandby;
            PEnabledState = status;
            try
            {

            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._CheckLaserEnabled,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber CheckLaserEnabled fail!";
                ret.Num = Num._CheckLaserEnabled;
                return ret;
            }
            PEnabledState = status;
            return ret;
        }

        //task: Check Laser Fault
        //retn: 有錯誤時回傳 AVIA355_LaserHardwearFault
        public override RetErr CheckLaserFault()
        {
            RetErr ret = new RetErr();
            try
            {
                // chk Laser status
                string cmd = "QS\r\n";
                gPort.Write(cmd);
                while (!isRecDone) Application.DoEvents();
                bool work = int.TryParse(gRecData, out int status) ? true : throw new Exception("Rec Data not int");
                for (int i = 10; i <= 15; i++)
                {
                    bool ramp = false;
                    switch (i)
                    {
                        case 10: // alarm
                            ramp = (status >> i & 1) == 1 ?
                                    true : throw new Exception("Laser has not been deactivated");
                            break;
                        case 11:    //System_Fault
                            ramp = (status >> i & 1) == 1 ?
                                    true : throw new Exception("one or more Laser power supplies are not in range");
                            break;
                        case 12:    //Beam_Delivery
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("temperature alarm or monitor detected");
                            break;
                        case 13:    //Laser_Temperature
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("beam delivery alarm or monitor detected");
                            break;
                        case 14:    //Laser_Emission_Warning
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("system fault detected");
                            break;
                        case 15:    //Laser_Deactivated
                            ramp = (status >> i & 1) == 0 ?
                                    true : throw new Exception("alarm condition detected");
                            break;
                    }
                }
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._CheckLaserFault,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber CheckLaserFault fail! " + ee.Message;
                ret.Num = Num._CheckLaserFault;
                return ret;
            }
            return ret;
        }

        //task: Set Mode
        //note: Mode 的定義隨機型不同
        public override RetErr SetMode(int pMode)
        {
            RetErr ret = new RetErr();
            string cmd = "";
            if (gInfo.IntFacMode == 0)
                return ret;
            try
            {
                cmd = "sc 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);

                #region Mode

                // Set
                cmd = "sw " + pMode.ToString() + "\r\n";
                bool rt = Cmd(cmd, out string rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gw\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);
                if (rec != pMode.ToString())
                    throw new Exception("Set Mode Fail ");

                #endregion

                cmd = "ss 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);


                cmd = "ss 1" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);

            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._SetMode,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber SetMode fail!" + ee.Message;
                ret.Num = Num._SetMode;
                return ret;
            }
            return ret;
        }

        //task: Set Bias Voltage
        //note: Bias Voltage 定義隨機型不同
        public override RetErr SetBiasVoltage(double pBias)
        {
            RetErr ret = new RetErr();
            string cmd = "";
            if (gInfo.IntFacMode == 0)
                return ret;
            try
            {
                cmd = "sc 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);

                #region Bias

                // Set
                cmd = "sh " + (pBias * 10).ToString() + "\r\n";
                bool rt = Cmd(cmd, out string rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);

                // chk
                cmd = "gh\r\n";
                rt = Cmd(cmd, out rec) ? true : throw new Exception("Cmd fail:" + cmd);
                if (rec.Contains("E"))
                    throw new Exception("Cmd fail:" + cmd);
                Log.Pushlist("Cmd", cmd);
                if (rec != (pBias * 10).ToString())
                    throw new Exception("Set Bias Fail ");

                #endregion

                cmd = "ss 0" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);


                cmd = "ss 1" + "\r\n";
                gPort.Write(cmd);
                Thread.Sleep(30);

            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._SetBiasVoltage,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber SetBiasVoltage fail!" + ee.Message;
                ret.Num = Num._SetBiasVoltage;
                return ret;
            }
            return ret;
        }

        //task: Get Power Voltage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPowerVoltage"> % </param>
        /// <returns></returns>
        public override RetErr GetPowerVoltage( out double pPowerVoltage)
        {
            RetErr ret = new RetErr();
            double tamp = 0;
            pPowerVoltage = tamp;
            if (gInfo.IntFacMode == 0)
                return ret;
            try
            {
                if (gInfo.IntFacMode == 0 || gInfo.IntFacMode == 2)
                    return ret;
                // analog IO
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._GetPowerVoltage,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber GetPowerVoltage fail!" + ee.Message;
                ret.Num = Num._GetPowerVoltage;
                return ret;
            }
            pPowerVoltage = tamp;
            return ret;
        }

        //task: Get Bias Voltage
        public override RetErr GetBiasVoltage(out double pBiasVoltage)
        {
            RetErr ret = new RetErr();
            double tamp = 0;
            pBiasVoltage = tamp;
            if (gInfo.IntFacMode == 0)
                return ret;
            try
            {
                if (gInfo.IntFacMode == 0 || gInfo.IntFacMode == 2 || gInfo.IntFacMode == 3)
                    return ret;
                // analog IO

            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._GetBiasVoltage,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber GetBiasVoltage fail!" + ee.Message;
                ret.Num = Num._GetBiasVoltage;
                return ret;
            }
            pBiasVoltage = tamp;
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
            int tamp = 0;
            pHours = tamp;
            try
            {
                // 通道是否連線
                if (!gPort.IsOpen)
                    throw new Exception("not connect");
                // chk Laser status
                string cmd = "QH\r\n";
                gPort.Write(cmd);
                while (!isRecDone) Application.DoEvents();
                bool w = int.TryParse(gRecData, out tamp) ?
                        true : throw new Exception("Rec Data not value : " + gRecData);
                

            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._GetHours,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber GetHours fail! \r\n" + ee.Message;
                ret.Num = Num._GetHours;
                return ret;
            }
            pHours = tamp;
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
            pSpot = -1;
            return ret;
        }

        public override RetErr GetTemperatrue(out double Temperature)
        {
            RetErr ret = new RetErr();
            double tamp = 0;
            Temperature = tamp;
            try
            {
                string cmd = "gh\r\n";
                gPort.Write(cmd);
                while (!isRecDone) Application.DoEvents();
                tamp = double.TryParse(gRecData, out double t) ? t  : -1;
                if (tamp == -1)
                    throw new Exception("RecData not value ");
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._GetTemperature,
                                System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                ee.Message);
#endif
                ret.flag = false;
                ret.Meg = "Fyber GetTemperature fail!";
                ret.Num = Num._GetTemperature;
                return ret;
            }
            Temperature = tamp;
            return ret;
        }
        #endregion



    }
}

namespace GalvoScan.Hardware.Laser.Fyber.Define
{
    struct Info
    {
        public int Port;
        public int Baud;
        /// <summary>
        /// 0 ~ 7,  interFaceMode 5:hardwareMode, 0:softwareMode
        /// </summary>
        public int IntFacMode; // 
    }

}
