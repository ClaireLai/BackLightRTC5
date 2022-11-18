#define Log
using System;
using System.Reflection;
using System.Collections.Generic;
using BackLight.GUI.Project.ErrCode;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackLight.GUI.Project
{
    class bas_Project
    {
    }
    public class c_PrjSettingBuild
    {
        private Boolean isInit = false;


        /// <summary>
        /// 一個專案所有的設定 (x1 設定, xN 圖層資料)
        /// </summary>
        private stru_PrjAllSetting l_struPrjAllSet;

        /// <summary>
        /// List 放入多個專案(一個專案格式為 "stru_AllSetting")
        /// </summary>
        private List<stru_PrjAllSetting> l_MulitPrjPara;

        #region 資料結構(因子)

        // 一個專案所有資料存取地方
        public struct stru_PrjAllSetting
        {
            public int LayerCunt;
            public stru_PrjSet l_stru_PrjSet;
            public List<stru_PrjLayerSet> l_LayerSetData;

            public void init()
            {
                l_LayerSetData = new List<stru_PrjLayerSet>();
            }
        }
        // 雕刻方向
        public enum MarkDir
        {
            // 水平
            standard,
            // 垂直
            vertucval,
        }

        // 一個專案所有對單一 LAYER 所做的設定
        public struct stru_PrjLayerSet
        {
            public string LayerName;
            public int Show;
            public int Mark;
            public double Thicksness;           // AutoRun時使用
            public int LayerMarkTimes;          // 1 ~ n
            public double ReDownHeigh;          // AutoRun時使用
            public double markDir;              // standard 0 , vertucal 1  
            public int isSplit;
            public double DivWidth;
            public double DivHeigh;
            //public double MaxRatLine;
            public double Power;
            public double Freq;
            public double PulseWidth;
            public double ObjMarkTimes;         // 分圖中物件雕刻次數
            public double MarkSpeed;
            public double JumpSpeed;
            public int JumpDelay;
            public int SpotDelay;
            public int OnDelay;
            public int MiddleDelay;
            public int EndDelay;
            public double LaserMode;                 // 雷射
            public double LaserBias;                 // 雷射
            public double LaserEpulse;               // 雷射
            public int isFill;
            public int isFillTwoWay;
            public int BalanceFill;
            //public int InverFill;               // 0:由外向內, 1:由內向外
            //public int FramMark;
            //public int FramFirst;
            public double FillBorder;
            public double FillPitch;
            public double FillTimes;
            public double FillAngleStart;
            public double FillAngleStep;
            public double Overlapsize;          // 僅對目前資料庫有用 (需先設定為重疊區域)
            public int isWobble;
            public double WobbleThick;
            public int WobbleFreq;
        }

       

        // 一個專案的設定
        public struct stru_PrjSet
        {
            public string DrawName;
            public double PosCCDFocalPos;
            //public double CuttingPiece;
            public double NeedChkMark;
            //public double AllMarkUseMark1Data;
            public double Use2DBarCode;
            public double isSKYW;                   // 打斷 1, 非打斷 0
            //public double EndPtVel;
            public double SKYW_Accel;
            public double SKYW_LimitAngle;
            //public double MaxRadErr;
            //public double MaxAccRat;
            public List<stru_VisionMark> VisionMark;
        }

        // 是決定位靶標參數
        public class stru_VisionMark
        {
            public string Path = "";
            /*<ProjectVisionMarkPara>*/
            public double PosMarkX = 10;
            public double PosMarkY = 10;
            /*<PatternMatchingPara>*/
            /*<MatchingPara>*/
            public double Score = 50;
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
            /*</FittingCommonPara>*/
            /*</RectFittingPara>*/
            /*<CircleFittingPara>*/
            public double Diameter_Circle = 50;
            /*<FittingCommonPara>*/
            public double CenterX_Circle = 0;
            public double CenterY_Circle = 0;
            public double Tolerance_Circle = 10;
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
            /*</CornerFittingPara>*/
            /*</PatternMatchingPara>*/
            /*<ImageAdjPara>*/
            public double Gain = 1;
            public double LUTCenterX = 128;
            public double LUTM = 1;
            /*</ImageAdjPara>*/
            /*</ProjectVisionMarkPara>*/


        }

        #endregion

        #region 資料處理(Func)

        /////////////////////////////////////////////////////////////////
        ///                     得到專案資料                          ///
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Project's Set
        /// </summary>
        /// <param name="PrjSet"></param>
        /// <returns></returns>
        public RetErr GetPrjDB(out stru_PrjSet PrjSet)
        {
            RetErr ret = new RetErr();
            stru_PrjSet _set = new stru_PrjSet();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                PrjSet = _set;
                return ret;
            }

            try
            {
                _set = l_struPrjAllSet.l_stru_PrjSet;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetPrjSetData,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetPrjSetData;
                ret.Meg = "Project Get Project Set data";
                PrjSet = _set;
                return ret;
            }
            PrjSet = _set;
            return ret;
        }

        /// <summary>
        /// Get Project's Layer Data
        /// </summary>
        /// <param name="index"> 0 ~ n </param>
        /// <param name="PrjLayerData"></param>
        /// <returns></returns>
        public RetErr GetPrjDB(int index, out stru_PrjLayerSet PrjLayerData)
        {
            RetErr ret = new RetErr();
            stru_PrjLayerSet _layer = new stru_PrjLayerSet();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                PrjLayerData = _layer;
                return ret;
            }

            try
            {
                _layer = l_struPrjAllSet.l_LayerSetData[index];
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetPrjLayerData,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetPrjLayerData;
                ret.Meg = "Project Get Project Layer data";
                PrjLayerData = _layer;
                return ret;
            }
            PrjLayerData = _layer;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allData"></param>
        /// <returns></returns>
        public RetErr GetHullPrjDB( out stru_PrjAllSetting allData)
        {
            RetErr ret = new RetErr();
            stru_PrjAllSetting data = new stru_PrjAllSetting();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                allData = data;
                return ret;
            }

            try
            {
                data = l_struPrjAllSet;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetPrjHullData,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetPrjHullData;
                ret.Meg = "Project Get all data";
                allData = data;
                return ret;
            }
            allData = data;
            return ret;
        }

        /////////////////////////////////////////////////////////////////
        /// 加入 專案設定Para 到 一個傳案的設定 "stru_PrjAllSetting") ///
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update "ProjectSet" to local register
        /// </summary>
        /// <param name="PrjSet"></param>
        /// <returns></returns>
        public RetErr Add(stru_PrjSet PrjSet)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                return ret;
            }

            try
            {
                l_struPrjAllSet.l_stru_PrjSet = PrjSet;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(   Num._PrjSetAdd,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._PrjSetAdd;
                ret.Meg = "Project Add Project Set data";
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// Add "ProjectLayerSet" to local register
        /// </summary>
        /// <param name="PrjSet"></param>
        /// <returns></returns>
        public RetErr Add(stru_PrjLayerSet layer)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                return ret;
            }

            try
            {
                l_struPrjAllSet.l_LayerSetData.Add(layer);
                l_struPrjAllSet.LayerCunt = l_struPrjAllSet.l_LayerSetData.Count;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._PrjlayerAdd,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._PrjlayerAdd;
                ret.Meg = "Project Add Project Layer data";
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// Update "List<>ProjectLayerSet" to local register
        /// </summary>
        /// <param name="PrjSet"></param>
        /// <returns></returns>
        public RetErr Add(List< stru_PrjLayerSet> layer)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                return ret;
            }

            try
            {
                l_struPrjAllSet.l_LayerSetData = layer;
                l_struPrjAllSet.LayerCunt = l_struPrjAllSet.l_LayerSetData.Count;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._PrjlayerAdd,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._PrjlayerAdd;
                ret.Meg = "Project use Project Layer data";
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"> 0 ~ n </param>
        /// <param name="PrjLayerData"></param>
        /// <returns></returns>
        public RetErr UpdatePrjDB(int index, stru_PrjLayerSet PrjLayerData)
        {
            RetErr ret = new RetErr();
            stru_PrjLayerSet _layer = new stru_PrjLayerSet();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                PrjLayerData = _layer;
                return ret;
            }

            try
            {
                l_struPrjAllSet.l_LayerSetData[index] = PrjLayerData;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetPrjLayerData,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetPrjLayerData;
                ret.Meg = "Project Get Project Layer data";
                PrjLayerData = _layer;
                return ret;
            }
            PrjLayerData = _layer;
            return ret;
        }


        /////////////////////////////////////////////////////////////////
        ///                 新增 專案 到 專案Box                      ///
        /////////////////////////////////////////////////////////////////

        public RetErr Add()
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                return ret;
            }

            try
            {
                // Layer Count
                l_struPrjAllSet.LayerCunt = l_struPrjAllSet.l_LayerSetData.Count;
                
                // Projet Box Add Project Data
                l_MulitPrjPara.Add(l_struPrjAllSet);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._PrjBoxAdd,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._PrjBoxAdd;
                ret.Meg = "Project Box Add Project data";
                return ret;
            }
            return ret;
        }


        /////////////////////////////////////////////////////////////////
        ///                     初始化 專案                           ///
        /////////////////////////////////////////////////////////////////

        public RetErr init()
        {
            RetErr ret = new RetErr();
            try
            {
                // Set Flag
                isInit = true;

                // 初始設定
                l_struPrjAllSet = new stru_PrjAllSetting();
                l_struPrjAllSet.l_LayerSetData = new List<stru_PrjLayerSet>();
                l_struPrjAllSet.l_stru_PrjSet = new stru_PrjSet();
                l_struPrjAllSet.l_stru_PrjSet.VisionMark = new List<stru_VisionMark>();
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._Initial,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._Initial;
                ret.Meg = "Project class initial";
                return ret;
            }
            return ret;
        }

        /////////////////////////////////////////////////////////////////
        ///                     重製 專案 Para                        ///
        /////////////////////////////////////////////////////////////////

        public RetErr Reset()
        {
            RetErr ret = new RetErr();

            try
            {
                init();
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._ResetPara,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._ResetPara;
                ret.Meg = "Project Reset Para";
                return ret;
            }
            return ret;
        }

        /////////////////////////////////////////////////////////////////
        ///                         存檔                              ///
        /////////////////////////////////////////////////////////////////

        public RetErr SaveFile(string FileName)
        {
            RetErr ret = new RetErr();
            string txt = "";

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                return ret;
            }

            try
            {
                // 組合
                CombineData_Prj(l_struPrjAllSet, ref txt);

                // write to file
                string path = Application.StartupPath + "\\Project\\" +
                                FileName +
                                ".prj";
                StreamWriter strr = new StreamWriter(path, false, Encoding.Default);
                strr.Write(txt);
                strr.Close();
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._SaveFile,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._SaveFile;
                ret.Meg = "Project Save File";
                return ret;
            }
            return ret;
        }

        private void CombineData_Prj(stru_PrjAllSetting PrjAllSet, ref string data)
        {
            // Vision MArk str
            string Vision = "";
            var tt = PrjAllSet.l_stru_PrjSet.VisionMark;
            if (tt != null)
            {
                int cunt = PrjAllSet.l_stru_PrjSet.VisionMark.Count;
                for (int i = 0; i < cunt; i++)
                {
                    Vision = Vision + "<ProjectVisionMarkPara>" + "\r\n" +
                                        "[PosMarkX]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].PosMarkX.ToString() + "\r\n" +
                                        "[PosMarkY]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].PosMarkY.ToString() + "\r\n" +
                                        "<PatternMatchingPara>" + "\r\n" +
                                        "<MatchingPara>" + "\r\n" +
                                        "[Score]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Score.ToString() + "\r\n" +
                                        "</MatchingPara>" + "\r\n" +
                                        "[CurveFittingMethod]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingMethod.ToString() + "\r\n" +
                                        "<RectFittingPara>" + "\r\n" +
                                        "[Width]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Width_Rect.ToString() + "\r\n" +
                                        "[Height]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Height_Rect.ToString() + "\r\n" +
                                        "[Angle]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Angle_Rect + "\r\n" +
                                        "<FittingCommonPara>" + "\r\n" +
                                        "[CenterX]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterX_Rect.ToString() + "\r\n" +
                                        "[CenterY]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterY_Rect.ToString() + "\r\n" +
                                        "[Tolerance]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Tolerance_Rect.ToString() + "\r\n" +
                                        "[CurveFittingSearchDir]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchDir_Rect.ToString() + "\r\n" +
                                        "[CurveFittingSearchTrans]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchTrans_Rect.ToString() + "\r\n" +
                                        "[Threshould]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Threshould_Rect.ToString() + "\r\n" +
                                        "</FittingCommonPara>" + "\r\n" +
                                        "</RectFittingPara>" + "\r\n" +
                                        "<CircleFittingPara>" + "\r\n" +
                                        "[Diameter]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Diameter_Circle.ToString() + "\r\n" +
                                        "<FittingCommonPara>" + "\r\n" +
                                        "[CenterX]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterX_Circle.ToString() + "\r\n" +
                                        "[CenterY]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterY_Circle.ToString() + "\r\n" +
                                        "[Tolerance]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Tolerance_Circle.ToString() + "\r\n" +
                                        "[CurveFittingSearchDir]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchDir_Circle + "\r\n" +
                                        "[CurveFittingSearchTrans]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchTrans_Circle.ToString() + "\r\n" +
                                        "[Threshould]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Threshould_Circle.ToString() + "\r\n" +
                                        "</FittingCommonPara>" + "\r\n" +
                                        "</CircleFittingPara>" + "\r\n" +
                                        "<CrossFittingPara>" + "\r\n" +
                                        "[Size]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Size_Cross.ToString() + "\r\n" +
                                        "[Thickness]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Thickness_Cross.ToString() + "\r\n" +
                                        "[Angle]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Angle_Cross.ToString() + "\r\n" +
                                        "<FittingCommonPara>" + "\r\n" +
                                        "[CenterX]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterX_Cross.ToString() + "\r\n" +
                                        "[CenterY]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterY_Cross.ToString() + "\r\n" +
                                        "[Tolerance]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Tolerance_Cross.ToString() + "\r\n" +
                                        "[CurveFittingSearchDir]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchDir_Cross.ToString() + "\r\n" +
                                        "[CurveFittingSearchTrans]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchTrans_Cross.ToString() + "\r\n" +
                                        "[Threshould]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Threshould_Cross.ToString() + "\r\n" +
                                        "</FittingCommonPara>" + "\r\n" +
                                        "</CrossFittingPara>" + "\r\n" +
                                        "<CornerFittingPara>" + "\r\n" +
                                        "[Size]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Size_Corner.ToString() + "\r\n" +
                                        "[Angle]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Angle_Corner.ToString() + "\r\n" +
                                        "[IncludedAngle]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].IncludedAngle_Corner.ToString() + "\r\n" +
                                        "<FittingCommonPara>" + "\r\n" +
                                        "[CenterX]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterX_Corner.ToString() + "\r\n" +
                                        "[CenterY]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CenterY_Corner.ToString() + "\r\n" +
                                        "[Tolerance]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Tolerance_Corner.ToString() + "\r\n" +
                                        "[CurveFittingSearchDir]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchDir_Corner.ToString() + "\r\n" +
                                        "[CurveFittingSearchTrans]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].CurveFittingSearchTrans_Corner.ToString() + "\r\n" +
                                        "[Threshould]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Threshould_Corner.ToString() + "\r\n" +
                                        "</FittingCommonPara>" + "\r\n" +
                                        "</CornerFittingPara>" + "\r\n" +
                                        "</PatternMatchingPara>" + "\r\n" +
                                        "<ImageAdjPara>" + "\r\n" +
                                        "[Gain]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].Gain.ToString() + "\r\n" +
                                        "[LUTCenterX]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].LUTCenterX.ToString() + "\r\n" +
                                        "[LUTM]" + PrjAllSet.l_stru_PrjSet.VisionMark[i].LUTM.ToString() + "\r\n" +
                                        "</ImageAdjPara>" + "\r\n" +
                                        "</ProjectVisionMarkPara>" + "\r\n";
                }
            }

            if (PrjAllSet.l_stru_PrjSet.NeedChkMark != 1) Vision = "";
            string Setting = "<Project>\r\n" +
                                "[DrawName]" + PrjAllSet.l_stru_PrjSet.DrawName + "\r\n" +
                                "[PosChkCCDFocalPos]" + PrjAllSet.l_stru_PrjSet.PosCCDFocalPos.ToString("f2") + "\r\n" +
                                //"[CuttedPieces]" + PrjAllSet.l_stru_PrjSet.CuttingPiece.ToString() + "\r\n" +
                                "[NeedChkMark]" + PrjAllSet.l_stru_PrjSet.NeedChkMark.ToString() + "\r\n" +
                                //"[AllMarkUseMark1Data]" + PrjAllSet.l_stru_PrjSet.AllMarkUseMark1Data.ToString() + "\r\n" +
                                "[Use2DBarCode]" + PrjAllSet.l_stru_PrjSet.Use2DBarCode.ToString() + "\r\n" +
                                "[isSKYW]" + PrjAllSet.l_stru_PrjSet.isSKYW.ToString() + "\r\n" +
                                //"[EndPtVel]" + PrjAllSet.l_stru_PrjSet.EndPtVel.ToString() + "\r\n" +
                                "[Accel]" + PrjAllSet.l_stru_PrjSet.SKYW_Accel.ToString() + "\r\n" +
                                "[LimitAngle]" + PrjAllSet.l_stru_PrjSet.SKYW_LimitAngle.ToString() + "\r\n" +
                                //"[MaxRadErr]" + PrjAllSet.l_stru_PrjSet.MaxRadErr.ToString() + "\r\n" +
                                //"[MaxAccRat]" + PrjAllSet.l_stru_PrjSet.MaxAccRat.ToString() + "\r\n" +
                                Vision +
                                "</Project>" + "\r\n";
            string Layers = "";
            for (int i = 0; i < PrjAllSet.LayerCunt; i++)
            {
                Layers = Layers + "<LayerData>" + "\r\n" +
                                    "[LayerName]" + PrjAllSet.l_LayerSetData[i].LayerName + "\r\n" +
                                    "[Show]" + PrjAllSet.l_LayerSetData[i].Show.ToString() + "\r\n" +
                                    "[Mark]" + PrjAllSet.l_LayerSetData[i].Mark.ToString() + "\r\n" +
                                    "[Thicksness]" + PrjAllSet.l_LayerSetData[i].Thicksness.ToString() + "\r\n" +
                                    "[LayerMarkTimes]" + PrjAllSet.l_LayerSetData[i].LayerMarkTimes.ToString() + "\r\n" +
                                    "[ReDownHeigh]" + PrjAllSet.l_LayerSetData[i].ReDownHeigh.ToString() + "\r\n" +
                                    "[markDir]" + PrjAllSet.l_LayerSetData[i].markDir.ToString() + "\r\n" +
                                    "[isSplit]" + PrjAllSet.l_LayerSetData[i].isSplit.ToString() + "\r\n" +
                                    "[DivWidth]" + PrjAllSet.l_LayerSetData[i].DivWidth.ToString() + "\r\n" +
                                    "[DivHeigh]" + PrjAllSet.l_LayerSetData[i].DivHeigh.ToString() + "\r\n" +
                                    //"[MaxRatLine]" + PrjAllSet.l_LayerSetData[i].MaxRatLine.ToString() + "\r\n" +
                                    "[Power]" + PrjAllSet.l_LayerSetData[i].Power.ToString() + "\r\n" +
                                    "[Freq]" + PrjAllSet.l_LayerSetData[i].Freq.ToString() + "\r\n" +
                                    "[PulseWidth]" + PrjAllSet.l_LayerSetData[i].PulseWidth.ToString() + "\r\n" +
                                    "[ObjMarkTimes]" + PrjAllSet.l_LayerSetData[i].ObjMarkTimes.ToString() + "\r\n" +
                                    "[MarkSpeed]" + PrjAllSet.l_LayerSetData[i].MarkSpeed.ToString() + "\r\n" +
                                    "[JumpSpeed]" + PrjAllSet.l_LayerSetData[i].JumpSpeed.ToString() + "\r\n" +
                                    "[JumpDelay]" + PrjAllSet.l_LayerSetData[i].JumpDelay.ToString() + "\r\n" +
                                    "[SpotDelay]" + PrjAllSet.l_LayerSetData[i].SpotDelay.ToString() + "\r\n" +
                                    "[OnDelay]" + PrjAllSet.l_LayerSetData[i].OnDelay.ToString() + "\r\n" +
                                    "[MiddleDelay]" + PrjAllSet.l_LayerSetData[i].MiddleDelay.ToString() + "\r\n" +
                                    "[EndDelay]" + PrjAllSet.l_LayerSetData[i].EndDelay.ToString() + "\r\n" +
                                    "[Mode]" + PrjAllSet.l_LayerSetData[i].LaserMode.ToString() + "\r\n" +
                                    "[Bias]" + PrjAllSet.l_LayerSetData[i].LaserBias.ToString() + "\r\n" +
                                    "[Epulse]" + PrjAllSet.l_LayerSetData[i].LaserEpulse.ToString() + "\r\n" +
                                    "[isFill]" + PrjAllSet.l_LayerSetData[i].isFill.ToString() + "\r\n" +
                                    "[isFillTwoWay]" + PrjAllSet.l_LayerSetData[i].isFillTwoWay.ToString() + "\r\n" +
                                    //"[BalanceFill]" + PrjAllSet.l_LayerSetData[i].BalanceFill.ToString() + "\r\n" +
                                    //"[InverFill]" + PrjAllSet.l_LayerSetData[i].InverFill.ToString() + "\r\n" +
                                    //"[FramMark]" + PrjAllSet.l_LayerSetData[i].FramMark.ToString() + "\r\n" +
                                    //"[FramFirst]" + PrjAllSet.l_LayerSetData[i].FramFirst.ToString() + "\r\n" +
                                    "[FillBorder]" + PrjAllSet.l_LayerSetData[i].FillBorder.ToString() + "\r\n" +
                                    "[FillPitch]" + PrjAllSet.l_LayerSetData[i].FillPitch.ToString() + "\r\n" +
                                    "[FillTimes]" + PrjAllSet.l_LayerSetData[i].FillTimes.ToString() + "\r\n" +
                                    "[FillAngleStart]" + PrjAllSet.l_LayerSetData[i].FillAngleStart.ToString() + "\r\n" +
                                    "[FillAngleStep]" + PrjAllSet.l_LayerSetData[i].FillAngleStep.ToString() + "\r\n" +
                                    "[Overlapsize]" + PrjAllSet.l_LayerSetData[i].Overlapsize.ToString() + "\r\n" +
                                    "[isWobble]" + PrjAllSet.l_LayerSetData[i].isWobble.ToString() + "\r\n" +
                                    "[WobbleThick]" + PrjAllSet.l_LayerSetData[i].WobbleThick.ToString() + "\r\n" +
                                    "[WobbleFreq]" + PrjAllSet.l_LayerSetData[i].WobbleFreq.ToString() + "\r\n" +
                                    "</LayerData>" + "\r\n";
            }
            // Combine
            data = Setting + Layers;
        }

        /////////////////////////////////////////////////////////////////
        ///                         存檔                              ///
        /////////////////////////////////////////////////////////////////

        public RetErr GetLayerCuount(out int count)
        {
            RetErr ret = new RetErr();
            int _count = 0;

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not init, please initial ";
                count = _count;
                return ret;
            }

            try
            {
                _count = l_struPrjAllSet.LayerCunt;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetLayerCount,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetLayerCount;
                ret.Meg = "Project Get Layer Count";
                count = _count;
                return ret;
            }
            count = _count;
            return ret;
        }

        #endregion
    }


    /// <summary>
    /// 外面只看得董這個
    /// </summary>
    public class Method_Prj
    {
        /// 1. 初始化
        /// 
        /// 

        ///////////////////////////////////////////////////////////
        ///                     Para                            ///
        ///////////////////////////////////////////////////////////
        
        #region Para
        
        c_PrjSettingBuild Prj;
        Info DB_info;
        /// <summary>
        /// 1 ~ n
        /// </summary>
        public int CurLIndex = 1;    

        #endregion

        ///////////////////////////////////////////////////////////
        ///                     SubRouting                      ///
        ///////////////////////////////////////////////////////////

        #region SubRouting


        #endregion

        ///////////////////////////////////////////////////////////
        ///                     Method                          ///
        ///////////////////////////////////////////////////////////

        #region Method

        public void init()
        {
            Prj = new c_PrjSettingBuild();
            DB_info = new Info();

            Prj.init();
        }

        public string GetPath()
        {
            return DB_info.CurFilePath;
        }

        public int GetLayerCunt()
        {
            RetErr ret = Prj.GetLayerCuount(out int cunt);
            if (!ret.flag)
                return -1;
            return cunt;
        }

        public Info GetInfo()
        {
            return DB_info;
        }

        /// <summary>
        /// Get Project's Set
        /// </summary>
        /// <param name="PrjSet"></param>
        /// <returns></returns>
        public RetErr GetPrjDB(out c_PrjSettingBuild.stru_PrjSet PrjSet)
        {
            RetErr ret = Prj.GetPrjDB(out PrjSet);
            return ret;
        }

        public RetErr UpdatePrjDB(List<c_PrjSettingBuild.stru_PrjLayerSet> layerDB)
        {
            RetErr ret = Prj.Add(layerDB);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"> 0 ~ n </param>
        /// <param name="layerDB"></param>
        /// <returns></returns>
        public RetErr UpdatePrjDB(int index, c_PrjSettingBuild.stru_PrjLayerSet layerDB)
        {
            RetErr ret = Prj.UpdatePrjDB(index, layerDB);
            return ret;
        }
        /// <summary>
        /// update without layername
        /// </summary>
        /// <param name="index"> 0 ~ n </param>
        /// <param name="layerDB"></param>
        /// <returns></returns>
        public RetErr UpdatePrjDB_withoutName(int index, c_PrjSettingBuild.stru_PrjLayerSet layerDB)
        {
            Prj.GetPrjDB(index, out c_PrjSettingBuild.stru_PrjLayerSet data);
            string name = data.LayerName;
            layerDB.LayerName = name;
            RetErr ret = Prj.UpdatePrjDB(index, layerDB);
            return ret;
        }

        public RetErr UpdatePrjDB(c_PrjSettingBuild.stru_PrjSet set)
        {
            RetErr ret = Prj.Add(set);
            return ret;
        }

        /// <summary>
        /// Get Project's Layer Data
        /// </summary>
        /// <param name="index"> 0 ~ n </param>
        /// <param name="PrjLayerData"></param>
        /// <returns></returns>
        public RetErr GetPrjDB(int index, out c_PrjSettingBuild.stru_PrjLayerSet PrjLayerData)
        {
            RetErr ret = Prj.GetPrjDB(index, out PrjLayerData);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allData"></param>
        /// <returns></returns>
        public RetErr GetHullPrjDB(out c_PrjSettingBuild.stru_PrjAllSetting allData)
        {
            RetErr ret = Prj.GetHullPrjDB(out allData);
            return ret;
        }

        public RetErr GetHullPrjDB(string path, out c_PrjSettingBuild.stru_PrjAllSetting allData)
        {
            RetErr ret = new RetErr();
            c_PrjSettingBuild.stru_PrjAllSetting tamp = new c_PrjSettingBuild.stru_PrjAllSetting();
            allData = tamp; 
            try
            {
                #region ReadFileData

                StreamReader strr = new StreamReader(path, Encoding.Default);
                string ori = strr.ReadToEnd();
                strr.Close();

                #endregion

                #region 資料解析

                c_PrjSettingBuild.stru_PrjAllSetting PrjAll = new c_PrjSettingBuild.stru_PrjAllSetting();
                PrjAll.init();
                string[] sec = ori.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                PrjAll.l_stru_PrjSet = JsonConvert.DeserializeObject<c_PrjSettingBuild.stru_PrjSet>(sec[0]);
                PrjAll.l_LayerSetData = JsonConvert.DeserializeObject<List<c_PrjSettingBuild.stru_PrjLayerSet>>(sec[1]);
                PrjAll.LayerCunt = PrjAll.l_LayerSetData.Count;

                #endregion

                #region 加入DB

                ret = Prj.Add(PrjAll.l_stru_PrjSet);
                if (!ret.flag) throw new Exception(ret.Meg);

                ret = Prj.Add(PrjAll.l_LayerSetData);
                if (!ret.flag) throw new Exception(ret.Meg);

                #endregion
            }
            catch(Exception ee)
            {
                ret.flag = false;
                ret.Meg = " Get File DB Fail";
                return ret;
            }

            ret = Prj.GetHullPrjDB(out allData);
            return ret;
        }

        public RetErr Add()
        {
            RetErr ret = new RetErr();
            try
            {
                // check
                if(DB_info.IsFileOpen && !DB_info.IsFileSave)
                {
                    DialogResult Result = MessageBox.Show("未儲存 是否要儲存?", "", MessageBoxButtons.OKCancel);
                    if (Result == DialogResult.OK)
                    {
                        AnoSave();
                        return ret;
                    }
                }

                /// initial path
                DB_info.CurFilePath = Application.StartupPath + "\\temp.prj";

                /// Set flag
                DB_info.IsFileOpen = true;
                DB_info.IsFileSave = false;
                DB_info.IsHaveDrawing = false;
                DB_info.IsNewPrj = true;

            }
            catch(Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AddPrj,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AddPrj;
                ret.Meg = "Project Form Add new Project " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr Open()
        {
            RetErr ret = new RetErr();
            try
            {
                #region check
                
                if (DB_info.IsFileOpen && !DB_info.IsFileSave)
                {
                    DialogResult Result = MessageBox.Show("未儲存 是否要儲存?", "", MessageBoxButtons.OKCancel);
                    if (Result == DialogResult.OK)
                    {
                        AnoSave();
                        return ret;
                    }
                }

                #endregion

                #region OpenFile
                
                OpenFileDialog OpenDia = new OpenFileDialog();
                OpenDia.Filter = ".prj|*.prj";
                OpenDia.InitialDirectory = Application.StartupPath + "\\Project";

                if (OpenDia.ShowDialog() != DialogResult.OK) return ret;

                DB_info.CurFilePath = OpenDia.FileName;

                #endregion

                #region ReadFileData

                StreamReader strr = new StreamReader(OpenDia.FileName, Encoding.Default);
                string ori = strr.ReadToEnd();
                strr.Close();

                #endregion

                #region 資料解析

                c_PrjSettingBuild.stru_PrjAllSetting PrjAll = new c_PrjSettingBuild.stru_PrjAllSetting();
                PrjAll.init();
                string[] sec = ori.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                PrjAll.l_stru_PrjSet = JsonConvert.DeserializeObject<c_PrjSettingBuild.stru_PrjSet>(sec[0]);
                PrjAll.l_LayerSetData = JsonConvert.DeserializeObject<List<c_PrjSettingBuild.stru_PrjLayerSet>>(sec[1]);
                PrjAll.LayerCunt = PrjAll.l_LayerSetData.Count;

                #endregion

                #region 加入DB

                ret = Prj.Add(PrjAll.l_stru_PrjSet);
                if (!ret.flag) throw new Exception(ret.Meg);

                ret = Prj.Add(PrjAll.l_LayerSetData);
                if (!ret.flag) throw new Exception(ret.Meg);

                #endregion

               

                #region Set flag

                DB_info.IsFileOpen = true;
                DB_info.IsFileSave = true;
                DB_info.IsNewPrj = false ;
                //if(err.flag) DB_info.IsHaveDrawing = false;

                #endregion

            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._OpenPrj,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._OpenPrj;
                ret.Meg = "Project Form Open Project " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr Save()
        {
            RetErr ret = new RetErr();
            try
            {
                #region check

                if(! (DB_info.IsFileOpen && DB_info.IsFileSave))
                {
                    AnoSave();
                    return ret;
                }


                #endregion

                #region 組合文字

                c_PrjSettingBuild.stru_PrjAllSetting AllData;
                Prj.GetHullPrjDB(out AllData);

                // draw name
                AllData.l_stru_PrjSet.DrawName = DB_info.CurFilePath.Replace(".prj", ".ezm");


                string set = "";
                set += JsonConvert.SerializeObject(AllData.l_stru_PrjSet) + "\r\n";
                set += JsonConvert.SerializeObject(AllData.l_LayerSetData);


                #endregion

                #region 寫入文檔

                StreamWriter strr = new StreamWriter(DB_info.CurFilePath, false, Encoding.Default);
                strr.Write(set);
                strr.Close();

                #endregion

                #region Set Flag

                DB_info.IsFileSave = true;
                
                #endregion
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._SavePrj,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._SavePrj;
                ret.Meg = "Project Form Save Project " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr AnoSave()
        {
            RetErr ret = new RetErr();
            try
            {
                #region Define

                SaveFileDialog SaveDia = new SaveFileDialog();
                SaveDia.Filter = ".prj|*.prj";
                SaveDia.InitialDirectory = Application.StartupPath + "\\Prj";


                c_PrjSettingBuild.stru_PrjAllSetting AllData;

                #endregion

                #region Get DB

                Prj.GetHullPrjDB(out AllData);

                #endregion


                #region 開啟存檔位置

                if (SaveDia.ShowDialog() != DialogResult.OK) return ret;

                c_PrjSettingBuild.stru_PrjSet tampSet = AllData.l_stru_PrjSet;
                tampSet.DrawName = SaveDia.FileName.Replace(".prj", ".ezm");
                AllData.l_stru_PrjSet = tampSet;

                #endregion

                #region 組合文字


                // draw name
                AllData.l_stru_PrjSet.DrawName = DB_info.CurFilePath.Replace(".prj", ".ezm"); ;

                string set = "";
                set += JsonConvert.SerializeObject(AllData.l_stru_PrjSet) + "\r\n";
                set += JsonConvert.SerializeObject(AllData.l_LayerSetData);


                #endregion

                #region 寫入檔案

                StreamWriter strr = new StreamWriter(SaveDia.FileName, false, Encoding.Default);
                strr.Write(set);
                strr.Close();

                #endregion

                #region 更新資料

                DB_info.CurFilePath = SaveDia.FileName;

                #endregion

                #region 更新旗標

                DB_info.IsFileSave = true;

                #endregion

            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AnoSavePrj,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AnoSavePrj;
                ret.Meg = "Project Form AnoSave Project " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr Close()
        {
            RetErr ret = new RetErr();
            try
            {
                #region DB Reset

                Prj.Reset();

                #endregion

                #region Flag Reset

                DB_info = new Info();

                #endregion

                #region window defaul (non)
                


                #endregion

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
                ret.Meg = "Project Form Close Project " + ee.Message;
                return ret;
            }
            return ret;
        }

       


        #endregion


    }
}

namespace BackLight.GUI.Project.ErrCode
{
    class Num   // -801 ~ -900
    {
        public static int _PrjSetAdd =          -801;
        public static int _PrjlayerAdd =        -802;
        public static int _PrjBoxAdd =          -803;
        public static int _Initial =            -804;
        public static int _ResetPara =          -805;
        public static int _GetPrjSetData =      -806;
        public static int _GetPrjLayerData =    -807;
        public static int _GetPrjHullData =     -808;
        public static int _SaveFile =           -809;
        public static int _GetLayerCount =      -810;
        public static int _AddPrj =             -811;
        public static int _OpenPrj =            -812;
        public static int _SavePrj =            -813;
        public static int _AnoSavePrj =         -814;
        public static int _Close =              -815;
        public static int _UpdatePrjDB =        -816;
    }

    public class Info
    {
        // flag
        public Boolean IsFileOpen = false;
        public Boolean IsFileSave = false;
        public Boolean IsHaveDrawing = false;
        public Boolean IsNewPrj = false;

        public void Reset()
        {
            IsFileOpen = false;
            IsFileSave = false;
        }

        // Register
        public string CurFilePath = "";
    }

    public class RetErr
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
            string pat = Application.StartupPath + "\\Log\\bas_Project.log";
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


