using GalvoScan.Hardware.Axis;
using GalvoScan.Hardware.Axis.ErrCode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace GalvoScan.Hardware.Axis.ACS_Controller
{
    public class Axis_ACS : IAxis
    {
        ///////////////////////////////////////////////////////
        ///                  Construct                      ///
        ///////////////////////////////////////////////////////
        public Axis_ACS()
        {
            //initial [isHomed] to false.
            for (int i = 0; i < isHomed.Length; i++)
                isHomed[i] = false;
        }

        ///////////////////////////////////////////////////////
        ///                     Para                        ///
        ///////////////////////////////////////////////////////
        private ACS.SPiiPlusNET.Api _ACS = new ACS.SPiiPlusNET.Api();
        private bool[] isHomed = new bool[4];  //是否回home
        private bool[] isError = new bool[4];  //是否有錯誤       
        private Thread servoInfoThread;
        private const int _TIMEOUT = 20000;


        ///////////////////////////////////////////////////////
        ///                   Subrouting                    ///
        ///////////////////////////////////////////////////////
        private ACS.SPiiPlusNET.Axis getAxisID(AxisName axis)
        {
            if (axis == AxisName.X)
                return ACS.SPiiPlusNET.Axis.ACSC_AXIS_0;
            else if (axis == AxisName.Y)
                return ACS.SPiiPlusNET.Axis.ACSC_AXIS_1;
            else if (axis == AxisName.Z)
                return ACS.SPiiPlusNET.Axis.ACSC_AXIS_2;
            else
                return ACS.SPiiPlusNET.Axis.ACSC_NONE;
        }


        ///////////////////////////////////////////////////////
        ///                     Method                      ///
        ///////////////////////////////////////////////////////
        #region Method

        #region implement
        public object GetAxis()
        {
            return _ACS;
        }
        public RetErr Open()
        {
            RetErr ret = new RetErr();

            try
            {
                //TCP / IP(Ethernet)
                this._ACS.OpenCommEthernetTCP(
                    "10.0.0.100",               // IP Address (Default : 10.0.0.100)
                    Convert.ToInt32("701")      // TCP/IP Port nubmer (default : 701)
                    );

                //Simulate
                //_ACS.OpenCommSimulator();

                servoInfoThread = new Thread(servoInfoPoll);
                servoInfoThread.IsBackground = true;
                servoInfoThread.Start();


                this._ACS.SetAccelerationImm(getAxisID(AxisName.X), 5000);
                this._ACS.SetAccelerationImm(getAxisID(AxisName.Y), 5000);
                this._ACS.SetAccelerationImm(getAxisID(AxisName.Z), 5000);
                this._ACS.SetDecelerationImm(getAxisID(AxisName.X), 5000);
                this._ACS.SetDecelerationImm(getAxisID(AxisName.Y), 5000);
                this._ACS.SetDecelerationImm(getAxisID(AxisName.Z), 5000);
                this._ACS.SetJerkImm(getAxisID(AxisName.X), 5000);
                this._ACS.SetJerkImm(getAxisID(AxisName.Y), 5000);
                this._ACS.SetJerkImm(getAxisID(AxisName.Z), 5000);
                this._ACS.SetKillDecelerationImm(getAxisID(AxisName.X), 5000);
                this._ACS.SetKillDecelerationImm(getAxisID(AxisName.Y), 5000);
                this._ACS.SetKillDecelerationImm(getAxisID(AxisName.Z), 5000);

            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
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
                servoInfoThread.Abort();

                this._ACS.HaltM(
                    new ACS.SPiiPlusNET.Axis[4] {
                        getAxisID(AxisName.X) ,
                        getAxisID(AxisName.Y) ,
                        getAxisID(AxisName.Z) ,
                        ACS.SPiiPlusNET.Axis.ACSC_NONE});
                this._ACS.DisableAll();

                this._ACS.CloseComm();
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Close;
            }

            return ret;
        }
        public RetErr Home()
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.ClearBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, 1, 100);
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE (0,1,2)");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 0,1,30");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 1,1,30");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 2,1,15");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);

                Thread.Sleep(200);
                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
                WaitMotionDone(AxisName.Z);

                for (int i = 0; i < isHomed.Length; i++)
                    isHomed[i] = true;
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
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
                this._ACS.ClearBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, 1, 100);
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE (0,1,2)");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 0,1,30");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 1,1,30");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 2,1,15");
                this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);

                Thread.Sleep(200);
                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
                WaitMotionDone(AxisName.Z);

                for (int i = 0; i < isHomed.Length; i++)
                    isHomed[i] = true;
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Home;
            }


            //將執行結果透過Callback方式回傳            
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(AxesExecuteCallbackArgs.OperationType.HOME_ALL));
        }
        public RetErr Home(AxisName Axis)
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.ClearBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, 1, 100);
                if (Axis == AxisName.X)
                {
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE 0");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 0,1,30");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                    this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);
                    Thread.Sleep(200);
                    WaitMotionDone(AxisName.X);
                    isHomed[(int)Axis] = true;
                }
                else if (Axis == AxisName.Y)
                {
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE 1");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 1,1,30");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                    this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);
                    Thread.Sleep(200);
                    WaitMotionDone(AxisName.Y);
                    isHomed[(int)Axis] = true;
                }
                else if (Axis == AxisName.Z)
                {
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE 2");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 2,1,15");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                    this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);
                    Thread.Sleep(200);
                    WaitMotionDone(AxisName.Z);
                    isHomed[(int)Axis] = true;
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
                ret.Num = Num._Home_OneAxis;
            }
            return ret;
        }
        public void HomeAsync(AxisName Axis, Action<RetErr, AxesExecuteCallbackArgs> callback)
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.ClearBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, 1, 100);
                if (Axis == AxisName.X)
                {
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE 0");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 0,1,30");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                    this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);
                    Thread.Sleep(200);
                    WaitMotionDone(AxisName.X);
                    isHomed[(int)Axis] = true;
                }
                else if (Axis == AxisName.Y)
                {
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE 1");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 1,1,30");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                    this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);
                    Thread.Sleep(200);
                    WaitMotionDone(AxisName.Y);
                    isHomed[(int)Axis] = true;
                }
                else if (Axis == AxisName.Z)
                {
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "ENABLE 2");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "HOME 2,1,15");
                    this._ACS.AppendBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, "STOP");
                    this._ACS.RunBuffer(ACS.SPiiPlusNET.ProgramBuffer.ACSC_BUFFER_5, null);
                    Thread.Sleep(200);
                    WaitMotionDone(AxisName.Z);
                    isHomed[(int)Axis] = true;
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
                ret.Num = Num._Home_OneAxis;
            }


            //將執行結果透過Callback方式回傳            
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(AxesExecuteCallbackArgs.OperationType.HOME_SINGLE));
        }
        public RetErr AxisEnable(AxisName Axis)
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.Enable(getAxisID(Axis));
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._AxisDisable;
            }

            return ret;
        }
        public RetErr AxisDisable(AxisName Axis)
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.Disable(getAxisID(Axis));
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._AxisDisable;
            }

            return ret;
        }
        public RetErr ResetAlarm()
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.FaultClearM(new ACS.SPiiPlusNET.Axis[4] { getAxisID(AxisName.X), getAxisID(AxisName.Y), getAxisID(AxisName.Z), ACS.SPiiPlusNET.Axis.ACSC_NONE });
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ResetAlarm;
            }

            return ret;
        }
        public RetErr GetPos(out Point3D point)
        {
            RetErr ret = new RetErr();
            point = new Point3D();
            try
            {
                point.X = this._ACS.GetFPosition(getAxisID(AxisName.X));
                point.Y = this._ACS.GetFPosition(getAxisID(AxisName.Y));
                point.Z = this._ACS.GetFPosition(getAxisID(AxisName.Z));
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._GetPos;
            }
            return ret;
        }
        public RetErr FreeRun(AxisName Axis, double Velocity)
        {
            RetErr ret = new RetErr();

            try
            {
                //this._ACS.JOG
                this._ACS.Jog(
                    ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY,      // Velocity flag
                    getAxisID(Axis),                    // Axis number
                    Velocity                            // Velocity
                );
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
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
            var isMoving = ACS.SPiiPlusNET.MotorStates.ACSC_MST_MOVE | ACS.SPiiPlusNET.MotorStates.ACSC_MST_ACC;
            try
            {
                while ((_ACS.GetMotorState(getAxisID(Axis)) & isMoving) > 0)
                    this._ACS.Halt(getAxisID(Axis));
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._FreeRunStop;
            }

            return ret;
        }
        public RetErr Jog(AxisName Axis, double distance, double Velocity)
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.ExtToPoint(
                        ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_RELATIVE | ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY,      // Flat
                        getAxisID(Axis),                    // Axis number
                        distance,                           // Move distance
                        Velocity,                           // 速度
                        Velocity                            // 尾速
                        );
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._Jog;
            }

            return ret;
        }
        public RetErr MotionStop()
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.HaltM(
                    new ACS.SPiiPlusNET.Axis[4] {
                        getAxisID(AxisName.X) ,
                        getAxisID(AxisName.Y) ,
                        getAxisID(AxisName.Z) ,
                        ACS.SPiiPlusNET.Axis.ACSC_NONE});
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._MotionStop;
            }

            return ret;
        }
        public RetErr XYJog(PointF distance, double Velocity)
        {
            RetErr ret = new RetErr();

            try
            {
                _ACS.ExtToPointM(
                   ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_RELATIVE,                                    // '0' - Absolute position
                   new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE },     // Axis number
                   new double[2] { distance.X, distance.Y },                                         // Target position
                   Velocity,
                   Velocity
                   );
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._XYJog;
            }

            return ret;
        }
        public RetErr XYLine(double pX, double pY, double Velocity, double ACC)
        {
            RetErr ret = new RetErr();

            try
            {
                _ACS.ExtToPointM(
                    ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY,                                                      // '0' - Absolute position
                   new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE }, // Axis number
                   new double[2] { pX, pY },                                                    // Target position
                   Velocity,
                   ACC
                   );

                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
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
            //ACS.SPiiPlusNET.Axis[]  = new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE }
            try
            {
                _ACS.ExtToPointM(
                    ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY,                                                      // '0' - Absolute position
                   new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE }, // Axis number
                   new double[2] { pX, pY },                                                    // Target position
                   Velocity,
                   ACC
                   );

                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._XYLine;
            }

            //將執行結果透過Callback方式回傳            
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(AxesExecuteCallbackArgs.OperationType.XY_LINEAR));
        }
        public RetErr ZMove(double pZ, double Velocity, double ACC)
        {
            RetErr ret = new RetErr();

            try
            {
                _ACS.ExtToPoint(
                    ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY,  // '0' - Absolute position
                    getAxisID(AxisName.Z),                          // Axis number
                    pZ,                                             // Target position
                    Velocity,
                    ACC
                    );

                WaitMotionDone(AxisName.Z);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ZMove;
            }

            return ret;
        }
        public void ZMoveAsync(double pZ, double Velocity, double ACC, Action<RetErr, AxesExecuteCallbackArgs> callback)
        {
            RetErr ret = new RetErr();

            try
            {
                _ACS.ExtToPoint(
                    ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY,                               // '0' - Absolute position
                    getAxisID(AxisName.Z),           // Axis number
                    pZ,                              // Target position
                    Velocity,
                    ACC
                    );

                WaitMotionDone(AxisName.Z);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._ZMove;
            }

            //將執行結果透過Callback方式回傳            
            callback?.Invoke(ret, new AxesExecuteCallbackArgs(AxesExecuteCallbackArgs.OperationType.Z_MOVE));
        }
        public RetErr WaitMotionDone(AxisName Axis)
        {
            RetErr ret = new RetErr();

            try
            {
                this._ACS.WaitMotionEnd(getAxisID(Axis), _TIMEOUT);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._WaitMotionDone;
            }

            return ret;
        }
        public RetErr GetErrStatus(AxisName Axis, out string Status)
        {
            RetErr ret = new RetErr();
            Status = "";

            try
            {
                var errState = _ACS.GetFault(getAxisID(Axis));

                //若沒有錯誤訊息即return
                if (errState == ACS.SPiiPlusNET.SafetyControlMasks.ACSC_NONE)
                {
                    isError[(int)Axis] = false;
                    return ret;
                }

                //foreach各個錯誤訊息
                foreach (ACS.SPiiPlusNET.SafetyControlMasks s in (ACS.SPiiPlusNET.SafetyControlMasks[])Enum.GetValues(typeof(ACS.SPiiPlusNET.SafetyControlMasks)))
                {
                    if (s == ACS.SPiiPlusNET.SafetyControlMasks.ACSC_ALL)
                        continue;

                    if (((uint)errState & (uint)s) > 0)
                    {
                        Status += s.ToString() + Environment.NewLine;
                        isError[(int)Axis] = true;
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
                ret.Num = Num._GetErrStatus;
            }

            return ret;
        }
        public RetErr CW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed)
        {
            RetErr ret = new RetErr();
            ACS.SPiiPlusNET.Axis[] axes = new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE };
            double[] centerPoint = new double[2] { x_vector, y_vector };
            double[] endPoint = new double[2] { 0, 0 };
            try
            {
                this._ACS.Segment(ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY, axes, endPoint);

                this._ACS.ExtArc2(axes, centerPoint, 6.28318, speed);

                this._ACS.EndSequenceM(axes);

                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
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
        public RetErr CCW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed)
        {
            RetErr ret = new RetErr();
            ACS.SPiiPlusNET.Axis[] axes = new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE };
            double[] centerPoint = new double[2] { x_vector, y_vector };
            double[] endPoint = new double[2] { 0, 0 };
            try
            {
                this._ACS.Segment(ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY, axes, endPoint);

                this._ACS.ExtArc2(axes, centerPoint, -6.28318, speed);

                this._ACS.EndSequenceM(axes);

                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CCW_Center;
            }

            return ret;
        }
        public RetErr CCW_Radius(double x_vector, double y_vector, double x_end, double y_end, double radius, double radian, double speed)
        {
            RetErr ret = new RetErr();
            ACS.SPiiPlusNET.Axis[] axes = new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE };
            double[] centerPoint = new double[2] { x_vector, y_vector };
            double[] strPoint = new double[2] { 0, 0 };
            try
            {
                this._ACS.Segment(ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY, axes, strPoint);

                this._ACS.ExtArc2(axes, centerPoint, radian, speed);

                this._ACS.EndSequenceM(axes);

                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CCW_Radius;
            }

            return ret;
        }
        public RetErr CW_Radius(double x_vector, double y_vector, double x_end, double y_end, double radius, double radian, double speed)
        {
            RetErr ret = new RetErr();
            ACS.SPiiPlusNET.Axis[] axes = new ACS.SPiiPlusNET.Axis[3] { getAxisID(AxisName.X), getAxisID(AxisName.Y), ACS.SPiiPlusNET.Axis.ACSC_NONE };
            double[] centerPoint = new double[2] { x_vector, y_vector };
            double[] strPoint = new double[2] { 0, 0 };
            try
            {
                this._ACS.Segment(ACS.SPiiPlusNET.MotionFlags.ACSC_AMF_VELOCITY, axes, strPoint);

                this._ACS.ExtArc2(axes, centerPoint, -radian, speed);

                this._ACS.EndSequenceM(axes);

                WaitMotionDone(AxisName.X);
                WaitMotionDone(AxisName.Y);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._CW_Radius;
            }

            return ret;
        }
        public RetErr GetAnglogInput(int index, AxisName axis, out double Value)
        {
            RetErr ret = new RetErr();
            Value = 0;

            try
            {
                Value = this._ACS.GetAnalogInputNT(index);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
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
                this._ACS.SetAnalogOutputNT(index, Value);
            }
            catch (Exception ex)
            {
#if (LOG)
                Log.Pushlist(Num._CW_Center, MethodBase.GetCurrentMethod().Name, ex.Message);
#endif
                string funcName = MethodBase.GetCurrentMethod().Name;
                ret.flag = false;
                ret.Meg = funcName + " : " + ex.Message;
                ret.Num = Num._SetAnalogOutput;
            }

            return ret;
        }
        #endregion

        #region Not implement
        public RetErr GetHome(AxisName Axis, out bool status)
        {
            throw new NotImplementedException();
        }     
        public RetErr ReadPara()
        {
            throw new NotImplementedException();
        }
        public RetErr SetNotHome(AxisName Axis)
        {
            throw new NotImplementedException();
        }
        public RetErr SetWorkingPos(double pX, double pY)
        {
            throw new NotImplementedException();
        }
        public void ResetControllerAsync(Action<RetErr, AxesExecuteCallbackArgs> callback)
        {
            throw new NotImplementedException();
        }
        public RetErr ResetController()
        {
            throw new NotImplementedException();
        }
        #endregion


        #endregion


        ///////////////////////////////////////////////////////
        ///               MyEvent & Callback                ///
        ///////////////////////////////////////////////////////
        #region MyEvent & Callback
        public event AxesInfoRecvCallbackDelegate AxesInfoRecv;
        public event StatusChange FaultOccur;
        #endregion


        ///////////////////////////////////////////////////////
        ///                   Subrouting                    ///
        ///////////////////////////////////////////////////////
        #region Subrouting

        //背景執行續，持續撈取伺服馬達的資料，並透過事件方式回傳
        private void servoInfoPoll()
        {
            AxesInfoCallbackArgs[] args = new AxesInfoCallbackArgs[3] { new AxesInfoCallbackArgs(), new AxesInfoCallbackArgs(), new AxesInfoCallbackArgs() };
            string errMsg = "";
            bool[] lastError = new bool[4];
            while (true)
            {
                #region [事件]當前位置、速度、錯誤                

                args[(int)AxisName.X].AxisName = AxisName.X.ToString();
                args[(int)AxisName.X].Homed = isHomed[(int)AxisName.X];
                args[(int)AxisName.X].Enabled = ((int)this._ACS.GetMotorState(getAxisID(AxisName.X)) & (int)ACS.SPiiPlusNET.MotorStates.ACSC_MST_ENABLE) == 1;
                GetErrStatus(AxisName.X, out errMsg);
                if (lastError[(int)AxisName.X] != isError[(int)AxisName.X])
                {
                    lastError[(int)AxisName.X] = isError[(int)AxisName.X];
                    if (isError[(int)AxisName.X])
                    {
                        errMsg += Environment.NewLine;
                        FaultOccur?.Invoke("[X軸錯誤]:" + Environment.NewLine + errMsg);
                    }
                }
                args[(int)AxisName.X].Position = this._ACS.GetFPosition(getAxisID(AxisName.X));
                args[(int)AxisName.X].Velocity = this._ACS.GetVelocity(getAxisID(AxisName.X));


                args[(int)AxisName.Y].AxisName = AxisName.Y.ToString();
                args[(int)AxisName.Y].Homed = isHomed[(int)AxisName.Y];
                args[(int)AxisName.Y].Enabled = ((int)this._ACS.GetMotorState(getAxisID(AxisName.Y)) & (int)ACS.SPiiPlusNET.MotorStates.ACSC_MST_ENABLE) == 1;
                GetErrStatus(AxisName.Y, out errMsg);
                if (lastError[(int)AxisName.Y] != isError[(int)AxisName.Y])
                {
                    lastError[(int)AxisName.Y] = isError[(int)AxisName.Y];
                    if (isError[(int)AxisName.Y])
                    {
                        errMsg += Environment.NewLine;
                        FaultOccur?.Invoke("[Y軸錯誤]:" + Environment.NewLine + errMsg);
                    }
                }
                args[(int)AxisName.Y].Position = this._ACS.GetFPosition(getAxisID(AxisName.Y));
                args[(int)AxisName.Y].Velocity = this._ACS.GetVelocity(getAxisID(AxisName.Y));

                args[(int)AxisName.Z].AxisName = AxisName.Z.ToString();
                args[(int)AxisName.Z].Homed = isHomed[(int)AxisName.Z];
                args[(int)AxisName.Z].Enabled = ((int)this._ACS.GetMotorState(getAxisID(AxisName.Z)) & (int)ACS.SPiiPlusNET.MotorStates.ACSC_MST_ENABLE) == 1;
                GetErrStatus(AxisName.Z, out errMsg);
                if (lastError[(int)AxisName.Z] != isError[(int)AxisName.Z])
                {
                    lastError[(int)AxisName.Z] = isError[(int)AxisName.Z];
                    if (isError[(int)AxisName.Z])
                    {
                        errMsg += Environment.NewLine;
                        FaultOccur?.Invoke("[Z軸錯誤]:" + Environment.NewLine + errMsg);
                    }
                }
                args[(int)AxisName.Z].Position = this._ACS.GetFPosition(getAxisID(AxisName.Z));
                args[(int)AxisName.Z].Velocity = this._ACS.GetVelocity(getAxisID(AxisName.Z));

                //透過Callback方式傳值出去
                AxesInfoRecv?.Invoke(args);
                #endregion

                Thread.Sleep(100);
            }
        }

        #endregion


        // 模板
        RetErr func()
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
