using System;
using BackLight.Laser.ErrCode;
using BackLight.Laser.Define;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.IO.Ports;
//using SPUV;
using BackLight.Laser.ReturnStatus;

namespace BackLight.Laser.SP_UV
{
    public class Laser_SP_UV : ILaser
    {
        //public static SerialPort RS232 = new SerialPort();
        //public static bool RS232Connected = false;
        //public static string status = "";
        public CStatus status = new CStatus();
        //bool bolFirstTime = true;
        Thread thread1;
        string pat = Application.StartupPath + "\\comport.par";
        public Laser_SP_UV()
        {
            //bool bolFirstTime = true;
        }
        static string RS232Read;
        public void DoWork()
        {
            do
            {
                try
                {
                    if (!ILaser.RS232Connected) break;

                    if (ILaser.RS232Connected)
                    {
                        RS232Read = ILaser.RS232.ReadExisting();

                        /////    Status ?   /////////////////////////////
                        ILaser.RS232.Write("?f" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                        
                        //ILaser.status = RS232Read;
                        status.strStatus = RS232Read;
                        //if (RS232Read.IndexOf("Ready", 0) > 1)
                        //{
                        //    lblSystemStatus.Image = ImageList1.Images[1];
                        //}
                        //else
                        //{
                        //    lblSystemStatus.Image = ImageList1.Images[0];
                        //}
                        //UpdateUI(RS232Read, lblRS232SystemStatus, false);

                        /////   Laser On/Off    /////
                        ///
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?D" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料              

                        if (RS232Read.IndexOf("1", 0) > -1)
                        {
                            status.bolLaserOnOff = true;
                        }
                        else
                        {
                            status.bolLaserOnOff = false;
                        }
                        //UpdateUI(RS232Read, chkLaserOnOff, false);

                        //if (bolLaserOnOff)
                        //{
                        //    if (chkLaserOnOff.Checked)
                        //        ILaser.RS232.Write("ON" + "\r");
                        //    else
                        //        ILaser.RS232.Write("OFF" + "\r");
                        //}
                        //do
                        //{
                        //    Application.DoEvents();
                        //    if (!ILaser.RS232Connected) break;
                        //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        //}
                        //while (RS232Read.IndexOf("\n", 0) < 0);
                        //bolLaserOnOff = false;

                        /////   Gate    /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?G" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料              

                        if (RS232Read.IndexOf("1", 0) > -1)
                        {
                            status.bolGate = true;
                        }
                        else
                        {
                            status.bolGate = false;
                        }
                        //UpdateUI(RS232Read, chkGate, false);
                        //if (bolGate)
                        //{
                        //    if (chkGate.Checked)
                        //        ILaser.RS232.Write("G:1" + "\r");
                        //    else
                        //        ILaser.RS232.Write("G:0" + "\r");
                        //}
                        //bolGate = false;
                        /////   External Gate   /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?GEXT" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料              

                        if (RS232Read.IndexOf("1", 0) > -1)
                        {
                            status.bolGEXT = true;
                        }
                        else
                        {
                            status.bolGEXT = false;
                        }

                        /////   Power (Watt)    /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?P" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料
                        status.dblPower = Convert.ToDouble(RS232Read);
                        //UpdateUI(RS232Read, lblPower, false);
                        //UpdateWattUI(Convert.ToSingle(lblPower.Text));

                        /////   Diode Current   /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?C1" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                    
                        status.dblCurrent = Convert.ToDouble(RS232Read);

                        /////       SHG         /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?SHG" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                    

                        if (bolFirstTime)
                        {
                            if (!ILaser.RS232Connected) break;
                            ILaser.RS232.Write("?SHGS" + "\r");
                            RS232Read = "";
                            do
                            {
                                Application.DoEvents();
                                if (!ILaser.RS232Connected) break;
                                RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                            }
                            while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料  
                            status.dblSHG = Convert.ToDouble(RS232Read);
                        }

                        /////       THG     /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?THG" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                    

                        status.dblTHG = Convert.ToDouble(RS232Read);

                        /////       QSW     /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?Q" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                    
                        status.dblQSW = Convert.ToDouble(RS232Read) / 1000;

                        /////       EPRF        /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?EPRF" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  //    
                        status.dblEPULSE = Convert.ToDouble(RS232Read) / 1000;

                        /////       Diode Temperature   /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?T1" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                    

                        status.dblTemp = Convert.ToDouble(RS232Read);

                        /////       Diode hours   /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?DH1" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                    

                        status.dblHours = Convert.ToDouble(RS232Read);

                        /////       Diode hours  Status /////
                        if (!ILaser.RS232Connected) break;
                        ILaser.RS232.Write("?fh" + "\r");
                        RS232Read = "";
                        do
                        {
                            Application.DoEvents();
                            if (!ILaser.RS232Connected) break;
                            RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                        }
                        while (RS232Read.IndexOf("\n", 0) < 0);  // 從序列連接埠讀取 回應資料                    

                        status.value = RS232Read;
                    }
                    Thread.Sleep(100);
                    //bolFirstTime = false;
                }
                catch (Exception e)
                //catch (ThreadAbortException e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            while (thread1.ThreadState == ThreadState.Background);
            //Console.WriteLine(thread1.ThreadState);
        }


        private RetErr ReadLaserPowerData(double W, out int intPercent)
        {
            RetErr retErr = new RetErr();
            Dictionary<float, float> LaserPower = new Dictionary<float, float> { };
            float fUK = 1, fUV = 1;
            float fDK = 0, fDV = 0;
            string strTemp;
            intPercent = 0;
            //txt檔中是  瓦特 ,百分比
            string pat = Application.StartupPath + "\\LaserPowerTable.ini";
            try
            {

                StreamReader streamReader = new StreamReader(pat);
                while ((strTemp = streamReader.ReadLine()) != null)
                {
                    string[] subs = strTemp.Split(',');
                    LaserPower.Add(Convert.ToSingle(subs[0]), Convert.ToSingle(subs[1]));
                }
                streamReader.Close();

                foreach (KeyValuePair<float, float> item in LaserPower)
                {
                    if (item.Key > W)
                    {
                        fUK = item.Key;
                        fUV = item.Value;
                        break;
                    }
                    fDK = item.Key;
                    fDV = item.Value;
                }
                intPercent = (int)((W - fDK) / (fUK - fDK) * (fUV - fDV) + fDV);
            }
            catch (Exception ex)
            {
                retErr.flag = false;
                retErr.Meg = ex.Message;
                retErr.Num = -1;
            }
            return retErr;
        }

        public override RetErr LaserOn()
        {
            RetErr retErr = new RetErr();
            try
            {
                ILaser.RS232.Write("ON" + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        //LaserOff
        public override RetErr LaserOff()
        {
            RetErr retErr = new RetErr();
            try
            {
                ILaser.RS232.Write("OFF" + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr SetShutter(bool bolOpen)
        {
            RetErr retErr = new RetErr();
            try
            {
                if (bolOpen)
                    ILaser.RS232.Write("SHT:1" + "\r");
                else
                    ILaser.RS232.Write("SHT:0" + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr GetShutter(out bool bolOpen)
        {
            RetErr retErr = new RetErr();
            bolOpen = false;
            try
            {
                ILaser.RS232.Write("?SHT" + "\r");
                string RS232Read = "";
                do
                {
                    //Application.DoEvents();
                    if (!ILaser.RS232Connected) return retErr;
                    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                }
                while (RS232Read.IndexOf("\n", 0) < 0);
                if (RS232Read.IndexOf("1", 0) > -1) bolOpen = true;

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr SetDiode(bool bolOpen)
        {
            RetErr retErr = new RetErr();
            bolOpen = false;
            try
            {
                //ILaser.RS232.Write("?" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    //Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //if (RS232Read.IndexOf("1", 0) > -1) bolOpen = true;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        //public abstract RetErr GetDiode(out bool bolOpen);
        public override RetErr GetDiode(out bool bolOpen)
        {
            RetErr retErr = new RetErr();
            bolOpen = false;
            try
            {
                //ILaser.RS232.Write("?SHT" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    //Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //if (RS232Read.IndexOf("1", 0) > -1) bolOpen = true;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        public override RetErr SetGate(bool bolOpen)
        {
            RetErr retErr = new RetErr();
            try
            {
                if (bolOpen)
                    ILaser.RS232.Write("G:1" + "\r");
                else
                    ILaser.RS232.Write("G:0" + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr GetGate(out bool bolOpen)
        {
            RetErr retErr = new RetErr();
            bolOpen = false;
            try
            {
                //ILaser.RS232.Write("?G" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    //Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //if (RS232Read.IndexOf("1", 0) > -1) bolOpen = true;
                if (status.bolGate)
                {
                    retErr.flag = true;
                    bolOpen = true;
                }
                else
                {
                    retErr.flag = false;
                    bolOpen = false;
                }
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        public override RetErr SetExtGate(bool bolOpen)
        {
            RetErr retErr = new RetErr();
            try
            {
                if (bolOpen)
                    ILaser.RS232.Write("GEXT:1" + "\r");
                else
                    ILaser.RS232.Write("GEXT:0" + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        public override RetErr GetExtGate(out bool bolOpen)
        {
            RetErr retErr = new RetErr();
            bolOpen = false;
            try
            {
                //ILaser.RS232.Write("?GEXT" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //if (RS232Read.IndexOf("1", 0) > -1) bolOpen = true;
                bolOpen = status.bolGEXT;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        public override RetErr SetDiodeCurrent(double PDCurr)
        {
            RetErr retErr = new RetErr();
            try
            {
                ILaser.RS232.Write("C1" + PDCurr.ToString() + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }


        public override RetErr GetCurrent(out double dblAMP)
        {
            RetErr retErr = new RetErr();
            dblAMP = 0;
            try
            {
                //ILaser.RS232.Write("?C1" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    //Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //if (RS232Read.IndexOf("1", 0) > -1) dblAMP = Convert.ToDouble(RS232Read);
                dblAMP = status.dblCurrent;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //SetPower
        public override RetErr SetPower(double dWatte, out int intPercent)
        {
            double dblAMP = 0;
            int i;
            RetErr retErr = new RetErr();
            retErr = ReadLaserPowerData(dWatte, out i);
            intPercent = i;
            try
            {       //txt檔中是  瓦數 ,百分比
                    //fDCL1
                dblAMP = (((float)intPercent / 100f) * (fDCL1 - fAMPDL)) + fAMPDL;
                retErr = SetDiodeCurrent(dblAMP);

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        // public abstract RetErr GetPower(out int intPower);//電流%
        public override RetErr GetPowerVoltage(out double pPowerVoltage)
        {
            RetErr retErr = new RetErr();
            pPowerVoltage = 0;
            //retErr = ReadPowerData();

            if (!retErr.flag) return retErr;
            try
            {
                //ILaser.RS232.Write("?P" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    //Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //pPowerVoltage = Convert.ToDouble(RS232Read);
                pPowerVoltage = status.dblPower;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        //RetErr SetQSW(Int32 intQSWHz) 
        public override RetErr SetQSW(Int32 intQSWHz)
        {
            RetErr retErr = new RetErr();
            try
            {
                ILaser.RS232.Write("Q:" + (intQSWHz * 1000).ToString() + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr GetQSW(out Int32 intQSWHz)
        {
            RetErr retErr = new RetErr();
            intQSWHz = 0;
            try
            {
                //ILaser.RS232.Write("?Q" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    //Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //if (RS232Read.IndexOf("1", 0) > -1) intQSWHz = Convert.ToInt32(RS232Read);
                intQSWHz = (int)(status.dblQSW);
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        //SetEpulse(Int32 intEPRFHz);  KHz
        public override RetErr SetEPulse(double PEPulse)
        {
            RetErr retErr = new RetErr();
            try
            {
                ILaser.RS232.Write("EPRF:" + (PEPulse * 1000).ToString() + "\r");
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr GetEpulse(out Int32 intEPRFHz)
        {
            RetErr retErr = new RetErr();
            intEPRFHz = 0;
            try
            {
                //ILaser.RS232.Write("?EPRF" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    //Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                //intEPRFHz = Convert.ToInt32(RS232Read);
                intEPRFHz = (int)(status.dblEPULSE);
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        //GetStatus(out string strStatus);
        public override RetErr GetStatus(out string strStatus)
        {
            RetErr retErr = new RetErr();
            strStatus = "";
            try
            {
                strStatus = status.strStatus;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr GetDCL(out float fDCL)
        {
            RetErr retErr = new RetErr();
            fDCL = 0;
            try
            {
                ILaser.RS232.Write("?DCL" + "\r");
                string RS232Read = "";
                DateTime dt1 = DateTime.Now;
                DateTime dt2 = DateTime.Now;
                do
                {
                    //Application.DoEvents();
                    if (!ILaser.RS232Connected) return retErr;

                    //判斷有沒有逾時
                    dt2 = DateTime.Now;
                    if (dt2.Subtract(dt1).TotalMilliseconds > 1000)
                    {
                        ILaser.RS232Connected = false;  //Timeout逾時了
                        retErr.flag = false;
                        retErr.Meg = "Open timeout";
                        return retErr;
                    }

                    RS232Read = RS232Read + ILaser.RS232.ReadExisting();

                }
                while (RS232Read.IndexOf("\n", 0) < 0);
                fDCL = (float)Convert.ToDouble(RS232Read);
                fDCL1 = fDCL;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        public override RetErr SetLaserPreset(double PPower, double PFreq,
                                        double PMode, double PBias,
                                        double PEpulse)
        {
            RetErr retErr = new RetErr();
            int i;
            try
            {
                retErr = SetPower(PPower, out i);
                if (retErr.flag)
                    retErr = SetQSW((int)PFreq);
                if (retErr.flag)
                    retErr = SetEPulse(PEpulse);
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        //task: Set Power(W) & Freq(KHz)
        //note: 會設定ThermaTrack
        public override RetErr SetPowerAndFreq(double PPower, double PFreq)
        {
            RetErr retErr = new RetErr();
            int i;
            try
            {
                retErr = SetPower(PPower, out i);
                if (retErr.flag)
                    retErr = SetQSW((int)PFreq);
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Check Laser Ready
        //note: 會確認All Temp Reached & All Servo Locked
        //retn: ready : OperOk
        //      Not Ready : AVIA355_LaserNotReady
        public override RetErr CheckLaserReady()
        {
            RetErr retErr = new RetErr();

            try
            {
                if (status.strStatus == "System Ready\n")
                { retErr.flag = true; }
                else
                { retErr.flag = false; }

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Check Shutter & DIODE Opened
        //retn: ready : OperOk
        //      Not Open : AVIA355_ShutterOrDIODENotOpened
        public override RetErr CheckShutterAndDIODEOpened()
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Check Laser Enabled

        public override RetErr CheckLaserEnabled(out EEnabledState PEnabledState)
        {
            RetErr retErr = new RetErr();
            EEnabledState tamp = new EEnabledState();
            PEnabledState = tamp;
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            PEnabledState = tamp;
            return retErr;
        }

        //task: Check Laser Fault
        //retn: 有錯誤時回傳 AVIA355_LaserHardwearFault
        public override RetErr CheckLaserFault()
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Set Mode
        //note: Mode 的定義隨機型不同
        public override RetErr SetMode(int pMode)
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Set Bias Voltage
        //note: Bias Voltage 定義隨機型不同
        public override RetErr SetBiasVoltage(double pBiasVoltage)
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Get Power Voltage
        //public override RetErr GetPowerVoltage(double pPowerVoltage)
        //{
        //    RetErr retErr = new RetErr();
        //    try
        //    {

        //    }
        //    catch (Exception ee)
        //    {
        //        retErr.Meg = " fail " + ee.Message;
        //        retErr.flag = false;
        //        return retErr;
        //    }
        //    return retErr;
        //}

        //task: Get Bias Voltage
        public override RetErr GetBiasVoltage(out double pBiasVoltage)
        {
            pBiasVoltage = -1;
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Set EPulse, in khz
        //public override RetErr SetEPulse(double PEPulse)
        //{
        //    RetErr retErr = new RetErr();
        //    try
        //    {

        //    }
        //    catch (Exception ee)
        //    {
        //        retErr.Meg = " fail " + ee.Message;
        //        retErr.flag = false;
        //        return retErr;
        //    }
        //    return retErr;
        //}

        //task: check if support EPulse
        public override RetErr HaveEPulse()
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Set Etime
        public override RetErr SetETime(int etime)
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Set patternbit
        public override RetErr SetPatternBin(int patbin)
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: check if support warm-up
        public override RetErr HaveWarmUp()
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: check if warming up
        public override RetErr IsWarmingUp(int pSecs, out bool isOK)
        {
            RetErr retErr = new RetErr();
            isOK = false;
            try
            {
                if (status.strStatus == "Warming Up\n")
                {
                    retErr.flag = true;
                    isOK = true;
                }
                else
                {
                    retErr.flag = false;
                }
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                //return retErr;
            }
            return retErr;
        }

        //task: Standby after open
        public override RetErr Standby()
        {
            RetErr retErr = new RetErr();
            try
            {
                retErr = SetQSW(0);
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Shutdown after open
        public override RetErr Shutdown()
        {
            RetErr retErr = new RetErr();
            try
            {
                retErr = SetDiodeCurrent(0);
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Set Gate
        //public override RetErr SetGate(bool POpen)
        //{
        //    RetErr retErr = new RetErr();
        //    try
        //    {

        //    }
        //    catch (Exception ee)
        //    {
        //        retErr.Meg = " fail " + ee.Message;
        //        retErr.flag = false;
        //        return retErr;
        //    }
        //    return retErr;
        //}

        //task: Set Diode Current
        //public override RetErr SetDiodeCurrent(double PDCurr)
        //{
        //    RetErr retErr = new RetErr();
        //    try
        //    {

        //    }
        //    catch (Exception ee)
        //    {
        //        retErr.Meg = " fail " + ee.Message;
        //        retErr.flag = false;
        //        return retErr;
        //    }
        //    return retErr;
        //}

        //task: Get Hours until now
        public override RetErr GetHours(int pHours)
        {
            RetErr retErr = new RetErr();
            try
            {

                if (status.dblHours > 0)
                {
                    retErr.flag = true;
                    pHours = (int)status.dblHours;
                }
                else
                {
                    retErr.flag = false;
                    pHours = -1;
                }
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        public override RetErr GetPatBins(char asBins)
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Check Hours Status
        public override RetErr CheckHours()
        {
            RetErr retErr = new RetErr();
            try
            {
                status.value.Substring(1, 3);
                if (status.value == "072")
                {
                    retErr.Meg = "check hours err 72 value";
                    retErr.flag = false;
                }
                else if (status.value == "071")
                {
                    retErr.Meg = "check hours err 71 value";
                    retErr.flag = false;
                }

                else if (status.value == "070")
                {
                    retErr.Meg = "check hours err 70 value";
                    retErr.flag = false;
                }
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Get Spot
        public override RetErr GetSpot(int pSpot)
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: open
        public override RetErr Open(int comPort)
        {
            RetErr retErr = new RetErr();
            try
            {
                float fDCL;

                thread1 = new Thread(DoWork);

                try
                {
                    StreamWriter streamWriter = new StreamWriter(pat);
                    ILaser.RS232.BaudRate = 115200;
                    ILaser.RS232.StopBits = System.IO.Ports.StopBits.One;
                    ILaser.RS232.Parity = System.IO.Ports.Parity.None;
                    ILaser.RS232.DataBits = 8;
                    ILaser.RS232.Handshake = System.IO.Ports.Handshake.None;
                    ILaser.RS232.PortName = "COM" + comPort.ToString();
                    streamWriter.WriteLine(comPort.ToString());
                    streamWriter.Close();

                    ILaser.RS232.Open();
                    ILaser.RS232.ReadTimeout = 100;

                }
                //catch (TimeoutException ex)
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    //MessageBox.Show("連接雷射失敗," + ex.Message);
                }
                if (ILaser.RS232.IsOpen)
                {
                    //btnConnectLaser.Text = "中斷雷射";
                    ILaser.RS232Connected = true;
                    GetDCL(out fDCL);
                    thread1.Start();
                    thread1.IsBackground = true;
                }
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: close
        public override RetErr Close()
        {
            RetErr retErr = new RetErr();
            try
            {
                ILaser.RS232.Close();
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }

        //task: Open Shutter & DIODE
        public override RetErr OpenShutterAndDIODE(bool POpen)
        {
            RetErr retErr = new RetErr();
            try
            {

            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
                return retErr;
            }
            return retErr;
        }
        public override RetErr GetTemperatrue(out double Temperature)
        {
            RetErr retErr = new RetErr();
            Temperature = -1;
            try
            {
                //ILaser.RS232.Write("?T1" + "\r");
                //string RS232Read = "";
                //do
                //{
                //    Application.DoEvents();
                //    if (!ILaser.RS232Connected) return retErr;
                //    RS232Read = RS232Read + ILaser.RS232.ReadExisting();
                //}
                //while (RS232Read.IndexOf("\n", 0) < 0);
                Temperature = status.dblTemp;
            }
            catch (Exception ee)
            {
                retErr.Meg = " fail " + ee.Message;
                retErr.flag = false;
            }
            return retErr;
        }
    }

}



