#define Log
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using BackLight.Run.Define;
using BackLight.Run.ErrCode;

using System.Diagnostics;

namespace BackLight.Run
{
    class Run_AutoRun
    {
    }
    class Method_MM
    {

        ////////////////////////////////////////////
        ///                 Para                ///
        ////////////////////////////////////////////

        #region Para

        File.File_SetFile cFileProcess;
        GUI.WorkingList.Method_WL cMethod_WL;
        Axis.IAxis cAxis;
        Laser.ILaser cLaser;

        //Record.basExcecuteRec bExcecutRec = new Record.basExcecuteRec();

        CCD.CCD_Method cMethod_CCD;

        // 加工原點
        PointF g_MarkOriPos = new PointF(0, 0);

        // 
        Stopwatch _Timer = new Stopwatch();


        string alarmMeg = "";
        int alarmNum = 0;
        public Boolean isAutoRunON = false;                         // isMarking
        public SAutoRunState _AutoRunState = SAutoRunState.idle;
        public SMarkingFlag _MarkingFlag;                           // 在 Start() 中實例化
        SMarkingRecord _MarkingRecord;                              // 在 Start() 中實例化
        SGlobalMarkingPara _GlobalMarkingPara;                      // 在 Start() 中實例化
        SchkNGValue _SchkNGValue;                                   // 在 Start() 中實例化
        public SHoldCancelFlag _HoldCancelFlag;                        // 在 Start() 中實例化
        SDrawingDistortion _DrawingDistortion;                  // 在 Start() 中實例化
        SLaserPara _LaserPara;                                  // 在 setLayer() 中實例化、初始化
        SLayerReMarkInfo _LayerReMarkInfo;                      // 在 setupLayer() 中實例化、初始化
        //Record.basExcecuteRec.ExceInfo_Prj _ExceInfo_Prj;              // 在 中實例化、初始化
        //Record.basExcecuteRec.ExceInfo_WL _ExceInfo_WL;                // 在 setup()中實例化、初始化
        

        double MoveToZPos = 0;

        Dot _CG_Draw = new Dot();
        Dot _CG_Table = new Dot();

        // chkPosMark沒有找到靶標 設定目前位置作用
        bool isSetPos_chkPosMark = false;


        #endregion


        ////////////////////////////////////////////
        ///                 subrouting           ///
        ////////////////////////////////////////////

        #region Sub
        
       
        void _Wait(int mmSec)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            while (true)
            {
                Application.DoEvents();
                long time = stopwatch.ElapsedMilliseconds;
                if (time > mmSec)
                    break;
            }
        }
        public static void stateButColor(Markstate e)
        {
            switch (e)
            {
                case Markstate.Marking:
                    Forms.frmMain.Bt_WorkingWorkingStart.BackColor = SystemColors.ControlDark;
                    Forms.frmMain.Bt_WorkingWorkingForceStop.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingPause.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingContinue.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingResetAlarm.BackColor = SystemColors.Control;
                    break;
                case Markstate.Alarm:
                    Forms.frmMain.Bt_WorkingWorkingStart.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingForceStop.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingPause.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingContinue.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingResetAlarm.BackColor = Color.Red;
                    break;
                case Markstate.Defaul:
                    Forms.frmMain.Bt_WorkingWorkingStart.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingForceStop.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingPause.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingContinue.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingResetAlarm.BackColor = SystemColors.Control;
                    break;
                case Markstate.Pause:
                    Forms.frmMain.Bt_WorkingWorkingStart.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingForceStop.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingPause.BackColor = Color.Yellow;
                    Forms.frmMain.Bt_WorkingWorkingContinue.BackColor = SystemColors.Control;
                    Forms.frmMain.Bt_WorkingWorkingResetAlarm.BackColor = SystemColors.Control;

                    break;

            }
        }
        //private void M_MMState_Alarm(object sender, _DMMStatusx64Events_AlarmEvent e)
        //{
        //    //MessageBox.Show(e.lStatus, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    alarmMeg = e.lStatus;
        //    _MarkingFlag.isLayerMarking = false;
        //    _MarkingFlag.isMarking = false;
        //    _AutoRunState = SAutoRunState.alarm;
        //    MessageBox.Show(e.lStatus.ToLower());
        //}
        //private void M_MMSplit_SplitLayerStart(object sender, _DMMSplitx64Events_SplitLayerStartEvent e)
        //{
        //    #region Z軸移至雕刻位置

        //    /*
        //    cFileProcess.GetAllSetData();
        //    File.Define.BasicMotion Basic = cFileProcess.GetBasicMotion();


        //    int CurrLIndex = _GlobalMarkingPara.CurrMarkingLayer;
        //    double thicks = _GlobalMarkingPara.MarkingDB_Prj.l_LayerSetData[CurrLIndex].Thicksness;

        //    double RedowHeigh = 0;
        //    if (_LayerReMarkInfo.RedowHeigh != null)
        //        RedowHeigh = _LayerReMarkInfo.RedowHeigh[CurrLIndex];
        //    MoveToZPos = Basic.BasicMarkingFocalDis
        //                        - thicks
        //                        + _LayerReMarkInfo.CurrMarkIndex * RedowHeigh;
        //    cAxis.ZMove(MoveToZPos, Basic.ManualZMoveSpeed, Basic.ZMoveAcc);


        //    cAxis.WaitMotionDone(Hardware.Axis.AxisName.Z);
            
        //    */

        //    #endregion
        //}
        //private void M_MMSplit_SplitLayerEnd(object sender, _DMMSplitx64Events_SplitLayerEndEvent e)
        //{
        //    object ob = new object();
        //    lock (ob)
        //    {
        //        _MarkingFlag.isLayerMarking = false;
        //    }
        //}
        //private void M_MMSplit_StartMarkBlock(object sender, _DMMSplitx64Events_StartMarkBlockEvent e)
        //{
        //    try
        //    {
        //        // if cancel >> cancel
        //        if (_HoldCancelFlag.Cancel)
        //        {
        //            ;
        //        }

        //        // if hole >> hold
        //        if (_HoldCancelFlag.Hold)
        //        {
        //            _HoldCancelFlag.Hold_Block = e.lIndex;
        //            MM_Center.mmMark.PauseMarking();
        //        }

        //        MM_Center.mmSplit.SetStartMarkBlockCode(1);  // 1: Mark Block, 
        //                                                     // 2: repeat Mark Block
        //                                                     // 0: Not Mark Block
        //                                                     // -1: Stop Mark Option, after block always not mark
        //                                                     //Program.Forms.frmProgUse.listBox1.Items.Insert(0, "StartMarkBlock " );
                
        //        // block index show 
        //        BackLight.Forms.frmMain.TextB_WorkingInfoCurrMarkingBlock.Text = e.lIndex.ToString();

        //        // Move To Z
        //        File.Define.BasicMotion basic = cFileProcess.GetBasicMotion();
        //        Hardware.Axis.ErrCode.RetErr ret_Axis = cAxis.ZMove(MoveToZPos, basic.ManualZMoveSpeed, basic.ZMoveAcc);


        //        // Move to XY
        //        ret_Axis = cAxis.XYLine(g_MarkOriPos.X + e.dXOffset,
        //                                g_MarkOriPos.Y + e.dYOffset,
        //                                basic.ManualXYLineSpeed,
        //                                basic.XYLineAcc);

        //    }
        //    catch (Exception ee)
        //    {

        //    }
        //}
        //private void m_MMStatus_MarkEnd(object sender, _DMMStatusx64Events_MarkEndEvent e)
        //{
        //    _MarkingFlag.isMarking = false;
        //}
        /// <summary>
        /// AutoRun中找Mark用
        /// </summary>
        /// <param name="pos"> 搜尋mark的座標</param>
        /// <param name="MarkNum"> 0 ~ n Mark編號</param>
        /// <param name="name"> 視覺靶標名稱</param>
        /// <returns></returns>
        Boolean VisionMarkFind(Dot pos, int MarkNum)
        {
            // 靶標
            File.Define.BasicMotion basic = cFileProcess.GetBasicMotion();
            cAxis.XYLine(pos.pX, pos.pY, basic.AutoXYLineSpeed, basic.XYLineAcc);
            Thread.Sleep(Convert.ToInt32(Forms.frmMain.TextB_WorkingWorkingMoveWaitTime.Text));

            cAxis.WaitMotionDone(Hardware.Axis.AxisName.X);
            cAxis.WaitMotionDone(Hardware.Axis.AxisName.Y);


            _Wait(500);
            CCD.ErrCode.RetErr ret_CCD = cMethod_CCD.AutoRun_FindMark(MarkNum, out bool isFindMark);

            if (!ret_CCD.flag)
            {
                MessageBox.Show(ret_CCD.Meg, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!isFindMark)
            {
               // MessageBox.Show("Can't fount Mark");
                return false;
            }
            return true;
        }

        public Boolean CalDistortion(TwinDot DrawingMark, TwinDot WLMark, ref SDrawingDistortion Distortion)
        {
            try
            {
                // Define
                SDrawingDistortion DrawDistortion = new SDrawingDistortion();
                double WLMark_Deg = 0;
                double Drawing_Deg = 0;
                // roate
                double dx = WLMark.Xp2 - WLMark.Xp1;
                double dy = WLMark.Yp2 - WLMark.Yp1;
                double mdx = DrawingMark.Xp2 - DrawingMark.Xp1;
                double mdy = DrawingMark.Yp2 - DrawingMark.Yp1;
                if (dx == 0) dx = 1;
                else if (mdx == 0) mdx = 1;

                // 判斷象限
                if (dx > 0 && dy > 0)
                    WLMark_Deg = (System.Math.Atan(System.Math.Abs(dy) / System.Math.Abs(dx)) / (System.Math.PI / 180));
                else if (dx < 0 && dy > 0)
                    WLMark_Deg = 180 - (System.Math.Atan(System.Math.Abs(dy / dx)) / (System.Math.PI / 180));
                else if (dx < 0 && dy < 0)
                    WLMark_Deg = 180 + (System.Math.Atan(System.Math.Abs(dy / dx)) / (System.Math.PI / 180));
                else if (dx > 0 && dy < 0)
                    WLMark_Deg = -(System.Math.Atan(System.Math.Abs(dy / dx)) / (System.Math.PI / 180));

                // 判斷象限
                if (mdx > 0 && mdy > 0)
                    Drawing_Deg = (System.Math.Atan(System.Math.Abs(mdy) / System.Math.Abs(mdx)) / (System.Math.PI / 180));
                else if (mdx < 0 && mdy > 0)
                    Drawing_Deg = 180 - (System.Math.Atan(System.Math.Abs(mdy / mdx)) / (System.Math.PI / 180));
                else if (mdx < 0 && mdy < 0)
                    Drawing_Deg = 180 + (System.Math.Atan(System.Math.Abs(mdy / mdx)) / (System.Math.PI / 180));
                else if (mdx > 0 && mdy < 0)
                    Drawing_Deg = -(System.Math.Atan(System.Math.Abs(mdy / mdx)) / (System.Math.PI / 180));



                double _theda = WLMark_Deg - Drawing_Deg;
                DrawDistortion.theda = _theda;// * (System.Math.PI / 180);


                // 縮放比例 (假設圖檔 無角度)
                double theda = (System.Math.Atan(System.Math.Abs(mdy) / System.Math.Abs(mdx)));
                double afterHeigh = (System.Math.Sqrt(mdx * mdx + mdy * mdy)) * System.Math.Sin(theda);
                double afterWindth = (System.Math.Sqrt(mdx * mdx + mdy * mdy)) * System.Math.Cos(theda);
                dx = System.Math.Abs(afterWindth);
                dy = System.Math.Abs(afterHeigh);

                // 計算table沒有theda時的座標值
                Dot ActPos = new Dot();
                ActPos.pX = WLMark.Xp2;
                ActPos.pY = WLMark.Yp2;

                Dot BasicPos = new Dot();
                BasicPos.pX = WLMark.Xp1;
                BasicPos.pY = WLMark.Yp1;

                Dot CalPos = new Dot();

                CalDistortionCoor(-_theda, ActPos, BasicPos, ref CalPos);

                // 計算dx dy
                mdx = CalPos.pX - BasicPos.pX;
                mdy = CalPos.pY - BasicPos.pY;

                if (dx != mdx)
                    DrawDistortion.ScaleX = System.Math.Abs(mdx) / System.Math.Abs(dx);
                if (dy != mdy)
                    DrawDistortion.ScaleY = System.Math.Abs(mdy) / System.Math.Abs(dy);

                if (DrawDistortion.ScaleX == 0) DrawDistortion.ScaleX = 1;
                if (DrawDistortion.ScaleY == 0) DrawDistortion.ScaleY = 1;


                Distortion = DrawDistortion;

            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(Num._CalDistortion,
                                MethodBase.GetCurrentMethod().Name,
                                "計算圖變形量錯誤 " + ee.Message);
#endif
                return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Distortion"> 變形量</param>
        /// <param name="CurVisIndex"> 專案中視覺靶標點編號</param>
        /// <param name="Mark"> 專案DB</param>
        /// <param name="MarkOriPos"> 變形依據點</param>
        /// <param name="pos"> 變形後座標</param>
        /// <returns></returns>
        Boolean CalDistortionCoor(SDrawingDistortion Distortion, int CurVisIndex, GUI.Project.c_PrjSettingBuild.stru_PrjSet Mark, Dot MarkOriPos, ref Dot pos)
        {
            try
            {
                double Point3_x = 0, Point3_y = 0;
                Dot Npoint = new Dot();
                // use the mark1 to do basic point 
                Npoint.pX = Mark.VisionMark[CurVisIndex].PosMarkX - Mark.VisionMark[0].PosMarkX;
                Npoint.pY = Mark.VisionMark[CurVisIndex].PosMarkY - Mark.VisionMark[0].PosMarkY;

                // 座標縮放
                Point3_x = Npoint.pX * Distortion.ScaleX;
                Point3_y = Npoint.pY * Distortion.ScaleY;
                // 座標旋轉;
                Distortion.theda = Distortion.theda * (System.Math.PI / 180);
                pos.pX = Point3_x * System.Math.Cos(Distortion.theda) - Point3_y * System.Math.Sin(Distortion.theda);
                pos.pY = Point3_x * System.Math.Sin(Distortion.theda) + Point3_y * System.Math.Cos(Distortion.theda);

                // 以mark1為0的座標系 to 以機台原點0 的座標系
                pos.pX = pos.pX + MarkOriPos.pX;
                pos.pY = pos.pY + MarkOriPos.pY;
            }
            catch (Exception ee)
            {

#if (Log)
                Log.Pushlist(Num._CalDistortion,
                                MethodBase.GetCurrentMethod().Name,
                                "計算Mark 3~n 座標錯誤 " + ee.Message);
#endif
                return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="theda"> 旋轉度數</param>
        /// <param name="ActPos"> table 實際座標</param>
        /// <param name="MarkOriPos"> table 依據點</param>
        /// <param name="pos"> table 旋轉後座標</param>
        /// <returns></returns>
        Boolean CalDistortionCoor(double theda, Dot ActPos, Dot MarkOriPos, ref Dot pos)
        {
            try
            {
                double Point3_x = 0, Point3_y = 0;
                Dot Npoint = new Dot();
                // use the mark1 to do basic point , use legt button be zero
                Npoint.pX = ActPos.pX - MarkOriPos.pX;
                Npoint.pY = ActPos.pY - MarkOriPos.pY;


                // 座標縮放
                Point3_x = Npoint.pX;
                Point3_y = Npoint.pY;

                // 座標旋轉;
                theda = theda * (System.Math.PI / 180);
                pos.pX = Point3_x * System.Math.Cos(theda) - Point3_y * System.Math.Sin(theda);
                pos.pY = Point3_x * System.Math.Sin(theda) + Point3_y * System.Math.Cos(theda);

                // 以mark1為0的座標系 to 以機台原點0 的座標系
                pos.pX = pos.pX + MarkOriPos.pX;
                pos.pY = pos.pY + MarkOriPos.pY;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._CalDistortion,
                                MethodBase.GetCurrentMethod().Name,
                                "計算 旋轉後座標 " + ee.Message);
#endif
                return false;
            }
            return true;
        }

        /// <summary>
        /// 以 basic 作為縮放比中心, 計算變形前座標
        /// </summary>
        /// <param name="Basic"></param>
        /// <param name="ScaleX"></param>
        /// <param name="ScaleY"></param>
        /// <param name="ori"></param>
        /// <param name="newDot"></param>
        /// <returns></returns>
        Boolean CalDistortionCoor(Dot Basic, double ScaleX, double ScaleY, Dot ori, ref Dot newDot)
        {
            try
            {
                double dx = 0, dy = 0;
                dx = (ori.pX - Basic.pX) / ScaleX;
                dy = (ori.pY - Basic.pY) / ScaleY;

                newDot.pX = Basic.pX + dx;
                newDot.pY = Basic.pY + dy;

            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._CalDistortion,
                                MethodBase.GetCurrentMethod().Name,
                                "... " + ee.Message);
#endif
                return false;
            }
            return true;
        }
        /// <summary>
        /// 計算變形前座標
        /// </summary>
        /// <param name="distortion"></param>
        /// <param name="Basic"></param>
        /// <param name="ori"></param>
        /// <param name="newDot"></param>
        /// <returns></returns>
        Boolean CalDistortionCoor(SDrawingDistortion distortion, Dot Basic, Dot ori, ref Dot newDot)
        {
            try
            {
                double Point3_x = 0, Point3_y = 0;
                Dot Npoint = new Dot();
                // use the mark1 to do basic point , use legt button be zero
                Npoint.pX = ori.pX - Basic.pX;
                Npoint.pY = ori.pY - Basic.pY;


                // 座標縮放
                Point3_x = Npoint.pX / distortion.ScaleX;
                Point3_y = Npoint.pY / distortion.ScaleY;

                // 座標旋轉;
                double theda = 0;
                theda = (-distortion.theda) * (System.Math.PI / 180);
                newDot.pY = Point3_x * System.Math.Cos(theda) - Point3_y * System.Math.Sin(theda);
                newDot.pX = Point3_x * System.Math.Sin(theda) + Point3_y * System.Math.Cos(theda);

                // 以mark1為0的座標系 to 以機台原點0 的座標系
                newDot.pX = newDot.pX + Basic.pX;
                newDot.pY = newDot.pY + Basic.pY;

            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._CalDistortion,
                                MethodBase.GetCurrentMethod().Name,
                                "... " + ee.Message);
#endif
                return false;
            }
            return true;
        }

        Boolean LoadMarkingPrjMarkingPara(ref SGlobalMarkingPara MP, GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting WL)
        {
            try
            {
                int CurrPrjIndex = WL.CurrPrjIndex;
                MP.CurrPrjIndex = WL.CurrPrjIndex;
                var DataPath = WL.l_WLLayerSetData[CurrPrjIndex];
                var PrjDataPath = WL.l_WLLayersPrjData[CurrPrjIndex];
                MP.Degree = DataPath.DegreeOffset;
                MP.isPosCompensation = DataPath.isPosCompensati;
                MP.Mark1PosX = DataPath.Mark1_PosX;
                MP.Mark1PosY = DataPath.Mark1_PosY;
                MP.Mark2PosX = DataPath.Mark2_PosX;
                MP.Mark2PosY = DataPath.Mark2_PosY;
                MP.MarkingDB_Prj = WL.l_WLLayersPrjData[CurrPrjIndex];
                MP.RoteCenterX = 0;
                MP.RoteCenterY = 0;
                MP.WorkingHomeX = 0;
                MP.WorkingHomeY = 0;
                MP.XOffset = DataPath.PosOffset_X;
                MP.XScale = DataPath.ScaleOffset_X;
                MP.YOffset = DataPath.PosOffset_Y;
                MP.YScale = DataPath.ScaleOffset_Y;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._LoadMarkingPrjMarkingPara,
                                MethodBase.GetCurrentMethod().Name,
                                "載入專案雕刻檔錯誤 " + ee.Message);
#endif
                return false;
            }
            return true;
        }

        /// <summary>
        ///  存放當下雕刻Layer 雷射資料(startmark用)
        /// </summary>
        /// <param name="PrjData"></param>
        /// <param name="_Laser"></param>
        /// <returns></returns>
        Boolean LoadLaserPara(GUI.Project.c_PrjSettingBuild.stru_PrjAllSetting PrjData, ref SLaserPara _Laser)
        {
            try
            {
                object ob = new object();
                lock (ob)
                {
                    int CurrLayerIndex = _GlobalMarkingPara.CurrMarkingLayer;
                    var LauerData = PrjData.l_LayerSetData[CurrLayerIndex];
                    _Laser.EndDelay = LauerData.EndDelay;
                    _Laser.EPulse = LauerData.LaserEpulse;
                    _Laser.Freq = LauerData.Freq;
                    _Laser.JumpDelay = LauerData.JumpDelay;
                    _Laser.JumpSpeed = LauerData.JumpSpeed;
                    _Laser.MarkSpeed = LauerData.MarkSpeed;
                    _Laser.MiddleDelay = LauerData.MiddleDelay;
                    _Laser.ObjMarkTimes = LauerData.ObjMarkTimes;
                    _Laser.OnDelay = LauerData.OnDelay;
                    _Laser.Power = LauerData.Power;
                    _Laser.PulseWidth = LauerData.PulseWidth;
                    _Laser.PWUS = 0;
                    _Laser.SpotDelay = LauerData.SpotDelay;

                }

            }
            catch (Exception ee)
            {
                inde_SettingBuild.Log.Meg(inde_SettingBuild.Log.AutoRun_BeforeMarkingLoadLayerLaserData,
                                              "Marking前載入 Layer Laser Para",
                                              inde_SettingBuild.Log.AutoRun_LOG, false);
                return false;
            }
            return true;
        }
        Boolean SetMarkingLayerPara(ref AxMMMarkx64 mmMark, ref AxMMEditx64 mmEdit, ref AxMMSplitx64 mmSplit,
                                    GUI.Project.c_PrjSettingBuild.stru_PrjAllSetting AllLayerPara)
        {
            try
            {

                //
                int ret = 0;
                int CurrLayerIndex = _GlobalMarkingPara.CurrMarkingLayer;
                string LayerName = "";


                var PSet = AllLayerPara.l_stru_PrjSet;
                ret = ret + mmMark.SetACCType(0);
                ret = ret + mmMark.SetACCEnable(PSet.isSKYW == 1 ? 1 : 0);
                ret = ret + mmMark.SetACC(PSet.SKYW_Accel);
                ret = ret + mmMark.SetACCLimitAngle(PSet.SKYW_LimitAngle);
                var LD = AllLayerPara.l_LayerSetData[CurrLayerIndex];
                ret = ret + mmEdit.GetLayerName(CurrLayerIndex + 1, ref LayerName);
                ret = ret + mmEdit.SetLayerView(LayerName, LD.Show);
                ret = ret + mmEdit.SetLayerOutput(LayerName, LD.Mark);
                //ret = ret + mmMark.SetMarkRepeat(LayerName, LD.LayerMarkTimes);
                // Markdir
                ret = ret + LD.isSplit == 1 ? mmSplit.Enable() : mmSplit.Disable();
                ret = ret + mmSplit.SetSplitSize(LD.DivWidth, LD.DivHeigh);
                ret = ret + mmMark.SetPower(LayerName, LD.Power);
                ret = ret + mmMark.SetFrequency(LayerName, LD.Freq);
                ret = ret + mmMark.SetPulseWidth(LayerName, LD.PulseWidth);
                double ra = mmMark.GetPulseWidth(LayerName);
                ret = ret + mmEdit.SetPulseWidth(LayerName, LD.PulseWidth);
                double ra2 = mmEdit.GetPulseWidth(LayerName);
                /* 設定obj mark times */
            // SetObjMarkTimes(ref mmMark, ref mmEdit, LayerName, LD.ObjMarkTimes);
                ret = ret + mmMark.SetSpeed(LayerName, LD.MarkSpeed);
                ret = ret + mmMark.SetJumpSpeed(LayerName, LD.JumpSpeed);
                ret = ret + mmMark.SetJumpDelay(LayerName, LD.JumpDelay);
                ret = ret + mmMark.SetSpotDelay(LayerName, LD.SpotDelay);
                ret = ret + mmMark.SetLaserOnDelay(LayerName, LD.OnDelay);
                ret = ret + mmMark.SetPolyDelay(LayerName, LD.MiddleDelay);
                ret = ret + mmMark.SetLaserOffDelay(LayerName, LD.EndDelay);
                ret = ret + mmEdit.SetFillSwitch(LayerName, LD.isFill);
                if (LD.isFill == 1)
                {
                    ret = ret + mmEdit.SetFillTwoway(LayerName, LD.isFillTwoWay);
                    ret = ret + mmEdit.SetFillAverageDistribution(LayerName, LD.BalanceFill);
                    ret = ret + mmEdit.SetFillBorder(LayerName, LD.FillBorder);
                    ret = ret + mmEdit.SetFillPitch(LayerName, LD.FillPitch);
                    ret = ret + mmEdit.SetFillTimes(LayerName, (int)LD.FillTimes);
                    ret = ret + mmEdit.SetFillStartAngle(LayerName, LD.FillAngleStart);
                    ret = ret + mmEdit.SetFillStepAngle(LayerName, LD.FillAngleStep);
                }
                if (LD.isSplit == 1)
                {
                    ret = ret + mmSplit.Enable();
                    ret = ret + mmSplit.SetSplitSize(LD.DivWidth, LD.DivHeigh);
                    ret = ret + mmSplit.SetSplitOverlapSize(LD.Overlapsize, LD.Overlapsize);
                }
                if (LD.isWobble == 1)
                {
                    ret = ret + mmEdit.SetWobble(LayerName, LD.WobbleThick, LD.WobbleFreq);
                }
                if (ret != 0)
                    throw new Exception("Set Layer Para NG");


                mmMark.Refresh();

            }
            catch (Exception ee)
            {
                inde_SettingBuild.Log.Meg(inde_SettingBuild.Log.AutoRun_SetObjMarkTimes,
                                              ee.Message,
                                              inde_SettingBuild.Log.AutoRun_LOG, false);
                return false;
            }
            return true;
        }
        Boolean GetPrjRedownInfo(ref SLayerReMarkInfo info, GUI.Project.c_PrjSettingBuild.stru_PrjAllSetting prj)
        {
            try
            {
                GUI.WorkingList.Errcode.RetErr ret_WL = cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);
                if (!ret_WL.flag)
                    throw new Exception(ret_WL.Meg);

                int CurrPrjIndex = _GlobalMarkingPara.CurrPrjIndex;
                int LayerCunt = alldata.l_WLLayersPrjData[CurrPrjIndex].LayerCunt;
                var WLPrjData = alldata.l_WLLayersPrjData[CurrPrjIndex];
                info.RedowHeigh = new List<double>();
                info.RedowTimes = new List<double>();
                for (int i = 0; i < LayerCunt; i++)
                {
                    info.RedowHeigh.Add(WLPrjData.l_LayerSetData[i].ReDownHeigh);
                    info.RedowTimes.Add(WLPrjData.l_LayerSetData[i].LayerMarkTimes);
                }
                info.CurrMarkIndex = 0;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._GetPrjRedownInfo,
                                MethodBase.GetCurrentMethod().Name,
                                "GetPrjRedownInfo錯誤 " + ee.Message);
#endif
                return false;
            }
            return true;
        }

        Boolean CalTimeAndShow(SMarkingRecord _record, ref Label passTime, ref Label endtime)
        {
            try
            {
                if (_record.MarkingStartTime == null) return true;
                _record.MarkingEndTime = DateTime.Now.ToString(" H:mm:ss");

                MarkingTime(_record.MarkingStartTime,
                            _record.MarkingEndTime,
                            out _record.MarkingPassingTime);
                passTime.Text = _record.MarkingPassingTime;
                endtime.Text = _record.MarkingEndTime;
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }

        Boolean CalTimeAndShow(SMarkingRecord _record, ref Label passTime)
        {
            try
            {
                if (_record.MarkingStartTime == null) return true;
                _record.MarkingEndTime = DateTime.Now.ToString(" H:mm:ss");

                MarkingTime(_record.MarkingStartTime,
                            _record.MarkingEndTime,
                            out _record.MarkingPassingTime);
                passTime.Text = _record.MarkingPassingTime;
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }


        public Boolean CalCGPos(MulitDot Poss, ref Dot pos)
        {
            try
            {
                int cunt = Poss.dot.Count;
                double Val_X = 0, Val_Y = 0;

                for (int i = 0; i < cunt; i++)
                {
                    Val_X = Val_X + Poss.dot[i].pX;
                    Val_Y = Val_Y + Poss.dot[i].pY;
                }

                pos.pX = Val_X / cunt;
                pos.pY = Val_Y / cunt;
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        public Boolean CalCGPos(List<Dot> nPoints, ref Dot CG)
        {
            try
            {
                List<Dot> mPoints = new List<Dot>();
                // 判別順序(順、逆時針)
                ReBuild(nPoints, ref mPoints);

                double area = 0.0f;//多边形面积  
                Dot CG_New = new Dot();// 重心的x、y
                for (int i = 1; i <= mPoints.Count; i++)
                {
                    double iLat = mPoints[(i % mPoints.Count())].pX;
                    double iLng = mPoints[(i % mPoints.Count())].pY;
                    double nextLat = mPoints[(i - 1)].pX;
                    double nextLng = mPoints[(i - 1)].pY;
                    double temp = (iLat * nextLng - iLng * nextLat) / 2.0f;
                    area += temp;
                    CG_New.pX += temp * (iLat + nextLat) / 3.0f;
                    CG_New.pY += temp * (iLng + nextLng) / 3.0f;
                }
                CG_New.pX = CG_New.pX / area;
                CG_New.pY = CG_New.pY / area;
                CG = CG_New;
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        public Boolean ReBuild(List<Dot> mPoints, ref List<Dot> nPoints)
        {
            try
            {
                ttemp sort = new ttemp();
                sort.dot = new Dot();

                List<ttemp> sortdata = new List<ttemp>();

                double sum_X = 0, sum_Y = 0;
                int cumt = mPoints.Count();
                for (int j = 0; j < cumt; j++)
                {
                    sum_X = sum_X + mPoints[j].pX;
                    sum_Y = sum_Y + mPoints[j].pY;

                }

                Dot Cg = new Dot();
                Cg.pX = sum_X / cumt;
                Cg.pY = sum_Y / cumt;


                for (int j = 0; j < cumt; j++)
                {
                    sort.atan2 = System.Math.Atan2(mPoints[j].pX - Cg.pX, mPoints[j].pY - Cg.pY);
                    sort.dot = mPoints[j];
                    sortdata.Add(sort);
                }

                sortdata.Sort((x, y) => x.atan2.CompareTo(y.atan2));

                for (int j = 0; j < sortdata.Count(); j++)
                {
                    Dot tt = new Dot();
                    tt.pX = sortdata[j].dot.pX;
                    tt.pY = sortdata[j].dot.pY;

                    nPoints.Add(tt);
                }

            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        public int MarkingTime(string start, string end, out string passtime)
        {
            int ret = 1;
            string[] starttime = start.Split(':');
            string[] endtime = end.Split(':');
            int hour = 0, min = 0, sec = 0;
            try
            {
                int ST = Convert.ToInt16(starttime[0]) * 60 * 60 + Convert.ToInt16(starttime[1]) * 60 + Convert.ToInt16(starttime[2]);
                int ET = Convert.ToInt16(endtime[0]) * 60 * 60 + Convert.ToInt16(endtime[1]) * 60 + Convert.ToInt16(endtime[2]);
                int tamp1 = ET - ST;
                hour = tamp1 / 60 / 60;
                min = tamp1 / 60 - hour * 60;
                sec = tamp1 % 60;
            }
            catch (Exception ee)
            {
#if (Log)
                Log.Pushlist(Num._CalDistortion,
                                MethodBase.GetCurrentMethod().Name,
                                "... " + ee.Message);
#endif
                passtime = "";
                return -1;
            }
            string hhour = "", mmin = "", ssec = "";
            if (hour == 0) hhour = "00"; else hhour = hour.ToString();
            if (min == 0) mmin = "00"; else mmin = min.ToString();
            if (sec < 10) ssec = "0" + sec.ToString(); else ssec = sec.ToString();
            passtime = Convert.ToString(hhour) + ":" + Convert.ToString(mmin) + ":" + Convert.ToString(ssec);
            return ret;
        }

        #endregion

        ////////////////////////////////////////////
        ///                 Method               ///
        ////////////////////////////////////////////


        #region Method

        public RetErr init(File.File_SetFile file,
                            GUI.WorkingList.Method_WL WL,
                            Hardware.Axis.IAxis axis,
                            Hardware.Laser.ILaser laser,
                            Hardware.CCD.ICCD ccd)
        {
            RetErr ret = new RetErr();
            try
            {
                cAxis = axis;
                cFileProcess = file;
                cMethod_WL = WL;
                cLaser = laser;

                cMethod_CCD = new CCD.CCD_Method();
                cMethod_CCD.init(axis, ccd, WL, file);
            }
            catch (Exception ee)
            {
#if Log
                Log.Pushlist(Num._Link,
                                MethodBase.GetCurrentMethod().Name,
                                ee.Message);
#endif
                ret.flag = false;
                ret.Num = Num._Link;
                ret.Meg = "AutoRun Link Fail " + ee.Message;
                return ret;
            }
            return ret;
        }
        public void SetStatus(SAutoRunState tt)
        {
            _AutoRunState = tt;
        }
        public SHoldCancelFlag GetHoldCancelFlag()
        {
            return _HoldCancelFlag;
        }
        public void SetStatus(SHoldCancelFlag tt)
        {
            _HoldCancelFlag = tt;
        }
        public bool GetIsMarking()
        {
            return isAutoRunON;
        }
        public void AutoRun()
        {
            Boolean isFinish = false;
            isAutoRunON = true;
            _AutoRunState = SAutoRunState.Start;
            while (true)
            {
                switch (_AutoRunState)
                {
                    case SAutoRunState.idle:
                        isFinish = true;
                        Finish();
                        break;
                    case SAutoRunState.Start:
                        _AutoRunState = Start();
                        break;
                    case SAutoRunState.SetupPrj:
                        _AutoRunState = SetupPrj();
                        break;
                    case SAutoRunState.GoNextPrj:               // 須將 CurrPrjIndex ++
                        _AutoRunState = GoNextPrj();
                        break;
                    case SAutoRunState.GoNextLayer:               // 須將 CurrPrjIndex ++
                        _AutoRunState = GoNextLayer();
                        break;
                    case SAutoRunState.chkPosMark:
                        _AutoRunState = ChkPosMark();
                        break;
                    case SAutoRunState.SetupWorkingH:
                        _AutoRunState = SetupWorkingH();
                        break;
                    case SAutoRunState.FindMarkingLayer:
                        _AutoRunState = FindMarkingLayer();
                        break;
                    case SAutoRunState.SetupLayer:
                        _AutoRunState = SetupLayer();
                        break;
                    case SAutoRunState.StartMarking:
                        _AutoRunState = StartMarking();
                        break;
                    case SAutoRunState.Finish:
                        isFinish = true;
                        _AutoRunState = Finish();
                        break;
                    case SAutoRunState.alarm:
                        //isFinish = true;
                        _AutoRunState = alarm();
                        break;
                    case SAutoRunState.cancel:
                        _AutoRunState = cancel();
                        break;
                    case SAutoRunState.hold:
                        _AutoRunState = hold();
                        break;
                    case SAutoRunState.reStart:
                        _AutoRunState = reStart();
                        break;
                }
                if (isFinish) break;
            }
            isAutoRunON = false;
        }

        public void SetPos_chkPosMark(Boolean status)
        {
            object ob = new object();
            lock (ob)
            {
                isSetPos_chkPosMark = status;
            }
        }
        private SAutoRunState Start()
        {
            Boolean ret = false;
            // ResetFlag
            _MarkingFlag = new SMarkingFlag();
            _MarkingRecord = new SMarkingRecord();
            _GlobalMarkingPara = new SGlobalMarkingPara();
            _HoldCancelFlag = new SHoldCancelFlag();
            _DrawingDistortion = new SDrawingDistortion();
            _SchkNGValue = new SchkNGValue();


            // initial
            GUI.WorkingList.Errcode.RetErr ret_WL = cMethod_WL.SetCurIndex(0);
            if (!ret_WL.flag)
            {
                alarmMeg = ret_WL.Meg;
                alarmNum = ret_WL.Num;
                return SAutoRunState.alarm;
            }

            /* 待測試 */
            
            bExcecutRec.ExcecuteInfo_ini();
            _ExceInfo_WL = new Record.basExcecuteRec.ExceInfo_WL();
            _ExceInfo_WL.StartTime = DateTime.Now.ToString(" H:mm:ss");
            _ExceInfo_WL.Name = cMethod_WL.GetPath();
            _Timer.Restart();

            // LaserEnable

            // Fail >> goto alarm

            // NG value
            if (Forms.frmMain.checkBox1.Checked)
            {
                _SchkNGValue.isNGlimit = Forms.frmMain.checkBox1.Checked;
                _SchkNGValue.ScaleX = double.TryParse(Forms.frmMain.TextB_NGCore_ScaleX.Text, out double ta) ? ta/100 : 1;
                _SchkNGValue.ScaleY = double.TryParse(Forms.frmMain.TextB_NGCore_ScaleY.Text, out ta) ? ta/100 : 1;
            }

            // Record
            _MarkingRecord.MarkingStartTime = DateTime.Now.ToString(" H:mm:ss");
            Forms.frmMain.Label_WorkingWorkingMarkingStart.Text = _MarkingRecord.MarkingStartTime;
            Forms.frmMain.Label_WorkingWorkingMarkingPassTime.Text = "";
            Forms.frmMain.Label_WorkingWorkingMarkingEndTime.Text = "";


            // Goto setupPrj
            return SAutoRunState.SetupPrj;



        }
        private SAutoRunState SetupPrj()
        {
            try
            {
                // if cancel >> cancel
                if (_HoldCancelFlag.Cancel)
                    return SAutoRunState.cancel;

                // if hole >> hold
                if (_HoldCancelFlag.Hold)
                {
                    _HoldCancelFlag.HoldARState = SAutoRunState.SetupPrj;
                    return SAutoRunState.hold;
                }

                // check prj file is exies
                GUI.WorkingList.Errcode.RetErr ret_WL = cMethod_WL.GetCurIndex(out int CurrPrjIndex);
                if (!ret_WL.flag)
                {
                    alarmMeg = ret_WL.Meg;
                    alarmNum = ret_WL.Num;
                    return SAutoRunState.alarm;
                }

                ret_WL = cMethod_WL.GetDB(CurrPrjIndex, out GUI.WorkingList.c_WLSettingBuild.stru_WL_LayerSet WLlayer);
                if (!ret_WL.flag)
                {
                    alarmMeg = ret_WL.Meg;
                    alarmNum = ret_WL.Num;
                    return SAutoRunState.alarm;
                }

                string FileName = WLlayer.FileName;
                if (!System.IO.File.Exists(FileName))
                {
                    inde_SettingBuild.Log.Meg(inde_SettingBuild.Log.AutoRun_FileNotExists,
                                                "File Not Exists",
                                                inde_SettingBuild.Log.AutoRun_LOG, false);
                    return SAutoRunState.Finish;
                }


                // check prj is need mark
                Boolean isNeedMark = WLlayer.Mark == 1 ? true : false;
                if (!isNeedMark)
                    return SAutoRunState.GoNextPrj;


                /* 待測試 */
                // Execute Info 
                bExcecutRec.ResetInfo_Prj(_ExceInfo_Prj);
                _ExceInfo_Prj.Name = WLlayer.FileName;
                _ExceInfo_Prj.StartTime = DateTime.Now.ToString(" H:mm:ss"); ;




                // load drawing who prj of need mark
                MM_Center.mmMark.ResetFile();
                ret_WL = cMethod_WL.GetDB(CurrPrjIndex, out GUI.Project.c_PrjSettingBuild.stru_PrjAllSetting PrjData);
                if (!ret_WL.flag)
                {
                    alarmMeg = ret_WL.Meg;
                    alarmNum = ret_WL.Num;
                    return SAutoRunState.alarm;
                }

                string path = PrjData.l_stru_PrjSet.DrawName;
                if(path.Length<10)
                {
                    alarmMeg = "圖檔路徑";
                    alarmNum = -10;
                    return SAutoRunState.alarm;
                }

                MM_Center.mmMark.LoadFile(path);
                Forms.frmMain.Panel_Draw.Refresh();


                // load prj set
                ret_WL = cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting WLData);
                if (!ret_WL.flag)
                {
                    alarmMeg = ret_WL.Meg;
                    alarmNum = ret_WL.Num;
                    return SAutoRunState.alarm;
                }
                LoadMarkingPrjMarkingPara(ref _GlobalMarkingPara, WLData);
                GetPrjRedownInfo(ref _LayerReMarkInfo, PrjData);


                // Set Split 
                int UseBlockEven = 1, state = -1;
                state = MM_Center.mmSplit.EnableBlockMarkEndEvent(UseBlockEven);
                state = MM_Center.mmSplit.EnableSplitEvent(1);
                state = MM_Center.mmSplit.SetSplitOptions(1);        // 1: Use Split with Layer
                                                                     // 2: Use Optimization
                                                                     // 4: Show range of axis
                                                                     //mmSplit.SetDrawMethod(1);



                MM_Center.mmStatus.MarkEnd += m_MMStatus_MarkEnd;
                MM_Center.mmStatus.Alarm += M_MMState_Alarm;
                MM_Center.mmSplit.StartMarkBlock += M_MMSplit_StartMarkBlock;
                MM_Center.mmSplit.SplitLayerEnd += M_MMSplit_SplitLayerEnd;
                MM_Center.mmSplit.SplitLayerStart += M_MMSplit_SplitLayerStart;


                // check is need chkpos
                Boolean isNeedChkPos =
                    PrjData.l_stru_PrjSet.NeedChkMark == 1 ? true : false;
                if (isNeedChkPos)
                    return SAutoRunState.chkPosMark;

                // Show
                Forms.frmMain.TextB_WorkingInfoCurrPrj.Text = (_GlobalMarkingPara.CurrPrjIndex + 1).ToString();

                return SAutoRunState.SetupWorkingH;
                // Fail >> goto alarm

            }
            catch (Exception ee)
            {
                alarmMeg = "設定專案 檔案錯誤";
                return SAutoRunState.alarm;
            }
        }
        SAutoRunState GoNextPrj()
        {
            cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);

            int PrjCunt = alldata.l_WLLayerSetData.Count();
            int index = alldata.CurrPrjIndex + 1;
            if (index > -1 && index < PrjCunt)
            {
                alldata.CurrPrjIndex++;
                cMethod_WL.SetCurIndex(alldata.CurrPrjIndex);
                return SAutoRunState.SetupPrj;
            }
            // 計算時間
            //CalTimeAndShow(_MarkingRecord,
            //           ref Program.Forms.frmCutSys.label23,
            //           ref Program.Forms.frmCutSys.label22);

            _ExceInfo_Prj.EndTime = DateTime.Now.ToString(" H:mm:ss");
            _ExceInfo_Prj.DiviTimes++;
            bExcecutRec.AddInfo_Prj(_ExceInfo_Prj);
            bExcecutRec.ResetInfo_Prj(_ExceInfo_Prj);


            return SAutoRunState.Finish;
        }
        SAutoRunState ChkPosMark()
        {
            try
            {
                // Define
                Boolean _rett = false;

                GUI.Project.c_PrjSettingBuild.stru_PrjSet __SMark = new GUI.Project.c_PrjSettingBuild.stru_PrjSet();

                // if cancel >> cancel
                if (_HoldCancelFlag.Cancel)
                    return SAutoRunState.cancel;

                // if hole >> hold
                if (_HoldCancelFlag.Hold)
                {
                    _HoldCancelFlag.HoldARState = SAutoRunState.chkPosMark;
                    return SAutoRunState.hold;
                }

                // 確認視覺點是否重複
                cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);
                int CurrPrjIndex =
                    alldata.CurrPrjIndex;
                int MarkCunt =
                    alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark.Count;
                Dot tamp1 = new Dot();
                Dot tamp2 = new Dot();

                // 確認專案靶標是否重複
                for (int i = 0; i < MarkCunt; i++)
                {
                    for (int j = 0; j < MarkCunt; j++)
                    {
                        if (i != j)
                        {
                            tamp1.pX =
                                alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[i].PosMarkX;
                            tamp1.pY =
                                alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[i].PosMarkY;
                            tamp2.pX =
                                alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[j].PosMarkX;
                            tamp2.pY =
                                alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[j].PosMarkY;

                            if (tamp1.pX == tamp2.pX && tamp1.pY == tamp2.pY)
                            {
                                inde_SettingBuild.Log.Meg(inde_SettingBuild.Log.AutoRun_VisionMarkReShow,
                                                            "視覺靶標重疊 ",
                                                            inde_SettingBuild.Log.AutoRun_LOG, false);
                                alarmMeg = "視覺靶標重疊";
                                return SAutoRunState.alarm;
                            }
                        }
                    }
                }


                // 定位靶標搜尋
                MulitDot _MulitDot_table = new MulitDot();
                _MulitDot_table.dot = new List<Dot>();
                MulitDot _MulitDot_Draw = new MulitDot();
                _MulitDot_Draw.dot = new List<Dot>();
                Dot tampDot = new Dot();

                /* Z軸移動 */
                double FocalDis =
                    alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.PosCCDFocalPos;
                File.Define.BasicMotion basic = cFileProcess.GetBasicMotion();
                cAxis.ZMove(FocalDis, basic.ManualZMoveSpeed, basic.ZMoveAcc);

                cAxis.WaitMotionDone(Hardware.Axis.AxisName.Z);

                // 對位靶標控制項新增至加工頁面, 並帶到畫面最前面
                Forms.frmMain.TabP_RegulatePage.Controls.Remove(Forms.frmMain.Panel_CCD_pos);
                Forms.frmMain.TabP_WorkingPage.Controls.Add(Forms.frmMain.Panel_CCD_pos);
                Forms.frmMain.Panel_CCD_pos.BringToFront();

                /* 第1靶標 */
                Dot pos1 = new Dot();
                Dot Autualpos1 = new Dot();
                pos1.pX = alldata.l_WLLayerSetData[CurrPrjIndex].Mark1_PosX;
                pos1.pY = alldata.l_WLLayerSetData[CurrPrjIndex].Mark1_PosY;
                //
                Boolean isFind = VisionMarkFind(pos1, 0);
                if (!isFind)
                {
                    alarmMeg = "視覺靶標 1 錯誤";
                    DialogResult result = MessageBox.Show("是否進行手動靶標位置設定?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if(result == DialogResult.Yes)
                    {
                        Forms.frmMain.GroupB_MarkingHandRedMark.Visible = true;
                        // 卡迴圈等到確定位置
                        while (!isSetPos_chkPosMark)
                        {
                            Application.DoEvents();
                        }
                        isSetPos_chkPosMark = false;
                    }
                    else
                        return SAutoRunState.alarm;

                }

                // 顯示畫面
                Forms.frmMain.pb_Mark1.Load(Application.StartupPath + "\\WorkDir\\Mark_0" + ".jpg");

                // 找到靶標位置, 更新畫面紀錄位置
                cAxis.GetPos(out Hardware.Axis.ErrCode.Point3D point);
                Forms.frmMain.TextB_WorkingWLMark1X.Text = point.X.ToString();
                Forms.frmMain.TextB_WorkingWLMark1Y.Text = point.Y.ToString();
                Autualpos1.pX = point.X;
                Autualpos1.pY = point.Y;

                // 紀錄table上找到的點
                Application.DoEvents();
                tampDot = new Dot();
                tampDot.pX = point.X;
                tampDot.pY = point.Y;
                _MulitDot_table.dot.Add(tampDot);

                // 更新工單DB的MARK
                GUI.WorkingList.c_WLSettingBuild.stru_WL_LayerSet tamp = new GUI.WorkingList.c_WLSettingBuild.stru_WL_LayerSet();
                tamp = alldata.l_WLLayerSetData[CurrPrjIndex];
                tamp.Mark1_PosX = Autualpos1.pX;
                tamp.Mark1_PosY = Autualpos1.pY;


                /* 第2靶標 */
                Dot pos2 = new Dot();
                Dot Autualpos2 = new Dot();
                pos2.pX = alldata.l_WLLayerSetData[CurrPrjIndex].Mark2_PosX;
                pos2.pY = alldata.l_WLLayerSetData[CurrPrjIndex].Mark2_PosY;
                //
                isFind = VisionMarkFind(pos2, 1);
                if (!isFind)
                {
                    alarmMeg = "視覺靶標 2 錯誤"; 
                    DialogResult result = MessageBox.Show("是否進行手動靶標位置設定?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        Forms.frmMain.GroupB_MarkingHandRedMark.Visible = true;
                        // 卡迴圈等到確定位置
                        while (!isSetPos_chkPosMark)
                        {
                            Application.DoEvents();
                        }
                        isSetPos_chkPosMark = false;
                    }
                    else
                        return SAutoRunState.alarm;
                }


                // 顯示畫面
                Forms.frmMain.pb_Mark2.Load(Application.StartupPath + "\\WorkDir\\Mark_1" + ".jpg");

                cAxis.GetPos(out point);

                Forms.frmMain.TextB_VisualMark2_X.Text = point.X.ToString();
                Forms.frmMain.TextB_VisualMark2_Y.Text = point.Y.ToString();
                Autualpos2.pX = point.X;
                Autualpos2.pY = point.Y;

                // 紀錄table上找到的點
                tampDot = new Dot();
                tampDot.pX = point.X;
                tampDot.pY = point.Y;
                _MulitDot_table.dot.Add(tampDot);

                // 更新工單DB的MARK
                tamp.Mark2_PosX = Autualpos2.pX;
                tamp.Mark2_PosY = Autualpos2.pY;
                alldata.l_WLLayerSetData[CurrPrjIndex] = tamp;


                /* 計算roate and StartMarkAndTablePosi */
                TwinDot DrawingMark = new TwinDot();
                TwinDot WLMark = new TwinDot();
                WLMark.Xp1 = pos1.pX; WLMark.Yp1 = pos1.pY;
                WLMark.Xp2 = pos2.pX; WLMark.Yp2 = pos2.pY;
                DrawingMark.Xp1 = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[0].PosMarkX;
                DrawingMark.Yp1 = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[0].PosMarkY;
                DrawingMark.Xp2 = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[1].PosMarkX;
                DrawingMark.Yp2 = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[1].PosMarkY;


                tampDot = new Dot();
                tampDot.pX = DrawingMark.Xp1;
                tampDot.pY = DrawingMark.Yp1;
                _MulitDot_Draw.dot.Add(tampDot);

                tampDot = new Dot();
                tampDot.pX = DrawingMark.Xp2;
                tampDot.pY = DrawingMark.Yp2;
                _MulitDot_Draw.dot.Add(tampDot);


                _DrawingDistortion = new SDrawingDistortion();
                _rett = CalDistortion(DrawingMark, WLMark, ref _DrawingDistortion);
                if (!_rett)
                {
                    alarmMeg = "視覺靶標 計算形變 錯誤";
                    return SAutoRunState.alarm;
                }

                /* 確認其餘是否再算出位置 */
                int VisionMarkCunt = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark.Count;
                Dot MarkOriPos = new Dot();
                MarkOriPos.pX = Autualpos1.pX;
                MarkOriPos.pY = Autualpos1.pY;

                //test
                Dot SumOffset = new Dot();

                for (int i = 2; i < VisionMarkCunt; i++)
                {
                    Application.DoEvents();

                    // Define
                    Dot MarkPos = new Dot();

                    // 座標計算
                    var _SMark = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet;
                    __SMark = _SMark;

                    _rett = CalDistortionCoor(_DrawingDistortion, i, __SMark, MarkOriPos, ref MarkPos);
                    if (!_rett)
                    {
                        alarmMeg = "計算形變後視覺靶標座標 NG";
                        return SAutoRunState.alarm;
                    }

                    // 紀錄 Draw 上的點
                    tampDot = new Dot();
                    tampDot.pX = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[i].PosMarkX;
                    tampDot.pY = alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.VisionMark[i].PosMarkY;
                    _MulitDot_Draw.dot.Add(tampDot);


                    // Search
                    //
                    isFind = VisionMarkFind(MarkPos, i);
                    if (!isFind)
                    {
                        alarmMeg = "尋找形變後視覺靶標 NG";
                        DialogResult result = MessageBox.Show("是否進行手動靶標位置設定?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            Forms.frmMain.GroupB_MarkingHandRedMark.Visible = true;
                            // 卡迴圈等到確定位置
                            while (!isSetPos_chkPosMark)
                            {
                                Application.DoEvents();
                            }
                            isSetPos_chkPosMark = false;
                        }
                        else
                            return SAutoRunState.alarm;
                    }

                    // 紀錄table上找到的點
                    cAxis.GetPos(out point);
                    tampDot = new Dot();
                    tampDot.pX = point.X;
                    tampDot.pY = point.Y;
                    _MulitDot_table.dot.Add(tampDot);

                }

                // 顯示畫面
                Forms.frmMain.pb_Mark3.Load(Application.StartupPath + "\\WorkDir\\Mark_2" + ".jpg");
                Forms.frmMain.pb_Mark4.Load(Application.StartupPath + "\\WorkDir\\Mark_3" + ".jpg");


                // 計算重心
                Dot CG_Table = new Dot();
                //CalCGPos(_MulitDot_table, ref CG_Table);
                CalCGPos(_MulitDot_table.dot, ref CG_Table);

                _CG_Table = CG_Table;

                Dot CG_Draw = new Dot();
                //CalCGPos(_MulitDot_Draw, ref CG_Draw);
                CalCGPos(_MulitDot_Draw.dot, ref CG_Draw);

                _CG_Draw = CG_Draw;


                // 
                CalDist_Scale(CG_Table, _MulitDot_table, CG_Draw, _MulitDot_Draw, ref _DrawingDistortion);
                CalDist_thdea(CG_Table, _MulitDot_table, CG_Draw, _MulitDot_Draw, ref _DrawingDistortion);
                CalDist_offset(_MulitDot_Draw, ref _DrawingDistortion);

                // 計算旋轉縮放完後的offset
                //CalDist_offset(CG_Draw, _DrawingDistortion, _MulitDot_Draw.dot[0], ref _DrawingDistortion);

                // 判斷是不是在NG值內
                if (_SchkNGValue.isNGlimit)
                {
                    if (_DrawingDistortion.ScaleX > _SchkNGValue.ScaleX ||
                        _DrawingDistortion.ScaleY > _SchkNGValue.ScaleY)
                    {
                        alarmMeg = "變形量超過設定值!";
                        return SAutoRunState.alarm;
                    }

                }


                #region V1.1.5 使用

                //_DrawingDistortion.ScaleX = Math.Round(_DrawingDistortion.ScaleX, 3);
                //_DrawingDistortion.ScaleY = Math.Round(_DrawingDistortion.ScaleY, 3);
                //_DrawingDistortion.theda = Math.Round(_DrawingDistortion.theda, 3);


                ///* 旋轉圖面 */
                //mmSplit.ModifyRootExt(0, 0,
                //                        DrawingMark.Xp1,
                //                        DrawingMark.Yp1,
                //                        _DrawingDistortion.theda,
                //                        DrawingMark.Xp1,
                //                        DrawingMark.Yp1,
                //                        _DrawingDistortion.ScaleX,
                //                        _DrawingDistortion.ScaleY);
                ///*  畫面顯示 */
                //Program.Forms.frm1.label_rotate.Text = _DrawingDistortion.theda.ToString("F4");
                //Program.Forms.frm1.label_scale.Text = "xx";
                //Program.Forms.frm1.label_scaleX.Text = _DrawingDistortion.ScaleX.ToString("F4");
                //Program.Forms.frm1.label_scaleY.Text = _DrawingDistortion.ScaleY.ToString("F4");


                #endregion

                #region V1.1.6 後使用

                _DrawingDistortion.CenterX = CG_Draw.pX;
                _DrawingDistortion.CenterY = CG_Draw.pY;

                ///* 定位補償 */
                //if (alldata.l_WLLayerSetData[CurrPrjIndex].isPosCompensati == 1)
                //    Compensate(
                //                alldata.l_WLLayerSetData[CurrPrjIndex],
                //                _DrawingDistortion,
                //                ref _DrawingDistortion
                //                );

                ///* 旋轉圖面 */
                //MM_Center.mmMark.SetMatrixExt(0,
                //                        0,
                //                        CG_Draw.pX,
                //                        CG_Draw.pY,
                //                        _DrawingDistortion.theda,
                //                        CG_Draw.pX,
                //                        CG_Draw.pY,
                //                        _DrawingDistortion.ScaleX,
                //                        _DrawingDistortion.ScaleY);

                //MM_Center.mmMark.SaveFile(Application.StartupPath + "\\tamp.ezm");

                /*  畫面顯示 */
                Forms.frmMain.Lab_Workpice_Rotete.Text = _DrawingDistortion.theda.ToString(BackLight.Define.Define.Axis_Dec);
                Forms.frmMain.Lab_Workpice_Scale.Text = "xx";
                Forms.frmMain.Lab_Workpice_ScaleX.Text = (_DrawingDistortion.ScaleX * 100).ToString(BackLight.Define.Define.Axis_Dec);
                Forms.frmMain.Lab_Workpice_ScaleY.Text = (_DrawingDistortion.ScaleY * 100).ToString(BackLight.Define.Define.Axis_Dec);



                #endregion
                //return SAutoRunState.Finish;
                return SAutoRunState.SetupWorkingH;
            }
            catch (Exception ee)
            {
                alarmMeg = "ChkPosMark NG";
                return SAutoRunState.alarm;
            }

            //return SAutoRunState.Finish;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool SetupDistortion(string name)
        {
            try
            {
                cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);
                int CurrPrjIndex =
                   alldata.CurrPrjIndex;

                if (alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.NeedChkMark == 1
                    || alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.NeedChkMark == 1)
                {
                    if (alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.NeedChkMark != 1)// 沒有視覺靶標狀態
                    {
                        _DrawingDistortion.CenterX = MM_Center.mmEdit.GetCenterX(name);
                        _DrawingDistortion.CenterY = MM_Center.mmEdit.GetCenterY(name);
                        _DrawingDistortion.ScaleX = 1;
                        _DrawingDistortion.ScaleY = 1;
                        _DrawingDistortion.theda = 0;
                        _DrawingDistortion.X_offset = 0;
                        _DrawingDistortion.Y_offset = 0;
                    }

                    /* 定位補償 */
                    if (alldata.l_WLLayerSetData[CurrPrjIndex].isPosCompensati == 1)
                        Compensate(
                                    alldata.l_WLLayerSetData[CurrPrjIndex],
                                    _DrawingDistortion,
                                    ref _DrawingDistortion
                                    );

                    /* 旋轉圖面 */
                    // 不影響原圖
                    MM_Center.mmSplit.ModifyRootExt(_DrawingDistortion.X_offset,
                                                    _DrawingDistortion.Y_offset,
                                                    _DrawingDistortion.CenterX,
                                                    _DrawingDistortion.CenterY,
                                                    _DrawingDistortion.theda,
                                                    _DrawingDistortion.CenterX,
                                                    _DrawingDistortion.CenterY,
                                                    _DrawingDistortion.ScaleX,
                                                    _DrawingDistortion.ScaleY);


                    //MM_Center.mmMark.SaveFile(Application.StartupPath + "\\tamp.ezm");

                    /*  畫面顯示 */
                    Forms.frmMain.Lab_Workpice_Rotete.Text = _DrawingDistortion.theda.ToString(BackLight.Define.Define.Axis_Dec);
                    Forms.frmMain.Lab_Workpice_Scale.Text = "xx";
                    Forms.frmMain.Lab_Workpice_ScaleX.Text = (_DrawingDistortion.ScaleX * 100).ToString(BackLight.Define.Define.Axis_Dec);
                    Forms.frmMain.Lab_Workpice_ScaleY.Text = (_DrawingDistortion.ScaleY * 100).ToString(BackLight.Define.Define.Axis_Dec);
                }


            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        SAutoRunState SetupWorkingH()
        {
            try
            {
                GUI.WorkingList.Errcode.RetErr ret_WL = cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);
                if (!ret_WL.flag)
                {
                    alarmMeg = ret_WL.Meg;
                    alarmNum = ret_WL.Num;
                    return SAutoRunState.alarm;
                }

                // Define
                int CurrPrjIndex = alldata.CurrPrjIndex;
                var WLPrjData = alldata.l_WLLayerSetData[CurrPrjIndex];
                var PrjData = alldata.l_WLLayersPrjData[CurrPrjIndex];

                // Procese

                // if cancel >> cancel
                if (_HoldCancelFlag.Cancel)
                {
                    return SAutoRunState.cancel;
                }

                // if hole >> hold
                if (_HoldCancelFlag.Hold)
                {
                    _HoldCancelFlag.HoldARState = SAutoRunState.SetupWorkingH;
                    return SAutoRunState.hold;
                }

                File.Define.BasicMotion basic = cFileProcess.GetBasicMotion();

                Boolean isVisionMark =
                    alldata.l_WLLayersPrjData[CurrPrjIndex].l_stru_PrjSet.NeedChkMark == 1 ? true : false;
                if (isVisionMark)
                {
                    g_MarkOriPos.X =Convert.ToSingle(
                                                                            _CG_Table.pX -
                                                                            _CG_Draw.pX +
                                                                            basic.PosChkCCDOffsetX);

                    g_MarkOriPos.Y =Convert.ToSingle(
                                                                            _CG_Table.pY -
                                                                            _CG_Draw.pY +
                                                                            basic.PosChkCCDOffsetY);

                }
                else
                {
                    g_MarkOriPos.X = Convert.ToSingle(
                                                                            WLPrjData.WorkingH_PosX +
                                                                            basic.PosChkCCDOffsetX);
                    g_MarkOriPos.Y = Convert.ToSingle(
                                                                            WLPrjData.WorkingH_PosY +
                                                                            basic.PosChkCCDOffsetY);

                }

            }
            catch (Exception ee)
            {
                alarmMeg = "設定工作起始座標 NG";
                return SAutoRunState.alarm;
            }
            return SAutoRunState.FindMarkingLayer;
        }

        SAutoRunState FindMarkingLayer()
        {
            try
            {
                cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);

                // Define
                int CurrPrjIndex = alldata.CurrPrjIndex;

                // if cancel >> cancel
                if (_HoldCancelFlag.Cancel)
                    return SAutoRunState.cancel;

                // if hole >> hold
                if (_HoldCancelFlag.Hold)
                {
                    _HoldCancelFlag.HoldARState = SAutoRunState.FindMarkingLayer;
                    return SAutoRunState.hold;
                }

                //
                int LayersCunt = alldata.l_WLLayersPrjData[CurrPrjIndex].LayerCunt;
                var PrjLayerPara = alldata.l_WLLayersPrjData[CurrPrjIndex].l_LayerSetData;
                Boolean isFindMarkingLayer = false;
                int nextLayer = _GlobalMarkingPara.CurrMarkingLayer;
                for (int i = nextLayer; i < LayersCunt; i++)
                {
                    if (1 == PrjLayerPara[i].Mark)
                    {
                        isFindMarkingLayer = true;
                        _GlobalMarkingPara.CurrMarkingLayer = i;
                        break;
                    }
                }
                if (!isFindMarkingLayer)
                {
                    return SAutoRunState.GoNextPrj;
                }
            }
            catch (Exception ee)
            {
                alarmMeg = "尋找 Marking Layer NG";
                return SAutoRunState.alarm;
            }
            return SAutoRunState.SetupLayer;
        }
        SAutoRunState GoNextLayer()
        {
            cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);
            int CurrPrjIndex = alldata.CurrPrjIndex;
            int LayerCunt = alldata.l_WLLayersPrjData[CurrPrjIndex].l_LayerSetData.Count;
            int index = _GlobalMarkingPara.CurrMarkingLayer;
            if (index + 1 > -1 && index + 1 < LayerCunt)
            {
                _GlobalMarkingPara.CurrMarkingLayer++;
                return SAutoRunState.FindMarkingLayer;
            }

            return SAutoRunState.GoNextPrj;
        }
        SAutoRunState SetupLayer()
        {
            try
            {
                // Define
                Boolean ret = false;
                //_LayerReMarkInfo = new SLayerReMarkInfo();

                // Define
                _LaserPara = new SLaserPara();

                // Get DB
                cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);

                // if cancel >> cancel
                if (_HoldCancelFlag.Cancel)
                    return SAutoRunState.cancel;

                // if hole >> hold
                if (_HoldCancelFlag.Hold)
                {
                    _HoldCancelFlag.HoldARState = SAutoRunState.SetupLayer;
                    return SAutoRunState.hold;
                }

                // Confirm the WL Offset
                int CurrPrjIndex = alldata.CurrPrjIndex;
                var WLData = alldata.l_WLLayerSetData[CurrPrjIndex];
                //if(WLData.isPosCompensati != 1) 
                //{

                //}

                // Load Redown Infor


                // Set LaserSet、MarkingPara

                /* Load */
                ret = LoadLaserPara(alldata.l_WLLayersPrjData[CurrPrjIndex],
                                ref _LaserPara);


                /* Set Layer */
                var CurPrjData = alldata.l_WLLayersPrjData[CurrPrjIndex];

                ret = SetMarkingLayerPara(ref MM_Center.mmMark, ref MM_Center.mmEdit, ref MM_Center.mmSplit, CurPrjData);
                if (!ret)
                    ;// throw new Exception("");

                /* Set LaserPower */
                cMethod_WL.GetDB(CurrPrjIndex, out GUI.Project.c_PrjSettingBuild.stru_PrjAllSetting prjData);
                GUI.Project.c_PrjSettingBuild.stru_PrjLayerSet layerData = prjData.l_LayerSetData[_GlobalMarkingPara.CurrMarkingLayer];
                Hardware.Laser.ErrCode.RetErr ret_laser = cLaser.SetLaserPreset(layerData.Power,
                                                                                layerData.Freq,
                                                                                (int)layerData.LaserMode,
                                                                                layerData.LaserBias,
                                                                                layerData.LaserEpulse);
                if (!ret_laser.flag)
                {
                    alarmMeg = ret_laser.Meg;
                    alarmNum = ret_laser.Num;
                    return SAutoRunState.alarm;
                }
                

                /* Set Laser */
                // 尚未補上
                /* ------------------------- */

                return SAutoRunState.StartMarking;
            }
            catch (Exception ee)
            {
                alarmMeg = "設定 Layer 錯誤";
                return SAutoRunState.alarm;
            }
        }
        SAutoRunState StartMarking()
        {
            try
            {
                // Set Select Layer
                string name = "";
                int retr = 0;
                retr = MM_Center.mmMark.SelectClearObjects();
                MM_Center.mmEdit.GetLayerName(_GlobalMarkingPara.CurrMarkingLayer + 1, ref name);
                retr = MM_Center.mmMark.SelectAddObject(name);
                retr = MM_Center.mmMark.SelectGetCount();


                int rety = MM_Center.mmMark.SetMarkSelect(1);       // 1: Mark Selct Obj
                                                                    // 0: Mark All Obj

                #region Set Distortion

                bool tr = SetupDistortion(name);
                if (!tr)
                    throw new Exception("設定變形量2圖錯誤 ");

                #endregion


                Forms.frmMain.TextB_WorkingInfoCurrLayer.Text =
                                                        (_GlobalMarkingPara.CurrMarkingLayer + 1).ToString();
                
                Forms.frmMain.Label_WorkingInfoLayerMarkCount.Text =
                                                        _GlobalMarkingPara.MarkingDB_Prj.l_LayerSetData[_GlobalMarkingPara.CurrMarkingLayer].LayerMarkTimes.ToString();
                Forms.frmMain.TextB_WorkingInfoCurrLayerMarkCounts.Text =
                                                        (_LayerReMarkInfo.CurrMarkIndex + 1).ToString();

                cMethod_WL.GetHullDB(out GUI.WorkingList.c_WLSettingBuild.stru_WLAllSetting alldata);
                int CurrPrjindex = alldata.CurrPrjIndex;

                Forms.frmMain.Label_WorkingInfoCurrMarkingPrjName.Text =
                                                        alldata.l_WLLayerSetData[CurrPrjindex].FileName;


                #region 一致加工位置

                File.Define.BasicMotion Basic = cFileProcess.GetBasicMotion();
                // Move to XY
                Hardware.Axis.ErrCode.RetErr ret_Axis = cAxis.XYLine(g_MarkOriPos.X,
                                                        g_MarkOriPos.Y,
                                                        Basic.ManualXYLineSpeed,
                                                        Basic.XYLineAcc);
                #endregion

                #region Z軸移至雕刻位置 

                cFileProcess.GetAllSetData();


                int CurrLIndex = _GlobalMarkingPara.CurrMarkingLayer;
                double thicks = _GlobalMarkingPara.MarkingDB_Prj.l_LayerSetData[CurrLIndex].Thicksness;

                double RedowHeigh = 0;
                if (_LayerReMarkInfo.RedowHeigh != null)
                    RedowHeigh = _LayerReMarkInfo.RedowHeigh[CurrLIndex];
                MoveToZPos = Basic.BasicMarkingFocalDis
                                    - thicks
                                    + _LayerReMarkInfo.CurrMarkIndex * RedowHeigh;
                cAxis.ZMove(MoveToZPos, Basic.ManualZMoveSpeed, Basic.ZMoveAcc);


                cAxis.WaitMotionDone(Hardware.Axis.AxisName.Z);

                #endregion



                // show window  
                Forms.frmMain.Panel_Draw.Visible = true;
                Forms.frmMain.Panel_Draw.BringToFront();
                // StartMark
                retr = MM_Center.mmMark.MarkStandBy();
                retr = MM_Center.mmMark.StartMarking(4);  // 1: 阻斷式雕刻(無對話盒)
                                                          // 2: 阻斷式雕刻(有對話盒)
                                                          // 3: 預覽雕刻
                                                          // 4: 非阻斷式雕刻(無對話和) >> 會發送 markEnd 事件

                // Wait LayerMarking done
                _MarkingFlag.isLayerMarking = true;
                _MarkingFlag.isMarking = true;

                while (true)
                {
                    Application.DoEvents();
                    TimeSpan t = _Timer.Elapsed;
                    Forms.frmMain.Label_WorkingWorkingMarkingPassTime.Text = " "+ 
                            t.Hours.ToString()+":"+t.Minutes.ToString()+":"+t.Seconds.ToString();
                    Application.DoEvents();
                    if (_AutoRunState == SAutoRunState.alarm)
                        return SAutoRunState.alarm;
                    if (!_MarkingFlag.isLayerMarking)       // 圖層結束跳離開
                        break;
                    if (MM_Center.mmMark.IsMarking() == 0)
                        break;
                    else if (_AutoRunState == SAutoRunState.reStart)
                        reStart();
                    else if (_AutoRunState == SAutoRunState.hold)
                        hold();
                    else if (_AutoRunState == SAutoRunState.cancel)
                        return SAutoRunState.cancel;
                    else if (_HoldCancelFlag.Cancel)
                        return SAutoRunState.cancel;
                    else if (_HoldCancelFlag.Hold)
                        hold();
                }

                int CurrLayerIndex = _GlobalMarkingPara.CurrMarkingLayer;
                Thread.Sleep(100);
                // chk Layer Marking Times
                if (_GlobalMarkingPara.MarkingDB_Prj.l_LayerSetData[CurrLayerIndex].LayerMarkTimes > 1)
                {
                    if (_LayerReMarkInfo.CurrMarkIndex + 1 < _GlobalMarkingPara.MarkingDB_Prj.l_LayerSetData[CurrLayerIndex].LayerMarkTimes)
                    {
                        _LayerReMarkInfo.CurrMarkIndex++;
                        return SAutoRunState.StartMarking;
                    }
                }
                return SAutoRunState.GoNextLayer;
            }
            catch (Exception ee)
            {
                alarmMeg = "開始雕刻 NG " + ee.Message;
                return SAutoRunState.alarm;
            }
            return SAutoRunState.GoNextLayer;
        }
        SAutoRunState Finish()
        {
            try
            {

                // 計算時間
                CalTimeAndShow(_MarkingRecord,
                           ref Forms.frmMain.Label_WorkingWorkingMarkingPassTime,
                           ref Forms.frmMain.Label_WorkingWorkingMarkingEndTime);
                // MM 
                MM_Center.mmMark.MarkShutdown();

                // Move To Out
                File.Define.BasicMotion basic = cFileProcess.GetBasicMotion();
                cAxis.ZMove(basic.JigOutputPosZ, basic.ManualZMoveSpeed, basic.ZMoveAcc);
                cAxis.WaitMotionDone(Hardware.Axis.AxisName.Z);
                cAxis.XYLine(basic.JigOutputPosX, basic.JigOutputPosY, basic.ManualXYLineSpeed, basic.XYLineAcc);


                // flag reset
                _MarkingFlag.isMarking = false;

                
                //Program.Forms.bProManaControl.MM_ReloadDraw();
            }
            catch (Exception ee)
            {

            }
            return SAutoRunState.idle;
        }
        SAutoRunState alarm()
        {
            try
            {
                MessageBox.Show("錯誤編好: " + alarmNum.ToString() + "\r\n 錯誤訊息: " + alarmMeg ,
                                "錯誤",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
            catch (Exception ee)
            {

            }
            return SAutoRunState.Finish;
        }
        SAutoRunState cancel()
        {
            try
            {
                MM_Center.mmMark.EmgStop();

                // 計算時間
                CalTimeAndShow(_MarkingRecord,
                           ref Forms.frmMain.Label_WorkingWorkingMarkingPassTime,
                           ref Forms.frmMain.Label_WorkingWorkingMarkingEndTime);

                // flag reset
                _MarkingFlag.isMarking = false;
            }
            catch (Exception ee)
            {

            }
            return SAutoRunState.Finish;
        }
        SAutoRunState hold()
        {
            try
            {
                Application.DoEvents();
                MM_Center.mmMark.PauseMarking();
            }
            catch (Exception ee)
            {

            }
            return SAutoRunState.hold;
        }
        SAutoRunState reStart()
        {
            try
            {
                MM_Center.mmMark.ResumeMarking();
            }
            catch (Exception ee)
            {

            }
            return _HoldCancelFlag.HoldARState;
        }
        SAutoRunState emgStop()
        {
            MM_Center.mmMark.EmgStop();
            return SAutoRunState.Finish;
        }

       
        struct ttemp
        {
            public double atan2;
            public Dot dot;
        }

        /// <summary>
        /// 計算以BasicPos為原點NextPos與水平的角度
        /// </summary>
        /// <param name="BasicPos"></param>
        /// <param name="NextPos"></param>
        /// <param name="theda"></param>
        /// <returns></returns>
        Boolean CalTheda(Dot BasicPos, Dot NextPos, ref double theda)
        {
            try
            {
                double dx = System.Math.Abs(NextPos.pX - BasicPos.pX);
                double dy = System.Math.Abs(NextPos.pY - BasicPos.pY);

                theda = System.Math.Atan(dy / dx) / (System.Math.PI / 180);
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 計算分量總和
        /// </summary>
        /// <param name="CG"></param>
        /// <param name="_MulitDot"></param>
        /// <param name="type"> 0 X分量, 1 Y分量</param>
        /// <param name="Sum"></param>
        /// <returns></returns>
        Boolean CalSum_Component(Dot CG, MulitDot _MulitDot, int type, ref List<double> Sum)
        {
            try
            {
                for (int i = 0; i < _MulitDot.dot.Count; i++)
                {
                    double tamp = 0;
                    if (type == 0)
                        tamp = System.Math.Abs(_MulitDot.dot[i].pX - CG.pX);
                    else
                        tamp = System.Math.Abs(_MulitDot.dot[i].pY - CG.pY);
                    Sum.Add(tamp);
                }
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 計算分量總和包含方向性
        /// </summary>
        /// <param name="CG"></param>
        /// <param name="_MulitDot"></param>
        /// <param name="type"> 0 X分量, 1 Y分量</param>
        /// <param name="Sum"></param>
        /// <returns></returns>
        Boolean CalSum_Component(Dot CG, MulitDot _MulitDot, double type, ref List<double> Sum)
        {
            try
            {
                for (int i = 0; i < _MulitDot.dot.Count; i++)
                {
                    double tamp = 0;
                    if (type == 0)
                        tamp = _MulitDot.dot[i].pX - CG.pX;
                    else
                        tamp = _MulitDot.dot[i].pY - CG.pY;
                    Sum.Add(tamp);
                }
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 計算變形量的theda角 (table - draw) , must calculate Scale first
        /// </summary>
        /// <param name="CG_Table"> table 的重心</param>
        /// <param name="Table"> table 上找地的所有點</param>
        /// <param name="CG_Draw"> draw(圖面上) 的重心</param>
        /// <param name="Draw"> draw(圖面上) 所有的點</param>
        /// <param name="Theda_Dist"> table 與 draw(圖面上) theda 總和差</param>
        /// <returns></returns>
        public Boolean CalDist_thdea(Dot CG_Table, MulitDot Table, Dot CG_Draw, MulitDot Draw, ref SDrawingDistortion Distortion)
        {
            try
            {
                // 
                List<double> _diff = new List<double>();
                List<double> af_diff = new List<double>();

                for (int i = 0; i < Table.dot.Count; i++)
                {
                    double table_theda = 0, Draw_theda = 0;
                    CalP1toP2Theda(CG_Table, Table.dot[i], ref table_theda);
                    CalP1toP2Theda(CG_Draw, Draw.dot[i], ref Draw_theda);

                    double value = table_theda - Draw_theda;

                    _diff.Add(value);
                }

                // 找出最大變化角度
                double Maxdeg = System.Math.Abs(_diff.Max()) > System.Math.Abs(_diff.Max()) ? _diff.Max() : _diff.Max();

                // 除最大化
                for (int i = 0; i < _diff.Count; i++)
                {
                    double tamp = _diff[i] - Maxdeg;
                    af_diff.Add(tamp);
                }

                // 總平均
                double sum = 0;
                for (int i = 0; i < af_diff.Count; i++)
                {
                    sum = sum + af_diff[i];
                }
                double balanceSum = sum / af_diff.Count;

                //
                Distortion.theda = balanceSum + Maxdeg;


            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }

        public Boolean CalDist_Scale(Dot CG_Table, MulitDot Table, Dot CG_Draw, MulitDot Draw, ref SDrawingDistortion Distortion)
        {
            try
            {
                //
                //
                MulitDot _table = new MulitDot();
                _table.dot = new List<Dot>();

                CalCoorBefore(Distortion, CG_Table, Table, ref _table);


                List<double> Table_ComX = new List<double>();
                List<double> Table_ComY = new List<double>();
                List<double> Draw_ComX = new List<double>();
                List<double> Draw_ComY = new List<double>();
                // Table X分量
                CalSum_Component(CG_Table, _table, 0, ref Table_ComX);
                // Table Y分量
                CalSum_Component(CG_Table, _table, 1, ref Table_ComY);
                // Draw X分量
                CalSum_Component(CG_Draw, Draw, 0, ref Draw_ComX);
                // Draw Y分量
                CalSum_Component(CG_Draw, Draw, 1, ref Draw_ComY);


                double SumX = 0, SumY = 0;
                for (int i = 0; i < Table_ComX.Count; i++)
                {
                    SumX = SumX + (Table_ComX[i] / Draw_ComX[i]);
                    SumY = SumY + (Table_ComY[i] / Draw_ComY[i]);
                }
                //

                Distortion.ScaleX = SumX / Table_ComX.Count;
                Distortion.ScaleY = SumY / Table_ComX.Count;


            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        public Boolean CalDist_ScaleEx(Dot CG_Table, MulitDot Table, Dot CG_Draw, MulitDot Draw, ref SDrawingDistortion Distortion)
        {
            try
            {
                //
                //
                MulitDot _table = new MulitDot();
                _table.dot = new List<Dot>();

                CalCoorBefore(Distortion, CG_Table, Table, ref _table);

                Dot CG = new Dot();
                CalCGPos(_table, ref CG);

                List<double> Table_ComX = new List<double>();
                List<double> Table_ComY = new List<double>();
                List<double> Draw_ComX = new List<double>();
                List<double> Draw_ComY = new List<double>();
                // Table X分量
                CalSum_Component(CG_Table, _table, 0, ref Table_ComX);
                // Table Y分量
                CalSum_Component(CG_Table, _table, 1, ref Table_ComY);
                // Draw X分量
                CalSum_Component(CG_Draw, Draw, 0, ref Draw_ComX);
                // Draw Y分量
                CalSum_Component(CG_Draw, Draw, 1, ref Draw_ComY);

                List<double> _diffX = new List<double>();
                List<double> _diffY = new List<double>();

                for (int i = 0; i < Table_ComX.Count; i++)
                {
                    _diffX.Add(Table_ComX[i] / Draw_ComX[i]);
                    _diffY.Add(Table_ComY[i] / Draw_ComY[i]);
                }

                // find max
                double Max_X = _diffX.Max();
                double Max_Y = _diffY.Max();

                List<double> __diffX = new List<double>();
                List<double> __diffY = new List<double>();

                for (int i = 0; i < Table_ComX.Count; i++)
                {
                    __diffX.Add(_diffX[i] - Max_X);
                    __diffY.Add(_diffY[i] - Max_Y);
                }

                double SumX = 0, SumY = 0;
                for (int i = 0; i < __diffX.Count; i++)
                {
                    SumX = SumX + __diffX[i];
                    SumY = SumY + __diffY[i];
                }



                Distortion.ScaleX = (SumX / __diffX.Count) + Max_X;
                Distortion.ScaleY = (SumY / __diffY.Count) + Max_Y;


            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }

        public Boolean CalDist_offset(Dot CG_Draw, SDrawingDistortion Distortion
                                        , Dot Mark1Pos_Draw, ref SDrawingDistortion oDistortion)
        {
            SDrawingDistortion tamp = new SDrawingDistortion();
            oDistortion = tamp;
            try
            {
                // 計算變形後座標
                Dot AfPos = new Dot();
                CalDistortionCoor(Distortion, CG_Draw, Mark1Pos_Draw, ref AfPos);

                // 計算offset 原始- 轉換後
                tamp = Distortion;
                tamp.X_offset = tamp.X_offset + (Mark1Pos_Draw.pX - AfPos.pX);
                tamp.Y_offset = tamp.Y_offset + (Mark1Pos_Draw.pY - AfPos.pY);



            }
            catch (Exception ee)
            {
                return false;
            }
            oDistortion = tamp;
            return true;
        }
        public Boolean CalDist_offset(MulitDot Draw, ref SDrawingDistortion Distortion)
        {
            try
            {
                //
                List<double> X_list = new List<double>();
                List<double> Y_list = new List<double>();
                Difference(Draw.dot, 0, ref X_list);
                Difference(Draw.dot, 1, ref Y_list);

                // Max Value
                double Max_X = 0, Max_Y = 0;
                FindMaxValue(X_list, ref Max_X);
                FindMaxValue(Y_list, ref Max_Y);

                Distortion.X_offset = (Max_X * Distortion.ScaleX - Max_X) / 2;
                Distortion.Y_offset = (Max_Y * Distortion.ScaleY - Max_Y) / 2;

            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        private Boolean CalCoorBefore(SDrawingDistortion distortion, Dot CG, MulitDot after, ref MulitDot before)
        {
            try
            {
                if (before.dot == null) return false;
                Dot tamp;
                for (int i = 0; i < after.dot.Count(); i++)
                {
                    tamp = new Dot();
                    double theda = -distortion.theda;
                    CalDistortionCoor(theda, after.dot[i], CG, ref tamp);
                    before.dot.Add(tamp);
                }
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        private Boolean FindMaxValue(List<double> list, ref double value)
        {
            try
            {
                double tamp = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] > tamp)
                        tamp = list[i];
                }
                value = tamp;
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 計算差值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"> 0 X value, 1 Y Value </param>
        /// <param name="Diff"></param>
        /// <returns></returns>
        private Boolean Difference(List<Dot> list, int type, ref List<double> Diff)
        {
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        double value = 0;
                        if (type == 0)
                            value = list[i].pX - list[j].pX;
                        if (type == 1)
                            value = list[i].pY - list[j].pY;
                        Diff.Add(value);
                    }
                }
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 計算以p1為原點p2與水平的角度 (360)
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="theda"></param>
        /// <returns></returns>
        private Boolean CalP1toP2Theda(Dot P1, Dot P2, ref double theda)
        {
            try
            {
                int Q = DiscQuad(P1, P2);
                double _theda = 0;
                CalTheda(P1, P2, ref _theda);

                // to360
                theda = Quadto360(Q, _theda);

            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 以P1為中心判別P2象限
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <returns></returns>
        private int DiscQuad(Dot P1, Dot P2)
        {
            int Q = 0;
            // 判斷以p1為中心p2象限
            double _X = P2.pX - P1.pX;
            double _Y = P2.pY - P1.pY;
            if (_X > 0 && _Y > 0) Q = 1;
            else if (_X < 0 && _Y > 0) Q = 2;
            else if (_X < 0 && _Y < 0) Q = 3;
            else if (_X > 0 && _Y < 0) Q = 4;
            return Q;
        }
        private double Quadto360(int Q, double theda)
        {
            switch (Q)
            {
                case 1:
                    return theda;
                    break;
                case 2:
                    return 180 - theda;
                    break;
                case 3:
                    return 180 + theda;
                    break;
                case 4:
                    return 360 - theda;
                    break;
            }
            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="dis"></param>
        /// <returns></returns>
        private double CalP1toP2dis(Dot P1, Dot P2)
        {
            double sum = 0;
            try
            {
                double theda = 0;
                double dx = System.Math.Abs(P1.pX - P2.pX);
                double dy = System.Math.Abs(P1.pY - P2.pY);
                sum = dx * dx + dy * dy;
            }
            catch (Exception ee)
            {

            }
            return (System.Math.Sqrt(sum));
        }

        private Boolean Compensate(GUI.WorkingList.c_WLSettingBuild.stru_WL_LayerSet data, SDrawingDistortion ori, ref SDrawingDistortion ret)
        {
            try
            {
                ret.ScaleX = ori.ScaleX * data.ScaleOffset_X;
                ret.ScaleY = ori.ScaleY * data.ScaleOffset_Y;
                ret.X_offset = ori.X_offset + data.PosOffset_X;
                ret.Y_offset = ori.Y_offset + data.PosOffset_Y;
                ret.theda = ori.theda + data.DegreeOffset;
            }
            catch (Exception ee)
            {
                return false;
            }
            return true;
        }
        #endregion


    }
}

namespace BackLight.Run.Define
{
    enum SAutoRunState
    {
        idle,
        Start,
        SetupPrj,
        GoNextPrj,
        GoNextLayer,
        chkPosMark,
        SetupWorkingH,
        FindMarkingLayer,
        SetupLayer,
        StartMarking,
        Finish,
        alarm,
        cancel,
        hold,
        reStart,
        emgStop,
    }
    struct SMarkingFlag
    {
        public int isMiddleStart;
        public int StartLayer;
        public int StartBlock;
        public Boolean isLayerMarking;
        public Boolean isMarking;
    }
    struct SMarkingRecord
    {
        public string MarkingStartTime;
        public string MarkingEndTime;
        public string MarkingPassingTime;
    }

    struct SGlobalMarkingPara   // prj在WL中的資料
    {
        public int CurrPrjIndex;                // 0 ~ n
        public int CurrMarkingLayer;            // 0 ~ n
        public double isPosCompensation;
        public double XOffset;
        public double YOffset;
        public double Degree;
        public double XScale;
        public double YScale;
        public double RoteCenterX;
        public double RoteCenterY;
        public GUI.Project.c_PrjSettingBuild.stru_PrjAllSetting MarkingDB_Prj;
        public double WorkingHomeX;
        public double WorkingHomeY;
        public double Mark1PosX;
        public double Mark1PosY;
        public double Mark2PosX;
        public double Mark2PosY;
        public double markDir;
    }


    struct SDrawingDistortion     /// 對位算出來
    {
        public double theda;    // 度
        public double ScaleX;
        public double ScaleY;
        public double CenterX;
        public double CenterY;

        public double X_offset;   // 因CG不是正確的故須補償
        public double Y_offset;   // 因CG不是正確的故須補償
    }
    struct SHoldCancelFlag
    {
        public SAutoRunState HoldARState;
        public Boolean Cancel;
        public Boolean Hold;
        public int Hold_PrjIndex;
        public int Hold_Layer;
        public int Hold_Block;
    }
    struct Dot
    {
        public double pX;
        public double pY;
    }
    struct TwinDot
    {
        public double Xp1;
        public double Xp2;
        public double Yp1;
        public double Yp2;
    }
    struct MulitDot
    {
        public List<Dot> dot;
    }
    struct SLaserPara
    {
        //task: 功率百分比
        public double Power;

        //task: freq(KHz)
        public double Freq;

        //HOLMES_20150625_新增雷射焦距的EPulse
        //task: eprf(KHz)
        public double EPulse;
        //HOLMES_20150625_新增雷射焦距的EPulse

        //task: 脈衝寬度(usec)
        public double PulseWidth;

        public int PWUS;      //amount under 1 us, 1~999 means 0.001~0.999

        //task: 雕刻次數
        public double ObjMarkTimes;

        //task: 點雕刻時間(usec)
        public int SpotDelay;

        //task: Mark Delay(usec)
        public int OnDelay;//有正負
        public int MiddleDelay;
        public int EndDelay;

        //task: Mark Speed(mm/s)
        public double MarkSpeed;

        //task: Jump Speed(mm/s)
        public double JumpSpeed;

        //task: Jump Delay(usec)
        public int JumpDelay;
    }
    struct SMarkingPara
    {
        //要從第幾個division 開始雕刻, 0 based
        public int DivIndexToStart;

        //位置調整 offset, 是代表要調整的量, 用加的
        //角度設定多少度就轉多少度
        public bool NeedBiasCompensation;
        public double XOffset;
        public double YOffset;
        public double RotDegree;
        //旋轉中心(圖面座標)
        public double RotCenterX;
        public double RotCenterY;

        //scale factor
        //real size = designed size * scale Factor
        public double XScaleFactor;
        public double YScaleFactor;

        //MarkingDB for marking
        //TMarkingDBPtr MarkingDB;

        public int LayerSplitDir;  //0: 水平; 1: 垂直; 其他保留
        public double CurveSagitta;

        //task: Division Overlap Size
        public double DivisionOverlapSize;


    }
    struct SLayerReMarkInfo
    {
        public int CurrMarkIndex;   // 0 ~ n
        public List<double> RedowTimes; //Prj 中的 LayerMarkTimes
        public List<double> RedowHeigh;

    }

    struct SchkNGValue
    {
        public bool isNGlimit;
        public double ScaleX;
        public double ScaleY;
    }
    public enum Markstate
    {
        Marking = 0,
        Pause = 1,
        Defaul = 3,
        Alarm = 4,
    }
}

namespace BackLight.Run.ErrCode
{
    class Num   // -1001 ~ -1100
    {
        public static int _Link = -1001;
        public static int _CalDistortion = -1002;
        public static int _LoadMarkingPrjMarkingPara = -1003;
        public static int _GetPrjRedownInfo = -1004;
        public static int _MarkingTime = -1005;

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
            string pat = Application.StartupPath + "\\Log\\AutoRun_Log.log";
            StreamWriter strr = new StreamWriter(pat, true);
            strr.WriteLine(NGString, false);
            strr.Close();

        }


    }
}
