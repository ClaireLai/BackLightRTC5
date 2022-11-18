//#define Log
using System;
using System.IO;
using System.Windows.Forms;
using BackLight.Axis.ErrCode;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BackLight.Axis
{
    public enum AxisName
    {
        X, Y, Z,
    }

    //若要使用A3200，則需繼承IExecuteFile
    interface IAxis //: IExecuteFile    // InterFace of Axis 
    {
        //Event
        //event MotionDoneCallbackDelegate MotionDone;
        event AxesInfoRecvCallbackDelegate AxesInfoRecv;
        event StatusChange FaultOccur;

        //return Axis
        object GetAxis();

        //Method
        RetErr Open();
        RetErr Close();
        RetErr ResetAlarm();
        RetErr ResetController();
        void ResetControllerAsync(Action<RetErr, AxesExecuteCallbackArgs> callback);
        RetErr ReadPara();
        RetErr XYLine(double pX, double pY, double Velocity, double ACC);
        void XYLineAsync(double pX, double pY, double Velocity, double ACC, Action<RetErr, AxesExecuteCallbackArgs> callback);
        RetErr ZMove(double pZ, double Velocity, double ACC);
        void ZMoveAsync(double pZ, double Velocity, double ACC, Action<RetErr, AxesExecuteCallbackArgs> callback);
        RetErr FreeRun(AxisName Axis, double Velocity);
        RetErr FreeRunStop(AxisName Axis);
        RetErr Jog(AxisName Axis, double distance, double Velocity);
        RetErr XYJog(PointF distance, double Velocity);
        RetErr MotionStop();
        RetErr AxisEnable(AxisName Axis);
        RetErr AxisDisable(AxisName Axis);
        RetErr Home();
        RetErr Home(AxisName Axis);
        void HomeAsync(Action<RetErr, AxesExecuteCallbackArgs> callback);
        void HomeAsync(AxisName Axis, Action<RetErr, AxesExecuteCallbackArgs> callback);
        RetErr GetHome(AxisName Axis, out Boolean status);
        RetErr SetNotHome(AxisName Axis);
        RetErr WaitMotionDone(AxisName Axis);
        RetErr GetErrStatus(AxisName Axis, out string Status);
        RetErr SetWorkingPos(double pX, double pY);
        RetErr GetPos(out Point3D point);
        RetErr GetAnglogInput(int index, AxisName axis, out double Value);
        RetErr SetAnalogOutput(int index, AxisName axis, double Value);

        /// <summary>
        /// 弧-逆時針
        /// </summary>
        /// <param name="x_vector">相對圓心位置X</param>
        /// <param name="y_vector">相對圓心位置Y</param>
        /// <param name="x_end">終點位置X</param>
        /// <param name="y_end">終點位置Y</param>
        /// <param name="radius">半徑</param>
        /// <param name="radian">弧度(徑度)，2pi = 360度 </param>
        /// <param name="speed">加工速度</param>
        /// <returns></returns>
        RetErr CCW_Radius(double x_vector, double y_vector, double x_end, double y_end, double radius, double radian, double speed);

        /// <summary>
        /// 弧-順時針
        /// </summary>
        /// <param name="x_vector">相對圓心位置X</param>
        /// <param name="y_vector">相對圓心位置Y</param>
        /// <param name="x_end">終點位置X</param>
        /// <param name="y_end">終點位置Y</param>
        /// <param name="radius">半徑</param>
        /// <param name="radian">弧度(徑度)，2pi = 360度 </param>
        /// <param name="speed">加工速度</param>
        /// <returns></returns>
        RetErr CW_Radius(double x_vector, double y_vector, double x_end, double y_end, double radius, double radian, double speed);

        /// <summary>
        /// 圓-逆時針
        /// 當前位置、終點位置、圓心位置來畫圓、弧                
        /// </summary>
        /// <param name="x_end">終點位置X</param>
        /// <param name="y_end">終點位置Y</param>
        /// <param name="x_vector">相對圓心位置X</param>
        /// <param name="y_vector">相對圓心位置Y</param>
        /// <param name="speed">加工速度</param>
        /// <returns></returns>
        RetErr CCW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed);

        /// <summary>
        /// 圓-順時針
        /// 當前位置、終點位置、圓心位置來畫圓、弧                
        /// </summary>
        /// <param name="x_end">終點位置X</param>
        /// <param name="y_end">終點位置Y</param>
        /// <param name="x_vector">相對圓心位置X</param>
        /// <param name="y_vector">相對圓心位置Y</param>
        /// <param name="speed">加工速度</param>
        /// <returns></returns>
        RetErr CW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed);
    }

    //只有A3200需要使用，用來執行PGM檔
    interface IExecuteFile
    {
        /// <summary>
        /// 執行檔案，若成功則回傳true
        /// </summary>
        /// <param name="filePath">執行檔案的路徑</param>
        /// <returns>Return 'true' if excuted result is good.</returns>
        RetErr ExeFile(string filePath);
    }

    interface ILaser
    {
        /// <summary>
        /// 開啟雷射
        /// </summary>
        /// <param name="isOn">true表示開啟雷射</param>
        /// <returns></returns>
        RetErr LaserLunch(bool isOn);
    }

    //Delegate    
    public delegate void AxesInfoRecvCallbackDelegate(AxesInfoCallbackArgs[] args);                  //軸即時資訊回傳    
    public delegate void MotionDoneCallbackDelegate(RetErr ret, AxesExecuteCallbackArgs exeArgs);    //移動完成時的事件
    public delegate void StatusChange(object args);

    //軸的資訊事件的變數
    public class AxesInfoCallbackArgs
    {
        public string AxisName;
        public bool Enabled;
        public bool Homed;
        public bool Fault;
        public double Position;
        public double Velocity;
    }

    public class AxesExecuteCallbackArgs
    {
        public AxesExecuteCallbackArgs(OperationType operation)
        {
            this.Operation = operation;
        }
        public OperationType Operation { get; }

        public enum OperationType { XY_LINEAR, Z_MOVE, HOME_ALL, HOME_SINGLE, CCW_RADIUS, CW_RADIUS, CCW_CENTER, CW_CENTER, RESET_CONTROLLER }
    }
}

namespace BackLight.Axis.ErrCode
{
    class Num   // -1 ~ -300
    {
        public static int _Open = -1;
        public static int _Close = -2;
        public static int _ReadPara = -3;
        public static int _XYLine = -4;
        public static int _ZMove = -5;
        public static int _FreeRun = -6;
        public static int _FreeRunStop = -7;
        public static int _MotionStop = -8;
        public static int _AxisDisable = -9;
        public static int _Home = -10;
        public static int _GetHome = -11;
        public static int _SetNotHome = -12;
        public static int _WaitMotionDone = -13;
        public static int _GetErrStatus = -14;
        public static int _SetWorkingPos = -15;
        public static int _GetPos = -16;
        public static int _GetAnglogInput = -17;
        public static int _SetAnalogOutput = -18;
        public static int _Jog = -19;
        public static int _AxisEnable = -20;
        public static int _XYJog = -21;
        public static int _Home_OneAxis = -22;
        public static int _CW_Radius = -23;
        public static int _CCW_Radius = -24;
        public static int _CW_Center = -25;
        public static int _CCW_Center = -26;
        public static int _ResetAlarm = -27;
        public static int _ResetAxisController = -28;
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
            string pat = Application.StartupPath + "\\Log\\Axis_Log.log";
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

    class Status
    {

        public CurrPos sCurrPos;
        public isHomed sIsHomed;
        public AxisStatus cAxisStatus;

        public void init()
        {
            sCurrPos = new CurrPos();
            sIsHomed = new isHomed();
            cAxisStatus = new AxisStatus();
        }
    }

    struct CurrPos
    {
        public Point3D cPoint3D;
    }
    struct isHomed
    {
        public bool X;
        public bool Y;
        public bool Z;
    }
    class AxisStatus
    {
        public AxisStatusMeg X = new AxisStatusMeg();
        public AxisStatusMeg Y = new AxisStatusMeg();
        public AxisStatusMeg Z = new AxisStatusMeg();
    }
    struct AxisStatusMeg
    {
        public bool IsError;
        public string Meg;
    }
    public class Point3D
    {
        public double X = 0;
        public double Y = 0;
        public double Z = 0;
        public Point3D transform()
        {
            Point3D tamp = new Point3D();
            tamp.X = Convert.ToSingle(X);
            tamp.Y = Convert.ToSingle(Y);
            tamp.Z = Convert.ToSingle(Z);
            return tamp;
        }
    }
}