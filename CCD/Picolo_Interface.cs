
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using GalvoScan.Hardware.CCD.Picolo;
using Euresys.Open_eVision_2_15;
using GalvoScan.Hardware.CCD.ErrCode;

namespace GalvoScan.Hardware.CCD
{    

    abstract class IPicolo    // InterFace of Picolo 
    {
        public abstract RetErr cam_init(VID ChannelId, Control ctrl1);
        public abstract RetErr cam_StartGetImage();
        public abstract RetErr cam_StopGetImage();
        public abstract RetErr CopyROI(Control ctrl2);
        public abstract RetErr LoadGoldenSample(string filepath,Control ctrl2);
        //public abstract RetErr SavePattern(string filepath);
        public abstract RetErr m_Match(string filepath, out float X, out float Y);
        public abstract RetErr m_CircleGauge(int cirX, int cirY, int cirR, int BW, out float X_offset, out float Y_offset);

        public abstract RetErr SaveROI(string filepath);

        public abstract RetErr SaveImage(string filepath);

        public abstract RetErr FindMark(string AOIPath, s_MarkPara GaugePara, out bool isFind, out PointF Offset);

        public abstract RetErr Save_s_MarkPara(s_MarkPara GaugePara);
        public abstract RetErr Load_s_MarkPara(s_MarkPara GaugePara);


        public enum VID { VID1,VID2}
    }
    

    public class s_MarkPara
    {
        //public string Path = "";
        /*<ProjectVisionMarkPara>*/
        //public double PosMarkX = 10;
        //public double PosMarkY = 10;
        /*<PatternMatchingPara>*/
        /*<MatchingPara>*/
        public double Score = 50;//..........use
        /*</MatchingPara>*/
        //public double CurveFittingMethod = 1;
        ///*<RectFittingPara>*/
        //public double Width_Rect = 50;
        //public double Height_Rect = 50;
        //public double Angle_Rect = 0;
        /*<FittingCommonPara>*/
        //public double CenterX_Rect = 0;
        //public double CenterY_Rect = 0;
        //public double Tolerance_Rect = 10;
        //public double CurveFittingSearchDir_Rect = 0;
        //public double CurveFittingSearchTrans_Rect = 0;
        //public double Threshould_Rect = 20;
        /*</FittingCommonPara>*/
        /*</RectFittingPara>*/
        /*<CircleFittingPara>*/
        public double Diameter_Circle = 70;//............use
        /*<FittingCommonPara>*/
        public double CenterX_Circle = 0;//............use
        public double CenterY_Circle = 0;//............use
        public double Tolerance_Circle = 20;//............use
        public double TransitionType = 1;//............use (0=bworwb,1=bw,2=wb)
        public double Threshold = 20;//............use
        public double Thickness = 30;//............use
        //public double CurveFittingSearchDir_Circle = 0;
        //public double CurveFittingSearchTrans_Circle = 0;
        //public double Threshould_Circle = 20;
        /*</FittingCommonPara>*/
        /*</CircleFittingPara>*/
        /*<CrossFittingPara>*/
        //public double Size_Cross = 50;
        //public double Thickness_Cross = 20;
        //public double Angle_Cross = 0;
        /*<FittingCommonPara>*/
        //public double CenterX_Cross = 0;
        //public double CenterY_Cross = 0;
        //public double Tolerance_Cross = 10;
        //public double CurveFittingSearchDir_Cross = 0;
        //public double CurveFittingSearchTrans_Cross = 0;
        //public double Threshould_Cross = 20;
        /*</FittingCommonPara>*/
        /*</CrossFittingPara>*/
        /*<CornerFittingPara>*/
        //public double Size_Corner = 50;
        //public double Angle_Corner = 0;
        //public double IncludedAngle_Corner = 90;
        /*<FittingCommonPara>*/
        //public double CenterX_Corner = 0;
        //public double CenterY_Corner = 0;
        //public double Tolerance_Corner = 10;
        //public double CurveFittingSearchDir_Corner = 0;
        //public double CurveFittingSearchTrans_Corner = 0;
        //public double Threshould_Corner = 20;
        /*</FittingCommonPara>*/
        /*</CornerFittingPara>*/
        /*</PatternMatchingPara>*/
        /*<ImageAdjPara>*/
        public double Gain = 1.6;//..................use
        public double offset = 75;//..................use
        //public double LUTCenterX = 128;
        //public double LUTM = 1;
        /*</ImageAdjPara>*/
        /*</ProjectVisionMarkPara>*/
    }
    public class Show_Img
    { 
        public bool bolShowROI = false;  //是否顯示ROI
        public bool bolShowMatch = false; //是否顯示Match
        public bool bolShowCirGauge = false;//是否顯示Cirgauge
        public float fZoomX = 1.0f;
        public float fZoomY = 1.0f;
    }

}
namespace GalvoScan.Hardware.CCD.ErrCode
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
        /// <param name="NGCode"> 編碼 </param>
        /// <param name="Who"> Method </param>
        /// <param name="what"> message</param>
        /// </summary>
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

    }
}

