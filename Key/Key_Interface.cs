#define Log
using System;
using System.IO;
using System.Windows.Forms;
using BackLight.Key.ErrCode;
using BackLight.Key.Define;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackLight.Key
{
    class Key_Interface
    {
    }
    abstract class IKey
    {
        public abstract RetErr Open();
        public abstract RetErr Close();
        public abstract RetErr GetHWType(out Define.HWType type);
        public abstract RetErr GetAxisType(out Define.Axis type);
        public abstract RetErr GetCCDType(out Define.CCD type);
        public abstract RetErr GetIOType(out Define.IO type);
        public abstract RetErr GetLaserType(out Define.Laser type);
        public abstract RetErr GetAO(out Define.AO type);
        public abstract RetErr GetGalvo(out Define.Galvo type);
        public abstract RetErr GetAI(out Define.AI type);
        public abstract RetErr SetHWType(Define.HWType type);
    }
}

namespace BackLight.Key.ErrCode
{
    class Num   // -601 ~ -700
    {
        public static int _chkLincense = -601;
        public static int _initPara = -602;
        public static int _Open = -603;
        public static int _Close = -604;
        public static int _GetAxisType = -605;
        public static int _GetCCDType = -606;
        public static int _GetIOType = -607;
        public static int _GetLaserType = -608;
        public static int _GetAOType = -609;
        public static int _GetGalvoType = -610;
        public static int _GetAIType = -611;
        public static int _GetHWType = -612;
    }
    class RetErr
    {
        public Boolean flag = true;
        public int Num = 0;
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
        public static void Pushlist(int NGCode, string Who, string what)
        {
            //初始化字串
            string NGString = "";
            string NGtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");


            // 重組字串
            NGString = "# " + NGtime + " @ " + NGCode.ToString() + " $ " + Who + " , " + what;

            //寫入文檔
            string pat = Application.StartupPath + "\\Log\\Key_Log.log";
            StreamWriter strr = new StreamWriter(pat, true, Encoding.Default);
            strr.WriteLine(NGString, false);
            strr.Close();

        }

        /// <summary>
        /// 取得 目前正在執行的 Function Info 資訊
        /// </summary>
        /// <returns></returns>
        public static String GetCurrentMethodInfo()
        {
            string showString = "";
            ////取得當前方法類別命名空間名稱
            //showString += "Namespace:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + ", ";
            ////取得當前類別名稱
            //showString += "class Name:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName + ", ";
            //取得當前所使用的方法
            showString += "Method:" + System.Reflection.MethodBase.GetCurrentMethod().Name + ", ";

            return showString;
        }
    }
}

namespace BackLight.Key.Define
{
    struct HWType
    {
        public Boolean XFlip;
        public Boolean YFlip;
        public Boolean XYChange;
        public Boolean Galvo_XFlip;
        public Boolean Galvo_YFlip;
        public Boolean Galvo_XYChange;
        public Boolean Wobble;
        public int VisionCunt;  //2~8 denotes counts, less than 2 to force 2
        public Boolean ExtensionA;
        public Axis _Axis;
        public CCD _CCD;
        public IO _IO;
        public Laser _Laser;
        public AO _AO;
        public Galvo _Galvo;
        public AI _AI;
    }
    enum Axis
    {
        na=99, A3200=2, PMC6=5, Servetronic=3, U500=1, UTC400=4, Dummy=0, ACS=6,
    }
    enum CCD
    {
        na=99, Picolo=1, AISYS=2, Dummy = 0,
    }
    enum IO
    {
        na=99, Advantech=2, L122=4, L112=3, U500=1, Dummy = 0,
    }
    enum Key
    {
        na, Sentinel,
    }
    enum Laser
    {
        na=99, Coherent=1, Mulitiwave=3, Optowave=2, PicoUV=7, SPUV=5, SPI=4, YVO4=6, Dummy = 0,
    }
    enum AO
    {
        na=99, U500=1, A3200=2, L112=3, L122=4, Dummy = 0,
    }
    enum Galvo
    {
        na=0, LightningII=1, RTC3=2, RTC5=3, Dummy = 0,
    }
    enum AI
    {
        na = 99, U500 = 1, A3200 = 2, L112 = 3, L122 = 4, Dummy = 0,
    }


}