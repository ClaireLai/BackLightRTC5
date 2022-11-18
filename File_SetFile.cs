#define Log
using System;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using BackLight.File.ErrCode;
using BackLight.File.Define;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
//using GalvoScan.GUI.Project;

namespace BackLight.File
{

    class File_SetFile
    {
        ///////////////////////////////////////////////////////
        ///                     Para                        ///
        ///////////////////////////////////////////////////////

        #region Para
        AllFile g_File = new AllFile();
        #endregion

        ///////////////////////////////////////////////////////
        ///                   Subrouting                    ///
        ///////////////////////////////////////////////////////

        #region Subrouting

        #endregion

        ///////////////////////////////////////////////////////
        ///                     Method                      ///
        ///////////////////////////////////////////////////////

        #region Method

        public RetErr GetAllSetData()
        {
            RetErr ret = new RetErr();
            try
            {
                #region BasicMotionPara.ini

                string path = Application.StartupPath + "\\SetFile\\BasicMotionPara.ini";
                if (!System.IO.File.Exists(path))
                {
                    BasicMotion _b = new BasicMotion();
                    string tt = JsonConvert.SerializeObject(_b, Formatting.Indented);
                    StreamWriter _strr = new StreamWriter(path, false, Encoding.Default);
                    _strr.Write(tt);
                    _strr.Close();
                }
                StreamReader strr;
                string ori = "";
                if (System.IO.File.Exists(path))
                {
                    #region ReadFile

                    strr = new StreamReader(path);
                    ori = strr.ReadToEnd();
                    strr.Close();

                    #endregion

                    #region ToDB

                    g_File._basicMotion = JsonConvert.DeserializeObject<BasicMotion>(ori);

                    #endregion
                }

                #endregion

                #region Aligment.ini


                path = Application.StartupPath + "\\SetFile\\Aligment.ini";
                if (!System.IO.File.Exists(path))
                {
                    Aligment _b = new Aligment();
                    string tt = JsonConvert.SerializeObject(_b, Formatting.Indented);
                    StreamWriter _strr = new StreamWriter(path, false, Encoding.Default);
                    _strr.Write(tt);
                    _strr.Close();
                }
                if (System.IO.File.Exists(path))
                {
                    #region ReadFile
                    strr = new StreamReader(path);
                    ori = strr.ReadToEnd();
                    strr.Close();

                    #endregion

                    #region ToDB

                    g_File._aligment = JsonConvert.DeserializeObject<Aligment>(ori);

                    #endregion
                }

                #endregion

                #region CCDSetup.ini


                path = Application.StartupPath + "\\SetFile\\CCDSetup.ini";
                if (!System.IO.File.Exists(path))
                {
                    CCDSetup _b = new CCDSetup();
                    string tt = JsonConvert.SerializeObject(_b, Formatting.Indented);
                    StreamWriter _strr = new StreamWriter(path, false, Encoding.Default);
                    _strr.Write(tt);
                    _strr.Close();
                }
                if (System.IO.File.Exists(path))
                {
                    #region ReadFile
                    strr = new StreamReader(path);
                    ori = strr.ReadToEnd();
                    strr.Close();

                    #endregion

                    #region ToDB

                    g_File._cCDSetup = JsonConvert.DeserializeObject<CCDSetup>(ori);


                    #endregion
                }

                #endregion

                #region IOName.ini


                path = Application.StartupPath + "\\SetFile\\IOName.ini";
                if (!System.IO.File.Exists(path))
                {
                    IOName _b = new IOName();
                    string tt = JsonConvert.SerializeObject(_b, Formatting.Indented);
                    StreamWriter _strr = new StreamWriter(path, false, Encoding.Default);
                    _strr.Write(tt);
                    _strr.Close();
                }
                if (System.IO.File.Exists(path))
                {
                    #region ReadFile
                    strr = new StreamReader(path);
                    ori = strr.ReadToEnd();
                    strr.Close();

                    #endregion

                    #region ToDB

                    g_File._iOName = JsonConvert.DeserializeObject<IOName>(ori);

                    #endregion
                }

                #endregion

                #region IoTable.ini


                path = Application.StartupPath + "\\SetFile\\IOTable.ini";
                if (!System.IO.File.Exists(path))
                {
                    IOTable _b = new IOTable();
                    string tt = JsonConvert.SerializeObject(_b, Formatting.Indented);
                    StreamWriter _strr = new StreamWriter(path, false, Encoding.Default);
                    _strr.Write(tt);
                    _strr.Close();
                }
                if (System.IO.File.Exists(path))
                {
                    #region ReadFile
                    strr = new StreamReader(path);
                    ori = strr.ReadToEnd();
                    strr.Close();

                    #endregion

                    #region ToDB

                    g_File._iOTable = JsonConvert.DeserializeObject<IOTable>(ori);

                    #endregion
                }


                #endregion

                #region GalvoSetup.ini


                path = Application.StartupPath + "\\SetFile\\GalvoSetup.ini";
                if (!System.IO.File.Exists(path))
                {
                    GalvoSetup _b = new GalvoSetup();
                    string tt = JsonConvert.SerializeObject(_b, Formatting.Indented);
                    StreamWriter _strr = new StreamWriter(path, false, Encoding.Default);
                    _strr.Write(tt);
                    _strr.Close();
                }
                if (System.IO.File.Exists(path))
                {
                    #region ReadFile
                    strr = new StreamReader(path);
                    ori = strr.ReadToEnd();
                    strr.Close();

                    #endregion

                    #region ToDB

                    g_File._galvoSetup = JsonConvert.DeserializeObject<GalvoSetup>(ori);

                    #endregion

                }

                #endregion

            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Meg = "File GetAllSetData Get " + ee.Message;
                return ret;
            }
            return ret;
        }

        public AllFile GetAllfile()
        {
            return g_File;
        }


        public BasicMotion GetBasicMotion()
        {
            return g_File._basicMotion;
        }

        public Aligment GetAlignment()
        {
            return g_File._aligment;
        }

        public CCDSetup GetCCDSetup()
        {
            return g_File._cCDSetup;
        }

        public IOName GetIOName()
        {
            return g_File._iOName;
        }

        public IOTable GetIOTable()
        {
            return g_File._iOTable;
        }

        public GalvoSetup GetGalvoSetup()
        {
            return g_File._galvoSetup;
        }

        public RetErr WriteBasicMotion(BasicMotion DB)
        {
            RetErr ret = new RetErr();
            try
            {
                #region Updata DB

                g_File._basicMotion = DB;

                #endregion

                #region DB2str

                string str = JsonConvert.SerializeObject(DB, Formatting.Indented);

                #endregion

                #region Write To File

                string path = Application.StartupPath + "\\SetFile\\BasicMotionPara.ini";
                StreamWriter strw = new StreamWriter(path, false, Encoding.Default);
                strw.Write(str);
                strw.Close();

                #endregion
            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Meg = "File BasicMotion Set " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr WriteAligment(Aligment DB)
        {
            RetErr ret = new RetErr();
            try
            {
                #region Updata DB

                g_File._aligment = DB;

                #endregion

                #region DB2str

                string str = JsonConvert.SerializeObject(DB, Formatting.Indented);

                #endregion

                #region Write To File

                string path = Application.StartupPath + "\\SetFile\\Aligment.ini";
                StreamWriter strw = new StreamWriter(path , false, Encoding.Default);
                strw.Write(str);
                strw.Close();

                #endregion
            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Meg = "File Aligment Set " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr WriteCCDSetup(CCDSetup DB)
        {
            RetErr ret = new RetErr();
            try
            {
                #region Updata DB

                g_File._cCDSetup = DB;

                #endregion

                #region DB2str

                string str = JsonConvert.SerializeObject(DB, Formatting.Indented);

                #endregion

                #region Write To File

                string path = Application.StartupPath + "\\SetFile\\CCDSetup.ini";
                StreamWriter strw = new StreamWriter(path, false, Encoding.Default);
                strw.Write(str);
                strw.Close();

                #endregion
            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Meg = "File CCDSetup Set " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr WriteIOName(IOName DB)
        {
            RetErr ret = new RetErr();
            try
            {
                #region Updata DB

                g_File._iOName = DB;

                #endregion

                #region DB2str

                string str = JsonConvert.SerializeObject(DB, Formatting.Indented);

                #endregion

                #region Write To File

                string path = Application.StartupPath + "\\SetFile\\IOName.ini";
                StreamWriter strw = new StreamWriter(path, false, Encoding.Default) ;
                strw.Write(str);
                strw.Close();

                #endregion
            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Meg = "File IOName Set " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr WriteIOTable(IOTable DB)
        {
            RetErr ret = new RetErr();
            try
            {
                #region Updata DB

                g_File._iOTable = DB;

                #endregion

                #region DB2str

                string str = JsonConvert.SerializeObject(DB, Formatting.Indented);

                #endregion

                #region Write To File

                string path = Application.StartupPath + "\\SetFile\\IOTable.ini";
                StreamWriter strw = new StreamWriter(path,false, Encoding.Default);
                strw.Write(str);
                strw.Close();

                #endregion
            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Meg = "File IOTable Set " + ee.Message;
                return ret;
            }
            return ret;
        }
        public RetErr WriteGalvoSetup(GalvoSetup DB)
        {
            RetErr ret = new RetErr();
            try
            {
                #region Updata DB

                g_File._galvoSetup = DB;

                #endregion

                #region DB2str

                string str = JsonConvert.SerializeObject(DB,Formatting.Indented);

                #endregion

                #region Write To File

                string path = Application.StartupPath + "\\SetFile\\GalvoSetup.ini";
                StreamWriter strw = new StreamWriter(path, false, Encoding.Default);
                strw.Write(str);
                strw.Close();

                #endregion
            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Meg = "File GalvoSetup Set " + ee.Message;
                return ret;
            }
            return ret;
        }

        #endregion
    }
}
namespace BackLight.File.ErrCode
{
    public class RetErr
    {
        public Boolean flag = true;
        public string Meg = "";
    }
    class Log
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="NGCode"> 編碼 </param>
        /// <param name="Who"> Method </param>
        /// <param name="what"> message</param>
        public static void Pushlist(string Who, string what)
        {
            //初始化字串
            string NGString = "";
            string NGtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 重組字串
            NGString = "# " + NGtime  + " $ " + Who + " , " + what;

            //寫入文檔
            string pat = Application.StartupPath + "\\Log\\File_Log.log";
            StreamWriter strr = new StreamWriter(pat, true, Encoding.Default);
            strr.WriteLine(NGString);
            strr.Close();

        }
    }
}

namespace BackLight.File.Define
{
    #region local

    #region Aligment
    public class ProjPara
    {
        //[Laser]
        public double StandbyFrequency = 100;
        public double StandbyPulseWidth = 0;
        public double LaserFrequency = 20;
        public double LaserPulseWidth = 10;
        public double LaserFPS = 200;
        public double LaserTimeBase = 0;
        public double JumpDelay = 700;
        public double MarkDelay = 10;
        public double PolygonDelay = 5;
        public double LaserOnDelay = 50;
        public double LaserOffDelay = 40;
        public double JumpSpeed = 1000;
        public double MarkSpeed = 1000;
        public double LaserDuty = 60;
        public double OneShot = 750;
        public int Mode = 3; 
        public double Power = 40;
        public double Epulse = 200;
        //[Vision]
        public double FocusLength = 0;
        //[Project]
        public double SortType = 4;//排列方式
        public double SortParam = 1;//blocksize
        public double FuzzyType = 3;//模糊化 使用 不使用
        public double FuzzyParam = 50;//模糊化%
        public double FieldSizeW = 3;
        public double FieldSizeH = 3;
        public double Length = 0;
        public double OffsetX = 0;
        public double OffsetY = 0;
        public double Thickness = 75;
        public double OriginalX = 0;
        public double OriginalY = 0;
        public double DPI = 363;
        public double LocationMode = 1;
        public int DistanceX=100;          //Table X行程
        public int DistanceY=100;          //Table Y行程
        //[Common]
        public double PixelPerMM = 1660;
        public double GalvoCCDOffsetX = 108.319;
        public double GalvoCCDOffsetY = -1.247;
        public string CorrectionFile = "Cor_1to1.ct5";
        public string DestPath = "D:\\WorkDir\\";
        //public string ProgramFile = "C:\\WorkDir\\RTC3d2.hex";
        public double K = 615;
        public double LaserFocusLength= 104.9;
        public double PowerLimit = 0.53;
        public double dblEdgeOriginalX = 581.305;
        public double dblEdgeOriginalY = -0.057;
    }
    public struct Aliment_Laser
    {
        public double Power;
        public double Freq;
        public double PulseWidth;
        public double MarkTimes;
        public double SpotDelay;
        public double OnDelay;
        public double MiddleDelay;
        public double EndDelay;
        public double MarkSpeed;
        public double JumpSpeed;
        public double JumpDelay;
        public double Epulse;
        public double Bias;
        public double Mode;
    }
    public struct Aliment_Cor_PosPara
    {
        public double GarCenter_X;
        public double GarCenter_Y;
        public double CCDCenter_X;
        public double CCDCenter_Y;
        public double CircleDia;
        public double FocusPos; // 加工高度(已包含thickness)
        public double Thickness;
        public double inposition;
        public double Velocity;
        public double StableTime;
        public CCD.Define.s_MarkPara VisionPara;
    }
    public struct Aliment_Cor_CalPara
    {
        public double GarCenter_X;
        public double GarCenter_Y;
        public double CCDCenter_X;
        public double CCDCenter_Y;
        public double CircleDia;
        public double FocusPos;
        public double Thickness;
        public double inposition;
        public double Velocity;
        public double StableTime;
        public CCD.Define.s_MarkPara VisionPara;
    }
    public struct Aliment_Cor_GarPara
    {
        public double GarCenter_X;
        public double GarCenter_Y;
        public double CCDCenter_X;
        public double CCDCenter_Y;
        public double CircleDia;
        public double FocusPos;
        public double Thickness;
        public double inposition;
        public double Velocity;
        public double StableTime;
        public CCD.Define.s_MarkPara VisionPara;
    }
    public struct Aliment_LFocusPara
    {
        public double GarCenter_X;
        public double GarCenter_Y;
        public double CCDCenter_X;
        public double CCDCenter_Y;
        public double FocusPos;
        public double Thickness;
        public double Velocity;
        public double StableTime;
        public double CrossSize;
        public double SearchRange;
        public double CrossSpace;
    }
    public struct Aliment_MotionControl
    {
        public double GarCenter_X;
        public double GarCenter_Y;
        public double CCDCenter_X;
        public double CCDCenter_Y;
        public double TextSpeed;
        public double MoveTo_X;
        public double MoveTo_Y;
        public double Thickness;
    }

    #endregion

    #region Pix2mm
    public struct Pix2mm
    {
        public double Cal_X;
        public double Cal_Y;
        public double Pos_X;
        public double Pos_Y;
    }

    #endregion

    #region IO

    public struct DI_Name
    {
        public string DI1;
        public string DI2;
        public string DI3;
        public string DI4;
        public string DI5;
        public string DI6;
        public string DI7;
        public string DI8;
        public string DI9;
        public string DI10;
        public string DI11;
        public string DI12;
        public string DI13;
        public string DI14;
        public string DI15;
        public string DI16;
    }
    public struct DO_Name
    {
        public string DO1;
        public string DO2;
        public string DO3;
        public string DO4;
        public string DO5;
        public string DO6;
        public string DO7;
        public string DO8;
        public string DO9;
        public string DO10;
        public string DO11;
        public string DO12;
        public string DO13;
        public string DO14;
        public string DO15;
        public string DO16;
        public string DO17;
        public string DO18;
        public string DO19;
    }
    public struct DI_Table
    {
        public int EStop;
        public int SafetySensor;
        public int SafetySensorInverted;
        public int FrontDoorUpperSensor;
        public int FrontDoorUpperSensorInverted;
        public int EnableVacuum;
        public int ManualOpenFrontDoor;
        public int PlateSensor;
        public int PlateSensorInverted;
        public int FrontDoorLowerSensor;
        public int FrontDoorLowerSensorInverted;
        public int FrontDoorLatchFrontSensor;
        public int FrontDoorLatchFrontSensorInverted;
        public int FrontDoorLatchRearSensor;
        public int FrontDoorLatchRearSensorInverted;
        public int FRSFlowFault;
        public int FRSFlowFaultInverted;
        public int FRSTempOverHigh;
        public int FRSTempOverHighInverted;
        public int FRSTempOverLow;
        public int FRSTempOverLowInverted;
        public int AxesInPos;
        public int AxesInPosInverted;
    }
    public struct DO_Table
    {
        public int GreenLight;
        public int YellowLight;
        public int RedLight;
        public int Buzzer;
        public int Plate;
        public int DustCollector;
        public int FrontDoor;
        public int AimingBeam;
        public int VacuumGenerator;
        public int VacuumBreaker;
    }

    #endregion

    #endregion

    #region public
    public struct BasicMotion
    {
        public double ManualXYLineSpeed;
        public double AutoXYLineSpeed;
        public double XYLineAcc;
        public double ManualZMoveSpeed;
        public double AutoZMoveSpeed;
        public double ZMoveAcc;
        public double ZSafePos;
        public double BasicMarkingFocalDis;
        public double BasicPosChkFocalDis;
        public double DivWidth;
        public double DivHeight;
        public double GalvoWidth;
        public double GalvoHeight;
        public double PosChkCCDOffsetX;
        public double PosChkCCDOffsetY;
        public double PosChkCCDMinCrossSection;
        public double AimingBeamOffsetX;
        public double AimingBeamOffsetY;
        public double XYSettleTime;
        public double AutoCalCCDBasicFocalDis;
        public double AutoCalCCDOffsetX;
        public double AutoCalCCDOffsetY;
        public double AutoCalCCDMinCrossSection;
        public double JigOutputPosX;
        public double JigOutputPosY;
        public double BeepAfterWorkDone;
        public double ScaleNGThreshold;
        public double ScaleXNGThreshold;
        public double ScaleYNGThreshold;
        public double GalvoJumpSpeedLimit;
        public double GalvoMarkSpeedLimit;
        public double UsePlate;
        public double UseFrontDoorLatch;
        public double CheckFrontDoorUpperSensor;
        public double FrontDoorUpperSensorWaitTime;
        public double MoveToMarkCenter;
        public double NeedWaitForLaserReady;
        public double JigOutputPosZ;
        public double UseJigOutputPosZ;
        public double EnableEStopAlarm;
        public double JigInputPosX;
        public double JigInputPosY;
        public double JigInputPosZ;
        public double UseJigInputPosZ;
        public double EnableFRSAlarm;
        public double OpenFrontDoorWaitTime;
        public double EnableOpenFrontDoorWaitTime;
    }

    public struct Aligment
    {
        public Aliment_Laser _Laser;
        public Aliment_Cor_PosPara _Cor_PosPara;//對位
        public Aliment_Cor_CalPara _Cor_CalPara;//校正
        public Aliment_Cor_GarPara _Cor_GarPara;
        public Aliment_LFocusPara _LFocusPara;
        public Aliment_MotionControl _MotionControl;
    }

    public struct CCDSetup
    {
        public int CCD_Cal;
        public int CCD_Pos;
        public int Xflip;
        public int Yflip;
        public Pix2mm _Pix2mm;
    }

    public struct IOName
    {
        public DI_Name _DI;
        public DO_Name _DO;
    }

    public struct IOTable
    {
        public DI_Table _DI_Table;
        public DO_Table _DO_Table;
    }

    public struct GalvoSetup
    {
        public double Width;
        public double Heigh;
    }
    public class AllFile
    {
        public BasicMotion _basicMotion = new BasicMotion();

        public Aligment _aligment = new Aligment();

        public CCDSetup _cCDSetup = new CCDSetup();

        public IOName _iOName = new IOName();

        public IOTable _iOTable = new IOTable();

        public GalvoSetup _galvoSetup = new GalvoSetup();
    }

    #endregion


}
