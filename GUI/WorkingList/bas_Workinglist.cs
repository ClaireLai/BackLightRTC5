#define Log
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using BackLight.GUI.Project;
using BackLight.GUI.WorkingList.Errcode;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BackLight.GUI.Project;

namespace BackLight.GUI.WorkingList
{
    
    class bas_Workinglist
    {
    }

    public class c_WLSettingBuild
    {
        private Boolean isInit = false;

        /// <summary>
        /// Working List Box 多個工單
        /// </summary>
        private List<stru_WLAllSetting> l_MulitWLPara;

        /// <summary>
        /// Hull WorkingList
        /// </summary>
        private stru_WLAllSetting l_struWLAllSet;


        #region 資料結構(因子)

        // 一個工單所有資料存放位置
        public struct stru_WLAllSetting
        {
            /// <summary>
            /// 對應相對應加工 Prj index 
            /// </summary>
            public UInt16 CurrPrjIndex;          /// 每次結束一個專案都要++, 由 0 -開始
            // Working List Set Data
            public stru_WL_GlobalSet l_stru_WLSet;
            // Working List Layer Data
            public List<stru_WL_LayerSet> l_WLLayerSetData;
            // Project Data
            public List<c_PrjSettingBuild.stru_PrjAllSetting> l_WLLayersPrjData;
        }

        

        // 一個專案所有對單一 LAYER 所做的設定
        public struct stru_WL_LayerSet
        {
            public String FileName;
            public double WorkingH_PosX;
            public double WorkingH_PosY;
            public double Mark1_PosX;
            public double Mark1_PosY;
            public double Mark2_PosX;
            public double Mark2_PosY;
            public double Mark;
            public double PosOffset_X;
            public double PosOffset_Y;
            public double DegreeOffset;
            public double ScaleOffset_X;
            public double ScaleOffset_Y;
            public double isPosCompensati;
        }

        // 一個專案的設定
        public struct stru_WL_GlobalSet
        {
            public double CurrPrjIndex;
            public double CurrPrj_Chip_Index;
            public double CurrPrj_Layer_Index;
            public double CurrPrj_LayerDivision_index;
            public double CurrLayer_MarkingTimes;
            public double HoldOn_DiviStart;
            public double HoldOn_LayerStart;
            public double HoldOn_PrjStart;
            public double UseBatchCtl;
            public double BatcjAmount;
            public double ExedBatchAmount;
        }

        #endregion

        #region 資料處理(Func)

        /////////////////////////////////////////////////////////////////
        ///                         初始化                            ///
        /////////////////////////////////////////////////////////////////

        public RetErr Init()
        {
            RetErr ret = new RetErr();
            try
            {
                // set flag
                isInit = true;

                l_MulitWLPara = new List<stru_WLAllSetting>();
                l_struWLAllSet = new stru_WLAllSetting();
                l_struWLAllSet.l_stru_WLSet = new stru_WL_GlobalSet();
                l_struWLAllSet.l_WLLayerSetData = new List<stru_WL_LayerSet>();
                l_struWLAllSet.l_WLLayersPrjData = new List<c_PrjSettingBuild.stru_PrjAllSetting>();

            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._initial,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._initial;
                ret.Meg = "WorkingList initial ";
                return ret;
            }
            return ret;
        }

        /////////////////////////////////////////////////////////////////
        ///                     加入到暫存器                          ///
        /////////////////////////////////////////////////////////////////

        public RetErr Add(stru_WL_GlobalSet WL_G_Set)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }

            try
            {
                l_struWLAllSet.l_stru_WLSet = WL_G_Set;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AddGlobalSet,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AddGlobalSet;
                ret.Meg = "WorkingList Add Global Set ";
                return ret;
            }
            return ret;
        }
        
        public RetErr Add(stru_WL_LayerSet WL_Layer_Set)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                l_struWLAllSet.l_WLLayerSetData.Add(WL_Layer_Set);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AddWLLayerSet,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AddWLLayerSet;
                ret.Meg = "WorkingList Add WL Layer Set ";
                return ret;
            }
            return ret;
        }

        public RetErr Add(c_PrjSettingBuild.stru_PrjAllSetting Prj_All_Set)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                l_struWLAllSet.l_WLLayersPrjData.Add(Prj_All_Set);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AddPrjLayerSet,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AddPrjLayerSet;
                ret.Meg = "WorkingList Add Prj Layer Set ";
                return ret;
            }
            return ret;
        }

        public RetErr UpdateDB(List<stru_WL_LayerSet> DB)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                l_struWLAllSet.l_WLLayerSetData = DB;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._UpdateDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._UpdateDB;
                ret.Meg = "WorkingList update WL Layer Set ";
                return ret;
            }
            return ret;
        }

        public RetErr UpdateDB(c_WLSettingBuild.stru_WLAllSetting alldata)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                l_struWLAllSet = alldata;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._UpdateDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._UpdateDB;
                ret.Meg = "WorkingList update WL Layer Set ";
                return ret;
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"> 0 ~ n</param>
        /// <param name="DB"></param>
        /// <returns></returns>
        public RetErr UpdateDB(int index, stru_WL_LayerSet DB)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                l_struWLAllSet.l_WLLayerSetData[index] = DB;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._UpdateDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._UpdateDB;
                ret.Meg = "WorkingList update index WL Layer Set ";
                return ret;
            }
            return ret;
        }
        public RetErr UpdateDB(int index, c_PrjSettingBuild.stru_PrjAllSetting DB)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                l_struWLAllSet.l_WLLayersPrjData[index] = DB;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._UpdateDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._UpdateDB;
                ret.Meg = "WorkingList update index Prj Layer Set ";
                return ret;
            }
            return ret;
        }

        public RetErr Index_Pluse()
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }

            try
            {
                l_struWLAllSet.CurrPrjIndex++;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._CurIndexPluse,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._CurIndexPluse;
                ret.Meg = "WorkingList Current index +1 ";
                return ret;
            }
            return ret;
        }

        public RetErr SetCurIndex(UInt16 index)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }

            try
            {
                l_struWLAllSet.CurrPrjIndex = index;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._SetCurIndex,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._SetCurIndex;
                ret.Meg = "WorkingList Set Current index  ";
                return ret;
            }
            return ret;
        }

       

        /////////////////////////////////////////////////////////////////
        ///                     讀取暫存器資料                          ///
        /////////////////////////////////////////////////////////////////

        public RetErr GetDB(out stru_WL_GlobalSet WL_G_Set)
        {
            RetErr ret = new RetErr();
            stru_WL_GlobalSet _tamp = new stru_WL_GlobalSet();
            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                WL_G_Set = _tamp;
                return ret;
            }

            try
            {
                _tamp = l_struWLAllSet.l_stru_WLSet ;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetWLSet,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetWLSet;
                ret.Meg = "WorkingList Get Global Set ";
                WL_G_Set = _tamp;
                return ret;
            }
            WL_G_Set = _tamp;
            return ret;
        }

        public RetErr GetDB(int index, out stru_WL_LayerSet WL_Layer_Set)
        {
            RetErr ret = new RetErr();
            stru_WL_LayerSet _tamp = new stru_WL_LayerSet();
            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                WL_Layer_Set = _tamp;
                return ret;
            }
            try
            {
                _tamp = l_struWLAllSet.l_WLLayerSetData[index];
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetDB;
                ret.Meg = "WorkingList Get WL Layer Set ";
                WL_Layer_Set = _tamp;
                return ret;
            }
            WL_Layer_Set = _tamp;
            return ret;
        }

        public RetErr GetDB(int index, out c_PrjSettingBuild.stru_PrjAllSetting Prj_All_Set)
        {
            RetErr ret = new RetErr();
            c_PrjSettingBuild.stru_PrjAllSetting _tamp = new c_PrjSettingBuild.stru_PrjAllSetting();
            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                Prj_All_Set = _tamp;
                return ret;
            }
            try
            {
                _tamp = l_struWLAllSet.l_WLLayersPrjData[index];
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetPrjLayer,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetPrjLayer;
                ret.Meg = "WorkingList Get Prj Layer Set ";
                Prj_All_Set = _tamp;
                return ret;
            }
            Prj_All_Set = _tamp;
            return ret;
        }


        public RetErr GetCurIndex(out int CurPrjindex)
        {
            RetErr ret = new RetErr();
            int _tamp = 0;
            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                CurPrjindex = _tamp;
                return ret;
            }

            try
            {
                _tamp = l_struWLAllSet.CurrPrjIndex;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetCurIndex,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetCurIndex;
                ret.Meg = "WorkingList Get Current Prj index ";
                CurPrjindex = _tamp;
                return ret;
            }
            CurPrjindex = _tamp;
            return ret;
        }

        public RetErr GetHullDB(out c_WLSettingBuild.stru_WLAllSetting Alldata)
        {
            RetErr ret = new RetErr();
            c_WLSettingBuild.stru_WLAllSetting _data = new stru_WLAllSetting();
            Alldata = _data;
            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }

            try
            {
                Alldata = l_struWLAllSet;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetHullDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetHullDB;
                ret.Meg = "WorkingList Get Hull DB";
                return ret;
            }
            return ret;
        }


        /////////////////////////////////////////////////////////////////
        ///                     Reset                                  ///
        /////////////////////////////////////////////////////////////////
        public RetErr Reset()
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                Init();
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._Reset,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._Reset;
                ret.Meg = "WorkingList Reset ";
                return ret;
            }
            return ret;
        }

        /////////////////////////////////////////////////////////////////
        ///                     刪除DB                                ///
        /////////////////////////////////////////////////////////////////

        public RetErr DeletDataBase(int index)
        {
            RetErr ret = new RetErr();

            // chk init
            if (!isInit)
            {
                ret.flag = false;
                ret.Meg = "not initial, please initial first";
                return ret;
            }
            try
            {
                l_struWLAllSet.l_WLLayerSetData.RemoveAt(index);
                l_struWLAllSet.l_WLLayersPrjData.RemoveAt(index);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._DeletDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._DeletDB;
                ret.Meg = "WorkingList Delet WL & Prj DB ";
                return ret;
            }
            return ret;
        }

        #endregion


    }

    public class Method_WL
    {
        ///////////////////////////////////////////////////////////
        ///                     define                          ///
        ///////////////////////////////////////////////////////////

        #region define
        class stru_WLAllSetting2File
        {
            // Working List Set Data
            public c_WLSettingBuild.stru_WL_GlobalSet l_stru_WLSet;
            // Working List Layer Data
            public List<c_WLSettingBuild.stru_WL_LayerSet> l_WLLayerSetData;
            
            public void init()
            {
                l_stru_WLSet = new c_WLSettingBuild.stru_WL_GlobalSet();
                l_WLLayerSetData = new List<c_WLSettingBuild.stru_WL_LayerSet>();
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////
        ///                     Para                            ///
        ///////////////////////////////////////////////////////////

        #region Para

        c_WLSettingBuild g_WL;
        Info DB_info;

        public int CurIndex = 0;
        #endregion

        ///////////////////////////////////////////////////////////
        ///                     SubRouting                      ///
        ///////////////////////////////////////////////////////////

        #region Subrouting

        string CheckWLFile()
        {
            int WorklistCount = 0;
            //確認資料夾的檔案名稱編號
            string path = Application.StartupPath.ToString() + "\\";
            foreach (string fname in System.IO.Directory.GetFileSystemEntries(path + "WL\\", "*.wrl"))
            {
                string[] tamp = fname.Split(new string[3] { "\\", "_", ".wrl" }, StringSplitOptions.RemoveEmptyEntries);
                if (tamp[tamp.Length - 2] == DateTime.Now.ToString("yyyyMMdd"))
                {
                    WorklistCount = Convert.ToInt16(tamp[tamp.Length - 1]);
                    WorklistCount++;
                }

            }
            
            return path + "WorkList\\" + Convert.ToString(DateTime.Now.ToString("yyyyMMdd_") + Convert.ToString(WorklistCount)) + ".wrl";
            
        }

        bool GetPrjDB(List<c_WLSettingBuild.stru_WL_LayerSet> WLDB,
                        out List<c_PrjSettingBuild.stru_PrjAllSetting> PrjDB)
        {
            List<c_PrjSettingBuild.stru_PrjAllSetting> tamp = new List<c_PrjSettingBuild.stru_PrjAllSetting>();
            PrjDB = tamp;

            Method_Prj _Method_Prj = new Method_Prj();
            _Method_Prj.init();
            try
            {
                int cunt = WLDB.Count;
                for(int i = 0; i < cunt; i++)
                {
                    c_PrjSettingBuild.stru_PrjAllSetting ramp = new c_PrjSettingBuild.stru_PrjAllSetting();
                    string name = WLDB[i].FileName;
                    _Method_Prj.GetHullPrjDB(name, out ramp);
                    tamp.Add(ramp);
                }
            }
            catch(Exception ee)
            {
                return false;
            }
            PrjDB = tamp;
            return true;
        }

        bool GetPrjDB(string path, out c_PrjSettingBuild.stru_PrjAllSetting data)
        {
            c_PrjSettingBuild.stru_PrjAllSetting alldata = new c_PrjSettingBuild.stru_PrjAllSetting();
            data = alldata;
            try
            {
                StreamReader strr = new StreamReader(path, Encoding.Default);
                string ori = strr.ReadToEnd();
                strr.Close();

                string[] sec = ori.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                c_PrjSettingBuild.stru_PrjSet set = JsonConvert.DeserializeObject<c_PrjSettingBuild.stru_PrjSet>(sec[0]);
                List<c_PrjSettingBuild.stru_PrjLayerSet> layer = JsonConvert.DeserializeObject<List<c_PrjSettingBuild.stru_PrjLayerSet>>(sec[1]);
                int count = layer.Count;

                alldata.LayerCunt = count;
                alldata.l_stru_PrjSet = set;
                alldata.l_LayerSetData = layer;
            }
            catch(Exception ee)
            {
                return false;
            }
            data = alldata;
            return true ;

        }
        #endregion

        ///////////////////////////////////////////////////////////
        ///                     Method                          ///
        ///////////////////////////////////////////////////////////

        #region Method

        public void init()
        {
            g_WL = new c_WLSettingBuild();
            DB_info = new Info();

            g_WL.Init();
        }

        public Info GetInfo()
        {
            return DB_info;
        }

        public string GetPath()
        {
            return DB_info.CurFilePath;
        }

        public RetErr GetHullDB(out c_WLSettingBuild.stru_WLAllSetting alldata)
        {
            RetErr ret = new RetErr();
            c_WLSettingBuild.stru_WLAllSetting dat = new c_WLSettingBuild.stru_WLAllSetting();
            alldata = dat;
            try
            {
                ret = g_WL.GetHullDB(out dat);
                if (!ret.flag)
                    throw new Exception(ret.Meg);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetHullDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetHullDB;
                ret.Meg = "WorkList Form Get hull data " + ee.Message;
                return ret;
            }
            alldata = dat;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"> 0 ~ n </param>
        /// <returns></returns>
        public RetErr DeletDB(int index)
        {
            RetErr ret = new RetErr();
            try
            {
                ret = g_WL.DeletDataBase(index);
                if (!ret.flag)
                    throw new Exception(ret.Meg);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._DeletDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._DeletDB;
                ret.Meg = "WorkList Form Delet DB " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr GetDB(int index, out c_WLSettingBuild.stru_WL_LayerSet layer)
        {
            RetErr ret = new RetErr();
            c_WLSettingBuild.stru_WL_LayerSet tamp = new c_WLSettingBuild.stru_WL_LayerSet();
            layer = tamp;
            try
            {
                ret = g_WL.GetDB(index, out tamp);
                if (!ret.flag)
                    throw new Exception(ret.Meg);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetDB;
                ret.Meg = "WorkList Form Delet DB " + ee.Message;
                return ret;
            }
            layer = tamp;
            return ret;
        }
        public RetErr GetDB(int index, out c_PrjSettingBuild.stru_PrjAllSetting layer)
        {
            RetErr ret = new RetErr();
            c_PrjSettingBuild.stru_PrjAllSetting tamp = new c_PrjSettingBuild.stru_PrjAllSetting();
            layer = tamp;
            try
            {
                ret = g_WL.GetDB(index, out tamp);
                if (!ret.flag)
                    throw new Exception(ret.Meg);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetDB;
                ret.Meg = "WorkList Form Get DB " + ee.Message;
                return ret;
            }
            layer = tamp;
            return ret;
        }

        public RetErr UpdateDB(List<c_WLSettingBuild.stru_WL_LayerSet> DB)
        {
            RetErr ret = new RetErr();
            try
            {
                ret = g_WL.UpdateDB(DB);
                if (!ret.flag)
                    throw new Exception(ret.Meg);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._UpdateDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._UpdateDB;
                ret.Meg = "WorkList Form Update DB " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr UpdateDB(int index, c_WLSettingBuild.stru_WL_LayerSet DB)
        {
            RetErr ret = new RetErr();
            try
            {
                ret = g_WL.UpdateDB(index, DB);
                if (!ret.flag)
                    throw new Exception(ret.Meg);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._UpdateDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._UpdateDB;
                ret.Meg = "WorkList Form Update index DB " + ee.Message;
                return ret;
            }
            return ret;
        }
        public RetErr UpdateDB(int index, c_PrjSettingBuild.stru_PrjAllSetting DB)
        {
            RetErr ret = new RetErr();
            try
            {
                ret = g_WL.UpdateDB(index, DB);
                if (!ret.flag)
                    throw new Exception(ret.Meg);
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._UpdateDB,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._UpdateDB;
                ret.Meg = "WorkList Form Update index DB " + ee.Message;
                return ret;
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"> WL Currrnt Prj index</param>
        /// <returns></returns>
        public RetErr GetCurIndex(out int index)
        {
            RetErr ret = new RetErr();
            int ramp = 0;
            index = ramp;
            try
            {
                ret = g_WL.GetCurIndex(out ramp);
                if (!ret.flag)
                    throw new Exception(ret.Meg);

            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetCurIndex,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._GetCurIndex;
                ret.Meg = "WorkList Form Get Current Prj index " + ee.Message;
                return ret;
            }
            index = ramp;
            return ret;
        }
        public RetErr SetCurIndex( int index)
        {
            RetErr ret = new RetErr();
            try
            {
                ret = g_WL.SetCurIndex((UInt16)index);
                if (!ret.flag)
                    throw new Exception(ret.Meg);

            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._SetCurIndex,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._SetCurIndex;
                ret.Meg = "WorkList Form Set Current Prj index " + ee.Message;
                return ret;
            }
            return ret;
        }


        public RetErr Add()
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

                #region 確認WL編號

                DB_info.CurFilePath = CheckWLFile();

                #endregion




                /// Set flag
                DB_info.IsFileOpen = true;
                DB_info.IsFileSave = false;



            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AddWL,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AddWL;
                ret.Meg = "WorkList Form Add new WL " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr Open()
        {
            RetErr ret = new RetErr();
            try
            {
                #region Check

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
                OpenDia.Filter = ".wrl|*.wrl";
                OpenDia.InitialDirectory = Application.StartupPath + "\\WL";
                if (OpenDia.ShowDialog() != DialogResult.OK) return ret;

                StreamReader strr = new StreamReader(OpenDia.FileName, Encoding.Default);
                string ori = strr.ReadToEnd();
                ori.Clone();

                DB_info.CurFilePath = OpenDia.FileName;

                #endregion

                #region 解析DB

                c_WLSettingBuild.stru_WLAllSetting Alldata = new c_WLSettingBuild.stru_WLAllSetting();
                string[] sec = ori.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Alldata.l_stru_WLSet = JsonConvert.DeserializeObject<c_WLSettingBuild.stru_WL_GlobalSet>(sec[0]);
                Alldata.l_WLLayerSetData = JsonConvert.DeserializeObject<List< c_WLSettingBuild.stru_WL_LayerSet>>(sec[1]);

                #endregion

                #region GetPrjData

                List<c_PrjSettingBuild.stru_PrjAllSetting> prjs = new List<c_PrjSettingBuild.stru_PrjAllSetting>();
                bool r = GetPrjDB(Alldata.l_WLLayerSetData, out prjs);
                if (!r)
                {
                    ret.flag = false;
                    ret.Meg = "讀取專案資料錯誤";
                    return ret;
                }
                Alldata.l_WLLayersPrjData = prjs;

                #endregion

                #region Update DB

                g_WL.UpdateDB(Alldata);

                #endregion

                #region SetFlag

                DB_info.IsFileOpen = true;
                DB_info.IsFileSave = true;

                #endregion 
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._OpenWL,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._OpenWL;
                ret.Meg = "WorkList Form Open WL " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr Save()
        {
            RetErr ret = new RetErr();
            try
            {
                #region Check

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

                #region Combine TxT

                GUI.WorkingList.Errcode.RetErr r = g_WL.GetHullDB(out c_WLSettingBuild.stru_WLAllSetting Alldata);
                if (!r.flag)
                    MessageBox.Show(r.Meg, "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);

                stru_WLAllSetting2File data = new stru_WLAllSetting2File();
                data.init();
                data.l_stru_WLSet = Alldata.l_stru_WLSet;
                data.l_WLLayerSetData = Alldata.l_WLLayerSetData;

                string str = JsonConvert.SerializeObject(data.l_stru_WLSet) + "\r\n";
                str = str + JsonConvert.SerializeObject(data.l_WLLayerSetData);

                #endregion

                #region write to file

                StreamWriter strw = new StreamWriter(DB_info.CurFilePath, false, Encoding.Default);
                strw.Write(str);
                strw.Close();

                #endregion


                // 專案更新資料

                int cunt = Alldata.l_WLLayersPrjData.Count();

                for(int i = 0; i < cunt; i++)
                {

                    #region Combine TxT

                    str = "";
                    str = JsonConvert.SerializeObject(Alldata.l_WLLayersPrjData[i].l_stru_PrjSet) + "\r\n";
                    str = str + JsonConvert.SerializeObject(Alldata.l_WLLayersPrjData[i].l_LayerSetData);

                    #endregion

                    #region write to file

                    strw = new StreamWriter(Alldata.l_WLLayerSetData[i].FileName, false, Encoding.Default);
                    strw.Write(str);
                    strw.Close();

                    #endregion
                }


                /// Set flag
                DB_info.IsFileSave = true;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._SaveWL,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._SaveWL;
                ret.Meg = "WorkList Form Save WL " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr AnoSave()
        {
            RetErr ret = new RetErr();
            try
            {
                #region define

                SaveFileDialog SaveDia = new SaveFileDialog();
                SaveDia.Filter = ".wrl|*.wrl";
                SaveDia.InitialDirectory = Application.StartupPath + "\\Wl";

                #endregion

                #region Combine TxT

                GUI.WorkingList.Errcode.RetErr r = g_WL.GetHullDB(out c_WLSettingBuild.stru_WLAllSetting Alldata);
                if (!r.flag)
                    MessageBox.Show(r.Meg, "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);

                stru_WLAllSetting2File data = new stru_WLAllSetting2File();
                data.init();
                data.l_stru_WLSet = Alldata.l_stru_WLSet;
                data.l_WLLayerSetData = Alldata.l_WLLayerSetData;

                string str = JsonConvert.SerializeObject(data.l_stru_WLSet) + "\r\n";
                str = str + JsonConvert.SerializeObject(data.l_WLLayerSetData);

                #endregion

                #region 位置

                if (SaveDia.ShowDialog() != DialogResult.OK) return ret;

                #endregion

                #region 寫入

                StreamWriter strw = new StreamWriter(SaveDia.FileName, false, Encoding.Default);
                strw.Write(str);
                strw.Close();

                #endregion


                // 專案檔更新(靶標資訊)
                // 專案更新資料

                int cunt = Alldata.l_WLLayersPrjData.Count();

                for (int i = 0; i < cunt; i++)
                {

                    #region Combine TxT

                    str = "";
                    str = JsonConvert.SerializeObject(Alldata.l_WLLayersPrjData[i].l_stru_PrjSet) + "\r\n";
                    str = str + JsonConvert.SerializeObject(Alldata.l_WLLayersPrjData[i].l_LayerSetData);

                    #endregion

                    #region write to file

                    strw = new StreamWriter(Alldata.l_WLLayerSetData[0].FileName, false, Encoding.Default);
                    strw.Write(str);
                    strw.Close();

                    #endregion
                }


                DB_info.CurFilePath = SaveDia.FileName;
                /// Set flag
                DB_info.IsFileSave = true;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AnoSaveWL,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AnoSaveWL;
                ret.Meg = "WorkList Form Another Save WL " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr Close()
        {
            RetErr ret = new RetErr();
            try
            {
                #region ResetDB

                g_WL.Reset();

                #endregion

                #region Reset Flag

                DB_info.Reset();

                #endregion
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._CloseWL,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._CloseWL;
                ret.Meg = "WorkList Form Close WL " + ee.Message;
                return ret;
            }
            return ret;
        }

        public RetErr AddPrj()
        {
            RetErr ret = new RetErr();
            try
            {
                #region def

                OpenFileDialog opendia = new OpenFileDialog();
                opendia.Filter = ".prj|*.prj";
                opendia.InitialDirectory = Application.StartupPath + "\\Prj";

                #endregion

                #region chk

                if (!DB_info.IsFileOpen)
                {
                    MessageBox.Show("請先開啟工單", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                #endregion

                #region Read prj data & add to DB

                if (opendia.ShowDialog() != DialogResult.OK)
                    return ret;
                // get data
                bool r = GetPrjDB(opendia.FileName, out c_PrjSettingBuild.stru_PrjAllSetting alldata);
                if (!r)
                    throw new Exception("load prj file data err ");

                // add prj to DB
                GUI.WorkingList.Errcode.RetErr re = g_WL.Add(alldata);
                if (!re.flag)
                    throw new Exception("add prj data to DB err");

                // add Wl layer to DB
                c_WLSettingBuild.stru_WL_LayerSet layer = new c_WLSettingBuild.stru_WL_LayerSet();
                layer.FileName = opendia.FileName;
                re = g_WL.Add(layer);
                

                #endregion


            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._AddPrj,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                // 通知外面 NG
                ret.flag = false;
                ret.Num = Num._AddPrj;
                ret.Meg = "WorkList Form Add prj data " + ee.Message;
                return ret;
            }
            return ret;
        }



        #endregion

    }
}


namespace BackLight.GUI.WorkingList.Errcode
{
    class Num   // -901 ~ -950
    {
        public static int _initial =        -901;
        public static int _AddGlobalSet =   -902;
        public static int _AddWLLayerSet =  -903;
        public static int _AddPrjLayerSet = -903;
        public static int _Reset =          -904;
        public static int _DeletDB =        -905;
        public static int _GetWLSet =       -906;
        public static int _GetWLLayer =     -907;
        public static int _GetPrjLayer =    -908;
        public static int _GetCurIndex =    -909;
        public static int _CurIndexPluse =  -910;
        public static int _SetCurIndex =    -911;
        public static int _AddWL =          -912;
        public static int _OpenWL =         -913;
        public static int _SaveWL =         -914;
        public static int _AnoSaveWL =      -915;
        public static int _CloseWL =        -916;
        public static int _AddPrj =         -917;
        public static int _GetHullDB =      -918;
        public static int _UpdateDB =       -919;
        public static int _GetDB =          -920;
    }
    public class RetErr
    {
        public Boolean flag = true;
        public string Meg = "";
        public int Num = 0;
    }
    public class Info
    {
        // flag
        public Boolean IsFileOpen = false;
        public Boolean IsFileSave = false;

        public void Reset()
        {
            IsFileOpen = false;
            IsFileSave = false;
        }

        // Register
        public string CurFilePath = "";
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
