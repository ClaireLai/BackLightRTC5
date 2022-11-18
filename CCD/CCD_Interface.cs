using System;
using System.IO;
using System.Windows.Forms;
using BackLight.CCD.ErrCode;
using System.Drawing;
using BackLight.CCD.Define;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Euresys.Open_eVision_2_15;

namespace BackLight.CCD
{
    class CCD_Interface
    {
    }
    public delegate void CCDMouseMoveCallbackDelegate(PointF point);
    interface ICCD
    {
        event CCDMouseMoveCallbackDelegate CCDMouseMoveInfoRecv;

        //public object GaugePara { get; internal set; }

        RetErr SetShowStatus(Show_Img Status);
        RetErr GetShowStatus(out Show_Img Status);
        RetErr SetGaugePara(s_MarkPara GaugePara);
        RetErr GetGaugePara(out s_MarkPara GaugePara);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AOIPath">Mark path</param>
        /// <param name="GaugePara"></param>
        /// <param name="g"></param>
        /// <param name="Offset"></param>
        /// <returns></returns>
        //public abstract RetErr FindMark(string AOIPath, s_MarkPara GaugePara, out bool isFind, out PointF Offset);

        RetErr cam_init(VID ChannelId, ref Panel ctrl1);
        RetErr cam_StartGetImage();
        RetErr cam_StopGetImage();
        RetErr CopyROI(Control ctrl2);
        RetErr LoadGoldenSample(string filepath, Control ctrl2);
        //public abstract RetErr SavePattern(string filepath);
        RetErr m_Match(string filepath, out float X, out float Y);
        //public abstract RetErr m_CircleGauge(int cirX, int cirY, int cirR,int BW, out float X_offset, out float Y_offset);

        RetErr SaveROI(string filepath);
        RetErr SaveImage(string filepath);
        RetErr FindMark(string AOIPath, s_MarkPara GaugePara, out bool isFind, out PointF Offset);
        //RetErr GetLineGaugPara(out s_lineGaugePara LineGaugPara);
        RetErr LineGauge_Attach(LineNum linNum, out PointF org, out PointF ent, out float ang);
        RetErr SetRECCenter(PointF point);
        RetErr GetIntersection(PointF lineFirstStar, PointF lineFirstEnd, PointF lineSecondStar, PointF lineSecondEnd, out bool bolFind, out PointF PIntersection);
        RetErr Save_s_MarkPara();
        RetErr Load_s_MarkPara();
        RetErr ROI_Attach();
        RetErr CirGauge_Attach();
    }
    public enum VID { VID1, VID2 }
    public enum LineNum { Line_L, Line_D, Line_R, Line_U }

}

namespace BackLight.CCD.ErrCode
{
    class Num   // -301 ~ -400
    {
        public static int _OpenErr = -301;
        public static int _CloseErr = -302;
        public static int _PathErr = -303;
        public static int _GoldenSampleErr = -304;
        public static int _MatchErr = -305;
        public static int _MatchParaErr = -306;
        public static int _GaugeErr = -307;
        public static int _GaugeParaErr = -308;
        public static int _CopyROIErr = -309;
        public static int _SetShowStatus = -310;
        public static int _GetShowStatus = -311;
        public static int _SetGaugePara = -312;
        public static int _GetGaugePara = -313;
        public static int _SaveROI = -314;
        //public static int _SetLineGaugePara = -315;
        public static int _GetLineGaugePara = -316;

    }
    class RetErr
    {
        public Boolean flag = true;
        public string Meg = "";
        public int Num = 0;
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
            string pat = Application.StartupPath + "\\Log\\CCD_Log.log";
            StreamWriter strr = new StreamWriter(pat, true);
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
            //取得當前方法類別命名空間名稱
            showString += "Namespace:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + ", ";
            //取得當前類別名稱
            showString += "class Name:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName + ", ";
            //取得當前所使用的方法
            showString += "Method:" + System.Reflection.MethodBase.GetCurrentMethod().Name + ", ";

            return showString;
        }
    }
}

namespace BackLight.CCD.Define
{
    public class Show_Img
    {
        public bool bolShowROI = false;  //是否顯示ROI
        public bool bolShowMatch = false; //是否顯示Match      
        public bool bolShowCirGauge = false;//是否顯示Cirgauge
        public bool bolShowLineGauge = false;//是否顯示Linegauge
        public bool bolShowLineGauge_L = false;//是否顯示Linegauge
        public bool bolShowLineGauge_D = false;//是否顯示Linegauge
        public bool bolShowLineGauge_R = false;//是否顯示Linegauge
        public bool bolShowLineGauge_U = false;//是否顯示Linegauge
        public bool bolShowInterSetion = false;//是否顯示Linegauge 交點
        public bool bolShowThreadhold = false;//是否顯示二值化
        public bool bolShowGainOffset = false;//是否顯示GainOffset
        public bool bolShowTen = true;//是否顯示十字線
        public bool bolShowFlipX = false;//是否顯示FlipX
        public bool bolShowFlipY = false;//是否顯示FlipY
        public double ptmm = 0.0019;//pixel to mm
        public uint UThreadhold = 50;
        public float fZoomX = 1.0f;
        public float fZoomY = 1.0f;
        public int ieX = 0;
        public int ieY = 0;
    }

    public class s_MarkPara
    {
        public string Path = "";
        /*<ProjectVisionMarkPara>*/
        public double PosMarkX = 10;
        public double PosMarkY = 10;
        /*<PatternMatchingPara>*/
        /*<MatchingPara>*/
        public double Score = 50;//.......use
        /*</MatchingPara>*/
        public double CurveFittingMethod = 1;
        /*<RectFittingPara>*/
        public double Width_Rect = 50;
        public double Height_Rect = 50;
        public double Angle_Rect = 0;
        /*<FittingCommonPara>*/
        public double CenterX_Rect = 0;
        public double CenterY_Rect = 0;
        public double Tolerance_Rect = 10;
        public double CurveFittingSearchDir_Rect = 0;
        public double CurveFittingSearchTrans_Rect = 0;
        public double Threshould_Rect = 20;

        /*<CircleFittingPara>*/
        public double Diameter_Circle = 70;//............use
        /*<FittingCommonPara>*/
        public double CenterX_Circle = 0;//............use
        public double CenterY_Circle = 0;//............use
        public double Tolerance_Circle = 20;//............use
        public double TransitionType = 1;//............use (0=bworwb,1=bw,2=wb)
        public double Threshold = 20;//............use
        public double Thickness = 30;//............use
        public double CurveFittingSearchDir_Circle = 0;
        public double CurveFittingSearchTrans_Circle = 0;
        public double Threshould_Circle = 20;
        /*</FittingCommonPara>*/
        /*</CircleFittingPara>*/
        /*<CrossFittingPara>*/
        public double Size_Cross = 50;
        public double Thickness_Cross = 20;
        public double Angle_Cross = 0;
        /*<FittingCommonPara>*/
        public double CenterX_Cross = 0;
        public double CenterY_Cross = 0;
        public double Tolerance_Cross = 10;
        public double CurveFittingSearchDir_Cross = 0;
        public double CurveFittingSearchTrans_Cross = 0;
        public double Threshould_Cross = 20;
        /*</FittingCommonPara>*/
        /*</CrossFittingPara>*/
        /*<CornerFittingPara>*/
        public double Size_Corner = 50;
        public double Angle_Corner = 0;
        public double IncludedAngle_Corner = 90;
        /*<FittingCommonPara>*/
        public double CenterX_Corner = 0;
        public double CenterY_Corner = 0;
        public double Tolerance_Corner = 10;
        public double CurveFittingSearchDir_Corner = 0;
        public double CurveFittingSearchTrans_Corner = 0;
        public double Threshould_Corner = 20;
        /*</FittingCommonPara>*/
        /*</LineFittingPara>*/
        public double CenterX_LinL = 0;
        public double CenterY_LinL = 0;
        public double CenterX_LinD = 0;
        public double CenterY_LinD = 0;
        public double CenterX_LinR = 0;
        public double CenterY_LinR = 0;
        public double CenterX_LinU = 0;
        public double CenterY_LinU = 0;
        public double LineL_Tol = 10;
        public double LineD_Tol = 20;
        public double LineR_Tol = 10;
        public double LineU_Tol = 10;
        public double LineL_Ang = 0;
        public double LineD_Ang = 0;
        public double LineR_Ang = 90;
        public double LineU_Ang = 90;
        public double LineL_Len = 90;
        public double LineD_Len = 90;
        public double LineR_Len = 90;
        public double LineU_Len = 90;
        /*</FittingCommonPara>*/
        public bool bolBW_L = true;
        public bool bolBW_D = true;
        public bool bolBW_R = true;
        public bool bolBW_U = true;
        public bool bolBE_L = true;
        public bool bolBE_D = true;
        public bool bolBE_R = true;
        public bool bolBE_U = true;
        public int Line_Dir = 0;//左=0 下=1 右=2 上=3
        /*</CornerFittingPara>*/
        /*</PatternMatchingPara>*/
        /*<ImageAdjPara>*/
        public double Gain = 1;//..................use
        public double offset = 75;//..................use
        public double LUTCenterX = 128;
        public double LUTM = 1;
        /*</ImageAdjPara>*/
        /*</ProjectVisionMarkPara>*/
    }
    //public class s_lineGaugePara
    //{
    //    /*</LineFittingPara>*/
    //    public double CenterX_LinL = 0;
    //    public double CenterY_LinL = 0;
    //    public double CenterX_LinD = 0;
    //    public double CenterY_LinD = 0;
    //    public double CenterX_LinR = 0;
    //    public double CenterY_LinR = 0;
    //    public double CenterX_LinU = 0;
    //    public double CenterY_LinU = 0;
    //    public double LineL_Tol = 10;
    //    public double LineD_Tol = 20;
    //    public double LineR_Tol = 10;
    //    public double LineU_Tol = 10;
    //    public double LineL_Ang = 0;
    //    public double LineD_Ang = 0;
    //    public double LineR_Ang = 90;
    //    public double LineU_Ang = 90;
    //    public double LineL_Len = 90;
    //    public double LineD_Len = 90;
    //    public double LineR_Len = 90;
    //    public double LineU_Len = 90;
    //    /*</FittingCommonPara>*/
    //    public bool bolBW_L = true;
    //    public bool bolBW_D = true;
    //    public bool bolBW_R = true;
    //    public bool bolBW_U = true;
    //    public bool bolBE_L = true;
    //    public bool bolBE_D = true;
    //    public bool bolBE_R = true;
    //    public bool bolBE_U = true;
    //    public int Line_Dir = 0;//左=0 下=1 右=2 上=3
    //}
}
