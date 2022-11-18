//#define LOG
using Aerotech.A3200;
using BackLight.Axis.ErrCode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using static BackLight.Axis.AxesExecuteCallbackArgs;

namespace BackLight.Axis.A3200
{
    public class Axis_A3200 : IAxis, IExecuteFile, ILaser
    {

        ///////////////////////////////////////////////////////
        ///                  Construct                      ///
        ///////////////////////////////////////////////////////
        #region Construct 建構子
        public Axis_A3200() { }
        public Axis_A3200(object controller) { this.controller = (Controller)controller; }
        #endregion

        ///////////////////////////////////////////////////////
        ///                     Para                        ///
        ///////////////////////////////////////////////////////
        #region Para A3200所需全域變數

        private Aerotech.A3200.Controller controller;  //A3200控制器
        private Point3D pos = new Point3D();
        private Status sStatus;  //軸的狀態
        private bool isOpen = false;  //是否已初始化
        private bool _DEBUG = false;
        private string errorMsg = "";

        #endregion


        ///////////////////////////////////////////////////////
        ///                   Subrouting                    ///
        ///////////////////////////////////////////////////////
        #region Subrouting

        //A3200原廠接收控制器的事件，在這邊解析之後存入全域變數
        private void Diagnostics_NewDiagPacketArrived(object sender, Aerotech.A3200.Status.NewDiagPacketArrivedEventArgs e)
        {
            //將得到資訊存在全域變數中，若需要時則呼叫對應的Get方法
            this.pos.X = e.Data["X"].PositionFeedback;
            if (this.sStatus.cAxisStatus.X.IsError != (!e.Data["X"].AxisFault.None))  //表示狀態有變換
            {
                this.sStatus.cAxisStatus.X.IsError = (!e.Data["X"].AxisFault.None);

                errorMsg = "";
                foreach (string msg in e.Data["X"].AxisFault.ActiveBits)
                {
                    errorMsg += "X軸錯誤: " + msg + Environment.NewLine;
                }

                FaultOccur?.Invoke(errorMsg);
            }

            this.pos.Y = e.Data["Y"].PositionFeedback;
            if (this.sStatus.cAxisStatus.Y.IsError != (!e.Data["Y"].AxisFault.None))  //表示狀態有變換
            {
                this.sStatus.cAxisStatus.Y.IsError = (!e.Data["Y"].AxisFault.None);

                errorMsg = "";
                foreach (string msg in e.Data["Y"].AxisFault.ActiveBits)
                {
                    errorMsg += "Y軸錯誤: " + msg + Environment.NewLine;
                }

                FaultOccur?.Invoke(errorMsg);
            }

            this.pos.Z = e.Data["Z"].PositionFeedback;
            if (this.sStatus.cAxisStatus.Z.IsError != (!e.Data["Z"].AxisFault.None))  //表示狀態有變換
            {
                this.sStatus.cAxisStatus.Z.IsError = (!e.Data["Z"].AxisFault.None);

                errorMsg = "";
                foreach (string msg in e.Data["Z"].AxisFault.ActiveBits)
                {
                    errorMsg += "Z軸錯誤: " + msg + Environment.NewLine;
                }

                FaultOccur?.Invoke(errorMsg);
            }

            #region [事件]當前位置、速度、錯誤
            AxesInfoCallbackArgs[] args = new AxesInfoCallbackArgs[3] { new AxesInfoCallbackArgs(), new AxesInfoCallbackArgs(), new AxesInfoCallbackArgs() };

            args[(int)AxisName.X].AxisName = AxisName.X.ToString();
            args[(int)AxisName.X].Homed = e.Data["X"].AxisStatus.Homed;
            args[(int)AxisName.X].Enabled = e.Data["X"].DriveStatus.Enabled;
            args[(int)AxisName.X].Fault = !(e.Data["X"].AxisFault.None);
            args[(int)AxisName.X].Position = e.Data["X"].PositionFeedback;
            args[(int)AxisName.X].Velocity = e.Data["X"].VelocityFeedback;

            args[(int)AxisName.Y].AxisName = AxisName.Y.ToString();
            args[(int)AxisName.Y].Homed = e.Data["Y"].AxisStatus.Homed;
            args[(int)AxisName.Y].Enabled = e.Data["Y"].DriveStatus.Enabled;
            args[(int)AxisName.Y].Fault = !(e.Data["Y"].AxisFault.None);
            args[(int)AxisName.Y].Position = e.Data["Y"].PositionFeedback;
            args[(int)AxisName.Y].Velocity = e.Data["Y"].VelocityFeedback;

            args[(int)AxisName.Z].AxisName = AxisName.Z.ToString();
            args[(int)AxisName.Z].Homed = e.Data["Z"].AxisStatus.Homed;
            args[(int)AxisName.Z].Enabled = e.Data["Z"].DriveStatus.Enabled;
            args[(int)AxisName.Z].Fault = !(e.Data["Z"].AxisFault.None);
            args[(int)AxisName.Z].Position = e.Data["Z"].PositionFeedback;
            args[(int)AxisName.Z].Velocity = e.Data["Z"].VelocityFeedback;

            //透過Callback方式傳值出去            
            AxesInfoRecv?.Invoke(args);
            #endregion

        }

        #endregion


        ///////////////////////////////////////////////////////
        ///               MyEvent & Callback                ///
        ///////////////////////////////////////////////////////
        #region MyEvent & Callback
        ////軸的資訊(目前位置、狀態、錯誤訊息等)
        public event AxesInfoRecvCallbackDelegate AxesInfoRecv;
        //private AxesInfoRecvCallbackDelegate _axesInfoRecvCallback;

        ////移動完成時會觸發的事件     

        public event StatusChange FaultOccur;
        #endregion


        ///////////////////////////////////////////////////////
        ///                     Method                      ///
        ///////////////////////////////////////////////////////
        #region Method
        public object GetAxis()
        {
            return this.controller;
        }

        public RetErr Open()
        {
            RetErr ret = new RetErr();
            try
            {
                //初始化軸的狀態
                sStatus = new Status();
                sStatus.init();

                // Connect to A3200 controller.  
                this.controller = Controller.Connect();

                //Enable
                this.controller.Commands.Axes["X", "Y", "Z"].Motion.Enable();

                //註冊事件
                this.controller.ControlCenter.Diagnostics.NewDiagPacketArrived += Diagnostics_NewDiagPacketArrived; //軸的相關資訊 (enable,homed,fault,position,velocity)

                this.isOpen = true;
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._Open, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Open;
            }

            return ret;
        }
        public RetErr Close()
        {
            RetErr ret = new RetErr();

            try
            {
                if (_DEBUG)
                    return ret;

                //Abort
                this.controller.Commands.Axes["X", "Y", "Z"].Motion.Abort();

                //Disable 
                this.controller.Commands.Axes["X", "Y", "Z"].Motion.Disable();

                //Disconnect from controller
                this.controller.Dispose();
                Controller.Disconnect();
            }
            catch (Exception ex)
            {

#if (LOG)
                Log.Pushlist(Num._Close, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Close;
            }

            return ret;
        }
        public RetErr ResetAlarm()
        {
            RetErr ret = new RetErr();

            try
            {
                this.controller.Commands.AcknowledgeAll();
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._ResetAlarm, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ResetAlarm;
            }

            return ret;
        }
        public RetErr AxisEnable(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                //Enable 
                this.controller.Commands.Axes[Axis.ToString()].Motion.Enable();

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._AxisEnable, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._AxisEnable;
            }

            return ret;
        }
        public RetErr AxisDisable(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                //Disable 
                this.controller.Commands.Axes[Axis.ToString()].Motion.Disable();

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._AxisDisable, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._AxisDisable;
            }

            return ret;

        }
        public RetErr XYLine(double pX, double pY, double Velocity, double ACC)
        {
            RetErr ret = new RetErr();
            try
            {
                if (_DEBUG)  
                    return ret;
                else
                {
                    controller.Commands.Motion.Setup.Absolute();
                    controller.Commands.Motion.Linear(new string[] { "X", "Y" }, new double[] { pX, pY }, Velocity);
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._XYLine, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._XYLine;
            }

            return ret;
        }
        public void XYLineAsync(double pX, double pY, double Velocity, double ACC, Action<RetErr, AxesExecuteCallbackArgs> callback)
        {
            RetErr ret = new RetErr();
            try
            {
                if (_DEBUG)
                {
                    //將執行結果透過Callback方式回傳
                    callback?.Invoke(ret, new AxesExecuteCallbackArgs(AxesExecuteCallbackArgs.OperationType.XY_LINEAR));
                }
                else
                {
                    controller.Commands.Motion.Setup.Absolute();
                    controller.Commands.Motion.Linear(new string[] { "X", "Y" }, new double[] { pX, pY }, Velocity);
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._XYLine, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._XYLine;
            }

            //將執行結果透過Callback方式回傳            
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(OperationType.XY_LINEAR));
        }
        public RetErr ZMove(double pZ, double Velocity, double ACC)
        {
            RetErr ret = new RetErr();

            try
            {
                if (_DEBUG)
                {
                    return ret;
                }
                else
                {                 
                    controller.Commands.Motion.Linear("Z", pZ, Velocity);
                    //controller.Commands.Motion.MoveAbs("Z", pZ, Velocity);    //此為非同步移動
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._ZMove, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ZMove;
            }
            return ret;
        }
        public void ZMoveAsync(double pZ, double Velocity, double ACC, Action<RetErr, AxesExecuteCallbackArgs> callback) //ACC加速度參數?
        {
            RetErr ret = new RetErr();

            try
            {
                if (_DEBUG)
                {
                    //將執行結果透過Callback方式回傳                    
                    callback?.Invoke(ret, new AxesExecuteCallbackArgs(OperationType.Z_MOVE));
                }
                else
                {
                    controller.Commands.Motion.Linear("Z", pZ, Velocity);
                    //controller.Commands.Motion.MoveAbs("Z", pZ, Velocity);    //此為非同步移動
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._ZMove, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ZMove;
            }

            //將執行結果透過Callback方式回傳
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(OperationType.Z_MOVE));
        }

        //單軸相對移動
        public RetErr Jog(AxisName Axis, double distance, double Velocity)
        {
            RetErr ret = new RetErr();

            try
            {
                controller.Commands.Motion.MoveInc(Axis.ToString(), distance, Velocity);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._Jog, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Jog;
            }

            return ret;
        }

        //XY軸相對移動
        public RetErr XYJog(PointF distance, double Velocity)
        {
            RetErr ret = new RetErr();

            try
            {
                controller.Commands.Motion.Setup.Incremental();
                controller.Commands.Motion.Linear(new string[] { "X", "Y" }, new double[] { distance.X, distance.Y }, Velocity);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._XYJog, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._XYJog;
            }

            return ret;
        }

        //Free Run,滑鼠長按時持續移動
        public RetErr FreeRun(AxisName Axis, double Velocity)
        {
            RetErr ret = new RetErr();

            try
            {
                controller.Commands.Motion.FreeRun(Axis.ToString(), Velocity);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._FreeRun, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._FreeRun;
            }

            return ret;
        }
        public RetErr FreeRunStop(AxisName Axis)
        {
            RetErr ret = new RetErr();

            try
            {
                controller.Commands.Motion.FreeRunStop(Axis.ToString());
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._FreeRunStop, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._FreeRunStop;
            }

            return ret;
        }
        public RetErr GetAnglogInput(int index, AxisName axis, out double Value)
        {
            RetErr ret = new RetErr();
            Value = 0;
            try
            {
                Value = controller.Commands.IO.AnalogInput(index, axis.ToString());
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._GetAnglogInput, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._GetAnglogInput;
            }

            return ret;
        }
        public RetErr SetAnalogOutput(int index, AxisName axis, double Value)
        {
            RetErr ret = new RetErr();
            try
            {
                controller.Commands.IO.AnalogOutput(index, axis.ToString(), Value);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._SetAnalogOutput, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._SetAnalogOutput;
            }

            return ret;
        }
        public RetErr GetErrStatus(AxisName Axis, out string Status)
        {
            RetErr ret = new RetErr();
            Status = "";
            try
            {
                switch (Axis)
                {
                    case AxisName.X:
                        if (this.sStatus.cAxisStatus.X.IsError) //cAxisStatus.[X,Y,Z]不曉得能否以中括號的方式做索引
                        {
                            Status = this.sStatus.cAxisStatus.X.Meg;

                            ret.flag = true;
                            ret.Meg = this.sStatus.cAxisStatus.X.Meg;
                            ret.Num = 0;
                        }
                        break;

                    case AxisName.Y:

                        if (this.sStatus.cAxisStatus.Y.IsError)
                        {
                            Status = this.sStatus.cAxisStatus.X.Meg;

                            ret.flag = true;
                            ret.Meg = this.sStatus.cAxisStatus.X.Meg;
                            ret.Num = 0;
                        }
                        break;

                    case AxisName.Z:
                        if (this.sStatus.cAxisStatus.Z.IsError)
                        {
                            Status = this.sStatus.cAxisStatus.X.Meg;

                            ret.flag = true;
                            ret.Meg = this.sStatus.cAxisStatus.X.Meg;
                            ret.Num = 0;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._GetErrStatus, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._GetErrStatus;
            }

            return ret;
        }
        public RetErr Home()
        {
            RetErr ret = new RetErr();

            try
            {
                //檢查是否初始化
                if (!this.isOpen) throw new Exception("doesn't Open , please Open First! ");

                //復歸X、Y、Z軸
                controller.Commands.Axes["X", "Y", "Z"].Motion.Home();

                //設定復歸旗標
                this.sStatus.sIsHomed.X = true;
                this.sStatus.sIsHomed.Y = true;
                this.sStatus.sIsHomed.Z = true;
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._Home, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Home;
            }
            return ret;
        }
        public void HomeAsync(Action<RetErr, AxesExecuteCallbackArgs> callback)
        {
            RetErr ret = new RetErr();

            try
            {
                //檢查是否初始化
                if (!this.isOpen) throw new Exception("doesn't Open , please Open First! ");

                //復歸X、Y、Z軸
                controller.Commands.Axes["X", "Y", "Z"].Motion.Home();

                //設定復歸旗標
                this.sStatus.sIsHomed.X = true;
                this.sStatus.sIsHomed.Y = true;
                this.sStatus.sIsHomed.Z = true;
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._Home, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Home;
            }

            //將執行結果透過Callback方式回傳
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(OperationType.HOME_ALL));
        }
        public RetErr Home(AxisName Axis)
        {
            RetErr ret = new RetErr();

            try
            {
                //檢查是否初始化
                if (!this.isOpen) throw new Exception("doesn't Open , please Open First! ");

                //復歸X、Y、Z軸
                switch (Axis)
                {
                    case AxisName.X:
                        controller.Commands.Axes["X"].Motion.Home();
                        this.sStatus.sIsHomed.X = true;
                        break;

                    case AxisName.Y:
                        controller.Commands.Axes["Y"].Motion.Home();
                        this.sStatus.sIsHomed.Y = true;
                        break;

                    case AxisName.Z:
                        controller.Commands.Axes["Z"].Motion.Home();
                        this.sStatus.sIsHomed.Z = true;
                        break;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._Home_OneAxis, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Home_OneAxis;
            }
            return ret;
        }
        public void HomeAsync(AxisName Axis, Action<RetErr, AxesExecuteCallbackArgs> callback)
        {
            RetErr ret = new RetErr();

            try
            {
                //檢查是否初始化
                if (!this.isOpen) throw new Exception("doesn't Open , please Open First! ");

                //復歸X、Y、Z軸
                switch (Axis)
                {
                    case AxisName.X:
                        controller.Commands.Axes["X"].Motion.Home();
                        this.sStatus.sIsHomed.X = true;
                        break;

                    case AxisName.Y:
                        controller.Commands.Axes["Y"].Motion.Home();
                        this.sStatus.sIsHomed.Y = true;
                        break;

                    case AxisName.Z:
                        controller.Commands.Axes["Z"].Motion.Home();
                        this.sStatus.sIsHomed.Z = true;
                        break;
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._Home_OneAxis, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Home_OneAxis;
            }

            //將執行結果透過Callback方式回傳
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(OperationType.HOME_SINGLE));
        }
        public RetErr GetHome(AxisName Axis, out bool status)
        {
            RetErr ret = new RetErr();
            status = false;

            try
            {
                switch (Axis)
                {
                    case AxisName.X:
                        status = this.sStatus.sIsHomed.X;
                        break;

                    case AxisName.Y:
                        status = this.sStatus.sIsHomed.Y;
                        break;

                    case AxisName.Z:
                        status = this.sStatus.sIsHomed.Z;
                        break;

                    default:
                        throw new Exception("The axis is not exist.");
                }

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._GetHome, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._GetHome;
            }

            return ret;
        }
        public RetErr GetPos(out Point3D point)
        {
            RetErr ret = new RetErr();
            point = new Point3D() { X = 0, Y = 0, Z = 0 };
            try
            {
                point = this.pos;
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._GetPos, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._GetPos;
            }

            return ret;
        }
        public RetErr MotionStop()
        {
            RetErr ret = new RetErr();

            try
            {
                this.controller.Commands.Axes["X", "Y", "Z"].Motion.Abort();
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._MotionStop, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._MotionStop;
            }

            return ret;
        }
        public RetErr WaitMotionDone(AxisName Axis)
        {
            RetErr ret = new RetErr();

            try
            {
                controller.Commands.Motion.WaitForMotionDone(Aerotech.A3200.Commands.WaitOption.MoveDone, Axis.ToString());
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._WaitMotionDone, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._WaitMotionDone;
            }

            return ret;
        }

        /// <summary>
        /// 弧-逆時針
        /// </summary>
        /// <param name="x_end">終點位置</param>
        /// <param name="y_end">終點位置</param>
        /// <param name="radius">弧的半徑</param>
        /// <param name="speed">速度</param>
        /// <returns></returns>
        public RetErr CCW_Radius(double x_vector, double y_vector, double x_end, double y_end, double radius, double radian, double speed)
        {
            RetErr ret = new RetErr();

            try
            {
                if (_DEBUG)
                {
                    return ret;
                }
                else
                {
                    this.controller.Commands.Motion.CCWRadius("X", x_end, "Y", y_end, radius, speed);
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CCW_Radius, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CCW_Radius;
            }
            return ret;
        }       


        /// <summary>
        /// 弧-順時針
        /// </summary>
        /// <param name="x_end">終點位置</param>
        /// <param name="y_end">終點位置</param>
        /// <param name="radius">弧的半徑</param>
        /// <param name="speed">速度</param>
        /// <returns></returns>
        public RetErr CW_Radius(double x_vector, double y_vector, double x_end, double y_end, double radius, double radian, double speed)
        {
            RetErr ret = new RetErr();

            try
            {
                if (_DEBUG)
                {
                    return ret;
                }

                this.controller.Commands.Motion.CWRadius("X", x_end, "Y", y_end, radius, speed);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Radius, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CW_Radius;
            }

            return ret;
        }
        

        /// <summary>
        /// 圓-逆時針
        /// </summary>
        /// <param name="x_end">走圓的起始位置，通常為左邊緣(Xc - r)</param>
        /// <param name="y_end">走圓的起始位置，通常為左邊緣(Yc)</param>
        /// <param name="x_vector">相對於起始位置的圓心分量，由於起始位置預設為圓的左邊緣，故分量預設為半徑 (半徑,0)</param>
        /// <param name="y_vector">相對於起始位置的圓心分量，由於起始位置預設為圓的左邊緣，故分量預設為0 (半徑,0)</param>
        /// <param name="speed">速度</param>
        /// <returns></returns>
        public RetErr CCW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed)
        {
            RetErr ret = new RetErr();

            try
            {
                if (_DEBUG)
                {                   
                    return ret;
                }
                this.controller.Commands.Motion.CCWCenter("X", x_end, "Y", y_end, x_vector, y_vector, speed);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CCW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CCW_Center;
            }
            return ret;
        }
        

        /// <summary>
        /// 圓-順時針
        /// </summary>
        /// <param name="x_end">走圓的起始位置，通常為左邊緣(Xc - r)</param>
        /// <param name="y_end">走圓的起始位置，通常為左邊緣(Yc)</param>
        /// <param name="x_vector">相對於起始位置的圓心分量，由於起始位置預設為圓的左邊緣，故分量預設為半徑 (半徑,0)</param>
        /// <param name="y_vector">相對於起始位置的圓心分量，由於起始位置預設為圓的左邊緣，故分量預設為0 (半徑,0)</param>
        /// <param name="speed">速度</param>
        /// <returns></returns>
        public RetErr CW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed)
        {
            RetErr ret = new RetErr();

            try
            {
                if (_DEBUG)
                {
                    return ret;
                }

                this.controller.Commands.Motion.CWCenter("X", x_end, "Y", y_end, x_vector, y_vector, speed);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CW_Center;
            }           
            return ret;
        }
        

        public RetErr ResetController()
        {
            RetErr ret = new RetErr();

            try
            {
                this.controller.Reset();
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ResetAxisController;
            }
            return ret;
        }
        public void ResetControllerAsync(Action<RetErr, AxesExecuteCallbackArgs> callback)
        {
            RetErr ret = new RetErr();

            try
            {
                this.controller.Reset();
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ResetAxisController;
            }

            //將執行結果透過Callback方式回傳
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(OperationType.RESET_CONTROLLER));
        }

        #region NotImplemented
        public RetErr ReadPara()
        {
            throw new NotImplementedException();
        }
        public RetErr SetNotHome(AxisName Axis)
        {
            throw new NotImplementedException();
        }

        //設定工作原點
        public RetErr SetWorkingPos(double pX, double pY)
        {
            throw new NotImplementedException();
        }


        #endregion

        #endregion

        //執行pgm方法
        public RetErr ExeFile(string filePath)
        {
            RetErr ret = new RetErr();

            try
            {
                //執行task
                this.controller.Tasks[TaskId.T01].Program.BufferedRun(filePath);

                //判斷task的執行狀況
                var taskState = this.controller.Tasks[TaskId.T01].State;
                while (taskState != Aerotech.A3200.Tasks.TaskState.ProgramComplete)
                {
                    taskState = this.controller.Tasks[TaskId.T01].State;
                    Thread.Sleep(10);
                    if (taskState == Aerotech.A3200.Tasks.TaskState.Error)
                    {
                        string funcName = MethodBase.GetCurrentMethod().Name;
                        ret.flag = false;
                        ret.Meg = funcName + " : TaskState意外結束 (TaskState.Error)";
                        ret.Num = Num._CW_Center;
                        return ret;
                    }
                }
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                //ret.Num = Num._CW_Center;
            }

            return ret;
        }


        //開雷射(PSO)
        public RetErr LaserLunch(bool isOn)
        {
            RetErr ret = new RetErr();

            try
            {
                if (isOn)
                    controller.Commands.PSO.Control("Y", Aerotech.A3200.Commands.PsoMode.On);
                else
                    controller.Commands.PSO.Control("Y", Aerotech.A3200.Commands.PsoMode.Off);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                //ret.Num = Num._CW_Center;
            }

            return ret;
        }

        // 模板
        private RetErr func()
        {
            RetErr ret = new RetErr();

            try
            {

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CW_Center;
            }

            return ret;
        }
    }
}
