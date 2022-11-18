#define Log
using System;
using System.Windows.Forms;
using System.Reflection;
using BackLight.Key.ErrCode;
using BackLight.Key.Define;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackLight.Key.Sentinel
{
    class Key_Sentinel:IKey
    {
        SentineKey gSentineKey;
        private HWType _HWtype;
        private Boolean isOpen = false;
        public override RetErr Open()
        {
            RetErr ret = new RetErr();
            try
            {
                // new
                gSentineKey = new SentineKey();
                _HWtype = new HWType();

                // init & Get Key info
                ret = gSentineKey.CheckLincense_Software(out _HWtype);
                if (!ret.flag)
                    throw new Exception(ret.Meg);

                // flag
                isOpen = true;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._Open,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._Open;
                ret.Meg = "Sentinel Key Open : " + ee.Message;
                return ret;
            }
            return ret;
        }
        public override RetErr Close()
        {
            RetErr ret = new RetErr();
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");
                throw new Exception("Cann't find Close Method !");
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._Close,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._Close;
                ret.Meg = "Sentinel Key Close ";
                return ret;
            }
            return ret;
        }
        public override RetErr GetHWType(out Define.HWType type)
        {
            RetErr ret = new RetErr();
            Define.HWType HWtype = new HWType();
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");

                
                HWtype = _HWtype;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetHWType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetHWType;
                ret.Meg = "Key Get HW Type";
                type = HWtype;
                return ret;
            }
            type = HWtype;
            return ret;
        }

        public override RetErr SetHWType(Define.HWType type)
        {
            RetErr ret = new RetErr();
            Define.HWType HWtype = new HWType();
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");


                _HWtype = type;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetHWType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetHWType;
                ret.Meg = "Key Set HW Type";
                type = HWtype;
                return ret;
            }
            type = HWtype;
            return ret;
        }
        public override RetErr GetAxisType(out Define.Axis type)
        {
            RetErr ret = new RetErr();
            Define.Axis _Axis = Define.Axis.na; 
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");
                
                if(_HWtype._Axis == 0)
                    throw new Exception("Get AxisType fail , Please chk HW Key/Open First! ");
                _Axis = _HWtype._Axis;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetAxisType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetAxisType;
                ret.Meg = "Key Get Axis Type";
                type = _Axis;
                return ret;
            }
            type = _Axis;
            return ret;
        }
        public override RetErr GetCCDType(out Define.CCD type)
        {
            RetErr ret = new RetErr();
            Define.CCD _CCD = Define.CCD.na;
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");

                if (_HWtype._CCD == 0)
                    throw new Exception("Get CCDType fail , Please chk HW Key/Open First! ");
                _CCD = _HWtype._CCD;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetCCDType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetCCDType;
                type = _CCD;
                ret.Meg = "Key Get CCD Type";
                return ret;
            }
            type = _CCD;
            return ret;
        }
        public override RetErr GetIOType(out Define.IO type)
        {
            RetErr ret = new RetErr();
            Define.IO _IO = Define.IO.na;
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");

                if (_HWtype._CCD == 0)
                    throw new Exception("Get IOType fail , Please chk HW Key/Open First! ");
                _IO = _HWtype._IO;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetIOType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetIOType;
                type = _IO;
                ret.Meg = "Key Get IO Type";
                return ret;
            }
            type = _IO;
            return ret;
        }
        public override RetErr GetLaserType(out Define.Laser type)
        {
            RetErr ret = new RetErr();
            Define.Laser _Laser = Define.Laser.na;
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");

                if (_HWtype._CCD == 0)
                    throw new Exception("Get LaserType fail , Please chk HW Key/Open First! ");
               
                _Laser = _HWtype._Laser;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetLaserType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetLaserType;
                type = _Laser;
                ret.Meg = "Key Get Laser Type";
                return ret;
            }
            type = _Laser;
            return ret;
        }
        public override RetErr GetAO(out Define.AO type)
        {
            RetErr ret = new RetErr();
            Define.AO _AO = Define.AO.na;
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");

                if (_HWtype._CCD == 0)
                    throw new Exception("Get AOType fail , Please chk HW Key/Open First! ");

                _AO = _HWtype._AO;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetLaserType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetLaserType;
                type = _AO;
                ret.Meg = "Key Get AO Type";
                return ret;
            }
            type = _AO;
            return ret;
        }
        public override RetErr GetGalvo(out Define.Galvo type)
        {
            RetErr ret = new RetErr();
            Define.Galvo _Galvo = Define.Galvo.na;
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");

                if (_HWtype._CCD == 0)
                    throw new Exception("Get GalvoType fail , Please chk HW Key/Open First! ");

                _Galvo = _HWtype._Galvo;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetGalvoType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetGalvoType;
                type = _Galvo;
                ret.Meg = "Key Get Galvo Type";
                return ret;
            }
            type = _Galvo;
            return ret;
        }
        public override RetErr GetAI(out Define.AI type)
        {
            RetErr ret = new RetErr();
            Define.AI _AI = Define.AI.na;
            try
            {
                if (!isOpen) throw new Exception("doesn't Open , please Open First! ");

                if (_HWtype._CCD == 0)
                    throw new Exception("Get AIType fail , Please chk HW Key/Open First! ");

                _AI = _HWtype._AI;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetAIType,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetAIType;
                type = _AI;
                ret.Meg = "Key Get AI Type";
                return ret;
            }
            type = _AI;
            return ret;
        }
    }

    class SentineKey
    {
        #region Para

        uint SP_LEASEDEMO_AES = 9; 
        uint SP_STANDALONE_MODE = 32;
        System.UIntPtr licHandle ;

        Byte[] STRING_INFO = new Byte[2032];


        public const uint  LICENSEID                                          = 0xB4DC;
        public const ulong DEVELOPERID                                        = 0xBD93047C;
        public const uint  SP_AESENCRYPTION_AES                               = 0x2C;
        public const uint  SP_LEASECTL_AES                                    = 0x9;
        public const uint  SP_SETUPSTRING1_STRING                             = 0xA;

        /*  Software Key:*/ 
        public const int SOFTWARE_KEY_Lenth = 280;
        public byte []  SOFTWARE_KEY = new byte[SOFTWARE_KEY_Lenth] {
         0x42, 0x1F, 0x2F, 0x3B, 0x26, 0x9B, 0x6F, 0x23, 0xDF, 0x06, 0x0E, 0x36, 0x2C, 0x0D, 0xA3, 0x13,
         0xA5, 0x87, 0x72, 0x3A, 0x48, 0x48, 0xD9, 0x08, 0x11, 0x86, 0xF1, 0xD1, 0xD0, 0x89, 0x83, 0x65,
         0x69, 0x3E, 0x04, 0x81, 0x9E, 0x9C, 0x5E, 0x15, 0x49, 0x24, 0x2E, 0x7B, 0x40, 0x4F, 0xDF, 0xC8,
         0xD7, 0xE8, 0x26, 0x78, 0x41, 0x5C, 0xF2, 0x77, 0x9C, 0x0B, 0x0E, 0x1A, 0x47, 0x6F, 0x43, 0x37,
         0x95, 0x98, 0xEB, 0xD0, 0x2B, 0xC7, 0x3C, 0xAB, 0xE9, 0xCC, 0xF2, 0xE2, 0xC9, 0x6D, 0xB0, 0x9C,
         0x8E, 0x84, 0xC2, 0x17, 0xA9, 0x29, 0x73, 0x3A, 0x7B, 0x0C, 0xCB, 0x3D, 0x55, 0x60, 0x46, 0x4E,
         0x38, 0x91, 0xD3, 0x26, 0x0A, 0x6F, 0x8D, 0x00, 0x3F, 0x78, 0x2A, 0x1C, 0x58, 0x46, 0xBD, 0x48,
         0xCF, 0xE0, 0x14, 0x79, 0xAC, 0xC4, 0x6C, 0x85, 0xB7, 0x8D, 0x2E, 0xC5, 0xCB, 0x6B, 0x78, 0x53,
         0x4A, 0x66, 0x8F, 0x93, 0x6E, 0xDD, 0xA3, 0x7A, 0x87, 0xBC, 0xA3, 0xDD, 0xFC, 0x42, 0x35, 0xA5,
         0x0D, 0x6D, 0xA2, 0xDF, 0xBD, 0x17, 0x81, 0x47, 0x9A, 0x26, 0x4F, 0x5A, 0x1C, 0x80, 0xBA, 0xB3,
         0xC8, 0xD2, 0x0F, 0x4E, 0x18, 0xB2, 0x1F, 0xE0, 0xF0, 0x78, 0x53, 0xE8, 0x69, 0x5D, 0x69, 0xB1,
         0xCC, 0x81, 0xA5, 0xFC, 0xE4, 0x36, 0x63, 0x4D, 0xE6, 0x13, 0x4B, 0x58, 0xB8, 0xEC, 0x38, 0xDA,
         0xF6, 0x84, 0x63, 0x7B, 0xD4, 0x41, 0x23, 0xD4, 0x26, 0xC8, 0xF8, 0x0A, 0xBB, 0x04, 0xF3, 0x0C,
         0x97, 0x54, 0xB5, 0x9D, 0xE3, 0xDD, 0xDD, 0xC3, 0x42, 0x4D, 0xA1, 0x68, 0x76, 0x76, 0xEB, 0xC5,
         0x82, 0xB1, 0x6E, 0xED, 0xD5, 0xFD, 0xAA, 0xAC, 0xB8, 0x19, 0xE3, 0xA8, 0xD9, 0x8B, 0x1E, 0xD9,
         0x6B, 0xF5, 0xDA, 0x41, 0x1E, 0x8F, 0xA4, 0x86, 0xE1, 0x88, 0xC9, 0x74, 0xB0, 0xE8, 0x2F, 0x17,
         0x65, 0x10, 0xF2, 0x36, 0x49, 0xC8, 0xB3, 0x7A, 0x7A, 0xEB, 0x2F, 0xCF, 0x69, 0x4D, 0x78, 0x0A,
         0x04, 0x7D, 0x5C, 0xE0, 0xBD, 0x88, 0xB3, 0x15
         };


        public FeatureInfo KeyInfo = new FeatureInfo();

        #endregion
        static class Dll
        {
            [DllImport("SentinelKeyW.dll",
               CharSet = CharSet.Ansi,
               EntryPoint = "SFNTGetLicense")]
            public static extern uint SFNTGetLicense( /* IN */ uint devID,
                                                   /* IN */ byte[] softwareKey,
                                                   /* IN */ uint licID,
                                                   /* IN */ uint flags,
                                                   /* OUT*/ out System.UIntPtr licHandle);

            [DllImport("SentinelKeyW.dll",
                 CharSet = CharSet.Ansi,
                 EntryPoint = "SFNTGetFeatureInfo")]
            public static extern uint SFNTGetFeatureInfo(/* IN */ System.UIntPtr licHandle,
                                                    /* IN */ uint featureID,
                                                    /* OUT*/ byte[] featureInfo);

            [DllImport("SentinelKeyW.dll",
                 CharSet = CharSet.Ansi,
                 EntryPoint = "SFNTReleaseLicense")]
            public static extern uint SFNTReleaseLicense(/* IN */ System.UIntPtr licHandle);

            [DllImport("SentinelKeyW.dll", 
				CharSet=CharSet.Ansi, 
				EntryPoint="SFNTReadString")]
		public static extern uint SFNTReadString ( /* IN */ System.UIntPtr licHandle,
														/* IN */ uint featureID,
														/* OUT*/ byte[] stringBuffer, 
														/* IN */ uint stringLength );

        }

        public class FeatureInfo
        {
            private int MAX_FEATUREINFO_LENGTH = 35;
            private int BYTE_OFFSET_FEATURETYPE = 0;
            private int BYTE_OFFSET_FEATURESIZE = 4;
            private int BYTE_OFFSET_FEATUREATTRIBUTES = 8;
            private int BYTE_OFFSET_ENABLECOUNTER = 12;
            private int BYTE_OFFSET_ENABLESTOPTIME = 13;
            private int BYTE_OFFSET_ENABLEDURATION = 14;
            private int BYTE_OFFSET_DURATION = 16;
            private int BYTE_OFFSET_FEATUREYEAR = 20;
            private int BYTE_OFFSET_FEATUREMONTH = 24;
            private int BYTE_OFFSET_FEATUREDAY = 25;
            private int BYTE_OFFSET_FEATUREHOUR = 26;
            private int BYTE_OFFSET_FEATUREMINUTE = 27;
            private int BYTE_OFFSET_FEATURESECOND = 28;
            private int BYTE_OFFSET_LEFTEXECUTIONNUMBER = 32;
            public Byte[] btt = new Byte[35];
            public Int32 FeatureType
            {
                get => BitConverter.ToInt32(btt, BYTE_OFFSET_FEATURETYPE);
            }

            public Int32 FeatureSize
            {
                get { return BitConverter.ToInt32(btt, BYTE_OFFSET_FEATURESIZE); }
            }

            public Int32 FeatureAttributes
            {
                get { return BitConverter.ToInt32(btt, BYTE_OFFSET_FEATUREATTRIBUTES); }
            }

            public Byte bEnableStopTime
            {
                get => btt[BYTE_OFFSET_ENABLESTOPTIME];
            }

            public Byte bEnableCounter
            {
                get => btt[BYTE_OFFSET_ENABLECOUNTER];
            }

            public Byte bEnableDurationTime
            {
                get => btt[BYTE_OFFSET_ENABLEDURATION];
            }

            public Int32 Duration
            {
                get => BitConverter.ToInt32(btt, BYTE_OFFSET_DURATION);
            }
            public Int32 Year
            {
                get { return BitConverter.ToInt32(btt, BYTE_OFFSET_FEATUREYEAR); }
            }
            public Int32 Month
            {
                get { return btt[BYTE_OFFSET_FEATUREMONTH]; }
            }
            public Int32 Day
            {
                get { return btt[BYTE_OFFSET_FEATUREDAY]; }
            }
            public Int32 Hour
            {
                get { return btt[BYTE_OFFSET_FEATUREHOUR]; }
            }
            public Int32 Minute
            {
                get { return btt[BYTE_OFFSET_FEATUREMINUTE]; }
            }
            public Int32 Second
            {
                get { return btt[BYTE_OFFSET_FEATURESECOND]; }
            }

            public Int32 LeftExecutionNumber
            {
                get => btt[BYTE_OFFSET_LEFTEXECUTIONNUMBER];
            }

        }

        #region SubRoute
        public  RetErr Initialize_SOFTWARE_KEY()
        {
            RetErr ret = new RetErr();
            try
            {
                #region DataSet
                
                SOFTWARE_KEY[0] = Byte.Parse("10", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[1] = Byte.Parse("82", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[2] = Byte.Parse("A3", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[3] = Byte.Parse("A4", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[4] = Byte.Parse("34", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[5] = Byte.Parse("9C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[6] = Byte.Parse("82", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[7] = Byte.Parse("E8", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[8] = Byte.Parse("6F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[9] = Byte.Parse("AD", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[10] = Byte.Parse("EE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[11] = Byte.Parse("4C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[12] = Byte.Parse("66", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[13] = Byte.Parse("54", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[14] = Byte.Parse("FA", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[15] = Byte.Parse("88", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[16] = Byte.Parse("F7", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[17] = Byte.Parse("1A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[18] = Byte.Parse("FE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[19] = Byte.Parse("A5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[20] = Byte.Parse("5A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[21] = Byte.Parse("4F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[22] = Byte.Parse("34", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[23] = Byte.Parse("C3", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[24] = Byte.Parse("A1", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[25] = Byte.Parse("2D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[26] = Byte.Parse("11", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[27] = Byte.Parse("AB", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[28] = Byte.Parse("9A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[29] = Byte.Parse("D0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[30] = Byte.Parse("DA", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[31] = Byte.Parse("FE", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[32] = Byte.Parse("0B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[33] = Byte.Parse("08", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[34] = Byte.Parse("CE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[35] = Byte.Parse("5A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[36] = Byte.Parse("01", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[37] = Byte.Parse("FE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[38] = Byte.Parse("2F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[39] = Byte.Parse("26", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[40] = Byte.Parse("59", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[41] = Byte.Parse("EF", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[42] = Byte.Parse("6F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[43] = Byte.Parse("40", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[44] = Byte.Parse("72", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[45] = Byte.Parse("4C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[46] = Byte.Parse("80", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[47] = Byte.Parse("69", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[48] = Byte.Parse("B5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[49] = Byte.Parse("DE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[50] = Byte.Parse("EC", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[51] = Byte.Parse("A3", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[52] = Byte.Parse("DE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[53] = Byte.Parse("3E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[54] = Byte.Parse("83", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[55] = Byte.Parse("44", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[56] = Byte.Parse("8C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[57] = Byte.Parse("C0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[58] = Byte.Parse("4F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[59] = Byte.Parse("21", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[60] = Byte.Parse("75", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[61] = Byte.Parse("6C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[62] = Byte.Parse("1C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[63] = Byte.Parse("96", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[64] = Byte.Parse("D1", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[65] = Byte.Parse("84", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[66] = Byte.Parse("E0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[67] = Byte.Parse("F2", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[68] = Byte.Parse("91", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[69] = Byte.Parse("FC", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[70] = Byte.Parse("36", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[71] = Byte.Parse("F0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[72] = Byte.Parse("7C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[73] = Byte.Parse("A0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[74] = Byte.Parse("01", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[75] = Byte.Parse("29", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[76] = Byte.Parse("67", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[77] = Byte.Parse("3D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[78] = Byte.Parse("D6", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[79] = Byte.Parse("4B", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[80] = Byte.Parse("CA", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[81] = Byte.Parse("98", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[82] = Byte.Parse("C9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[83] = Byte.Parse("35", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[84] = Byte.Parse("13", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[85] = Byte.Parse("12", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[86] = Byte.Parse("79", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[87] = Byte.Parse("61", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[88] = Byte.Parse("EE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[89] = Byte.Parse("60", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[90] = Byte.Parse("38", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[91] = Byte.Parse("F6", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[92] = Byte.Parse("FB", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[93] = Byte.Parse("30", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[94] = Byte.Parse("20", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[95] = Byte.Parse("99", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[96] = Byte.Parse("DC", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[97] = Byte.Parse("0C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[98] = Byte.Parse("AE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[99] = Byte.Parse("58", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[100] = Byte.Parse("F5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[101] = Byte.Parse("87", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[102] = Byte.Parse("06", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[103] = Byte.Parse("18", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[104] = Byte.Parse("E8", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[105] = Byte.Parse("07", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[106] = Byte.Parse("66", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[107] = Byte.Parse("53", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[108] = Byte.Parse("AE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[109] = Byte.Parse("23", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[110] = Byte.Parse("D5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[111] = Byte.Parse("86", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[112] = Byte.Parse("B9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[113] = Byte.Parse("0A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[114] = Byte.Parse("1A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[115] = Byte.Parse("80", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[116] = Byte.Parse("4F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[117] = Byte.Parse("CE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[118] = Byte.Parse("54", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[119] = Byte.Parse("F8", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[120] = Byte.Parse("04", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[121] = Byte.Parse("7F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[122] = Byte.Parse("1E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[123] = Byte.Parse("F9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[124] = Byte.Parse("CB", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[125] = Byte.Parse("AA", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[126] = Byte.Parse("33", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[127] = Byte.Parse("3A", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[128] = Byte.Parse("63", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[129] = Byte.Parse("F1", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[130] = Byte.Parse("B2", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[131] = Byte.Parse("72", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[132] = Byte.Parse("E2", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[133] = Byte.Parse("49", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[134] = Byte.Parse("90", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[135] = Byte.Parse("6D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[136] = Byte.Parse("11", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[137] = Byte.Parse("B5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[138] = Byte.Parse("95", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[139] = Byte.Parse("5C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[140] = Byte.Parse("0F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[141] = Byte.Parse("AD", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[142] = Byte.Parse("25", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[143] = Byte.Parse("99", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[144] = Byte.Parse("59", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[145] = Byte.Parse("4F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[146] = Byte.Parse("4E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[147] = Byte.Parse("5D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[148] = Byte.Parse("C4", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[149] = Byte.Parse("5F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[150] = Byte.Parse("04", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[151] = Byte.Parse("43", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[152] = Byte.Parse("E2", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[153] = Byte.Parse("35", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[154] = Byte.Parse("19", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[155] = Byte.Parse("7F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[156] = Byte.Parse("D2", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[157] = Byte.Parse("8E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[158] = Byte.Parse("5A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[159] = Byte.Parse("52", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[160] = Byte.Parse("2A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[161] = Byte.Parse("D1", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[162] = Byte.Parse("BE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[163] = Byte.Parse("27", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[164] = Byte.Parse("01", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[165] = Byte.Parse("53", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[166] = Byte.Parse("4D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[167] = Byte.Parse("E9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[168] = Byte.Parse("9F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[169] = Byte.Parse("6D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[170] = Byte.Parse("9C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[171] = Byte.Parse("0B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[172] = Byte.Parse("1D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[173] = Byte.Parse("F5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[174] = Byte.Parse("E1", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[175] = Byte.Parse("BA", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[176] = Byte.Parse("FC", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[177] = Byte.Parse("74", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[178] = Byte.Parse("95", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[179] = Byte.Parse("2A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[180] = Byte.Parse("3E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[181] = Byte.Parse("D0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[182] = Byte.Parse("D9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[183] = Byte.Parse("97", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[184] = Byte.Parse("C0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[185] = Byte.Parse("CF", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[186] = Byte.Parse("78", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[187] = Byte.Parse("E4", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[188] = Byte.Parse("6B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[189] = Byte.Parse("72", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[190] = Byte.Parse("1A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[191] = Byte.Parse("00", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[192] = Byte.Parse("08", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[193] = Byte.Parse("F8", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[194] = Byte.Parse("6B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[195] = Byte.Parse("29", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[196] = Byte.Parse("23", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[197] = Byte.Parse("EE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[198] = Byte.Parse("B5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[199] = Byte.Parse("30", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[200] = Byte.Parse("82", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[201] = Byte.Parse("47", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[202] = Byte.Parse("DB", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[203] = Byte.Parse("3C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[204] = Byte.Parse("6B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[205] = Byte.Parse("B9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[206] = Byte.Parse("DB", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[207] = Byte.Parse("2A", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[208] = Byte.Parse("FD", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[209] = Byte.Parse("E4", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[210] = Byte.Parse("6A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[211] = Byte.Parse("6E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[212] = Byte.Parse("4F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[213] = Byte.Parse("7A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[214] = Byte.Parse("9E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[215] = Byte.Parse("AE", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[216] = Byte.Parse("FD", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[217] = Byte.Parse("5C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[218] = Byte.Parse("4A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[219] = Byte.Parse("3C", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[220] = Byte.Parse("B0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[221] = Byte.Parse("02", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[222] = Byte.Parse("EB", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[223] = Byte.Parse("9C", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[224] = Byte.Parse("7D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[225] = Byte.Parse("6D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[226] = Byte.Parse("AA", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[227] = Byte.Parse("53", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[228] = Byte.Parse("99", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[229] = Byte.Parse("F4", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[230] = Byte.Parse("00", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[231] = Byte.Parse("C4", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[232] = Byte.Parse("36", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[233] = Byte.Parse("3F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[234] = Byte.Parse("71", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[235] = Byte.Parse("BB", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[236] = Byte.Parse("C9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[237] = Byte.Parse("E8", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[238] = Byte.Parse("C2", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[239] = Byte.Parse("D6", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[240] = Byte.Parse("19", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[241] = Byte.Parse("4A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[242] = Byte.Parse("0A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[243] = Byte.Parse("5B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[244] = Byte.Parse("E1", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[245] = Byte.Parse("2B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[246] = Byte.Parse("81", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[247] = Byte.Parse("B9", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[248] = Byte.Parse("6E", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[249] = Byte.Parse("72", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[250] = Byte.Parse("30", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[251] = Byte.Parse("9F", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[252] = Byte.Parse("1D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[253] = Byte.Parse("91", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[254] = Byte.Parse("1A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[255] = Byte.Parse("4A", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[256] = Byte.Parse("2B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[257] = Byte.Parse("12", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[258] = Byte.Parse("C5", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[259] = Byte.Parse("3B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[260] = Byte.Parse("70", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[261] = Byte.Parse("3B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[262] = Byte.Parse("59", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[263] = Byte.Parse("2A", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[264] = Byte.Parse("05", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[265] = Byte.Parse("77", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[266] = Byte.Parse("0B", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[267] = Byte.Parse("51", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[268] = Byte.Parse("D0", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[269] = Byte.Parse("A6", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[270] = Byte.Parse("10", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[271] = Byte.Parse("02", System.Globalization.NumberStyles.HexNumber);

                SOFTWARE_KEY[272] = Byte.Parse("30", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[273] = Byte.Parse("41", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[274] = Byte.Parse("C7", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[275] = Byte.Parse("61", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[276] = Byte.Parse("D8", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[277] = Byte.Parse("CD", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[278] = Byte.Parse("0D", System.Globalization.NumberStyles.HexNumber);
                SOFTWARE_KEY[279] = Byte.Parse("64", System.Globalization.NumberStyles.HexNumber);
                

                #endregion



            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(Num._initPara,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._initPara;
                ret.Meg = "SentineKey init para";
                return ret;
            }
            return ret;

            //return 0;//Christa 2015/11/24
        }

        public RetErr CheckLincense_Software(out HWType hwType)
        {
            RetErr ret = new RetErr();
            HWType _HWTYPE = new HWType();
            try
            {

                UInt32 decID = 0;
                decID = UInt32.Parse("BD93047C", System.Globalization.NumberStyles.HexNumber);

                //ret = Initialize_SOFTWARE_KEY();
                //if (!ret.flag) throw new Exception(ret.Meg);

                //// 硬體鎖連線
                uint tamp = Dll.SFNTGetLicense(decID, SOFTWARE_KEY, LICENSEID, SP_STANDALONE_MODE, out licHandle);
                if (tamp != 0)
                    throw new Exception("Hardway Key not Find !");

                //// 取出硬體鎖資料
                FeatureInfo info = new FeatureInfo();
                tamp = Dll.SFNTGetFeatureInfo(licHandle, SP_LEASEDEMO_AES, KeyInfo.btt);
                if (tamp != 0)
                    throw new Exception("Hardway Key Get Infomation Fail !");

                //// 取出硬體字串資料
                byte[] ReadValue = new Byte[256];
                tamp = Dll.SFNTReadString(licHandle, SP_SETUPSTRING1_STRING, ReadValue, 50);
                if (tamp != 0)
                throw new Exception("Hardway Key Get string Fail !");

                string array = System.Text.Encoding.Default.GetString(ReadValue, 0, ReadValue.Length).Replace("\0", "");
                //MessageBox.Show(array);
                TypeParse(array, out _HWTYPE);


                //// Release
                Dll.SFNTReleaseLicense(licHandle);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._chkLincense,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                //// Release
                Dll.SFNTReleaseLicense(licHandle);

                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._chkLincense;
                ret.Meg = "SentineKey chk licese & Set HWType: " + ee.Message;
                hwType = _HWTYPE;
                return ret;

            }
            hwType = _HWTYPE;
            return ret;
        }

        private Boolean TypeParse(string str, out HWType hwType)
        {
            HWType _hwType = new HWType();
            try
            {
                // AA=4;AB=1;AC=0;AD=2;AE=2;BA=0;BB=0;BC=1;CA=1;CB=0;CC=0;DA=0;DB=8;
                if (str == null || str == "") throw new Exception("string fail ");
                string[] ori = str.Split(';');

                for(int i = 0; i < ori.Length; i++)
                {
                    string head = ori[i].Substring(0, 2);
                    string value = ori[i].Substring(3, 1);
                    switch (head)
                    {
                        case "AA":
                            _hwType._Laser = (Define.Laser)Convert.ToInt16(value);
                            break;
                        case "AB":
                            _hwType._CCD = (Define.CCD)Convert.ToInt16(value);
                            break;
                        case "AC":
                            _hwType._IO = (Define.IO)Convert.ToInt16(value);
                            break;
                        case "AD":
                            _hwType._Axis = (Define.Axis)Convert.ToInt16(value);
                            break;
                        case "AE":
                            _hwType._AO = (Define.AO)Convert.ToInt16(value);
                            break;
                        case "AF":
                            _hwType._Galvo = (Define.Galvo)Convert.ToInt16(value);
                            break;
                        case "AG":
                            _hwType._AI = (Define.AI)Convert.ToInt16(value);
                            break;
                        case "BA": //0: false, 1: true
                            _hwType.XFlip = value == "1" ? true : false;
                            break;
                        case "BB": //0: false, 1: true
                            _hwType.YFlip = value == "1" ? true : false;
                            break;
                        case "BC": //0: false, 1: true
                            _hwType.XYChange = value == "1" ? true : false;
                            break;
                        case "CA": //0: false, 1: true
                            _hwType.Galvo_XFlip = value == "1" ? true : false;
                            break;
                        case "CB": //0: false, 1: true
                            _hwType.Galvo_YFlip = value == "1" ? true : false;
                            break;
                        case "CC": //0: false, 1: true
                            _hwType.Galvo_XYChange = value == "1" ? true : false;
                            break;
                        case "DA": //0: false, 1: true
                            _hwType.Wobble = value == "1" ? true : false;
                            break;
                        case "DB": //2~8 denotes counts, less than 2 to force 2
                            _hwType.VisionCunt = Convert.ToInt32(value);                    
                            break;
                        case "EA": // 0: Ignored, 1: ProduceRecorder 
                            _hwType.ExtensionA = value == "1" ? true : false;               
                            break;

                    }
                }
            }
            catch(Exception ee)
            {
                hwType = _hwType;
                return false;

            }
            hwType = _hwType;
            return true;
        }

        #endregion
    }


}
