using System;
using BackLight.Axis.ErrCode;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace BackLight.Axis.Servetronic
{
    class Axis_Servertronic:IAxis
    {
        #region Method

        public override RetErr Open()
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr Close()
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr ResetAlarm()
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr ReadPara()
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr XYLine(double pX, double pY, double Velocity, double ACC)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr ZMove(double pZ, double Velocity, double ACC)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr FreeRun(AxisName Axis, double Velocity)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr FreeRunStop(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }

        public override RetErr Jog(AxisName Axis, double distance, double Velocity)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }

        public override RetErr XYJog(PointF distance, double Velocity)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }

        public override RetErr MotionStop()
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr AxisEnable(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr AxisDisable(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr Home()
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr Home(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr GetHome(AxisName Axis, out Boolean status)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            status = false;
            return ret;

        }
        public override RetErr SetNotHome(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr WaitMotionDone(AxisName Axis)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr GetErrStatus(AxisName Axis, out string Status)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            Status = "";
            return ret;

        }
        public override RetErr SetWorkingPos(double pX, double pY)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr GetPos(out Point3D point)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            point = new Point3D();
            return ret;

        }
        public override RetErr GetAnglogInput(int index, AxisName axis, out double Value)
        {
            RetErr ret = new RetErr();

            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            Value = 0;
            return ret;

        }
        public override RetErr SetAnalogOutput(int index, AxisName axis, double Value)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr CCW_Radius(double x_end, double y_end, double radius, double speed)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr CW_Radius(double x_end, double y_end, double radius, double speed)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr CCW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }
        public override RetErr CW_Center(double x_end, double y_end, double x_vector, double y_vector, double speed)
        {
            RetErr ret = new RetErr();
            try
            {
                throw new Exception("Cann't find set axis not home Cmd!");
            }
            catch (Exception ee)
            {
            }
            return ret;

        }

        #endregion
    }
}
