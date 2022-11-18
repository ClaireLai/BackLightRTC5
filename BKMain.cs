using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BackLight.File;
using BackLight.File.Define;
//using BackLight.CCD.Picolo;
using BackLight.CCD.Define;
using BackLight.Define;
using BackLight.Laser;
using BackLight.Laser.Define;
//using BackLight.Laser.SP_UV;
using static BackLight.Axis.A3200.Axis_A3200;
using System.Threading;
using BackLight.Axis;
using System.Runtime.InteropServices;
using RTC5Import;

namespace BackLight
{

    public partial class frmBKMain : Form
    {
        // [System.Runtime.InteropServices.DllImport("kernel32")]
        // static extern uint GetPrivateProfileString(
        //string lpAppName,
        //string lpKeyName,
        //string lpDefault,
        //StringBuilder lpReturnedString,
        //uint nSize,
        //string lpFileName);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
            string section,
            string key,
            string def,
            StringBuilder retVal,
            int size,
            string filePath);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(
            string section,
            string key,
            string val,
            string filePath);
        //[System.Runtime.InteropServices.DllImport("kernel32")]

        //private static extern Int32 GetPrivateProfileString(string lpApplicationName,
        //    string lpKeyName, 
        //    string lpdefault, 
        //    string lpretrunedstring, 
        //    Int32 nSize, string lpFilename);
        //*******************Axis****************************

        Axis.IAxis cAxis = new Axis.A3200.Axis_A3200();
        Axis.ErrCode.RetErr ARetErr;
        Mctl_Mode cMctl_Mode = Mctl_Mode.Line;
        double Mctl_value = 0.001;
        double dblXYStageSpeed = 100;
        double dblXYAcc = 10;
        double dblZStageSpeed = 20;
        double dblZAcc = 5;
        //Mctl_ReferPos cMctl_ReferPos = Mctl_ReferPos.Galvo;

        //*******************CCD****************************

        CCD.ErrCode.RetErr CRetErr;
        //BackLight.CCD.ICCD cCCD = new BackLight.CCD.Picolo.CCD_Picolo;
        BackLight.CCD.ICCD cCCD;
        Show_Img show_Img = new Show_Img();
        s_MarkPara markPara = new s_MarkPara();
        PointF point = new PointF();
        string datFileName;

        // Lens Calibration ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        double CalibrationOriginX;
        double CalibrationOriginY;

        PointF[,] LensCalibrationPosition;
        //List<PointF>[,] subLensCalibrationPosition;

        PointF[,] LensTheoreticalPosition;
        //List<double> subLensTheoreticalPositionX = new List<double>();

        bool IsAutoCalibrating;
        bool DisposeMM;
        double MatrixStep;
        string GoldenSamplePath;
        //CheckBox[] chk1 = new CheckBox[] { };
        int MatrixSize;
        int beforeMatrixSize=0;
        int MatrixRow;
        int MatrixColumn;
        ListView view;
        //*******************Laser***************************

        BackLight.Laser.ErrCode.RetErr LRetErr;
        BackLight.Laser.ILaser cLaser = new Laser_CoherentMatrix();
        BackLight.Laser.Define.LaserPara laserPara = new LaserPara();
        // CoorLaserFocus
        //bool isCoorLaserFocusStopAct = false;
        List<PointF> CoorLaserFocusArray;
        //int CoorLFocusCenterValue = 4;
        List<Image> images = new List<Image>();
        List<double> CoorLFocusZPos = new List<double>();
        int picNo = 0;
        string CoorFocusCurRadio = "RadioB_CoorLaserFocusLB";
        //Aliment_Laser laser = new Aliment_Laser();

        //*******************RTC5***************************
        bool IsTransformed = false;
        int Columns = 0;
        int Rows = 0;
        double MinX = 0;
        double MinY = 0;
        int K = 615;//校正係數
        string strCalibrationFilePath;


        //***************total setting***********************
        File_SetFile cFilsProcese = new File.File_SetFile();
        ProjPara projPara = new ProjPara();
        string strIniFilePath;
        string fileName;
        double CCDXOffset, CCDYOffset;
        double LaserFouseLength, CCDPosition;//Laser高度, CCD預設高度
        double ZPositionTemp, CCDPositionTemp;//物品高度修正
        double LaserHightTemp;
        bool bolcheckHardware = false;
        bool bolLaserStop = false;



        public frmBKMain()
        {
            InitializeComponent();
        }
        private void BKMain_Load(object sender, EventArgs e)
        {
            //載入所有組態設定          
            File.ErrCode.RetErr rete = cFilsProcese.GetAllSetData();
            if (!rete.flag)
            {
                MessageBox.Show("Load .ini fail \r\n" + rete.Meg,
                                "Warnning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
#if (ErrClose)
                this.Close();
#endif
            }
            File.Define.CCDSetup ccdsetup = cFilsProcese.GetCCDSetup();
            LoadCommonPara();

            //載入影像 && 比對參數設定         *************************************************   

            if (bolcheckHardware)
            {
                cCCD = new BackLight.CCD.Picolo.CCD_Picolo();
                CRetErr = cCCD.GetGaugePara(out CCD.Define.s_MarkPara s_MarkPara);
                CRetErr = cCCD.GetShowStatus(out show_Img);
                show_Img.ptmm = ccdsetup._Pix2mm.Cal_X;//將Pix2mm傳到 show_Img.ptmm 可以修正畫面尺規
                CRetErr = cCCD.cam_init(CCD.VID.VID1, ref Panel_CCD_cal);
                cCCD.cam_StartGetImage();
                cCCD.CCDMouseMoveInfoRecv += CCCD_CCDMouseMoveInfoRecv;
            }

            //軸OPEN                           *************************************************              
            if (bolcheckHardware)
            {
                cAxis.Open();
                //cAxis.AxesInfoRecv += CAxis_AxesInfoRecv;
                cAxis.AxesInfoRecv += CAxis_AxesInfoRecv;
                cAxis.FaultOccur += CAxis_FaultOccur;
            }

            //載入RTC5                          *************************************************
            uint ErrorCode;
            if (bolcheckHardware)
            {
                ErrorCode = RTC5Wrap.init_rtc5_dll();//確定dll版本            
                if (ErrorCode != 0U && ErrorCode != 2U)
                {
                    Console.WriteLine(
                      "Error happened during RTC5's DLL initialization.\n\n");
                    Console.Read();
                    return;
                }

                RTC5Wrap.set_rtc4_mode();
                if (RTC5Wrap.select_rtc(1U) != 1U)
                {
                    Console.WriteLine(
                        "Error: Failed to select the 1st RTC5.\n\n");
                    Console.Read();
                    return;
                }

                ErrorCode = RTC5Wrap.load_correction_file(
                    "Cor_1to1.ct5",
                    1U,         // number
                    2U);        // dimension
                if (ErrorCode != 0)
                {
                    Console.WriteLine("Error: Correction file loading.\n\n");
                    Console.Read();
                    return;
                }
                string strtemp = Application.StartupPath;
                ErrorCode = RTC5Wrap.load_program_file(strtemp);//Claire
                if (ErrorCode != 0)
                {
                    Console.WriteLine("Program file loading err" +
                        "" +
                        "or: {0:D}\n\n",
                        ErrorCode);
                    Console.Read();
                    return;
                }

                RTC5Wrap.config_list(4000U, 4000U);
                RTC5Wrap.set_laser_mode(1U);      // YAG mode 1 selected
                RTC5Wrap.set_laser_control(0U);
                // Turn on the optical pump source and wait for 2 seconds.
                // (The following assumes that signal ANALOG OUT1 of the
                // laser connector controls the pump source.)
                RTC5Wrap.write_da_1(640U);
                // Timing, delay and speed preset.
                // Transmit the following list commands to the list buffer.
                RTC5Wrap.set_start_list(1U);
                // Wait for 1 seconds
                RTC5Wrap.long_delay(100000U);
                RTC5Wrap.set_laser_pulses(
                    40U,    // half of the laser signal period.
                    400U);  // pulse widths of signal LASER1.
                RTC5Wrap.set_scanner_delays(
                    25U,    // jump delay, in 10 microseconds.
                    10U,    // mark delay, in 10 microseconds.
                    5U);    // polygon delay, in 10 microseconds.
                RTC5Wrap.set_laser_delays(
                    100,    // laser on delay, in microseconds.w96
                    100);   // laser off delay, in microseconds.
                            // jump speed in bits per milliseconds.
                RTC5Wrap.set_jump_speed(1000.0);
                // marking speed in bits per milliseconds.
                RTC5Wrap.set_mark_speed(250.0);
                RTC5Wrap.set_end_of_list();
                RTC5Wrap.execute_list(1U);

                Console.WriteLine("Pump source warming up - please wait.\r");
                uint Busy, Position;
                do
                {
                    RTC5Wrap.get_status(out Busy, out Position);
                } while (Busy != 0U);

                Console.WriteLine("Plotting started.");
            }
        }

        private void CCCD_CCDMouseMoveInfoRecv(PointF point)
        {
            UpdateUI(point.X.ToString(), lblImg_x);
            UpdateUI(point.Y.ToString(), lblImg_y);
        }

        private void CAxis_FaultOccur(object args)
        {
            throw new NotImplementedException("Axis Error!!");
        }

        private void CAxis_AxesInfoRecv(Axis.AxesInfoCallbackArgs[] args)
        {
            if (!args[(int)Axis.AxisName.X].Fault) UpdateUI(args[(int)Axis.AxisName.X].Position.ToString(), Label_Xpos);
            if (!args[(int)Axis.AxisName.Y].Fault) UpdateUI(args[(int)Axis.AxisName.Y].Position.ToString(), Label_Ypos);
            if (!args[(int)Axis.AxisName.Z].Fault) UpdateUI(args[(int)Axis.AxisName.Z].Position.ToString(), Label_Zpos);
        }

        private delegate void UpdateUICallBack(String newText, Control c);
        private void UpdateUI(String newText, Control c)
        {
            //判斷這個TextBox的物件是否在同一個執行緒上
            try
            {
                if (c.InvokeRequired)
                {
                    //當InvokeRequired為true時，表示在不同的執行緒上，所以進行委派的動作!!
                    UpdateUICallBack cb = new UpdateUICallBack(UpdateUI);
                    c.Invoke(cb, newText, c);
                }
                else
                    //表示在同一個執行緒上了，所以可以正常的呼叫到這個TextBox物件
                    c.Text = newText;
            }
            catch (Exception ee)
            { }
        }

        private void bt_CW_MouseDown(object sender, MouseEventArgs e)
        {
            //File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();
            Button btn = (Button)sender;
            AxisName axis;

            //判斷使用者點選哪一軸
            if (btn.Name == Bt_Mctl_MoveXp.Name)
            {
                axis = AxisName.X;
            }
            else if (btn.Name == Bt_Mctl_MoveYp.Name)
            {
                axis = AxisName.Y;
            }
            else if (btn.Name == Bt_Mctl_MoveZp.Name)
            {
                axis = AxisName.Z;
            }
            else
                throw new Exception("The axis not exist");

            //判斷移動模式
            if (cMctl_Mode == Mctl_Mode.Jog)
            {
                cAxis.FreeRun(axis, Mctl_value);
            }
            else
            {
                cAxis.Jog(axis, Mctl_value, dblXYStageSpeed);
            }
        }

        private void bt_CW_MouseUp(object sender, MouseEventArgs e)
        {

            //File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();
            Button btn = (Button)sender;
            if (cMctl_Mode == Mctl_Mode.Line)  //判斷是否為連動模式
            {

                //判斷哪一軸
                if (btn.Name == Bt_Mctl_MoveXp.Name)
                    cAxis.FreeRunStop(AxisName.X);
                else if (btn.Name == Bt_Mctl_MoveYp.Name)
                    cAxis.FreeRunStop(AxisName.Y);
                else if (btn.Name == Bt_Mctl_MoveZp.Name)
                    cAxis.FreeRunStop(AxisName.Z);
                else
                    throw new Exception("The axis not exist");
            }
        }

        private void bt_CCW_MouseDown(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            AxisName axis;
            //File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();

            //判斷使用者點選哪一軸
            if (btn.Name == Bt_Mctl_MoveXn.Name)
            {
                axis = AxisName.X;
            }
            else if (btn.Name == Bt_Mctl_MoveYn.Name)
            {
                axis = AxisName.Y;
            }
            else if (btn.Name == Bt_Mctl_MoveZn.Name)
            {
                axis = AxisName.Z;
            }
            else
                throw new Exception("The axis not exist");

            //判斷移動模式
            if (cMctl_Mode == Mctl_Mode.Line)
            {
                cAxis.FreeRun(axis, -Mctl_value);
            }
            else
            {
                cAxis.Jog(axis, Mctl_value * -1, dblXYStageSpeed);
            }
        }
        private void bt_CCW_MouseUp(object sender, MouseEventArgs e)
        {

            //File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();
            Button btn = (Button)sender;
            if (cMctl_Mode == Mctl_Mode.Line)  //判斷是否為連動模式
            {
                //判斷哪一軸
                if (btn.Name == Bt_Mctl_MoveXn.Name)
                    cAxis.FreeRunStop(AxisName.X);
                else if (btn.Name == Bt_Mctl_MoveYn.Name)
                    cAxis.FreeRunStop(AxisName.Y);
                else if (btn.Name == Bt_Mctl_MoveZn.Name)
                    cAxis.FreeRunStop(AxisName.Z);
                else
                    throw new Exception("The axis not exist");
            }
        }
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked == true)
                cMctl_Mode = Mctl_Mode.Jog;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked == true)
                cMctl_Mode = Mctl_Mode.Line;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked == true)
                cMctl_Mode = Mctl_Mode.View;
        }

        private void Panel_CCD_cal_MouseDown(object sender, MouseEventArgs e)
        {
            if (cMctl_Mode == Mctl_Mode.View)
            {
                File.Define.CCDSetup ccdsetup = cFilsProcese.GetCCDSetup();
                double XMoveOffset = ((320 - (e.X)) * ccdsetup._Pix2mm.Cal_X / 2) / 1000;
                double YMoveOffset = (((e.Y) - 240) * ccdsetup._Pix2mm.Cal_Y / 2) / 1000;
                //File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();
                ARetErr = cAxis.XYLine(XMoveOffset, YMoveOffset, dblXYStageSpeed, dblXYAcc);
                if (!ARetErr.flag)
                {
                    MessageBox.Show("AxisMove FreeRun fail \r\n" + ARetErr.Meg, "Warnning",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void TextB_MctlValue_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(TextB_MctlValue.Text, out double value))
                Mctl_value = value;
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            radioButton12.BackColor = radioButton12.Checked ? SystemColors.ControlDark : SystemColors.Control;
            Mctl_value = Convert.ToDouble(TextB_MctlValue.Text);
        }

        private void radioButton15_CheckedChanged(object sender, EventArgs e)
        {
            radioButton15.BackColor = radioButton15.Checked ? SystemColors.ControlDark : SystemColors.Control;

            Mctl_value = 0.001;
        }

        private void radioButton16_CheckedChanged(object sender, EventArgs e)
        {
            radioButton16.BackColor = radioButton16.Checked ? SystemColors.ControlDark : SystemColors.Control;

            Mctl_value = 0.01;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            radioButton3.BackColor = radioButton3.Checked ? SystemColors.ControlDark : SystemColors.Control;

            Mctl_value = 0.1;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            radioButton6.BackColor = radioButton6.Checked ? SystemColors.ControlDark : SystemColors.Control;

            Mctl_value = 1;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.BackColor = radioButton1.Checked ? SystemColors.ControlDark : SystemColors.Control;

            Mctl_value = 10;
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            radioButton11.BackColor = radioButton11.Checked ? SystemColors.ControlDark : SystemColors.Control;

            Mctl_value = 50;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton2.BackColor = radioButton2.Checked ? SystemColors.ControlDark : SystemColors.Control;

            Mctl_value = 100;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();

            double _X = Convert.ToDouble(TextB_MctlXpos.Text);
            double _Y = Convert.ToDouble(TextB_MctlYPos.Text);


            Axis.ErrCode.RetErr ret = cAxis.XYLine(_X, _Y, dblXYStageSpeed, dblXYAcc);
            if (!ret.flag)
            {
                MessageBox.Show("Axis XY Move to fail \r\n" + ret.Meg, "Warnning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();

            double Pos = Convert.ToDouble(TextB_MctlZPos.Text);


            Axis.ErrCode.RetErr ret = cAxis.ZMove(Pos, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            if (!ret.flag)
            {
                MessageBox.Show("Axis Z to Thick heigh fail \r\n" + ret.Meg, "Warnning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }
        }

        private void button38_Click(object sender, EventArgs e)
        {
            File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();

            Axis.ErrCode.RetErr ret = cAxis.ZMove(basic.ZSafePos, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            if (!ret.flag)
            {
                MessageBox.Show("Axis Z to SafePos fail \r\n" + ret.Meg, "Warnning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            cAxis.ZMove(CCDPositionTemp, dblZStageSpeed , dblZAcc );
        }
        private void MegShow(StatusLevel level, string Meg)
        {
            string title = "";
            if (level == StatusLevel.defaul) title = "<<提示>> ";
            else if (level == StatusLevel.danger) title = "<<嚴重警告>> ";
            else if (level == StatusLevel.warmming) title = "<<警告>> ";

            this.Label_ProgStatus_text.Text = title + Meg;

            // 啟動顯示計時器.
            //Timer_StatusRefresh.Enabled = true;
        }

        private void MoveToOut_Click(object sender, EventArgs e)
        {
            File.Define.BasicMotion Bac_file = cFilsProcese.GetBasicMotion();

            Axis.ErrCode.RetErr ret;

            ret = cAxis.ZMove(Bac_file.ZSafePos,
                                Bac_file.ManualZMoveSpeed,
                                Bac_file.ZMoveAcc);
            if (!ret.flag)
            {
                MessageBox.Show("AxisMove Z safePos fail \r\n" + ret.Meg, "Warnning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }
            ret = cAxis.XYLine(Bac_file.JigOutputPosX,
                                Bac_file.JigOutputPosY,
                                Bac_file.ManualXYLineSpeed,
                                Bac_file.XYLineAcc);
            if (!ret.flag)
            {
                MessageBox.Show("AxisMove Jig fail \r\n" + ret.Meg, "Warnning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnGoLasetHigh_Click(object sender, EventArgs e)
        {          
            ZPositionTemp = LaserFouseLength - Convert.ToDouble(TextB_MctlThickness.Text);
            cAxis.ZMove(ZPositionTemp, dblZStageSpeed , dblZAcc );
        }

        void motionDone(Axis.ErrCode.RetErr ret, AxesExecuteCallbackArgs exeArgs)
        {
            if (exeArgs.Operation == AxesExecuteCallbackArgs.OperationType.HOME_ALL)
            {
                if (!ret.flag)
                {
                    MessageBox.Show("AxisHome fail \r\n" + ret.Meg, "Warnning",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void butHome_Click(object sender, EventArgs e)
        {
            new Thread(() => cAxis.HomeAsync(motionDone)).Start();

            ////Axis.ErrCode.RetErr ret = cAxis.HomeAsync();
            //if (!ret.flag)
            //{
            //    MessageBox.Show("AxisHome fail \r\n" + ret.Meg, "Warnning",
            //                    MessageBoxButtons.OK,
            //                    MessageBoxIcon.Warning);
            //    return;
            //}
        }
        private void button18_Click(object sender, EventArgs e)
        {
            cAxis.ResetAlarm();
        }


        //private void Bt_WorkingWorkFindMark_Click(object sender, EventArgs e)
        //{
        //    #region find mark

        //    //Software.CCD.CCD_Method cMethod_CCD = new Software.CCD.CCD_Method();
        //    //Software.CCD.ErrCode.RetErr ret_CCD = cMethod_CCD.init(cAxis, cCCD_pos, cMethod_WL, cFilsProcese);
        //    //if (!ret_CCD.flag)
        //    //{
        //    //    MessageBox.Show("CCD Method Init fail \r\n" + ret_CCD.Meg, "Warnning",
        //    //                  MessageBoxButtons.OK,
        //    //                  MessageBoxIcon.Warning);
        //    //    return;
        //    //}

        //    // 取得設定的靶標資訊
        //    cCCD.GetGaugePara(out CCD.Define.s_MarkPara Para);

        //    string path = Application.StartupPath + "\\WorkDir\\Correct_cal.jpg";
        //    // 更新CCD方法中的靶標資訊
        //    //cCCD.SetVisionPara(thod.Correct_type.pos, Para);
        //    //找靶標
        //    //ret_CCD = cCCD.Correct_FindMark(Software.CCD.CCD_Method.Correct_type.pos, out bool isFind);
        //    CRetErr = cCCD.FindMark(path, Para, out bool isFind, out PointF point);
        //    if (!CRetErr.flag)
        //    {
        //        MessageBox.Show("CCD Method Find mark fail \r\n" + CRetErr.Meg, "Warnning",
        //                      MessageBoxButtons.OK,
        //                      MessageBoxIcon.Warning);
        //        return;
        //    }
        //    if (!isFind)
        //    {
        //        MessageBox.Show("Not Find Mark !\r\n", "Warnning",
        //                      MessageBoxButtons.OK,
        //                      MessageBoxIcon.Warning);
        //        return;
        //    }

        //    #endregion
        //}


        private void btn_LoadImgFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "*.*|*.txt;*.dxf;*.bmp|TXT File(*.txt)|*.txt|DXF File(*.dxf)|*.dxf|BMP File(*.bmp)|*.bmp";
            openFileDialog1.ShowDialog();
            string filepath = openFileDialog1.FileName;
            if (!System.IO.File.Exists(filepath))
            {
                MessageBox.Show("file path not found!");
                return;
            }
            //讀出project 檔
            txtProj.Text = filepath;
            fileName = Path.GetFileNameWithoutExtension(filepath);
            //設定proj檔名的路徑
            strIniFilePath =  "D:\\workdir\\" + fileName;
            //如果檔案之路徑不存在, 建立一個新路徑....OK
            if (!System.IO.Directory.Exists(strIniFilePath))
            {
                Directory.CreateDirectory(strIniFilePath);
            }

            if (System.IO.File.Exists("D:\\workdir\\param.ini"))
            {
                System.IO.File.Delete("D:\\workdir\\param.ini");
            }
            //存入param.ini檔案
            StreamWriter strr = new StreamWriter("D:\\workdir\\param.ini");
            strr.WriteLine(filepath);
            strr.Close();

            //建立專案設定檔
            if (!System.IO.File.Exists(strIniFilePath + "\\Setting.ini"))
            {
                //System.IO.File.Create(strIniFilePath + "\\Setting.ini");
                System.IO.File.Copy("D:\\workdir\\DefaultSetting.ini", strIniFilePath + "\\Setting.ini");
            }

            ShowSetting(strIniFilePath, "Setting.ini");


            //    '===========================================================
            //    '檢查是否已分割過檔案
            IsTransformed = false;
            btnStartMarking.Enabled = false ;
            string str = strIniFilePath + "\\" + fileName + ".res";
            if (System.IO.File.Exists(str))
            {
                IsTransformed = true;
                btnStartMarking.Enabled = true;
                StreamReader @in = new StreamReader(str);

                //Columns     
                string strTemp = @in.ReadLine();
                if (strTemp == null)
                {
                    @in.Close();
                    //Res File 無資料            
                }
                else
                {
                    Columns = Convert.ToInt32(strTemp);
                    //Rows
                    strTemp = @in.ReadLine();
                    Rows = Convert.ToInt32(strTemp);
                    //Path
                    strTemp = @in.ReadLine();
                    //Points
                    strTemp = @in.ReadLine();
                    Console.Write("點數");
                    Console.Write(strTemp);
                    Console.Write("\n");
                    //Max XY coordinate
                    strTemp = @in.ReadLine();
                    //Min XY coordinate
                    strTemp = @in.ReadLine();
                    MinX = Convert.ToDouble(strTemp.Substring(0, strTemp.IndexOf(" ") - 1));//claire 確定要不要加 -
                    MinY = Convert.ToDouble(strTemp.Substring(strTemp.IndexOf(" ") + 1));
                    //MinY = strTemp.SubString(strTemp.AnsiPos(" ") + 1, strTemp.Length).ToDouble();
                    @in.Close();
                }
            }

            RTC5Setting();
            SetLaser();//套用雷射參數

        }
        public void ShowSetting(string Path, string filename)
        {
            string strValue;
            string filepath = Path + "\\" + filename;
            //    '0 StandbyFrequency | StandbyPulseWidth | 頻率 | 脈寬 | 首發抑制 | LaserTimeBase |
            //    '6 JumpDelay | MarkDelay | PolygonDelay | LaserOn 延遲 | LaserOff 延遲 |
            //    '11 位移速度 | MarkSpeed | 功率 | 單點加工時間 |
            //    '15 鏡頭焦距 | 排序方式 | BlockSize | 是否使用模糊化 | 模糊化比例 | 振鏡加工範圍W | 振鏡加工範圍H | 鋼板長度 | 偏移量X | 偏移量Y
            //    '25 鋼板厚度 | 工作原點X | 工作原點Y
            //    '28 DPI | 對位模式            
            //    '載入專案設定檔
            StringBuilder strIni1 = new StringBuilder("", 11);
            StringBuilder strIni2 = new StringBuilder("", 50);
            GetPrivateProfileString("Laser", "StandbyFrequency", "", strIni1, 11, filepath);
            //txtFrequency.Text = strIni1.ToString().Trim();
            projPara.StandbyFrequency = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "StandbyPulseWidth", "", strIni1, 11, filepath);
            projPara.StandbyPulseWidth = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "LaserFrequency", "", strIni1, 11, filepath);
            txtFrequency.Text = strIni1.ToString();
            txtLaserFreq.Text = strIni1.ToString();
            TextB_CoorInfoFreq.Text = strIni1.ToString();
            projPara.LaserFrequency = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "Epulse", "", strIni1, 11, filepath);
            txtLaserEpulse.Text = strIni1.ToString();
            txtEpulse.Text = strIni1.ToString();
            TextB_CoorInfoEpulse.Text= strIni1.ToString(); ;
            projPara.Epulse = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "Power", "", strIni1, 11, filepath);
            txtLaserPower.Text = strIni1.ToString();
            TextB_CoorInfoLaserPower.Text = strIni1.ToString();
            txtPower.Text = strIni1.ToString();
            projPara.Power = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "Mode", "", strIni1, 11, filepath);
            txtLaserMode.Text = strIni1.ToString();
            txtMode.Text = strIni1.ToString();
            TextB_CoorInfoMode.Text= strIni1.ToString();
            projPara.Mode = (int)Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "LaserPulseWidth", "", strIni1, 11, filepath);
            txtLaserPulseWidth.Text = strIni1.ToString();
            projPara.LaserPulseWidth = Convert.ToDouble(strIni1.ToString().Trim());
            TextB_CoorInfoPulseWidth.Text = strIni1.ToString(); 
            GetPrivateProfileString("Laser", "LaserFPS", "", strIni1, 11, filepath);
            txtLaserFPS.Text = strIni1.ToString();
            projPara.LaserFPS = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "LaserTimeBase", "", strIni1, 11, filepath);
            projPara.LaserTimeBase = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "JumpDelay", "", strIni1, 11, filepath);
            txtJumpDelay.Text = strIni1.ToString();
            TextB_CoorInfoJumpDelay.Text = strIni1.ToString();
            projPara.JumpDelay = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "MarkDelay", "", strIni1, 11, filepath);

            projPara.MarkDelay = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "PolygonDelay", "", strIni1, 11, filepath);

            projPara.PolygonDelay = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "LaserOnDelay", "", strIni1, 11, filepath);
            txtLaserOnDelay.Text = strIni1.ToString();
            TextB_CoorInfoOnDelay.Text = strIni1.ToString();
            projPara.LaserOnDelay = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "LaserOffDelay", "", strIni1, 11, filepath);
            TextB_CoorInfoEndDelay.Text = strIni1.ToString();
            txtLaserOffDelay.Text = strIni1.ToString();
            projPara.LaserOffDelay = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "JumpSpeed", "", strIni1, 11, filepath);
            txtJumpSpeed.Text = strIni1.ToString();
            projPara.JumpSpeed = Convert.ToDouble(strIni1.ToString().Trim());
            TextB_CoorInfoJumpSpeed.Text = strIni1.ToString(); 
            GetPrivateProfileString("Laser", "MarkSpeed", "", strIni1, 11, filepath);
            TextB_CoorInfoMarkSpeed.Text = strIni1.ToString(); 
            projPara.MarkSpeed = Convert.ToDouble(strIni1.ToString().Trim());
            //GetPrivateProfileString("Laser", "LaserDuty", "", strIni1, 11, filepath);
            //txtPower.Text = strIni1.ToString();
            //projPara.LaserDuty = Convert.ToDouble(strIni1.ToString().Trim());
            GetPrivateProfileString("Laser", "OneShot", "", strIni1, 11, filepath);
            txtOneShot.Text = strIni1.ToString();
            TextB_CoorInfoSpotDelay.Text = strIni1.ToString();
            projPara.OneShot = Convert.ToDouble(strIni1.ToString().Trim());

            GetPrivateProfileString("Vision", "FocusLength", "", strIni1, 11, filepath);
            projPara.FocusLength = Convert.ToDouble(strIni1.ToString().Trim());

            GetPrivateProfileString("Project", "SortType", "", strIni1, 11, filepath);
            projPara.SortType = Convert.ToDouble(strIni1.ToString().Trim());
            switch ((int)projPara.SortType)
            {
                case 1:
                    rdoSortLR.Checked = true;
                    break;
                case 2:
                    rdoSortLRRL.Checked = true;
                    break;
                case 3:
                    rdoSortDU.Checked = true;
                    break;
                case 4:
                    rdoSortDUUD.Checked = true;
                    break;
                case 5:
                    rdoSortCir.Checked = true;
                    break;
            }

            GetPrivateProfileString("Project", "SortParam", "", strIni1, 11, filepath);
            projPara.SortParam = Convert.ToDouble(strIni1.ToString().Trim());
            txtSortParam.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "FuzzyType", "", strIni1, 11, filepath);
            projPara.FuzzyType = Convert.ToDouble(strIni1.ToString().Trim());
            if (projPara.FuzzyType == 3)
                chkFuzzyType.Checked = true;
            else
                chkFuzzyType.Checked = false;
            GetPrivateProfileString("Project", "FuzzyParam", "", strIni1, 11, filepath);
            projPara.FuzzyParam = Convert.ToDouble(strIni1.ToString().Trim());
            txtFuzzyParam.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "FieldSizeW", "", strIni1, 11, filepath);
            projPara.FieldSizeW = Convert.ToDouble(strIni1.ToString().Trim());
            txtFieldSizeW.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "FieldSizeH", "", strIni1, 11, filepath);
            projPara.FieldSizeH = Convert.ToDouble(strIni1.ToString().Trim());
            txtFieldSizeH.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "Length", "", strIni1, 11, filepath);
            projPara.Length = Convert.ToDouble(strIni1.ToString().Trim());
            txtLength.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "OffsetX", "", strIni1, 11, filepath);
            projPara.OffsetX = Convert.ToDouble(strIni1.ToString().Trim());
            txtOffsetX.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "OffsetY", "", strIni1, 11, filepath);
            projPara.OffsetY = Convert.ToDouble(strIni1.ToString().Trim());
            txtOffsetY.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "Thickness", "", strIni1, 11, filepath);
            projPara.Thickness = Convert.ToDouble(strIni1.ToString().Trim());
            txtThickness.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "OriginalX", "", strIni1, 11, filepath);
            projPara.OriginalX = Convert.ToDouble(strIni1.ToString().Trim());
            txtOriginalX.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "OriginalY", "", strIni1, 11, filepath);
            projPara.OriginalY = Convert.ToDouble(strIni1.ToString().Trim());
            txtOriginalY.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "DPI", "", strIni1, 11, filepath);
            projPara.DPI = Convert.ToDouble(strIni1.ToString().Trim());
            txtDPI.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Project", "LocationMode", "", strIni1, 11, filepath);
            projPara.LocationMode = Convert.ToDouble(strIni1.ToString().Trim());
            switch ((int)projPara.LocationMode)
            {
                case 1:
                    rdoLaser.Checked = true;
                    break;
                case 2:
                    rdoCCD.Checked = true;
                    break;
                case 3:
                    rdoSide.Checked = true;
                    break;
            }
            //filepath = Application.StartupPath + "\\Common_Parameters.ini";
            //GetPrivateProfileString("Common", "PixelPerMM", "", strIni1, 11, filepath);
            //projPara.PixelPerMM = Convert.ToDouble(strIni1.ToString().Trim());
            //txtMM2Pixel.Text = strIni1.ToString().Trim();
            //GetPrivateProfileString("Common", "CCDXoffset", "", strIni1, 11, filepath);
            //projPara.GalvoCCDOffsetX = Convert.ToDouble(strIni1.ToString().Trim());
            //txtGalvoCCDOffsetX.Text = strIni1.ToString().Trim();
            //txtCCDXoffset.Text= strIni1.ToString().Trim();
            //GetPrivateProfileString("Common", "CCDYoffset", "", strIni1, 11, filepath);
            //projPara.GalvoCCDOffsetY = Convert.ToDouble(strIni1.ToString().Trim());
            //txtGalvoCCDOffsetY.Text = strIni1.ToString().Trim();
            //txtCCDYoffset.Text = strIni1.ToString().Trim();
            //GetPrivateProfileString("Common", "CorrectionFile", "", strIni2, 50, filepath);
            //projPara.CorrectionFile = (strIni2.ToString().Trim());
            //txtCorrectionFile.Text = strIni2.ToString().Trim();
            ////GetPrivateProfileString("Common", "ProgramFile", "", strIni2, 50, filepath);
            ////projPara.ProgramFile = strIni2.ToString().Trim();
            ////txtProgramFile.Text = strIni2.ToString().Trim();

            //GetPrivateProfileString("Common", "LaserFocusLength", "", strIni1, 11, filepath);
            //projPara.LaserFocusLength = Convert.ToDouble(strIni1.ToString().Trim());
            //txtLaserFocusLength.Text = strIni1.ToString().Trim();
            //GetPrivateProfileString("Common", "PowerLimit", "", strIni1, 11, filepath);
            //projPara.PowerLimit = Convert.ToDouble(strIni1.ToString().Trim());
            //txtPowerLimit.Text = strIni1.ToString().Trim();
            //GetPrivateProfileString("Common", "dblEdgeOriginalX", "", strIni1, 11, filepath);
            //projPara.dblEdgeOriginalX = Convert.ToDouble(strIni1.ToString().Trim());
            //txtEdgeOriginalX.Text = strIni1.ToString().Trim();
            //GetPrivateProfileString("Common", "dblEdgeOriginalY", "", strIni1, 11, filepath);
            //projPara.dblEdgeOriginalY = Convert.ToDouble(strIni1.ToString().Trim());
            //txtEdgeOriginalY.Text = strIni1.ToString().Trim();

            //GetPrivateProfileString("Common", "K", "", strIni1, 11, Application.StartupPath + "\\Common_Parameters.ini");
            //projPara.K = Convert.ToDouble(strIni1.ToString().Trim());
            //txtFieldCalibration.Text = strIni1.ToString().Trim();
            //RTC5Wrap.load_correction_file(projPara.CorrectionFile,1,2);

        }
        private void btnSaveCommonProjIni_Click(object sender, EventArgs e)
        {
            double dTemp = 0;
            try
            {
                if (rdoSortLR.Checked) projPara.SortType = 1;
                else if (rdoSortLRRL.Checked) projPara.SortType = 2;
                else if (rdoSortDU.Checked) projPara.SortType = 3;
                else if (rdoSortDUUD.Checked) projPara.SortType = 4;
                else projPara.SortType = 5;
                WritePrivateProfileString("Project", "SortType", projPara.SortType.ToString(), strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtSortParam.Text);
                WritePrivateProfileString("Project", "SortParam", txtSortParam.Text, strIniFilePath + "\\Setting.ini");
                projPara.FuzzyType = (chkFuzzyType.Checked) ? 3 : 1;
                WritePrivateProfileString("Project", "FuzzyType", projPara.FuzzyType.ToString(), strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtFuzzyParam.Text);
                projPara.FuzzyParam = dTemp;
                WritePrivateProfileString("Project", "FuzzyParam", txtFuzzyParam.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtFieldSizeW.Text);
                projPara.FieldSizeW = dTemp;
                WritePrivateProfileString("Project", "FieldSizeW", txtFieldSizeW.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtFieldSizeH.Text);
                projPara.FieldSizeH = dTemp;
                WritePrivateProfileString("Project", "FieldSizeH", txtFieldSizeH.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtLength.Text);
                projPara.Length = dTemp;
                WritePrivateProfileString("Project", "Length", txtLength.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtOffsetX.Text);
                projPara.OffsetX = dTemp;
                WritePrivateProfileString("Project", "OffsetX", txtOffsetX.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtOffsetY.Text);
                projPara.OffsetY = dTemp;
                WritePrivateProfileString("Project", "OffsetY", txtOffsetY.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtThickness.Text);
                projPara.Thickness = dTemp;
                WritePrivateProfileString("Project", "Thickness", txtThickness.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtOriginalX.Text);
                projPara.OriginalX = dTemp;
                WritePrivateProfileString("Project", "OriginalX", txtOriginalX.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtOriginalY.Text);
                projPara.OriginalY = dTemp;
                WritePrivateProfileString("Project", "OriginalY", txtOriginalY.Text, strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtDPI.Text);
                projPara.DPI = dTemp;
                WritePrivateProfileString("Project", "DPI", txtDPI.Text, strIniFilePath + "\\Setting.ini");
                if (rdoLaser.Checked) projPara.LocationMode = 1;
                else if (rdoCCD.Checked) projPara.LocationMode = 2;
                else if (rdoSide.Checked) projPara.LocationMode = 3;
                WritePrivateProfileString("Project", "LocationMode", projPara.LocationMode.ToString(), strIniFilePath + "\\Setting.ini");
            }
            catch (Exception ee)
            {
                MessageBox.Show("請確認所填的項目");
            }
        }

        private void LoadCommonPara()
        {
            StringBuilder strIni1 = new StringBuilder("", 11);
            StringBuilder strIni2 = new StringBuilder("", 50);

            string filepath = Application.StartupPath + "\\Common_Parameters.ini";
            GetPrivateProfileString("Common", "PixelPerMM", "", strIni1, 11, filepath);
            projPara.PixelPerMM = Convert.ToDouble(strIni1.ToString().Trim());
            txtMM2Pixel.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Common", "CCDXoffset", "", strIni1, 11, filepath);
            projPara.GalvoCCDOffsetX = Convert.ToDouble(strIni1.ToString().Trim());
            txtGalvoCCDOffsetX.Text = strIni1.ToString().Trim();
            txtCCDXoffset.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Common", "CCDYoffset", "", strIni1, 11, filepath);
            projPara.GalvoCCDOffsetY = Convert.ToDouble(strIni1.ToString().Trim());
            txtGalvoCCDOffsetY.Text = strIni1.ToString().Trim();
            txtCCDYoffset.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Common", "CorrectionFile", "", strIni2, 50, filepath);
            projPara.CorrectionFile = (strIni2.ToString().Trim());
            txtCorrectionFile.Text = strIni2.ToString().Trim();
            //GetPrivateProfileString("Common", "ProgramFile", "", strIni2, 50, filepath);
            //projPara.ProgramFile = strIni2.ToString().Trim();
            //txtProgramFile.Text = strIni2.ToString().Trim();

            GetPrivateProfileString("Common", "LaserFocusLength", "", strIni1, 11, filepath);
            projPara.LaserFocusLength = Convert.ToDouble(strIni1.ToString().Trim());
            txtLaserFocusLength.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Common", "PowerLimit", "", strIni1, 11, filepath);
            projPara.PowerLimit = Convert.ToDouble(strIni1.ToString().Trim());
            txtPowerLimit.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Common", "dblEdgeOriginalX", "", strIni1, 11, filepath);
            projPara.dblEdgeOriginalX = Convert.ToDouble(strIni1.ToString().Trim());
            txtEdgeOriginalX.Text = strIni1.ToString().Trim();
            GetPrivateProfileString("Common", "dblEdgeOriginalY", "", strIni1, 11, filepath);
            projPara.dblEdgeOriginalY = Convert.ToDouble(strIni1.ToString().Trim());
            txtEdgeOriginalY.Text = strIni1.ToString().Trim();

            GetPrivateProfileString("Common", "K", "", strIni1, 11, filepath);
            projPara.K = Convert.ToDouble(strIni1.ToString().Trim());
            txtFieldCalibration.Text = strIni1.ToString().Trim();
            RTC5Wrap.load_correction_file(projPara.CorrectionFile, 1, 2);

        }

        private void SetLaser()
        {
            //套用雷射參數
            projPara.Power = Convert.ToDouble(txtLaserPower.Text);
            projPara.LaserFrequency = Convert.ToDouble(txtLaserFreq.Text);
            //projPara.Bias = Convert.ToDouble(Text_CoorLaserTestBias.Text);
            projPara.Epulse = Convert.ToDouble(txtLaserEpulse.Text);

            //projPara.Mode = Convert.ToInt32(txtLaserMode.Text);
            lbStatus.Text = "Not Ready";
            if (!bolcheckHardware) return;
            BackLight.Laser.ErrCode.RetErr ret_Laser = cLaser.SetPowerAndFreq(projPara.Power, projPara.LaserFrequency);
            if (!ret_Laser.flag || ret_Laser == null)
            {
                MessageBox.Show("Power Freq 確認數值是否正確\r\n" + ret_Laser.Meg, "Warnning",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                return;
            }

            if (txtLaserMode.Text != projPara.Mode.ToString())
            {
                ret_Laser = cLaser.SetMode(Convert.ToInt32(txtLaserMode.Text));
                if (!ret_Laser.flag || ret_Laser == null)
                {
                    MessageBox.Show("Mode 確認數值是否正確\r\n" + ret_Laser.Meg, "Warnning",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Warning);
                    return;
                }
                ret_Laser = cLaser.SetEPulse(projPara.Epulse);

                if (!ret_Laser.flag || ret_Laser == null)
                {
                    MessageBox.Show("epulse 確認數值是否正確\r\n" + ret_Laser.Meg, "Warnning",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Warning);
                    return;
                }
                projPara.Mode = Convert.ToInt32(txtLaserMode.Text);
            }
            #region 設定到aliment

            BackLight.File.Define.Aligment aliment = cFilsProcese.GetAlignment();
            //aliment._Laser = Para;
            BackLight.File.ErrCode.RetErr ret_file = cFilsProcese.WriteAligment(aliment);
            if (!ret_file.flag)
            {
                MessageBox.Show("File Write Para fail \r\n" + ret_file.Meg, "Warnning",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
            }
            #endregion
        }
        private void button16_Click(object sender, EventArgs e)
        {
            //套用雷射參數
            SetLaser(); 
        }
        bool Window2LaserPara(out File.Define.Aliment_Laser _laser)
        {
            double tamp = 0;
            File.Define.Aliment_Laser laser = new File.Define.Aliment_Laser();
            _laser = laser;
            try
            {
                if (double.TryParse(TextB_CoorInfoLaserPower.Text, out tamp)) laser.Power = tamp;
                if (double.TryParse(TextB_CoorInfoFreq.Text, out tamp)) laser.Freq = tamp;
                if (double.TryParse(TextB_CoorInfoPulseWidth.Text, out tamp)) laser.PulseWidth = tamp;
                if (double.TryParse(TextB_CoorInfoMarkTimes.Text, out tamp)) laser.MarkTimes = tamp;
                if (double.TryParse(TextB_CoorInfoSpotDelay.Text, out tamp)) laser.SpotDelay = tamp;
                if (double.TryParse(TextB_CoorInfoOnDelay.Text, out tamp)) laser.OnDelay = tamp;
                if (double.TryParse(TextB_CoorInfoMiddleDelay.Text, out tamp)) laser.MiddleDelay = tamp;
                if (double.TryParse(TextB_CoorInfoEndDelay.Text, out tamp)) laser.EndDelay = tamp;
                if (double.TryParse(TextB_CoorInfoMarkSpeed.Text, out tamp)) laser.MarkSpeed = tamp;
                if (double.TryParse(TextB_CoorInfoJumpSpeed.Text, out tamp)) laser.JumpSpeed = tamp;
                if (double.TryParse(TextB_CoorInfoJumpDelay.Text, out tamp)) laser.JumpDelay = tamp;
                if (double.TryParse(TextB_CoorInfoEpulse.Text, out tamp)) laser.Epulse = tamp;
                if (double.TryParse(TextB_CoorInfoBias.Text, out tamp)) laser.Bias = tamp;
                if (double.TryParse(TextB_CoorInfoMode.Text, out tamp)) laser.Mode = tamp;
            }
            catch (Exception ee)
            {
                return false;
            }
            _laser = laser;
            return true;
        }
 
        private void button4_Click(object sender, EventArgs e)
        {
            projPara.LaserFocusLength = Convert.ToDouble(txtZfouse.Text);
            //projPara.LaserFocusLength = LaserFouseLength;
            WritePrivateProfileString("Common", "LaserFouseLength", txtZfouse.Text, Application.StartupPath + "\\Common_Parameters.ini");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //Label_Xpos
            txtOriginalX.Text = Label_Xpos.Text;
            txtOriginalY.Text = Label_Ypos.Text;
        }

        private void btnSaveLaserProjIni_Click(object sender, EventArgs e)
        {
            double dTemp = 0;
            try
            {
                WritePrivateProfileString("Laser", "StandbyFrqunecy", projPara.StandbyFrequency.ToString(), strIniFilePath + "\\Setting.ini");
                WritePrivateProfileString("Laser", "StandbyPulse", projPara.LocationMode.ToString(), strIniFilePath + "\\Setting.ini");
                dTemp = double.Parse(txtFrequency.Text);
                projPara.LaserPulseWidth = (Convert.ToInt32(txtLaserPulseWidth.Text) > 0) ? Convert.ToDouble(txtLaserPulseWidth.Text) : 0;
                WritePrivateProfileString("Laser", "LaserPulseWidth", projPara.LaserPulseWidth.ToString(), strIniFilePath + "\\Setting.ini");
                projPara.LaserPulseWidth = (Convert.ToInt32(txtLaserFPS.Text) > 0) ? Convert.ToDouble(txtLaserFPS.Text) : 0;
                WritePrivateProfileString("Laser", "LaserFPS", projPara.LaserFPS.ToString(), strIniFilePath + "\\Setting.ini");
                WritePrivateProfileString("Laser", "LaserTimeBase", projPara.LaserTimeBase.ToString(), strIniFilePath + "\\Setting.ini");
                projPara.JumpDelay = (Convert.ToInt32(txtJumpDelay.Text) > 0) ? Convert.ToDouble(txtJumpDelay.Text) : 0;
                WritePrivateProfileString("Laser", "JumpDelay", projPara.JumpDelay.ToString(), strIniFilePath + "\\Setting.ini");
                //projPara.JumpDelay = (Convert.ToInt32(txtMarkDelay.Text) > 0) ? Convert.ToDouble(txtMarkDelay.Text) : 0;
                WritePrivateProfileString("Laser", "MarkDelay", projPara.MarkDelay.ToString(), strIniFilePath + "\\Setting.ini");
                WritePrivateProfileString("Laser", "Power", projPara.Power.ToString(), strIniFilePath + "\\Setting.ini");
                WritePrivateProfileString("Laser", "Mode", projPara.Mode.ToString(), strIniFilePath + "\\Setting.ini");
                WritePrivateProfileString("Laser", "EPulse", projPara.Epulse.ToString(), strIniFilePath + "\\Setting.ini");
                WritePrivateProfileString("Laser", "PolygonDelay", projPara.PolygonDelay.ToString(), strIniFilePath + "\\Setting.ini");
                projPara.LaserOnDelay = (Convert.ToInt32(txtLaserOnDelay.Text) > 0) ? Convert.ToDouble(txtLaserOnDelay.Text) : 0;
                WritePrivateProfileString("Laser", "LaserOnDelay", projPara.LaserOnDelay.ToString(), strIniFilePath + "\\Setting.ini");
                projPara.LaserOffDelay = (Convert.ToInt32(txtLaserOffDelay.Text) > 0) ? Convert.ToDouble(txtLaserOffDelay.Text) : 0;
                WritePrivateProfileString("Laser", "LaserOffDelay", projPara.LaserOffDelay.ToString(), strIniFilePath + "\\Setting.ini");
                projPara.JumpSpeed = (Convert.ToInt32(txtJumpSpeed.Text) > 0) ? Convert.ToDouble(txtJumpSpeed.Text) : 0;
                WritePrivateProfileString("Laser", "JumpSpeed", projPara.JumpSpeed.ToString(), strIniFilePath + "\\Setting.ini");
                //projPara.MarkSpeed = (Convert.ToInt32(txtMarkSpeed.Text) > 0) ? Convert.ToDouble(txtMarkSpeed.Text) : 0;
                WritePrivateProfileString("Laser", "MarkSpeed", projPara.MarkSpeed.ToString(), strIniFilePath + "\\Setting.ini");
                projPara.LaserDuty = (Convert.ToInt32(txtPower.Text) > 0) ? Convert.ToDouble(txtPower.Text) : 0;
                WritePrivateProfileString("Laser", "LaserDuty", projPara.LaserDuty.ToString(), strIniFilePath + "\\Setting.ini");
                projPara.OneShot = (Convert.ToInt32(txtOneShot.Text) > 0) ? Convert.ToDouble(txtOneShot.Text) : 0;
                WritePrivateProfileString("Laser", "OneShot", projPara.OneShot.ToString(), strIniFilePath + "\\Setting.ini");
                //projPara.K = Convert.ToInt32(txtFieldCalibration.Text);
                //WritePrivateProfileString("Common", "K", projPara.K.ToString(), Application.StartupPath + "\\Common_Parameters.ini");

            }
            catch (Exception ee)
            {
                MessageBox.Show("請檢查資料格式");
            }
        }

        private void button28_Click(object sender, EventArgs e)
        {
            //File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();
            txtGalvoCCDOffsetX.Text = projPara.GalvoCCDOffsetX.ToString(); 
            txtGalvoCCDOffsetY.Text = projPara.GalvoCCDOffsetY.ToString();
        }
        private void Bt_ROI_Click(object sender, EventArgs e)
        {
            if (!check_ROI.Checked)
            {
                MessageBox.Show("請先勾選教導模式");
                return;
            }
            cCCD.CopyROI(PicBox_CoorTeach);
            GoldenSamplePath = Application.StartupPath + "\\ROI.jpg";
            lblLoadRecipe.Text = GoldenSamplePath;
        }
        private void Bt_learnROI_Click(object sender, EventArgs e)
        {
            if (!check_ROI.Checked) return;


            cCCD.GetShowStatus(out Show_Img status);
            status.bolShowROI = false;
            CCD.ErrCode.RetErr ret_CCD = cCCD.SetShowStatus(status);
            if (!ret_CCD.flag)
            {
                MessageBox.Show("CCD Set Show status fail \r\n" + ret_CCD.Meg, "Warnning",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
            }

            // Save Sample Image
            string path = strIniFilePath + "\\Correct_pos.jpg";

            ret_CCD = cCCD.SaveROI(path);
            if (!ret_CCD.flag)
            {
                MessageBox.Show("CCD Save Sample image fail \r\n" + ret_CCD.Meg, "Warnning",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
            }
            //Bt_learnROI.Enabled = false;
            //Bt_Gauge.Enabled = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Panel_CCD_cal.BringToFront();
        }

        private void TabP__MouseDown(object sender, MouseEventArgs e)
        {
            #region 畫布位置更新

            int i = TabP_.SelectedIndex;
            Point point = new Point(0, 0);
            Point point_ctl = new Point(7, 500);
            var tamp = TabP_WorkingPage;
            tamp.Controls.Remove(Panel_MotionControl);
            tamp.Controls.Remove(Panel_CCD_cal);
            tamp = TabP_RegulatePage;
            tamp.Controls.Remove(Panel_MotionControl);
            tamp.Controls.Remove(Panel_CCD_cal);

            switch (i)
            {
                case 0:
                    tamp = TabP_WorkingPage;
                    tamp.Controls.Add(Panel_MotionControl);
                    tamp.Controls.Add(Panel_CCD_cal);
                    Panel_MotionControl.Location = point_ctl;
                    break;
                case 1:
                    tamp = TabP_RegulatePage;
                    tamp.Controls.Add(Panel_MotionControl);
                    tamp.Controls.Add(Panel_CCD_cal);
                    Panel_MotionControl.Location = new Point(7,520) ;
                    break;
            }

            //tamp.Controls.Add(Panel_MotionControl);
            //tamp.Controls.Add(Panel_CCD_cal);
            //Panel_MotionControl.Location = point_ctl;
            Panel_CCD_cal.Location = new Point(3, 3);

            #endregion
        }

        private void Bt_Gauge_Click(object sender, EventArgs e)
        {
            if (chk_CircleGauge.Checked)
                show_Img.bolShowCirGauge = true;

            else
                show_Img.bolShowCirGauge = false;
            cCCD.CirGauge_Attach();
        }

        private void Bt_GetCenter_Click(object sender, EventArgs e)
        {
            show_Img.bolShowCirGauge = true;
            PointF offset = new PointF();
            cCCD.CirGauge_Attach();
            //picolo.FindMark(GoldenSamplePath, _MarkPara, out bool isFind, out offset);
            cCCD.FindMark(GoldenSamplePath, markPara, out bool isFind, out offset);
            lblCirX_offset.Text = offset.X.ToString();
            lblCirY_offset.Text = offset.Y.ToString();
        }

        private void bt_set_Click(object sender, EventArgs e)
        {
            Bt_ROI.Enabled = true;
            bt_set.Enabled = false;
            MessageBox.Show("設定OK", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)//Save Common of Project
        {
            double dTemp = 0;
            try
            {
                dTemp = double.Parse(txtMM2Pixel.Text);
                WritePrivateProfileString("Common", "PixelPerMM", dTemp.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
                projPara.PixelPerMM = dTemp;         

                dTemp = double.Parse(txtLaserFocusLength.Text);
                WritePrivateProfileString("Common", "CCDXoffset", dTemp.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
                projPara.GalvoCCDOffsetX = dTemp;
                dTemp = double.Parse(txtPowerLimit.Text);
                WritePrivateProfileString("Common", "CCDYoffset", dTemp.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
                projPara.GalvoCCDOffsetY = dTemp;
                if (txtCorrectionFile.Text != "")
                    WritePrivateProfileString("Common", "CorrectionFile", txtCorrectionFile.Text, Application.StartupPath + "\\Common_Parameters.ini");
                projPara.CorrectionFile = txtCorrectionFile.Text;
                //if (txtProgramFile.Text != "")
                //    WritePrivateProfileString("Common", "ProgramFile", txtProgramFile.Text, Application.StartupPath + "\\Common_Parameters.ini");
                //projPara.ProgramFile = txtProgramFile.Text;
                dTemp = double.Parse(txtFieldCalibration.Text);//K值  
                WritePrivateProfileString("Common", "K", dTemp.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
                projPara.K = dTemp;
                dTemp = double.Parse(txtLaserFocusLength.Text);
                WritePrivateProfileString("Common", "LaserFocusLength", dTemp.ToString(), strIniFilePath + "\\Common_Parameters.ini");
                projPara.LaserFocusLength = dTemp;
                dTemp = double.Parse(txtPowerLimit.Text);
                WritePrivateProfileString("Common", "PowerLimit", dTemp.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
                projPara.PowerLimit = dTemp;
                dTemp = double.Parse(txtEdgeOriginalX.Text);
                WritePrivateProfileString("Common", "dblEdgeOriginalX", dTemp.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
                projPara.dblEdgeOriginalX = dTemp;
                dTemp = double.Parse(txtEdgeOriginalY.Text);
                WritePrivateProfileString("Common", "dblEdgeOriginalY", dTemp.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
                projPara.dblEdgeOriginalY = dTemp;
                projPara.K = Convert.ToInt32(txtFieldCalibration.Text);
                WritePrivateProfileString("Common", "K", projPara.K.ToString(), Application.StartupPath + "\\Common_Parameters.ini");
            }
            catch (Exception ee)
            {
                MessageBox.Show("請確認輸入都是數值");
            }
        }

        private void btnSplit_Click(object sender, EventArgs e)
        {

            if (txtProj.Text == "")
            {
                MessageBox.Show("請先選取要加工的檔案");
                return;            
            }

            string ExeName = @"D:\claire\豪晶\BackLight\transform_CS\bin\x64\Debug\netcoreapp3.1\transform.exe";         

            string argStr = "/FN:1 /DT:" + projPara.DestPath + " /CG:" + projPara.FieldSizeW + " /RG:" + projPara.FieldSizeH + " /KV:" + projPara.K +
           " /FT:" + projPara.FuzzyType + " /ST:" + projPara.SortType + " /FP:" + projPara.FuzzyParam + " /SP:" + projPara.SortParam + " /DX:" +
           projPara.DistanceX + " /DY:" + projPara.DistanceY + " /DP:" + projPara.DPI;
            Debug.Print(argStr);
            var a = Process.Start(ExeName, argStr);
            MegShow(StatusLevel.defaul, "讀取檔案中....");
            a.WaitForExit();


            //argStr = "/FN:2 /DT:c:\\workdir\\ /CG:25 /RG:1 /KV:615 /FT:3 /ST:2 /FP:50 /SP:50 /DX:600 /DY:600 /DP:363";
            argStr = "/FN:2 /DT:" + projPara.DestPath + " /CG:" + projPara.FieldSizeW + " /RG:" + projPara.FieldSizeH + " /KV:" + projPara.K +
               " /FT:" + projPara.FuzzyType + " /ST:" + projPara.SortType + " /FP:" + projPara.FuzzyParam + " /SP:" + projPara.SortParam + " /DX:" +
               projPara.DistanceX + " /DY:" + projPara.DistanceY + " /DP:" + projPara.DPI;
            Debug.Print(argStr);

            a = Process.Start(ExeName, argStr);
            MegShow(StatusLevel.defaul, "檔案分割中....");
            a.WaitForExit();

            MegShow(StatusLevel.defaul, "檔案分割完成!");
            MessageBox.Show("分割完成");
            btnStartMarking.Enabled = true;
        }
        private void RTC5Setting()
        {
            if (!bolcheckHardware) return;
            RTC5Wrap.set_laser_pulses(
                    (uint)(1000 / projPara.LaserFrequency),    // half of the laser signal period.
                    (uint)projPara.LaserPulseWidth);  // pulse widths of signal LASER1.
            RTC5Wrap.set_scanner_delays(
                (uint)projPara.JumpDelay,    // jump delay, in 10 microseconds.
                (uint)projPara.MarkDelay,    // mark delay, in 10 microseconds.
                (uint)projPara.PolygonDelay);    // polygon delay, in 10 microseconds.
            RTC5Wrap.set_laser_delays(
                (int)projPara.LaserPulseWidth,    // laser on delay, in microseconds.w96
                (uint)projPara.LaserOffDelay);   // laser off delay, in microseconds.
                                                 // jump speed in bits per milliseconds.
            RTC5Wrap.set_jump_speed(projPara.JumpSpeed);
            // marking speed in bits per milliseconds.
            RTC5Wrap.set_mark_speed(projPara.MarkSpeed);
        }

        //private void LaserSetting()
        //{

        //}

        private void Galvo_WaitForMoveDone()
        {
            uint buzy = 1;
            uint position;
            while (buzy == 1)
            {
                RTC5Wrap.get_status(out buzy, out position);
                Application.DoEvents();
                if (bolLaserStop) break;
            }
            bolLaserStop = false; 
        }
        private void LaserOnShot(double interval)
        {
            Galvo_WaitForMoveDone();
            RTC5Wrap.set_start_list_1();
            Galvo_WaitForMoveDone();
            RTC5Wrap.laser_on_list(Convert.ToUInt32(interval / 10));
            RTC5Wrap.set_end_of_list();
            RTC5Wrap.execute_list(1);
            Galvo_WaitForMoveDone();
        }
        private void LaserContinusShot()
        {
            Galvo_WaitForMoveDone();
            RTC5Wrap.set_start_list(1);
            Galvo_WaitForMoveDone();
            RTC5Wrap.laser_signal_on_list();
            RTC5Wrap.set_end_of_list();
            RTC5Wrap.execute_list(1);
            Galvo_WaitForMoveDone();
        }
        private void LaserOffShot()
        {
            bolLaserStop = true;
            Application.DoEvents(); 
            RTC5Wrap.set_start_list(1);
            RTC5Wrap.stop_execution(); 
            //RTC5Wrap.laser_signal_off();
            RTC5Wrap.set_end_of_list();
            RTC5Wrap.execute_list(1);
            Galvo_WaitForMoveDone();
        }

        private void DrawCircle()
        {
            double R = Convert.ToDouble(textBox12.Text);
            StreamWriter sw = new StreamWriter(strIniFilePath + "\\dotbinary.cnc");
            for (int i = 0; i < 90; i++)
            {
                sw.WriteLine((int)(R * projPara.K * Math.Sin(i * 4 * 4 * Math.Atan(1) / 180)));
                sw.WriteLine((int)(R * projPara.K * Math.Cos(i * 4 * 4 * Math.Atan(1) / 180)));
            }
            sw.Close();
        }
        private void button19_Click(object sender, EventArgs e)//雷射出光
        {
            if (rdoOneShot.Checked)
            {
                LaserOnShot(Convert.ToDouble(txtOneShot.Text));
            }
            else
                LaserContinusShot();
        }

        private void button20_Click(object sender, EventArgs e)//雷射off
        {
            LaserOffShot();
        }

        private void button21_Click(object sender, EventArgs e)//雷射測試畫圓
        {
            int i = 0;
            int j = 0;
            DrawCircle();
            //Galvo_WaitForMoveDone();

            long MyLocation = GetbinFileLineNum(strIniFilePath + "\\dotbinary.cnc");
            StreamReader sr = new StreamReader(strIniFilePath + "\\dotbinary.cnc");

            for (long l = 0; l < MyLocation / 2; l++)
            {
                i = Convert.ToInt32(sr.ReadLine());
                j = Convert.ToInt32(sr.ReadLine());
            }
            if (bolcheckHardware)
            {
                RTC5Wrap.set_start_list(1);
                RTC5Wrap.jump_abs(i, j);
                RTC5Wrap.laser_on_list((uint)projPara.OneShot);
                RTC5Wrap.set_end_of_list();
                RTC5Wrap.execute_list(1);
                Galvo_WaitForMoveDone();
            }
        }
        private void button22_Click(object sender, EventArgs e)
        {
            picNo++;
            LoadImg(picNo);
        }
        private void LoadImg(int pic)
        {
            picNo = pic + 9;
            this.pictureBox3.Image = images[(picNo - 2) % 9];
            this.pictureBox1.Image = images[(picNo - 1) % 9];
            this.pictureBox2.Image = images[(picNo) % 9];
            this.pictureBox4.Image = images[(picNo + 1) % 9];
            this.pictureBox5.Image = images[(picNo + 2) % 9];
            label183.Text = ((picNo) % 9).ToString();
            if (CoorLFocusZPos.Count > 0)
                label95.Text = CoorLFocusZPos[(picNo) % 9].ToString();
            picNo = (picNo) % 9;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            picNo--;
            LoadImg(picNo);
        }
        private void button23_Click(object sender, EventArgs e)
        {
            projPara.LaserFocusLength = CoorLFocusZPos[(picNo) % 9];
            txtZfouse.Text = projPara.LaserFocusLength.ToString();
        }

        private void CoorLaserFocusSelect_CheckedChanged(object sender, EventArgs e)
        {
            PointF Pos = new PointF();
            File.Define.BasicMotion basic = cFilsProcese.GetBasicMotion();
            File.Define.Aligment aliment = cFilsProcese.GetAlignment();

            #region 哪個被選

            RadioButton rb = (RadioButton)sender;
            if (rb.Name == CoorFocusCurRadio)
                return;
            switch (rb.Name)
            {
                case "RadioB_CoorLaserFocusLB":
                    Pos = CoorLaserFocusArray[0];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusLB";
                    break;
                case "RadioB_CoorLaserFocusCB":
                    Pos = CoorLaserFocusArray[1];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusCB";
                    break;
                case "RadioB_CoorLaserFocusRB":
                    Pos = CoorLaserFocusArray[2];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusRB";
                    break;
                case "RadioB_CoorLaserFocusLC":
                    Pos = CoorLaserFocusArray[3];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusLC";
                    break;
                case "RadioB_CoorLaserFocusCC":
                    Pos = CoorLaserFocusArray[4];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusCC";
                    break;
                case "RadioB_CoorLaserFocusRC":
                    Pos = CoorLaserFocusArray[5];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusRC";
                    break;
                case "RadioB_CoorLaserFocusLT":
                    Pos = CoorLaserFocusArray[6];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusLT";
                    break;
                case "RadioB_CoorLaserFocusCT":
                    Pos = CoorLaserFocusArray[7];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusCT";
                    break;
                case "RadioB_CoorLaserFocusRT":
                    Pos = CoorLaserFocusArray[8];
                    CoorFocusCurRadio = "RadioB_CoorLaserFocusRT";
                    break;
            }

            #endregion

            #region 雕刻或移動

            // 移動 ? 重新Mark?
            bool isMark = radioButton22.Checked ? false : true;

            #region MoveTo

            Axis.ErrCode.RetErr ret_axis = cAxis.XYLine(Pos.X,
                                                                    Pos.Y,
                                                                    dblXYStageSpeed,
                                                                    dblXYAcc);

            if (!ret_axis.flag)
            {
                MessageBox.Show("Axis Move fail \r\n" + ret_axis.Meg, "Warnning",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
            }

            #endregion

            if (isMark)
            {
                #region Z Move

                ret_axis = cAxis.ZMove(basic.BasicMarkingFocalDis - aliment._LFocusPara.Thickness,
                                        basic.ManualZMoveSpeed,
                                        basic.ZMoveAcc);
                if (!ret_axis.flag)
                {
                    MessageBox.Show("Axis Move fail \r\n" + ret_axis.Meg, "Warnning",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                }
                cAxis.WaitMotionDone(Axis.AxisName.Z);
                #endregion

                #region Mark

                //Run.Mark.ErrCode.RetErr ret_Mark = cMark.StartMarkCross();
                //if (!ret_Mark.flag)
                //{
                //    MessageBox.Show("Mark Mark cross fail \r\n" + ret_Mark.Meg, "Warnning",
                //                       MessageBoxButtons.OK,
                //                       MessageBoxIcon.Warning);
                //}

                #endregion
            }
            else
            {
                #region Z Move

                ret_axis = cAxis.ZMove(basic.AutoCalCCDBasicFocalDis,
                                        basic.ManualZMoveSpeed,
                                        basic.ZMoveAcc);
                if (!ret_axis.flag)
                {
                    MessageBox.Show("Axis Move fail \r\n" + ret_axis.Meg, "Warnning",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                }
                cAxis.WaitMotionDone(Axis.AxisName.Z);
                #endregion
            }

            #endregion
        }

        private void radioButton22_CheckedChanged(object sender, EventArgs e)
        {
            File.Define.AllFile allfile = cFilsProcese.GetAllfile();
            if (radioButton22.Checked)
            {
                #region XY position

                double move = allfile._aligment._LFocusPara.CrossSpace;
                // 計算基本矩陣座標 +　操考座標

                double StartX = allfile._aligment._LFocusPara.CCDCenter_X;
                double StartY = allfile._aligment._LFocusPara.CCDCenter_Y;

                PointF point = new PointF();
                CoorLaserFocusArray = new List<PointF>();

                for (int i = 0; i <= 2; i++)  //裝座標的陣列
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        point.X = (float)(StartX + move * Convert.ToDouble(j) - move);

                        point.Y = (float)(StartY + move * Convert.ToDouble(i) - move);

                        CoorLaserFocusArray.Add(point);
                    }
                }

                #endregion

                radioButton23.Checked = false;
            }
            else
            {
                #region XY position

                double move = allfile._aligment._LFocusPara.CrossSpace;
                // 計算基本矩陣座標 +　操考座標

                double StartX = allfile._aligment._LFocusPara.GarCenter_X;
                double StartY = allfile._aligment._LFocusPara.GarCenter_Y;

                PointF point = new PointF();
                CoorLaserFocusArray = new List<PointF>();

                for (int i = 0; i <= 2; i++)  //裝座標的陣列
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        point.X = (float)(StartX + move * Convert.ToDouble(j) - move);

                        point.Y = (float)(StartY + move * Convert.ToDouble(i) - move);

                        CoorLaserFocusArray.Add(point);
                    }
                }

                #endregion

                radioButton23.Checked = true;
            }
        }

        private void radioButton23_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button29_Click(object sender, EventArgs e)
        {
            double pitch = 1;//水平相距
            double interval = Convert.ToDouble(txtZInterval.Text);
            double OneShotTime = Convert.ToDouble(txtOneShot.Text);
            BasicMotion basic = cFilsProcese.GetBasicMotion();
            LaserHightTemp = Convert.ToDouble(txtZfouse.Text) - 4 * interval;
            cAxis.ZMove(LaserHightTemp, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            btnGetXYPosition_Click(null, null);//紀錄目前CCD位址


            if (CoorLFocusZPos.Count > 0) CoorLFocusZPos.Clear();

            for (int ii = 0; ii < 9; ii++)
            {
                CoorLFocusZPos.Add(LaserHightTemp + ii * interval);
            }
            //到P1
            //m_MMMark.LoadFile(Application.StartupPath + "\\Zhight.ezm");
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text) - pitch, Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text) + pitch, dblXYStageSpeed, dblXYAcc);
            //MMEdit.SetSpeed("root", laser.MarkSpeed);

            //MMEdit.SetMarkRepeat("root", (int)laser.MarkTimes);
            //MMEdit.SetFrequency("root", laser.Freq);
            cLaser.SetGate(true);
            ;
            LaserOnShot(OneShotTime);
            //到P2
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text), Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text) + pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);

            cAxis.WaitMotionDone(AxisName.Z);
            LaserOnShot(OneShotTime);
            //到P3
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text) + pitch, Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text) + pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + 2 * interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.WaitMotionDone(AxisName.Z);
            LaserOnShot(OneShotTime);
            //到P4
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text) - pitch, Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text), dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + 3 * interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.WaitMotionDone(AxisName.Z);
            LaserOnShot(OneShotTime);
            //到P5
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text), Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text), dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + 4 * interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.WaitMotionDone(AxisName.Z);
            //m_MMMark.MarkStandBy();
            LaserOnShot(OneShotTime);
            //到P6
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text) + pitch, Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text), dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + 5 * interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.WaitMotionDone(AxisName.Z);
            //m_MMMark.MarkStandBy();
            LaserOnShot(OneShotTime);
            //到P7
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text) - pitch, Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text) - pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + 6 * interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.WaitMotionDone(AxisName.Z);
            //m_MMMark.MarkStandBy();
            LaserOnShot(OneShotTime);
            //到P8
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text), Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text) - pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + 7 * interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.WaitMotionDone(AxisName.Z);
            //m_MMMark.MarkStandBy();
            LaserOnShot(OneShotTime);
            //到P9
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text) + pitch, Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text) - pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(LaserHightTemp + 8 * interval, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.WaitMotionDone(AxisName.Z);
            //m_MMMark.MarkStandBy();
            LaserOnShot(OneShotTime);


            //拍照
            if (images.Count > 0) images.Clear();
            //到P1               
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) - pitch, Convert.ToDouble(txtYPosition.Text) + pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.ZMove(CCDPositionTemp, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            Thread.Sleep(1000);

            cCCD.SaveImage(Application.StartupPath + "\\img\\1.jpg");
            //}
            Thread.Sleep(100);
            //到P2
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text), Convert.ToDouble(txtYPosition.Text) + pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\2.jpg");
            Thread.Sleep(100);
            //到P3
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + pitch, Convert.ToDouble(txtYPosition.Text) + pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\3.jpg");
            Thread.Sleep(100);
            //到P4
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) - pitch, Convert.ToDouble(txtYPosition.Text), dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\4.jpg");
            Thread.Sleep(100);
            //到P5
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text), Convert.ToDouble(txtYPosition.Text), dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\5.jpg");
            Thread.Sleep(100);
            //到P6
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + pitch, Convert.ToDouble(txtYPosition.Text), dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\6.jpg");
            Thread.Sleep(100);
            //到P7
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) - pitch, Convert.ToDouble(txtYPosition.Text) - pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\7.jpg");
            Thread.Sleep(100);
            //到P8
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text), Convert.ToDouble(txtYPosition.Text) - pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\8.jpg");
            Thread.Sleep(100);
            //到P9
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + pitch, Convert.ToDouble(txtYPosition.Text) - pitch, dblXYStageSpeed, dblXYAcc);
            cAxis.WaitMotionDone(AxisName.X);
            Thread.Sleep(1000);
            cCCD.SaveImage(Application.StartupPath + "\\img\\9.jpg");
            Thread.Sleep(100);
            Panel_CoorLaserFocus.BringToFront();

            if (images.Count > 0) images.Clear();
            Image img;
            for (int i = 1; i < 10; i++)
            {
                using (var bmpTemp = new Bitmap(Application.StartupPath + "\\img\\+" + i + ".jpg"))
                {
                    img = new Bitmap(bmpTemp);
                    images.Add(img);
                }
            }

            LoadImg(2);
        }

        private void btnGetXYPosition_Click(object sender, EventArgs e)
        {
            txtXPosition.Text = Label_Xpos.Text;
            txtYPosition.Text = Label_Ypos.Text;

            txtLaserPointX.Text = (Convert.ToDouble(Label_Xpos.Text) + Convert.ToDouble(txtCCDXoffset.Text)).ToString();
            txtLaserPointY.Text = (Convert.ToDouble(Label_Ypos.Text) + Convert.ToDouble(txtCCDYoffset.Text)).ToString();
            CCDPositionTemp = CCDPosition - Convert.ToDouble(TextB_MctlThickness.Text);
            ZPositionTemp = projPara.LaserFocusLength - Convert.ToDouble(TextB_MctlThickness.Text);
        }

        private void btnGotoXYPosition_Click(object sender, EventArgs e)
        {
            BasicMotion basic = cFilsProcese.GetBasicMotion();
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text), Convert.ToDouble(txtYPosition.Text), dblXYStageSpeed, dblXYAcc);
            CCDPositionTemp = CCDPosition - Convert.ToDouble(TextB_MctlThickness.Text);
            cAxis.ZMove(CCDPositionTemp, basic.ManualZMoveSpeed, basic.ZMoveAcc);
        }

        private void btnGotoLaserPoint_Click(object sender, EventArgs e)
        {
            BasicMotion basic = cFilsProcese.GetBasicMotion();
            ZPositionTemp = projPara.LaserFocusLength - Convert.ToDouble(TextB_MctlThickness.Text);
            cAxis.ZMove(ZPositionTemp, basic.ManualZMoveSpeed, basic.ZMoveAcc);
            cAxis.XYLine(Convert.ToDouble(txtXPosition.Text) + Convert.ToDouble(txtCCDXoffset.Text), Convert.ToDouble(txtYPosition.Text) + Convert.ToDouble(txtCCDYoffset.Text), dblXYStageSpeed, dblXYAcc);
        }

        private void btnSaveOffset_Click(object sender, EventArgs e)
        {
            txtLaserPointX.Text = Label_Xpos.Text;
            txtLaserPointY.Text = Label_Ypos.Text;

            txtCCDXoffset.Text = (Convert.ToDouble(txtCCDXoffset.Text) - Convert.ToDouble(Label_Xpos.Text) + Convert.ToDouble(txtXPosition.Text)).ToString("F3");
            txtCCDYoffset.Text = (Convert.ToDouble(txtCCDYoffset.Text) - Convert.ToDouble(Label_Ypos.Text) + Convert.ToDouble(txtYPosition.Text)).ToString("F3");

            CCDXOffset = Convert.ToDouble(txtCCDXoffset.Text);
            CCDYOffset = Convert.ToDouble(txtCCDYoffset.Text);
            //WritePrivateProfileString("Common", "GalvoCCDOffsetX", txtCCDXoffset.Text, strIniFilePath + "\\Setting.ini");
            //WritePrivateProfileString("Common", "GalvoCCDOffsetY", txtCCDYoffset.Text, strIniFilePath + "\\Setting.ini");
            WritePrivateProfileString("CCD", "CCDXoffset", txtCCDXoffset.Text, Application.StartupPath + "\\Common_Parameters.ini");
            WritePrivateProfileString("CCD", "CCDYoffset", txtCCDYoffset.Text, Application.StartupPath + "\\Common_Parameters.ini");
        }

        private void btnManualLensCalibration_Click(object sender, EventArgs e)
        {
            int X_Index;
            int Y_Index;
            //long time1;
            string tmpstr;
            //double XMoveOffset;
            //double YMoveOffset;
            
            int ReMatchThreshold = System.Convert.ToInt32(txtReMatchThreshold.Text);

            for (int i = 1; i <= lvCalibrationGridView.Items.Count; i++)
            {

                if (lvCalibrationGridView.Items[i - 1].Checked)
                {
                    //LensCalibrationPosition[,]

                    // Me.btnMoveToFirstCheckedPosition.PerformClick() 不能使用自動移至校正點, 原因是校正 mark 可能不在視野內, 會永遠是失敗狀態

                    if (cCCD.FindMark(GoldenSamplePath, markPara, out bool isFind, out point).flag)
                    {
                        if (cAxis.XYJog(point, dblXYStageSpeed).flag)
                        {
                            delaytime(500);
                            cCCD.FindMark(GoldenSamplePath, markPara, out isFind, out point);
                            cAxis.XYJog(point, dblXYStageSpeed);
                            delaytime(500);
                        }

                        if (isFind)
                        {
                            if (Math.Abs(320 - point.X) <= ReMatchThreshold & Math.Abs(point.Y - 240) <= ReMatchThreshold)
                            {
                                // 紀錄目前位置並寫入listview
                                Application.DoEvents();

                                tmpstr = lvCalibrationGridView.Items[i - 1].Text;
                                tmpstr = tmpstr.Replace("[", "");
                                tmpstr = tmpstr.Replace("]", "");
                                string[] subs = tmpstr.Split(',');
                                X_Index = Convert.ToInt32(subs[0]);
                                Y_Index = Convert.ToInt32(subs[1]);

                                LensCalibrationPosition[X_Index, Y_Index].X = (float)System.Convert.ToDouble(Label_Xpos.Text);
                                LensCalibrationPosition[X_Index, Y_Index].Y = (float)System.Convert.ToDouble(Label_Ypos.Text);

                                // .Items(.Items.Count - 1).SubItems.Add(Format(LensTheoreticalPositionX(i, j), "0.0000"))
                                if (lvCalibrationGridView.Items[i - 1].SubItems.Count == 3)
                                {
                                    lvCalibrationGridView.Items[i - 1].SubItems.Add((LensCalibrationPosition[X_Index, Y_Index].X - CalibrationOriginX).ToString("F3"));
                                    lvCalibrationGridView.Items[i - 1].SubItems.Add((LensCalibrationPosition[X_Index, Y_Index].Y - CalibrationOriginY).ToString("F3"));
                                }
                                else if (lvCalibrationGridView.Items[i - 1].SubItems.Count == 5)
                                {
                                    lvCalibrationGridView.Items[i - 1].SubItems[3].Text = (LensCalibrationPosition[X_Index, Y_Index].X - CalibrationOriginX).ToString("F3");
                                    lvCalibrationGridView.Items[i - 1].SubItems[4].Text = (LensCalibrationPosition[X_Index, Y_Index].Y - CalibrationOriginY).ToString("F3");
                                }
                                lvCalibrationGridView.Items[i - 1].Checked = false;
                            }
                        }
                        else
                            MessageBox.Show("第二次對位失敗"); // 如果再找一次的 pixel 誤差小於多少即為成功
                    }
                    else
                    {
                        MessageBox.Show("第一次對位就失敗");
                        break;
                    } // 第一次 find mark

                    break; // 暫時先一次做一個
                } // if listview checked 尋找第一個 有 checked 的
            } // for 所有 listview item 
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //移至雷射位址
            btnGotoLaserPoint_Click(null, null);
            //產生靶標
            GenerateMark();
        }

        private void GenerateMark()
        {
            //產生靶標

            double x, y;
            int FOV;
            if (!int.TryParse(txtLensCalibrationFOV.Text, out FOV))
            {
                MessageBox.Show("FOV 必須是正整數");
            }

            //真空產生
            //
            for (int c = 1; c <= MatrixSize; c++)
            {
                for (int r = 1; r <= MatrixSize; r++)
                {
                    x = MatrixStep * (c - (MatrixSize / 2) - 1);
                    y = MatrixStep * (r - (MatrixSize / 2) - 1);
                    RTC5Wrap.set_start_list_1();
                    RTC5Wrap.jump_abs(0, 0);
                    RTC5Wrap.jump_abs((int)(x * K), (int)(y * K));
                    RTC5Wrap.laser_on_list((uint)projPara.OneShot);
                    RTC5Wrap.set_end_of_list();
                    RTC5Wrap.execute_list_1();
                    Galvo_WaitForMoveDone();
                }
            }

        }

        private void btnAutoLensCalibration_Click(object sender, EventArgs e)
        {
            IsAutoCalibrating = true;
            //真空產生
            //記錄目前座標為工作原點
            if (lblLoadRecipe.Text == "")
            {
                MessageBox.Show("對位影像尚未載入");
                return;
            }
            //CCD移至雷射頭目前位置,//鏡頭移至CCD聚焦位置
            btnGotoXYPosition_Click(null, null);

            //尋找靶標
            for (int i = 0; i <= lvCalibrationGridView.Items.Count; i++)
            {
                if (!IsAutoCalibrating) return;
                lvCalibrationGridView.EnsureVisible(i - 1);
                string tmpstr = lvCalibrationGridView.Items[i - 1].Text;
                tmpstr = tmpstr.Replace("[", "");
                tmpstr = tmpstr.Replace("]", "");
                string[] subs = tmpstr.Split(',');
                int X_Index = Convert.ToInt32(subs[0]);
                int Y_Index = Convert.ToInt32(subs[1]);

                cAxis.XYLine(LensTheoreticalPosition[X_Index, Y_Index].X, LensTheoreticalPosition[X_Index, Y_Index].Y, dblXYStageSpeed, dblXYAcc);
                cAxis.WaitMotionDone(AxisName.X);
                cAxis.WaitMotionDone(AxisName.Y);

                //照相
                show_Img.bolShowCirGauge = true;
                cCCD.CirGauge_Attach();
                cCCD.FindMark(GoldenSamplePath, markPara, out bool isFind, out point);//第一次找Mark
                if (isFind)
                {
                    double X = Convert.ToDouble(Label_Xpos.Text) - point.X * show_Img.ptmm;
                    double Y = Convert.ToDouble(Label_Ypos.Text) + point.Y * show_Img.ptmm;
                    cAxis.XYLine(X, Y, dblXYStageSpeed, dblXYAcc);

                }
                cCCD.FindMark(GoldenSamplePath, markPara, out isFind, out point);//第二次找Mark
                if (isFind)
                {
                    double X = Convert.ToDouble(Label_Xpos.Text) - point.X * show_Img.ptmm;
                    double Y = Convert.ToDouble(Label_Ypos.Text) + point.Y * show_Img.ptmm;
                    cAxis.XYLine(X, Y, dblXYStageSpeed, dblXYAcc);
                }
                show_Img.bolShowCirGauge = false;

                int ReMatchThreshold = Convert.ToInt32(txtReMatchThreshold.Text);

                if ((Math.Abs(320 - point.X) <= ReMatchThreshold) && Math.Abs(point.Y - 240) <= ReMatchThreshold)//Match OK了
                {
                    LensCalibrationPosition[X_Index, Y_Index].X = (float)Convert.ToDouble(Label_Xpos.Text);
                    LensCalibrationPosition[X_Index, Y_Index].Y = (float)Convert.ToDouble(Label_Ypos.Text);

                    if (view.Items[i - 1].SubItems.Count == 3)
                    {
                    }
                    else
                    {
                        view.Items[i - 1].SubItems[3].Text = (LensCalibrationPosition[X_Index, Y_Index].X - CalibrationOriginX).ToString("F3");
                        view.Items[i - 1].SubItems[4].Text = (LensCalibrationPosition[X_Index, Y_Index].Y - CalibrationOriginY).ToString("F3");
                    }
                    view.Items[i - 1].Checked = false;
                }
                else
                {
                    view.Items[i - 1].Checked = true;
                }
            }
            IsAutoCalibrating = false;
        }

        private void button33_Click(object sender, EventArgs e)
        {
            Panel_CoorLaserFocus.BringToFront();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ; int i;
            i = Convert.ToInt32(label183.Text);
            picNo = (i + 9 - 2) % 9;
            label183.Text = (picNo).ToString();
            LoadImg(picNo);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            int i;
            i = Convert.ToInt32(label183.Text);
            picNo = (i + 9 - 1) % 9;
            label183.Text = (picNo).ToString();
            LoadImg(picNo);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            int i;
            i = Convert.ToInt32(label183.Text);
            picNo = (i + 9 - 1) % 9;
            label183.Text = (picNo).ToString();
            LoadImg(picNo);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            int i;
            i = Convert.ToInt32(label183.Text);
            picNo = (i + 9 + 1) % 9;
            label183.Text = (picNo).ToString();
            LoadImg(picNo);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            int i;
            i = Convert.ToInt32(label183.Text);
            picNo = (i + 9 + 2) % 9;
            label183.Text = (picNo).ToString();
            LoadImg(picNo);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            LaserOnShot(projPara.OneShot);
        }

        private void button27_Click(object sender, EventArgs e)
        {
            txtLaserFocusLength.Text = Label_Zpos.Text;
        }

        private void btnSavePara_Click(object sender, EventArgs e)
        {
            markPara.Score = Convert.ToInt32(txtMatchMinScore.Text);
            markPara.Diameter_Circle = Convert.ToDouble(txtDiameter_Circle.Text);
            markPara.Tolerance_Circle = Convert.ToDouble(txtTolerance_Circle.Text);
            markPara.TransitionType = Convert.ToInt32(cmbTransitionType.SelectedIndex);
            markPara.Gain = Convert.ToDouble(txtGain.Text);
            markPara.offset = Convert.ToInt32(txtOffset.Text);
            cCCD.Save_s_MarkPara();
        }

        private void btnLoadPara_Click(object sender, EventArgs e)
        {
            cCCD.Load_s_MarkPara();
            txtMatchMinScore.Text = markPara.Score.ToString();
            txtDiameter_Circle.Text = markPara.Diameter_Circle.ToString();
            txtTolerance_Circle.Text = markPara.Tolerance_Circle.ToString();
            cmbTransitionType.SelectedIndex = (int)markPara.TransitionType;
            txtGain.Text = markPara.Gain.ToString();
            txtOffset.Text = markPara.offset.ToString();
        }

        private void btnLoadLearnPattern_A_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "影像檔(.jpg)|*.jpg";//"Text documents (.txt)|*.txt"
            openFileDialog1.ShowDialog();
            string filepath = openFileDialog1.FileName;
            GoldenSamplePath = filepath;
            cCCD.LoadGoldenSample(filepath, PicBox_CoorTeach);
        }

        private void check_ROI_CheckedChanged(object sender, EventArgs e)
        {
            if (check_ROI.Checked)
            {
                show_Img.bolShowROI = true;
                cCCD.ROI_Attach();
            }

            else
            {
                show_Img.bolShowROI = false;
                cCCD.ROI_Attach();
            }
        }

        private void btnStopAutoCalibration_Click(object sender, EventArgs e)
        {
            IsAutoCalibrating = false;
        }

        private void btnGenerateCalibrationDataMatrix_Click(object sender, EventArgs e)
        {
            //產生列表            
            int XLoop;
            int YLoop;
            strCalibrationFilePath = Application.StartupPath + "\\cal";

            CalibrationOriginX = System.Convert.ToDouble(Label_Xpos.Text);
            CalibrationOriginY = System.Convert.ToDouble(Label_Ypos.Text);

            txtXPosition.Text = CalibrationOriginX.ToString("F3");
            txtYPosition.Text = CalibrationOriginY.ToString("F3");
            MatrixSize = Convert.ToInt32(combMatrixSize.Text);
            lvCalibrationGridView.Items.Clear();
            LensTheoreticalPosition = new PointF[MatrixSize + 1, MatrixSize + 1];
            LensCalibrationPosition = new PointF[MatrixSize + 1, MatrixSize + 1];
            XLoop = (MatrixSize - 1) / 2;
            YLoop = (MatrixSize - 1) / 2;
            K = (int)(1048575 / Convert.ToInt32(txtLensCalibrationFOV.Text));//K
                                                                             //MatrixStep = System.Convert.ToDouble(txtLensCalibrationFOV.Text) / (System.Convert.ToDouble(combMatrixSize.Text) - 1) * K;
            MatrixStep = System.Convert.ToDouble(txtLensCalibrationFOV.Text) / (System.Convert.ToDouble(combMatrixSize.Text) - 1);
            //MatrixSize = Convert.ToInt32(combMatrixSize.Text);

            for (int c = 1; c <= MatrixSize; c++)
            {
                XLoop = (MatrixSize - 1) / 2;
                for (int r = 1; r <= MatrixSize; r++)
                {
                    PointF data = new PointF();
                    data.X = (float)(-XLoop * MatrixStep);
                    data.Y = (float)(-YLoop * MatrixStep);

                    LensTheoreticalPosition[c, r] = data;
                    {
                        lvCalibrationGridView.Items.Add("[ " + c.ToString() + " , " + r.ToString() + " ]");
                        lvCalibrationGridView.Items[lvCalibrationGridView.Items.Count - 1].SubItems.Add(data.X.ToString("F3"));
                        lvCalibrationGridView.Items[lvCalibrationGridView.Items.Count - 1].SubItems.Add(data.Y.ToString("F3"));
                    }

                    LensTheoreticalPosition[c, r].X = (float)(CalibrationOriginX + XLoop * MatrixStep);
                    LensTheoreticalPosition[c, r].Y = (float)(CalibrationOriginY + YLoop * MatrixStep);
                    XLoop = XLoop - 1;
                }
                YLoop = YLoop - 1;
            }
        }

        private void btnMoveToFirstCheckedPosition_Click(object sender, EventArgs e)
        {

            for (int i = 1; i <= lvCalibrationGridView.Items.Count; i++)
            {
                if (lvCalibrationGridView.Items[i - 1].Checked)
                {
                    //if (!IsAutoCalibrating) return;
                    lvCalibrationGridView.EnsureVisible(i - 1);
                    string tmpstr = lvCalibrationGridView.Items[i - 1].Text;
                    tmpstr = tmpstr.Replace("[", "");
                    tmpstr = tmpstr.Replace("]", "");
                    string[] subs = tmpstr.Split(',');
                    int X_Index = Convert.ToInt32(subs[0]);
                    int Y_Index = Convert.ToInt32(subs[1]);

                    if (lvCalibrationGridView.Items[i - 1].SubItems.Count == 3)
                    {
                        cAxis.XYLine(LensTheoreticalPosition[X_Index, Y_Index].X, LensTheoreticalPosition[X_Index, Y_Index].Y, dblXYStageSpeed, dblXYAcc);
                        break;
                    }
                    else if (lvCalibrationGridView.Items[i - 1].SubItems.Count == 5)
                    {
                        cAxis.XYLine(LensCalibrationPosition[X_Index, Y_Index].X, LensCalibrationPosition[X_Index, Y_Index].Y, dblXYStageSpeed, dblXYAcc);
                        break;
                    }
                }
            }
        }
        private void btnManualWriteCalibration_Click(object sender, EventArgs e)
        {
            if (lvCalibrationGridView.Items.Count != 0)
            {
                for (int i = 1; i <= lvCalibrationGridView.Items.Count; i++)
                {
                    if (lvCalibrationGridView.Items[i - 1].Checked)
                    {
                        string tmpstr = lvCalibrationGridView.Items[i - 1].Text;
                        tmpstr = tmpstr.Replace("[", "");
                        tmpstr = tmpstr.Replace("]", "");
                        string[] subs = tmpstr.Split(',');
                        int X_Index = Convert.ToInt32(subs[0]);
                        int Y_Index = Convert.ToInt32(subs[1]);

                        LensCalibrationPosition[X_Index, Y_Index].X = System.Convert.ToSingle(Label_Xpos.Text);
                        LensCalibrationPosition[X_Index, Y_Index].Y = System.Convert.ToSingle(Label_Ypos.Text);

                        if (lvCalibrationGridView.Items[i - 1].SubItems.Count == 3)
                        {
                            lvCalibrationGridView.Items[i - 1].SubItems.Add((LensCalibrationPosition[X_Index, Y_Index].X - CalibrationOriginX).ToString("F3"));
                            lvCalibrationGridView.Items[i - 1].SubItems.Add((LensCalibrationPosition[X_Index, Y_Index].Y - CalibrationOriginY).ToString("F3"));      // 0517 Y軸相反，用振鏡初始座標系
                        }
                        else if (lvCalibrationGridView.Items[i - 1].SubItems.Count == 5)
                        {
                            lvCalibrationGridView.Items[i - 1].SubItems[3].Text = (LensCalibrationPosition[X_Index, Y_Index].X - CalibrationOriginX).ToString("F3");
                            lvCalibrationGridView.Items[i - 1].SubItems[4].Text = (LensCalibrationPosition[X_Index, Y_Index].Y - CalibrationOriginY).ToString("F3");
                        }
                        lvCalibrationGridView.Items[i - 1].Checked = false;
                        break; // 只做第一個選取的項目
                    }
                }
            }
        }

        private void btnCheckedAll_Click(object sender, EventArgs e)
        {
            if (lvCalibrationGridView.Items.Count != 0)
            {
                for (int i = 1; i <= lvCalibrationGridView.Items.Count; i++)
                    lvCalibrationGridView.Items[i - 1].Checked = true;
            }
        }

        private void btnUnCheckedAll_Click(object sender, EventArgs e)
        {
            if (lvCalibrationGridView.Items.Count != 0)
            {
                for (int i = 1; i <= lvCalibrationGridView.Items.Count; i++)
                    lvCalibrationGridView.Items[i - 1].Checked = false;
            }
        }

        private void btnSelectUnCalibration_Click(object sender, EventArgs e)
        {
            if (lvCalibrationGridView.Items.Count != 0)
            {
                for (var I = 1; I <= lvCalibrationGridView.Items.Count; I++)
                {
                    if (lvCalibrationGridView.Items[I - 1].SubItems.Count == 3)
                        lvCalibrationGridView.Items[I - 1].Checked = true;
                }
            }
        }

        private void button30_Click(object sender, EventArgs e)
        {
            //載入校正檔            
            if (System.IO.File.Exists(strCalibrationFilePath + "\\" + combMatrixSize.Text + "_" + combMatrixSize.Text + ".ct5 "))
                RTC5Wrap.load_correction_file(
                combMatrixSize.Text + "_" + combMatrixSize.Text + ".ct5 ",
                1U,         // number
                2U);        // dimension
            else
                MessageBox.Show("找不到ct5檔");
        }

        private void button31_Click(object sender, EventArgs e)
        {//轉 CT5
            if (System.IO.File.Exists(datFileName))
            {
                string fileName = Application.StartupPath + "\\correXionPro.exe";
                string argStr = strCalibrationFilePath + "\\" + combMatrixSize.Text + "_" + combMatrixSize.Text + ".dat -s";
                //combMatrixSize.Text + "_" + combMatrixSize.Text + ".dat"
                //string argStr = "Template.dat -s";
                var a = Process.Start(fileName, argStr);
            }
            else
            {
                MessageBox.Show("找不到dat檔");
            }
        }

        private void btnStartMarking_Click(object sender, EventArgs e)
        {
            uint Busy, Position;
            uint list = 1U;
            bool wait = false;
            bool ListStart = true;
            uint ListLevel = 0U;
            int x, y;
            double dx, dy, dz;
            int intColumn, intRow;
            long MyLocation;
            int actpointnum = 0;//每bin檔讀到的總點數
            string strBinFile;
            List<PointF> data = new List<PointF>();
            if (!System.IO.File.Exists("D:\\WorkDir\\" + fileName +"\\"+ fileName + ".res"))
            {
                MegShow(StatusLevel.defaul, "請重新分割圖檔");
                return;
            }
            StreamReader sr = new StreamReader("D:\\WorkDir\\" + fileName + "\\" + fileName + ".res");
            intColumn = Convert.ToInt32(sr.ReadLine());
            intRow = Convert.ToInt32(sr.ReadLine());
            string DestPath = sr.ReadLine();
            int pointnum = Convert.ToInt32(sr.ReadLine());//總點數
            sr.Close();

            //檢查bin檔
            for (int c = 1; c <= intColumn; c++)
            {
                for (int r = 1; r <= intRow; r++)
                {
                    strBinFile = DestPath + "1" + AddZero(((r - 1) * intColumn + c).ToString(), 6) + ".bin";
                    //AddZero((r - 1) * intColumn + c), 6) 
                    if (!System.IO.File.Exists(strBinFile))
                    {
                        MegShow(StatusLevel.defaul, "請重新分割圖檔");
                        return;
                    }
                    actpointnum = actpointnum + GetbinFileLineNum(strBinFile);
                }
            }
            if (pointnum != actpointnum)
            {
                MessageBox.Show("請重新分割圖檔，總點數 = " + pointnum.ToString() + "，實際點數 = " + actpointnum.ToString());
                return;
            }

            int processedpointnum = 0;//實際加工點數

            for (int c = 1; c <= intColumn; c++)
            {
                for (int r = 1; r <= intRow; r++)
                {
                    dx = ((c - 1) * Convert.ToInt32(txtFieldSizeW.Text));
                    dy = ((r - 1) * Convert.ToInt32(txtFieldSizeH.Text));

                    //存入bin file待排序
                    strBinFile = DestPath + "1" + AddZero(((r - 1) * intColumn + c).ToString(), 6) + ".bin";
                    MyLocation = GetbinFileLineNum(strBinFile);
                    if (MyLocation > 0)
                    {
                        processedpointnum = processedpointnum + (int) MyLocation;
                        StreamWriter sr2 = new StreamWriter(DestPath + "param.inf");
                        sr2.WriteLine(strBinFile);
                        sr2.Close();

                        //每個 bin file 排序            
                        //string ExeName = Application.StartupPath + "\\transform.exe";
                        string ExeName = @"D:\claire\豪晶\BackLight\transform_CS\bin\x64\Debug\netcoreapp3.1\transform.exe";        
                        string argStr = "/FN:3 /DT:" + projPara.DestPath + " /CG:" + projPara.FieldSizeW + " /RG:" + projPara.FieldSizeH + " /KV:" + projPara.K +
                            " /FT:" + projPara.FuzzyType + " /ST:" + projPara.SortType + " /FP:" + projPara.FuzzyParam + " /SP:" + projPara.SortParam + " /DX:" +
                            projPara.DistanceX + " /DY:" + projPara.DistanceY + " /DP:" + projPara.DPI;
                        Debug.Print(argStr);
                        var a = Process.Start(ExeName, argStr);
                        MegShow(StatusLevel.defaul, "讀取檔案中....");
                        a.WaitForExit();
                        MegShow(StatusLevel.defaul, "執行中...");

                    }
                    else
                        continue;

                    if(bolcheckHardware)
                    cAxis.XYLine(Convert.ToDouble(txtOriginalX.Text) + dx, Convert.ToDouble(txtOriginalY.Text) + dy, dblXYStageSpeed, dblXYAcc);

                    sr = new StreamReader(strBinFile);
                    PointF point = new PointF();
                    string[] buff = new string[2];
                    for (int k = 1; k <= MyLocation; k++)
                    {
                        string[] temp = sr.ReadLine().Split();

                        point.X = Convert.ToInt32(temp[0]);
                        point.Y = Convert.ToInt32(temp[1]);
                        data.Add(point);
                    }
                    sr.Close();


                    for (int i = 0; i < MyLocation; i++)
                    {
                        //x = i; y = i;

                        do
                        {

                            RTC5Wrap.get_status(out Busy, out Position);
                            if (Busy != 0U)//忙  1=忙 0=不忙
                            {
                                if ((list & 0x1U) == 0x0U)//一開始到此 List2
                                {
                                    if (Position >= 4000U)//>4000代表已經到 List2??
                                    {
                                        wait = true;
                                    }
                                }
                                else
                                {
                                    if (Position < 4000U) //List1   <4000代表還在List1??
                                    {
                                        wait = true;
                                    }
                                }
                            }
                        } while (wait);
                        if (ListStart)
                        {
                            // Open the list at the beginning.
                            RTC5Wrap.set_start_list(list);//開 List2
                            ListLevel = 0U;
                            ListLevel++;//ListLevel=1
                                        // Start to capture the X/Y-axis output.
                            RTC5Wrap.set_trigger(10U, 7U, 8U);
                            ListStart = false;
                        }
                        ListLevel++;//ListLevel=2
                        RTC5Wrap.mark_abs((int)data[i].X, (int)data[i].Y);

                        if (ListLevel < 4000U - 1U && i < MyLocation) continue;

                        RTC5Wrap.set_end_of_list();
                        ListStart = true;

                        // Execute.
                        // Notice that execute_list was already called.
                        RTC5Wrap.auto_change();
                        list = (list + 1) % 2 + 1;
                    }
                }
            }

            MegShow(StatusLevel.defaul, "加工完成...");

            //Start RTC5 List..

            //for (int i = 0; i < 400; i++)
            //{
            //    x = i; y = i;
            //    do
            //    {

            //        RTC5Wrap.get_status(out Busy, out Position);
            //        if (Busy != 0U)//忙  1=忙 0=不忙
            //        {
            //            if ((list & 0x1U) == 0x0U)//一開始到此 List2
            //            {
            //                if (Position >= 4000U)//>4000代表已經到 List2??
            //                {
            //                    wait = true;
            //                }
            //            }
            //            else
            //            {
            //                if (Position < 4000U) //List1   <4000代表還在List1??
            //                {
            //                    wait = true;
            //                }
            //            }
            //        }
            //    } while (wait);
            //    if (ListStart)
            //    {
            //        // Open the list at the beginning.
            //        RTC5Wrap.set_start_list(list);//開 List2
            //        ListLevel = 0U;
            //        ListLevel++;//ListLevel=1
            //                    // Start to capture the X/Y-axis output.
            //        RTC5Wrap.set_trigger(10U, 7U, 8U);
            //        ListStart = false;
            //    }
            //    ListLevel++;//ListLevel=2
            //    RTC5Wrap.mark_abs(x, y);

            //    if (ListLevel < 4000U - 1U) continue;

            //    RTC5Wrap.set_end_of_list();
            //    ListStart = true;

            //    // Execute.
            //    // Notice that execute_list was already called.
            //    RTC5Wrap.auto_change();

            //    list = (list+1) % 2 + 1;
            //}
        }
        public static String AddZero(String strTemp, int intLen)
        {
            int i;
            String Temp = "";

            if (strTemp.Length < intLen && strTemp.Trim() != "" && intLen > 0)
            {
                Temp = strTemp.Trim();
                while (Temp.Length != intLen)
                {
                    Temp = "0" + Temp;
                }
            }
            return Temp;
        }
        private void btnExportCalibrationResult_Click(object sender, EventArgs e)
        {
            string beforeCTfile;

            if (beforeMatrixSize == 0)
                beforeCTfile = "cor_1to1.ct5";
            else
                beforeCTfile = beforeMatrixSize.ToString() + "_" + beforeMatrixSize.ToString() + ".dat";

            K = (int)(1048575 / Convert.ToInt32(txtLensCalibrationFOV.Text));//K
            datFileName = strCalibrationFilePath + "\\" + combMatrixSize.Text + "_" + combMatrixSize.Text + ".dat";

            if (lvCalibrationGridView.Items.Count == 0)
            {
                MessageBox.Show("沒有資料");
                return;
            }
            StreamWriter sw = new StreamWriter(datFileName);
            sw.WriteLine("OLDCTFILE =" + beforeCTfile);
            sw.WriteLine(combMatrixSize.Text + "_" + combMatrixSize.Text + ".ct5 ");
            sw.WriteLine("TOLERANCE      =  50.0");
            sw.WriteLine("NEWCAL	       = " + K.ToString());
            for (int i = 1; i <= Convert.ToInt32(combMatrixSize.Text); i++)
            {
                for (int j = 1; j <= Convert.ToInt32(combMatrixSize.Text); j++)
                {
                    sw.WriteLine(((int)(LensTheoreticalPosition[i, j].X * K)).ToString() + " " + ((int)(LensTheoreticalPosition[i, j].Y * K)).ToString() + "  "
                        + LensCalibrationPosition[i, j].X.ToString("F3").Replace(".", ",") + " " + LensCalibrationPosition[i, j].Y.ToString("F3").Replace(".", ","));//modify...claire
                }
            }
            sw.Close();
            beforeMatrixSize = MatrixSize;
        }

        private void button26_Click(object sender, EventArgs e)
        {
            txtEdgeOriginalX.Text = Label_Xpos.Text;
            txtEdgeOriginalY.Text = Label_Ypos.Text;
        }


        private void button25_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            projPara.LaserFrequency = Convert.ToInt32(txtFrequency.Text);
            projPara.OneShot = Convert.ToInt32(txtOneShot.Text);
            projPara.Power = Convert.ToInt32(txtPower.Text);
            projPara.LaserPulseWidth = Convert.ToInt32(txtLaserPulseWidth.Text);
            projPara.LaserFPS = Convert.ToInt32(txtLaserFPS.Text);
            projPara.K = Convert.ToInt32(txtFieldCalibration.Text);
            projPara.JumpSpeed = Convert.ToInt32(txtJumpSpeed.Text);
            projPara.Epulse = Convert.ToInt32(txtEpulse.Text);
            //projPara.Mode = Convert.ToInt32(txtMode.Text);
            projPara.JumpDelay = Convert.ToInt32(txtJumpDelay.Text);
            projPara.LaserOnDelay = Convert.ToInt32(txtLaserOnDelay.Text);
            projPara.LaserOffDelay = Convert.ToInt32(txtLaserOffDelay.Text);
                       
            SetLaser();//套用雷射參數
        }

        private void btnStopMarking_Click(object sender, EventArgs e)
        {
            bolLaserStop = true;            
            RTC5Wrap.stop_execution();
            RTC5Wrap.jump_abs(0, 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LaserOnShot(projPara.OneShot);
        }
        private void delaytime(int Milliseconds)
        {
            var delay = Task.Run(async () =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                await Task.Delay(Milliseconds);
                sw.Stop();
                return sw.ElapsedMilliseconds;
            });

        }
        //讀取txt文件中總行數的方法
        public static int GetbinFileLineNum(String _fileName)
        {
            Stopwatch sw = new Stopwatch();
            var path = _fileName;
            int lines = 0;

            //按行讀取
            sw.Restart();
            using (var sr = new StreamReader(path))
            {
                var ls = "";
                while ((ls = sr.ReadLine()) != null)
                {
                    lines++;
                }
            }
            sw.Stop();
            return lines;
        }

    }
}

namespace BackLight.Define
{
    public static class Define
    {
        /// <summary>
        /// 小數點顯示位數
        /// </summary>
        public static string Axis_Dec = "F3";
    }

    enum Mctl_Mode
    {
        Line, Jog, View,
    }

    enum Mctl_ReferPos
    {
        Galvo, //振鏡
        cal,   //校正CCD
    }

    struct Position
    {
        public Point Galvo;
        public Point Pos;
        public Point Cal;
    }
    enum StatusLevel
    {
        defaul, warmming, danger,
    }

}
