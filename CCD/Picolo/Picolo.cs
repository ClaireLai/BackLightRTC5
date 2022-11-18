
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using BackLight.CCD.Picolo;
using Euresys.Open_eVision_2_15;
using BackLight.CCD;
using BackLight.CCD.ErrCode;
using BackLight.CCD.Define;
using System.Collections.Generic;
//using Euresys.Picolo;

namespace BackLight.CCD.Picolo
{    class CCD_Picolo : ICCD
    {

        ///////////////////////////////////////////////////////
        ///                     Para                        ///
        ///////////////////////////////////////////////////////
        #region Para

        private bool m_bDragging = false; //是否被拖拉       
        //public float show_Img.fZoomX = 1.0f;
        //public float show_Img.fZoomY = 1.0f;
        private bool channelactive = false;
        //public int eX = 0;
        //public int eY = 0;
        // The MultiCam object that contains the acquired buffer
        private UInt32 currentSurface;

        // The Mutex object that will protect image objects during processing
        private static Mutex imageMutex = new Mutex();
        
        private EImageBW8 _image = new EImageBW8(640, 480);     // 即時影像
        private EImageBW8 m_Source = new EImageBW8(640, 480); // copy find mark用
        private EImageBW8 m_Source2 = new EImageBW8(640, 480); // copy 即時影像處理
        private EImageBW8 m_Source3 = new EImageBW8(640, 480); // copy + gain=1.5
        private EROIBW8 _roi = new EROIBW8();         // 即時影像中ROI
        private EImageBW8 _imagePattern = new EImageBW8();     // Pattern
        private EMatcher m_Matcher = new EMatcher();
        private ELine measureLine = new ELine();
        private EDragHandle _handle;      
        private ECircleGauge m_CirGauger = new ECircleGauge();
        private ELineGauge m_LineGauger1 = new ELineGauge();
        private ELineGauge m_LineGauger2 = new ELineGauge();
        private ELineGauge m_LineGauger3 = new ELineGauge();
        private ELineGauge m_LineGauger4 = new ELineGauge();
        private ELineGauge m_LineGauger_Drag = new ELineGauge();
        private drag dragType;
        private delegate void PaintDelegate(Graphics g);

        private Panel ctrl = null;
        // The MultiCam object that controls the acquisition
        private UInt32 channel;
        //UInt32 channel;
        Picolo.MultiCam.MC.CALLBACK multiCamCallback;
        public Show_Img show_Img = new Define.Show_Img();
        //public s_lineGaugePara GaugePara = new Define.s_lineGaugePara();
        Log log = new Log();
        //public Picolo.Define.Show_Img show_Img = new Define.Show_Img();
        public s_MarkPara GaugePara = new s_MarkPara();
        public PointF PInterSection = new PointF(0, 0);
        //public PointF PInterSection;
        public event CCDMouseMoveCallbackDelegate CCDMouseMoveInfoRecv;
        private int x1, y1, x2, y2, x3, y3, x4, y4, len;
        #endregion

        ///////////////////////////////////////////////////////
        ///                   Subrouting                    ///
        ///////////////////////////////////////////////////////
        #region Subrouting
        //public event CCDMouseMoveCallbackDelegate CCDMouseMoveInfoRecv;

        private void ctrl_MouseUp(object sender, MouseEventArgs e)
        {
            _handle = EDragHandle.NoHandle;
            m_bDragging = false;

            if (dragType == drag.Line_L)
            {
                GaugePara.LineL_Tol = m_LineGauger_Drag.Tolerance;
                GaugePara.LineL_Len = m_LineGauger_Drag.Length;
                GaugePara.LineL_Ang = m_LineGauger_Drag.Angle;
                GaugePara.CenterX_LinL = m_LineGauger_Drag.CenterX;
                GaugePara.CenterY_LinL = m_LineGauger_Drag.CenterY;
            }
            else if (dragType == drag.Line_D)
            {
                GaugePara.LineD_Tol = m_LineGauger_Drag.Tolerance;
                GaugePara.LineD_Len = m_LineGauger_Drag.Length;
                GaugePara.LineD_Ang = m_LineGauger_Drag.Angle;
                GaugePara.CenterX_LinD = m_LineGauger_Drag.CenterX;
                GaugePara.CenterY_LinD = m_LineGauger_Drag.CenterY;
            }
            else if (dragType == drag.Line_R)
            {
                GaugePara.LineR_Tol = m_LineGauger_Drag.Tolerance;
                GaugePara.LineR_Len = m_LineGauger_Drag.Length;
                GaugePara.LineR_Ang = m_LineGauger_Drag.Angle;
                GaugePara.CenterX_LinR = m_LineGauger_Drag.CenterX;
                GaugePara.CenterY_LinR = m_LineGauger_Drag.CenterY;
            }
            else if (dragType == drag.Line_U)
            {
                GaugePara.LineU_Tol = m_LineGauger_Drag.Tolerance;
                GaugePara.LineU_Len = m_LineGauger_Drag.Length;
                GaugePara.LineU_Ang = m_LineGauger_Drag.Angle;
                GaugePara.CenterX_LinU = m_LineGauger_Drag.CenterX;
                GaugePara.CenterY_LinU = m_LineGauger_Drag.CenterY;
            }
            else if (dragType == drag.Cir)
            {
                GaugePara.Tolerance_Circle = m_CirGauger.Tolerance;
                GaugePara.Diameter_Circle = m_CirGauger.Diameter;
            }
        }

        private void ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            PointF point = new PointF();
            show_Img.ieX = e.X;
            show_Img.ieY = e.Y;
            point.X = e.X;
            point.Y = e.Y;
            //return;
            if (dragType == drag.Cir)
            {
                if (m_bDragging)
                {
                    m_CirGauger.Drag(e.X, e.Y);

                    GaugePara.Tolerance_Circle = m_CirGauger.Tolerance;
                    GaugePara.Diameter_Circle = m_CirGauger.Diameter;

                    try
                    {
                        m_CirGauger.Measure(m_Source);
                    }
                    catch (EException)
                    {
                        return;
                    }
                   // Redraw(ctrl.CreateGraphics());
                }
                else
                {
                    m_CirGauger.SetCursor(e.X, e.Y);
                    m_CirGauger.HitTest();
                }
            }
            else if (dragType == drag.Line_L|| dragType == drag.Line_D|| dragType == drag.Line_R|| dragType == drag.Line_U)
            {
                if (m_bDragging)
                {
                    m_LineGauger_Drag.Drag(e.X, e.Y);                    
                    m_LineGauger_Drag.Measure(m_Source);
                }
                else
                {
                    m_LineGauger_Drag.SetCursor(e.X, e.Y);
                    m_LineGauger_Drag.HitTest();
                }
            }        


            if (!_roi.IsVoid)
            {

                if (_handle != EDragHandle.NoHandle)
                    _roi.Drag(_handle, e.X, e.Y, show_Img.fZoomX, show_Img.fZoomY);

                switch (_roi.HitTest(e.X, e.Y, show_Img.fZoomX, show_Img.fZoomY))
                {
                    case EDragHandle.North:
                        ctrl.Cursor = Cursors.SizeNS;
                        break;
                    case EDragHandle.South:
                        ctrl.Cursor = Cursors.SizeNS;
                        break;
                    case EDragHandle.West:
                        ctrl.Cursor = Cursors.SizeWE;
                        break;
                    case EDragHandle.East:
                        ctrl.Cursor = Cursors.SizeWE;
                        break;
                    case EDragHandle.NorthEast:
                        ctrl.Cursor = Cursors.SizeNESW;
                        break;
                    case EDragHandle.SouthWest:
                        ctrl.Cursor = Cursors.SizeNESW;
                        break;
                    case EDragHandle.NorthWest:
                        ctrl.Cursor = Cursors.SizeNWSE;
                        break;
                    case EDragHandle.SouthEast:
                        ctrl.Cursor = Cursors.SizeNWSE;
                        break;
                    case EDragHandle.Inside:
                        ctrl.Cursor = Cursors.SizeAll;
                        break;
                    case EDragHandle.NoHandle:
                        ctrl.Cursor = Cursors.Arrow;
                        break;
                }

            }
            if (!channelactive) return;
            CCDMouseMoveInfoRecv?.Invoke(point);

        }

        private void ctrl_MouseDown(object sender, MouseEventArgs e)
        {
            m_bDragging = true;
            if (show_Img.bolShowROI)
                _handle = _roi.HitTest(e.X, e.Y, show_Img.fZoomX, show_Img.fZoomY);       

            if (m_LineGauger1.HitTest())
            {
                m_LineGauger_Drag = m_LineGauger1;
                dragType = drag.Line_L;
            }
            else if (m_LineGauger2.HitTest())
            {
                m_LineGauger_Drag = m_LineGauger2;
                dragType = drag.Line_D;
            }
            else if (m_LineGauger3.HitTest())
            {
                m_LineGauger_Drag = m_LineGauger3;
                dragType = drag.Line_R;
            }
            else if (m_LineGauger4.HitTest())
            {
                m_LineGauger_Drag = m_LineGauger4;
                dragType = drag.Line_U;
            }
            else if (m_CirGauger.HitTest())
            {
                dragType = drag.Cir;
            }
        }

        private void MultiCamCallback(ref Picolo.MultiCam.MC.SIGNALINFO signalInfo)
        {
            switch (signalInfo.Signal)
            {
                case Picolo.MultiCam.MC.SIG_SURFACE_PROCESSING:
                    ProcessingCallback(signalInfo);
                    break;
                case Picolo.MultiCam.MC.SIG_ACQUISITION_FAILURE:
                    AcqFailureCallback(signalInfo);
                    break;
                case Picolo.MultiCam.MC.SIG_END_CHANNEL_ACTIVITY:
                    channelactive = false;
                    break;
                default:
                    throw new Picolo.MultiCamException("Unknown signal");
            }
        }
        private void ProcessingCallback(Picolo.MultiCam.MC.SIGNALINFO signalInfo)
        {
            UInt32 currentChannel = (UInt32)signalInfo.Context;

            //statusBar.Text = "Processing";
            currentSurface = signalInfo.SignalInfo;

            // + PicoloVideo Sample Program

            try
            {
                // Update the image with the acquired image buffer data 
                Int32 width, height, bufferPitch;
                IntPtr bufferAddress;
                Picolo.MultiCam.MC.GetParam(currentChannel, "ImageSizeX", out width);
                Picolo.MultiCam.MC.GetParam(currentChannel, "ImageSizeY", out height);
                Picolo.MultiCam.MC.GetParam(currentChannel, "BufferPitch", out bufferPitch);
                Picolo.MultiCam.MC.GetParam(currentSurface, "SurfaceAddr", out bufferAddress);

                try
                {
                    imageMutex.WaitOne();

                    //image = new Bitmap(width, height, bufferPitch, PixelFormat.Format24bppRgb, bufferAddress);
                    _image.SetImagePtr(width, height, bufferAddress);

                    /* Insert image analysis and processing code here */
                }
                finally
                {
                    imageMutex.ReleaseMutex();
                }

                // Retrieve the frame rate
                Double frameRate_Hz;
                Picolo.MultiCam.MC.GetParam(channel, "PerSecond_Fr", out frameRate_Hz);

                // Retrieve the channel state
                String channelState;
                Picolo.MultiCam.MC.GetParam(channel, "ChannelState", out channelState);

                // Display frame rate and channel state
                //statusBar.Text = String.Format("Frame Rate: {0:f2}, Channel State: {1}", frameRate_Hz, channelState);

                // Display the new image
                ctrl.BeginInvoke(new PaintDelegate(Redraw), new object[1] { ctrl.CreateGraphics() });
                //this.BeginInvoke(new PaintDelegate(Redraw), new object[1] { CreateGraphics() });
            }
            catch (Picolo.MultiCamException exc)
            {
                MessageBox.Show(exc.Message, "MultiCam Exception");
            }
            catch (System.Exception exc)
            {
                MessageBox.Show(exc.Message, "System Exception");
            }
        }

        private void GetRuleScalePoint(out List<int> Px, out List<int> Py, out int NX, out int NY)
        {
            double ptmm = show_Img.ptmm;
            int w = 640; int h = 480;
            double pitch = 0.1;//mm
            int Nx = (int)Math.Floor((decimal)(((w / 2) * ptmm) / pitch));//半邊個數 6
            int Ny = (int)Math.Floor((decimal)(((h / 2) * ptmm) / pitch));//4

            List<int> PX = new List<int>();//每一個X軸刻度位置 pixel
            List<int> PY = new List<int>();//每一個Y軸刻度位置 pixel        
            for (int i = 0; i < 2 * Nx + 1; i++)
            {
                PX.Add(Convert.ToInt32(((w / 2 * ptmm) / pitch - Nx + i) / (ptmm / pitch)));
            }
            for (int i = 0; i < 2 * Ny + 1; i++)
            {
                PY.Add(Convert.ToInt32(((h / 2 * ptmm) / pitch - Ny + i) / (ptmm / pitch)));
            }
            Px = PX;
            Py = PY;
            NX = Nx;
            NY = Ny;
        }

        private void Redraw(Graphics g)
        {
            var greenPen = new ERGBColor(0, 255, 0);
            var YellowPen = new Pen(Color.Yellow);
            var RedPen = new Pen(Color.DeepPink);
            // + PicoloVideo Sample Program
            if (_image.IsVoid) return;

            try
            {
                imageMutex.WaitOne();
                //EasyImage.Copy(_image, m_Source3);
                //EasyImage.GainOffset(m_Source3, m_Source2, 1.5f, 0);
                EasyImage.Copy(_image, m_Source2);
                if (show_Img.bolShowThreadhold)
                {
                    EasyImage.Threshold(m_Source2, m_Source2, show_Img.UThreadhold);
                }


                if (true)
                {
                    EasyImage.GainOffset(m_Source2, m_Source2, (float)GaugePara.Gain, (float)GaugePara.offset);
                }
                if (show_Img.bolShowFlipX && show_Img.bolShowFlipX)
                {
                    EasyImage.Flip(m_Source2, m_Source2, EFlipAxis.EFlip_Both_Axis);
                }
                else if (show_Img.bolShowFlipX)
                {
                    EasyImage.Flip(m_Source2, m_Source2, EFlipAxis.EFlip_Vertical_Axis);
                }
                else if (show_Img.bolShowFlipY)
                {
                    EasyImage.Flip(m_Source2, m_Source2, EFlipAxis.EFlip_Horinzontal_Axis);
                }
                m_Source2.Draw(g, show_Img.fZoomX, show_Img.fZoomY);

                if (show_Img.bolShowROI)
                    _roi.DrawFrame(g, true, show_Img.fZoomX, show_Img.fZoomY);

                if (show_Img.bolShowMatch)
                    m_Matcher.DrawPositions(g, greenPen, true, show_Img.fZoomX, show_Img.fZoomY);

                if (show_Img.bolShowInterSetion && (PInterSection.X != 0 || PInterSection.Y != 0))
                {
                    //劃十字
                    len = 5;
                    x1 = (int)((PInterSection.X - len) > 0 ? (PInterSection.X - len) : 0);
                    y1 = (int)(PInterSection.Y);
                    x2 = (int)((PInterSection.X + len) < 640 ? (PInterSection.X + len) : 640);
                    y2 = (int)(PInterSection.Y);
                    x3 = (int)(PInterSection.X);
                    y3 = (int)((PInterSection.Y - len) > 0 ? (PInterSection.Y - len) : 0);
                    x4 = (int)(PInterSection.X);
                    y4 = (int)((PInterSection.Y + len) < 480 ? (PInterSection.Y + len) : 480);
                    g.DrawLine(RedPen, x1 * show_Img.fZoomX, y1 * show_Img.fZoomY, x2 * show_Img.fZoomX, y2 * show_Img.fZoomY);
                    g.DrawLine(RedPen, x3 * show_Img.fZoomX, y3 * show_Img.fZoomY, x4 * show_Img.fZoomX, y4 * show_Img.fZoomY);
                }
                //g.DrawLine(RedPen,
                if (show_Img.bolShowCirGauge)
                {
                    //Draw the Circle Gauge                    
                    m_CirGauger.SetZoom(show_Img.fZoomX, show_Img.fZoomY);
                    m_CirGauger.Draw(g);
                    // Draw the fitted circle
                    m_CirGauger.Draw(g, EDrawingMode.Actual);
                }
                if (show_Img.bolShowLineGauge)
                {
                    if (show_Img.bolShowLineGauge_L)
                    {
                        m_LineGauger1.SetZoom(show_Img.fZoomX, show_Img.fZoomY);
                        m_LineGauger1.Draw(g);
                        // Draw the fitted circle
                        m_LineGauger1.Draw(g, EDrawingMode.Actual);
                    }
                    if (show_Img.bolShowLineGauge_D)
                    {
                        m_LineGauger2.SetZoom(show_Img.fZoomX, show_Img.fZoomY);
                        m_LineGauger2.Draw(g);
                        // Draw the fitted circle
                        m_LineGauger2.Draw(g, EDrawingMode.Actual);
                    }
                    if (show_Img.bolShowLineGauge_R)
                    {
                        m_LineGauger3.SetZoom(show_Img.fZoomX, show_Img.fZoomY);
                        m_LineGauger3.Draw(g);
                        // Draw the fitted circle
                        m_LineGauger3.Draw(g, EDrawingMode.Actual);
                    }
                    if (show_Img.bolShowLineGauge_U)
                    {
                        m_LineGauger4.SetZoom(show_Img.fZoomX, show_Img.fZoomY);
                        m_LineGauger4.Draw(g);
                        // Draw the fitted circle
                        m_LineGauger4.Draw(g, EDrawingMode.Actual);
                    }
                }
                if (show_Img.bolShowTen)
                {

                    double ptmm = show_Img.ptmm;
                    int w = 640; int h = 480;
                    //int Nx = (int)Math.Floor((decimal)((w / 2) * ptmm));
                    //int Ny = (int)Math.Floor((decimal)((h / 2) * ptmm));

                    //List<int> PX = new List<int>();//每一個X軸刻度位置 pixel
                    //List<int> PY = new List<int>();//每一個Y軸刻度位置 pixel
                    GetRuleScalePoint(out List<int> Px, out List<int> Py, out int Nx, out int Ny);
                    g.DrawLine(RedPen, w / 2 * show_Img.fZoomX, h * show_Img.fZoomY, w / 2 * show_Img.fZoomX, 0 * show_Img.fZoomY);
                    g.DrawLine(RedPen, 0 * show_Img.fZoomX, h / 2 * show_Img.fZoomY, w * show_Img.fZoomX, h / 2 * show_Img.fZoomY);
                    for (int i = 0; i < 2 * Nx + 1; i++)
                    {
                        Px.Add(Convert.ToInt32((w / 2 * ptmm - Nx + i) / ptmm));
                        if (i < Nx)
                        {
                            if ((Nx - i) % 10 == 0)
                                g.DrawLine(RedPen, Px[i] * show_Img.fZoomX, (h / 2 + h / 30) * show_Img.fZoomY, Px[i] * show_Img.fZoomX, h / 2 * show_Img.fZoomY);
                            else if ((Nx - i) % 5 == 0)
                                g.DrawLine(RedPen, Px[i] * show_Img.fZoomX, (h / 2 + h / 50) * show_Img.fZoomY, Px[i] * show_Img.fZoomX, h / 2 * show_Img.fZoomY);
                            else
                                g.DrawLine(RedPen, Px[i] * show_Img.fZoomX, (h / 2 + h / 120) * show_Img.fZoomY, Px[i] * show_Img.fZoomX, h / 2 * show_Img.fZoomY);
                        }
                        else
                        {
                            if ((i - Nx) % 10 == 0)
                                g.DrawLine(RedPen, Px[i] * show_Img.fZoomX, (h / 2 - h / 30) * show_Img.fZoomY, Px[i] * show_Img.fZoomX, h / 2 * show_Img.fZoomY);
                            else if ((i - Nx) % 5 == 0)
                                g.DrawLine(RedPen, Px[i] * show_Img.fZoomX, (h / 2 - h / 50) * show_Img.fZoomY, Px[i] * show_Img.fZoomX, h / 2 * show_Img.fZoomY);
                            else
                                g.DrawLine(RedPen, Px[i] * show_Img.fZoomX, (h / 2 - h / 120) * show_Img.fZoomY, Px[i] * show_Img.fZoomX, h / 2 * show_Img.fZoomY);
                        }

                    }
                    for (int i = 0; i < 2 * Ny + 1; i++)
                    {
                        Py.Add(Convert.ToInt32((h / 2 * ptmm - Ny + i) / ptmm));
                        if (i < Ny)
                        {
                            if ((Ny - i) % 10 == 0)
                                g.DrawLine(RedPen, (w / 2 - h / 30) * show_Img.fZoomX, Py[i] * show_Img.fZoomY, w / 2 * show_Img.fZoomX, Py[i] * show_Img.fZoomY);
                            else if ((Ny - i) % 5 == 0)
                                g.DrawLine(RedPen, (w / 2 - h / 50) * show_Img.fZoomX, Py[i] * show_Img.fZoomY, w / 2 * show_Img.fZoomX, Py[i] * show_Img.fZoomY);
                            else
                                g.DrawLine(RedPen, (w / 2 - h / 120) * show_Img.fZoomX, Py[i] * show_Img.fZoomY, w / 2 * show_Img.fZoomX, Py[i] * show_Img.fZoomY);
                        }
                        else
                        {
                            if ((i - Ny) % 10 == 0)
                                g.DrawLine(RedPen, (w / 2 + h / 30) * show_Img.fZoomX, Py[i] * show_Img.fZoomY, w / 2 * show_Img.fZoomX, Py[i] * show_Img.fZoomY);

                            else if ((i - Ny) % 5 == 0)
                                g.DrawLine(RedPen, (w / 2 + h / 50) * show_Img.fZoomX, Py[i] * show_Img.fZoomY, w / 2 * show_Img.fZoomX, Py[i] * show_Img.fZoomY);
                            else
                                g.DrawLine(RedPen, (w / 2 + h / 120) * show_Img.fZoomX, Py[i] * show_Img.fZoomY, w / 2 * show_Img.fZoomX, Py[i] * show_Img.fZoomY);
                        }
                    }
                    g.DrawLine(RedPen, Px[0] * show_Img.fZoomX, (h / 2 - h / 30) * show_Img.fZoomY, Px[0] * show_Img.fZoomX, (h / 2 - h / 100) * show_Img.fZoomY);
                    g.DrawLine(RedPen, Px[1] * show_Img.fZoomX, (h / 2 - h / 30) * show_Img.fZoomY, Px[1] * show_Img.fZoomX, (h / 2 - h / 100) * show_Img.fZoomY);
                    g.DrawLine(RedPen, Px[0] * show_Img.fZoomX, (h / 2 - h / 50) * show_Img.fZoomY, Px[1] * show_Img.fZoomX, (h / 2 - h / 50) * show_Img.fZoomY);
                    g.DrawString("0.1mm", new Font("新細明體", 9, FontStyle.Regular), RedPen.Brush, Px[0], h / 2 - h / 15);

                }

            }
            catch (System.Exception exc)
            {
                MessageBox.Show(exc.Message, "System Exception");
            }
            finally
            {
                imageMutex.ReleaseMutex();
            }


        }

        private void AcqFailureCallback(Picolo.MultiCam.MC.SIGNALINFO signalInfo)
        {
            UInt32 currentChannel = (UInt32)signalInfo.Context;

            // + PicoloVideo Sample Program

            try
            {
                // Display frame rate and channel state
                //statusBar.Text = String.Format("Acquisition Failure, Channel State: IDLE");
                ctrl.BeginInvoke(new PaintDelegate(Redraw), new object[1] { ctrl.CreateGraphics() });
            }
            catch (System.Exception exc)
            {
                MessageBox.Show(exc.Message, "System Exception");
            }

            // - PicoloVideo Sample Program
        }


        #endregion


        ///////////////////////////////////////////////////////
        ///                     Method                      ///
        ///////////////////////////////////////////////////////
        #region Method

        public RetErr cam_init(VID ChannelId, ref Panel ctrl1)
        {
            RetErr retErr = new RetErr();

            ctrl = ctrl1;
            string VID = "VID1";
            if (ChannelId == CCD.VID.VID1) VID = "VID1";
            else if (ChannelId == CCD.VID.VID2) VID = "VID2";
            try
            {
                // Open MultiCam driver
                Picolo.MultiCam.MC.OpenDriver();

                // Enable error logging
                Picolo.MultiCam.MC.SetParam(Picolo.MultiCam.MC.CONFIGURATION, "ErrorLog", "error.log");

                // Create a channel and associate it with the first connector on the first board
                Picolo.MultiCam.MC.Create("CHANNEL", out channel);
                Picolo.MultiCam.MC.SetParam(channel, "DriverIndex", 0);
                Picolo.MultiCam.MC.SetParam(channel, "Connector", VID);

                // Choose the video standard
                //Picolo.MultiCam.MC.SetParam(channel, "CamFile", "PAL");
                Picolo.MultiCam.MC.SetParam(channel, "CamFile", "NTSC");
                // Choose the pixel color format
                Picolo.MultiCam.MC.SetParam(channel, "ColorFormat", "Y8");

                // Choose the acquisition mode
                Picolo.MultiCam.MC.SetParam(channel, "AcquisitionMode", "VIDEO");
                // Choose the way the first acquisition is triggered
                Picolo.MultiCam.MC.SetParam(channel, "TrigMode", "IMMEDIATE");
                // Choose the triggering mode for subsequent acquisitions
                Picolo.MultiCam.MC.SetParam(channel, "NextTrigMode", "REPEAT");
                // Choose the number of images to acquire
                Picolo.MultiCam.MC.SetParam(channel, "SeqLength_Fr", Picolo.MultiCam.MC.INDETERMINATE);

                // Register the callback function
                multiCamCallback = new Picolo.MultiCam.MC.CALLBACK(MultiCamCallback);
                Picolo.MultiCam.MC.RegisterCallback(channel, multiCamCallback, channel);

                // Enable the signals corresponding to the callback functions
                Picolo.MultiCam.MC.SetParam(channel, Picolo.MultiCam.MC.SignalEnable + Picolo.MultiCam.MC.SIG_SURFACE_PROCESSING, "ON");
                Picolo.MultiCam.MC.SetParam(channel, Picolo.MultiCam.MC.SignalEnable + Picolo.MultiCam.MC.SIG_ACQUISITION_FAILURE, "ON");
                Picolo.MultiCam.MC.SetParam(channel, Picolo.MultiCam.MC.SignalEnable + Picolo.MultiCam.MC.SIG_END_CHANNEL_ACTIVITY, "ON");


                // Prepare the channel in order to minimize the acquisition sequence startup latency
                Picolo.MultiCam.MC.SetParam(channel, "ChannelState", "READY");
                _image = new EImageBW8(640, 480);     // 即時影像
                m_Source = new EImageBW8(640, 480); // copy 即時影像

                ctrl.MouseDown += new System.Windows.Forms.MouseEventHandler(ctrl_MouseDown);
                ctrl.MouseMove += new System.Windows.Forms.MouseEventHandler(ctrl_MouseMove);
                ctrl.MouseUp += new System.Windows.Forms.MouseEventHandler(ctrl_MouseUp);
            }
            catch (Picolo.MultiCamException exc)
            {
                // An exception has occurred in the try {...} block. 
                // Retrieve its description and display it in a message box.
                MessageBox.Show(exc.Message, "MultiCam Exception");
                //Close();
                retErr.Meg = exc.Message;
                retErr.Num = Num._OpenErr;
                retErr.flag = false;
                Log.Pushlist(Num._OpenErr, "cam_init", exc.Message);
                return retErr;
            }

            show_Img.fZoomX = 1.0f;
            show_Img.fZoomY = 1.0f;
            m_CirGauger.Dragable = true;
            m_CirGauger.Resizable = true;
            m_CirGauger.Rotatable = true;
            m_LineGauger1.Dragable = true;
            m_LineGauger1.Resizable = true;
            m_LineGauger1.Rotatable = true;
            m_LineGauger2.Dragable = true;
            m_LineGauger2.Resizable = true;
            m_LineGauger2.Rotatable = true;
            m_LineGauger3.Dragable = true;
            m_LineGauger3.Resizable = true;
            m_LineGauger3.Rotatable = true;
            m_LineGauger4.Dragable = true;
            m_LineGauger4.Resizable = true;
            m_LineGauger4.Rotatable = true;

            return retErr;
        }

        //public RetErr GetLineGaugPara(out s_lineGaugePara _lineGaugePara)
        //{
        //    RetErr retErr = new RetErr();
        //    s_lineGaugePara ltamp = new s_lineGaugePara();
        //    _lineGaugePara = ltamp;
        //    try
        //    {
        //        _lineGaugePara = GaugePara;
        //    }
        //    catch (EException exc)
        //    {
        //        MessageBox.Show(exc.Message);
        //        retErr.flag = false;
        //        retErr.Num = Num._GetLineGaugePara;
        //        retErr.Meg = exc.Message;
        //        Log.Pushlist(retErr.Num, "SetLineGaugePara", retErr.Meg);
        //    }
        //    return retErr;
        //}
        public RetErr ROI_Attach()
        {
            RetErr retErr = new RetErr();
            if (show_Img.bolShowROI)
            {
                _roi.SetPlacement(50, 50, 100, 100);
                if (show_Img.bolShowGainOffset || show_Img.bolShowThreadhold)
                    _roi.Attach(m_Source2);
                else
                    _roi.Attach(m_Source2);
            }
            else
            {
                _roi.Detach();
            }
            return retErr;
        }
        public RetErr CirGauge_Attach()
        {
            RetErr retErr = new RetErr();
            if (show_Img.bolShowCirGauge)
            {
                try
                {
                    // Set the transition choice to Largest Amplitude
                    m_CirGauger.TransitionChoice = ETransitionChoice.LargestAmplitude;
                    // Center the circle gauge, its diameter is a quarter of the image width
                    m_CirGauger.SetCenterXY(m_Source.Width / 2, m_Source.Height / 2);
                    m_CirGauger.Diameter = (float)GaugePara.Diameter_Circle;
                    m_CirGauger.Tolerance = (float)GaugePara.Tolerance_Circle;
                    //m_CirGauger.TransitionType = GaugePara.TransitionType;
                    //picolo.GaugePara.TransitionType = Convert.ToInt32(cmbTransitionType.SelectedIndex);
                    if (GaugePara.TransitionType == 1)
                        m_CirGauger.TransitionType = ETransitionType.Bw;
                    else if (GaugePara.TransitionType == 2)
                        m_CirGauger.TransitionType = ETransitionType.Wb;
                    else
                        m_CirGauger.TransitionType = ETransitionType.BwOrWb;
                    // Perform the measurement
                    m_CirGauger.Measure(m_Source);
                }
                catch (EException exc)
                {
                    MessageBox.Show(exc.Message);
                    //return;
                }
                // Refresh
                ctrl.Refresh();
            }
            else
            {
                m_CirGauger.Detach();
            }
            return retErr;
        }


        public RetErr cam_StartGetImage()
        {
            RetErr retErr = new RetErr();
            //throw new NotImplementedException();

            // + PicoloVideo Sample Program
            // Start an acquisition sequence by activating the channel
            String channelState;
            Picolo.MultiCam.MC.GetParam(channel, "ChannelState", out channelState);
            if (channelState != "ACTIVE")

                Picolo.MultiCam.MC.SetParam(channel, "ChannelState", "ACTIVE");
            ctrl.Refresh();
            channelactive = true;

            // - PicoloVideo Sample Program
            return retErr;
        }
        public RetErr cam_StopGetImage()
        {
            RetErr retErr = new RetErr();

            // + PicoloVideo Sample Program
            // Stop an acquisition sequence by deactivating the channel 
            try
            {
                if (channel != 0)
                    Picolo.MultiCam.MC.SetParam(channel, "ChannelState", "IDLE");
                // - PicoloVideo Sample Program

                while (channelactive == true)
                {
                    Thread.Sleep(10);
                }

                // Delete the channel
                if (channel != 0)
                {
                    Picolo.MultiCam.MC.Delete(channel);
                    channel = 0;
                }
            }
            catch (Picolo.MultiCamException exc)
            {
                MessageBox.Show(exc.Message, "MultiCam Exception");
                retErr.Meg = exc.Message;
                retErr.Num = Num._CloseErr;
                retErr.flag = false;
                Log.Pushlist(Num._CloseErr, "cam_StopGetImage", exc.Message);
            }
            return retErr;
        }

        public RetErr CopyROI(Control ctrl2)
        {
            RetErr retErr = new RetErr();
            if (!_roi.IsVoid)
            {
                try
                {

                    _roi.Save(Application.StartupPath + "\\ROI.jpg");
                    Image img;
                    using (var bmpTemp = new Bitmap(Application.StartupPath + "\\ROI.jpg"))
                    {
                        img = new Bitmap(bmpTemp);
                    }
                    _imagePattern.Load(Application.StartupPath + "\\ROI.jpg");//載入學習檔
                    ctrl2.BackgroundImageLayout = ImageLayout.Center;
                    ctrl2.BackgroundImage = img;                          // Learn the pattern defined by the ROI
                    //m_Matcher.LearnPattern(_imagePattern);
                }
                catch (System.Exception exc)
                {
                    MessageBox.Show(exc.Message, "System Exception");
                    retErr.Meg = exc.Message;
                    retErr.Num = Num._CopyROIErr;
                    retErr.flag = false;
                    Log.Pushlist(Num._CopyROIErr, "CopyROI", exc.Message);
                }
            }
            return retErr;
        }

        #region load filepath顯示到 ctrl2元件
        public RetErr LoadGoldenSample(string filepath, Control ctrl2)
        {
            RetErr retErr = new RetErr();
            bool result = System.IO.File.Exists(filepath);
            if (!result)
            {
                retErr.Meg = "GoldenSample 檔案不存在";
                retErr.Num = Num._GoldenSampleErr;
                retErr.flag = false;
                Log.Pushlist(Num._GoldenSampleErr, "LoadGoldenSample", retErr.Meg);
                return retErr;
            }

            try
            {
                ctrl2.BackgroundImageLayout = ImageLayout.Center;
                Image img;
                using (var bmpTemp = new Bitmap(filepath))
                {
                    img = new Bitmap(bmpTemp);
                }
                ctrl2.BackgroundImage = img;
            }
            catch (System.Exception exc)
            {
                MessageBox.Show(exc.Message, "System Exception");
                retErr.Meg = exc.Message;
                retErr.Num = Num._GoldenSampleErr;
                retErr.flag = false;
                Log.Pushlist(Num._GoldenSampleErr, "LoadGoldenSample", exc.Message);
                return retErr;
            }
            return retErr;
        }
        #endregion

        #region Save pattern到 filepath
        //public  RetErr SavePattern(string filepath)
        //{
        //    RetErr retErr = new RetErr();
        //    if (!_roi.IsVoid && filepath!="")
        //    _roi.Save(filepath);
        //    return retErr;
        //}

        public RetErr SaveROI(string filepath)
        {
            RetErr retErr = new RetErr();
            if (!_roi.IsVoid && filepath != "")
                _roi.Save(filepath);
            return retErr;
        }

        public RetErr SaveImage(string filepath)
        {
            //Image img;
            RetErr retErr = new RetErr();
            if (!_image.IsVoid && filepath != "")
                //using (var bmpTemp = new Bitmap(filepath))
                //{
                //    img = new Bitmap(bmpTemp);
                //}
                    _image.Save(filepath);
            return retErr;
        }
        #endregion

        #region Match 把filepath裡面的圖檔當golden sample比對後輸出 X Y
        public RetErr m_Match(string filepath, out float X, out float Y)
        {
            RetErr retErr = new RetErr();

            bool result = System.IO.File.Exists(filepath);
            if (result == false)
            {
                X = -1;
                Y = -1;
                retErr.Meg = "檔案不存在";
                retErr.Num = Num._PathErr;
                retErr.flag = false;
                Log.Pushlist(Num._PathErr, "m_Match", retErr.Meg);
                return retErr;
            }
            EMatchPosition K;
            try
            {
                _imagePattern.Load(filepath);//載入學習檔
                // Learn the pattern defined by the ROI
                m_Matcher.LearnPattern(_imagePattern);

                //EasyImage.Copy(_image, m_Source);
                // If a pattern has been learnt
                if (m_Matcher.PatternLearnt)
                    // Find the pattern in the Image

                    EasyImage.Copy(m_Source2, m_Source);
                m_Matcher.Match(m_Source);

                K = m_Matcher.GetPosition(0);

                if (K.Score > GaugePara.Score / 100)
                {
                    X = (float)Math.Round(K.CenterX, 2, MidpointRounding.AwayFromZero);
                    Y = (float)Math.Round(K.CenterY, 2, MidpointRounding.AwayFromZero);
                    return retErr;
                }
                else
                {
                    retErr.Meg = "Match NG";
                    retErr.Num = Num._MatchErr;
                    retErr.flag = false;
                    Log.Pushlist(Num._MatchErr, "m_Match", retErr.Meg);
                    X = -1;
                    Y = -1;
                    return retErr;

                }

            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                X = -1;
                Y = -1;
                retErr.Meg = exc.Message;
                retErr.Num = Num._MatchErr;
                retErr.flag = false;
                Log.Pushlist(Num._MatchErr, "m_Match", retErr.Meg);
                return retErr;
            }
        }
        #endregion

        //public  RetErr m_CircleGauge(int cirX, int cirY, int cirR, s_MarkPara GaugePara, int BW, out float X_offset, out float Y_offset)
        //{
        //    RetErr retErr = new RetErr();
        //    try
        //    {
        //        // Set the transition choice to Largest Amplitude
        //        m_CirGauger.TransitionChoice = ETransitionChoice.LargestAmplitude;
        //        // Center the circle gauge, its diameter is a quarter of the image width
        //        m_CirGauger.SetCenterXY(cirX, cirY);
        //        m_CirGauger.Diameter = cirR;
        //        m_CirGauger.Tolerance = cirR / 2;
        //        if (BW == 1)
        //            m_CirGauger.TransitionType = ETransitionType.Bw;
        //        else if (BW == 2)
        //            m_CirGauger.TransitionType = ETransitionType.Wb;
        //        else
        //            m_CirGauger.TransitionType = ETransitionType.BwOrWb;
        //        // Perform the measurement
        //        m_CirGauger.Measure(m_Source);
        //        X_offset = 320 - m_CirGauger.MeasuredCircle.Center.X;
        //        Y_offset = 240 - m_CirGauger.MeasuredCircle.Center.Y;
        //        return retErr;
        //    }
        //    catch (EException exc)
        //    {
        //        MessageBox.Show(exc.Message);
        //        m_CirGauger.Detach();
        //        retErr.Meg = exc.Message;
        //        retErr.Num = Num._GaugeErr;
        //        retErr.flag = false;
        //        Log.Pushlist(retErr.Num, "m_CircleGauge", retErr.Meg);
        //        X_offset = -1;
        //        Y_offset = -1;
        //        return retErr;
        //    }
        //}
        #endregion
        public RetErr FindMark(string AOIPath, s_MarkPara GaugePara, out bool isFind, out PointF Offset)
        {
            RetErr retErr = new RetErr();
            PointF point = new PointF(-1, -1);           
            if (!System.IO.File.Exists(AOIPath))
            {
                MessageBox.Show("沒載入Golden Sample");
                isFind = false;
                Offset = point;
                retErr.flag = false;
                retErr.Num = Num._GoldenSampleErr;
                retErr.Meg = "沒有Load golden sample";
                Log.Pushlist(retErr.Num, "FindMark", retErr.Meg);
                return retErr;
            }
            EMatchPosition K;
            try
            {
                _imagePattern.Load(AOIPath);//載入學習檔
                // Learn the pattern defined by the ROI
                m_Matcher.LearnPattern(_imagePattern);

                EasyImage.Copy(m_Source2, m_Source);
                // If a pattern has been learnt
                if (m_Matcher.PatternLearnt)
                    // Find the pattern in the Image
                    m_Matcher.Match(m_Source);
                K = m_Matcher.GetPosition(0);
                if (K.Score > GaugePara.Score / 100)
                {
                    GaugePara.CenterX_Circle = (float)Math.Round(K.CenterX, 2, MidpointRounding.AwayFromZero);
                    GaugePara.CenterY_Circle = (float)Math.Round(K.CenterY, 2, MidpointRounding.AwayFromZero);
                    point.X = (float)GaugePara.CenterX_Circle;
                    point.Y = (float)GaugePara.CenterY_Circle;
                    isFind = true;
                    Offset = point;
                }
                else
                {
                    isFind = false;
                    Offset = point;
                    retErr.flag = false;
                    retErr.Num = Num._GaugeErr;
                    retErr.Meg = "沒有找到Circle";
                    Log.Pushlist(retErr.Num, "FindMark", retErr.Meg);
                }
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                isFind = false;
                Offset = point;
                retErr.flag = false;
                retErr.Num = Num._GaugeErr;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "FindMark", retErr.Meg);
                return retErr;
            }
            try
            {
                // Set the transition choice to Largest Amplitude
                m_CirGauger.TransitionChoice = ETransitionChoice.LargestAmplitude;
                // Center the circle gauge, its diameter is a quarter of the image width
                m_CirGauger.SetCenterXY((float)GaugePara.CenterX_Circle, (float)GaugePara.CenterY_Circle);
                m_CirGauger.Diameter = (float)GaugePara.Diameter_Circle;
                m_CirGauger.Tolerance = (float)GaugePara.Tolerance_Circle;
                if (GaugePara.TransitionType == 1)
                    m_CirGauger.TransitionType = ETransitionType.Bw;
                else if (GaugePara.TransitionType == 2)
                    m_CirGauger.TransitionType = ETransitionType.Wb;
                else
                    m_CirGauger.TransitionType = ETransitionType.BwOrWb;
                //EasyImage.GainOffset(_image, m_Source, (float)GaugePara.Gain, (float)GaugePara.offset);
                m_CirGauger.Threshold = (uint)GaugePara.Threshold;
                m_CirGauger.Thickness = (uint)GaugePara.Thickness;

                m_CirGauger.Measure(m_Source);
                point.X = 320 - m_CirGauger.MeasuredCircle.Center.X;
                point.Y = 240 - m_CirGauger.MeasuredCircle.Center.Y;
                Offset = point;
                isFind = true;
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                m_CirGauger.Detach();
                Offset = point;
                isFind = false;
                retErr.flag = false;
                retErr.Num = Num._GaugeErr;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "FindMark", retErr.Meg);
            }
            return retErr;
        }

        public RetErr Save_s_MarkPara()
        {
            RetErr retErr = new RetErr();
            try
            {
                //寫入文檔
                string pat = Application.StartupPath + "\\s_MarkPara.par";
                StreamWriter strr = new StreamWriter(pat);
                strr.WriteLine(GaugePara.Score.ToString());
                strr.WriteLine(GaugePara.Diameter_Circle.ToString());
                strr.WriteLine(GaugePara.Tolerance_Circle.ToString());
                strr.WriteLine(GaugePara.TransitionType.ToString());
                strr.WriteLine(GaugePara.Threshold.ToString());
                strr.WriteLine(GaugePara.Thickness.ToString());
                strr.WriteLine(GaugePara.Gain.ToString());
                strr.WriteLine(GaugePara.offset.ToString());
                strr.Close();
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                retErr.flag = false;
                retErr.Num = Num._GaugeParaErr;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "Save_s_MarkPara", retErr.Meg);
            }
            return retErr;
        }
        public RetErr Load_s_MarkPara()
        {
            RetErr retErr = new RetErr();
            try
            {
                //讀出文檔
                string pat = Application.StartupPath + "\\s_MarkPara.par";
                StreamReader strr = new StreamReader(pat);
                string score = strr.ReadLine();
                string Diameter_Circle = strr.ReadLine();
                string Tolerance_Circle = strr.ReadLine();
                string TransitionType = strr.ReadLine();
                string Threshold = strr.ReadLine();
                string Thickness = strr.ReadLine();
                string Gain = strr.ReadLine();
                string offset = strr.ReadLine();
                strr.Close();

                GaugePara.Score = Convert.ToDouble(score);
                GaugePara.Diameter_Circle = Convert.ToDouble(Diameter_Circle);
                GaugePara.Tolerance_Circle = Convert.ToDouble(Tolerance_Circle);
                GaugePara.TransitionType = Convert.ToDouble(TransitionType);
                GaugePara.Threshold = Convert.ToDouble(Threshold);
                GaugePara.Thickness = Convert.ToDouble(Thickness);
                GaugePara.Gain = Convert.ToDouble(Gain);
                GaugePara.offset = Convert.ToDouble(offset);
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                retErr.flag = false;
                retErr.Num = Num._GaugeParaErr;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "Load_s_MarkPara", retErr.Meg);
            }
            return retErr;
        }
        public RetErr LineGauge_Attach(LineNum lineNum, out PointF org, out PointF end, out float Ang)
        {
            RetErr retErr = new RetErr();
            PointF orgTemp = new PointF();
            PointF endTemp = new PointF();
            float ang = 0;
            orgTemp.X = 0;
            orgTemp.Y = 0;
            endTemp.X = 0;
            endTemp.Y = 0;
            if (show_Img.bolShowLineGauge)
            {
                try
                {
                    switch (lineNum.ToString())
                    {
                        case "Line_L":
                            m_LineGauger1.SetCenterXY((float)GaugePara.CenterX_LinL, (float)GaugePara.CenterY_LinL);
                            m_LineGauger1.Length = (float)GaugePara.LineL_Len;
                            m_LineGauger1.Angle = (float)GaugePara.LineL_Ang;
                            m_LineGauger1.Tolerance = (float)GaugePara.LineL_Tol;
                            m_LineGauger1.TransitionChoice = (GaugePara.bolBE_L == true) ? ETransitionChoice.NthFromBegin : ETransitionChoice.NthFromEnd;
                            m_LineGauger1.TransitionType = (GaugePara.bolBW_L == true) ? ETransitionType.Bw : ETransitionType.Wb;
                            m_LineGauger1.Thickness = 5;

                            if (GaugePara.bolBW_L)
                                m_LineGauger1.TransitionType = ETransitionType.Bw;
                            else
                                m_LineGauger1.TransitionType = ETransitionType.Wb;
                            if (GaugePara.bolBE_L)
                                m_LineGauger1.TransitionChoice = ETransitionChoice.NthFromBegin;
                            else
                                m_LineGauger1.TransitionChoice = ETransitionChoice.NthFromEnd;
                            EasyImage.Copy(m_Source2, m_Source);
                            // Perform the measurement
                            m_LineGauger1.Measure(m_Source);
                            if (m_LineGauger1.NumMeasuredPoints > 0)
                            {
                                measureLine = m_LineGauger1.MeasuredLine;
                                orgTemp.X = measureLine.Org.X;
                                orgTemp.Y = measureLine.Org.Y;
                                endTemp.X = measureLine.End.X;
                                endTemp.Y = measureLine.End.Y;
                                ang = measureLine.Angle;
                                Ang = ang;
                            }

                            break;
                        case "Line_D":

                            m_LineGauger2.SetCenterXY((float)GaugePara.CenterX_LinD, (float)GaugePara.CenterY_LinD);
                            m_LineGauger2.Length = (float)GaugePara.LineD_Len;
                            m_LineGauger2.Angle = (float)GaugePara.LineD_Ang;
                            m_LineGauger2.Tolerance = (float)GaugePara.LineD_Tol;
                            m_LineGauger2.Thickness = 5;

                            if (GaugePara.bolBW_D)
                                m_LineGauger2.TransitionType = ETransitionType.Bw;
                            else
                                m_LineGauger2.TransitionType = ETransitionType.Wb;
                            if (GaugePara.bolBE_D)
                                m_LineGauger2.TransitionChoice = ETransitionChoice.NthFromBegin;
                            else
                                m_LineGauger2.TransitionChoice = ETransitionChoice.NthFromEnd;
                            EasyImage.Copy(m_Source2, m_Source);
                            // Perform the measurement
                            m_LineGauger2.Measure(m_Source);
                            if (m_LineGauger2.NumMeasuredPoints > 0)
                            {
                                measureLine = m_LineGauger2.MeasuredLine;
                                orgTemp.X = measureLine.Org.X;
                                orgTemp.Y = measureLine.Org.Y;
                                endTemp.X = measureLine.End.X;
                                endTemp.Y = measureLine.End.Y;
                                ang = measureLine.Angle;
                                Ang = ang;
                            }
                            break;
                        case "Line_R":

                            m_LineGauger3.SetCenterXY((float)GaugePara.CenterX_LinR, (float)GaugePara.CenterY_LinR);
                            m_LineGauger3.Length = (float)GaugePara.LineR_Len;
                            m_LineGauger3.Angle = (float)GaugePara.LineR_Ang;
                            m_LineGauger3.Tolerance = (float)GaugePara.LineR_Tol;
                            m_LineGauger3.Thickness = 5;

                            if (GaugePara.bolBW_R)
                                m_LineGauger3.TransitionType = ETransitionType.Bw;
                            else
                                m_LineGauger3.TransitionType = ETransitionType.Wb;
                            if (GaugePara.bolBE_R)
                                m_LineGauger3.TransitionChoice = ETransitionChoice.NthFromBegin;
                            else
                                m_LineGauger3.TransitionChoice = ETransitionChoice.NthFromEnd;
                            EasyImage.Copy(m_Source2, m_Source);
                            // Perform the measurement
                            m_LineGauger3.Measure(m_Source);
                            if (m_LineGauger3.NumMeasuredPoints > 0)
                            {
                                measureLine = m_LineGauger3.MeasuredLine;
                                orgTemp.X = measureLine.Org.X;
                                orgTemp.Y = measureLine.Org.Y;
                                endTemp.X = measureLine.End.X;
                                endTemp.Y = measureLine.End.Y;
                                ang = measureLine.Angle;
                                Ang = ang;
                            }
                            break;
                        case "Line_U":

                            m_LineGauger4.SetCenterXY((float)GaugePara.CenterX_LinU, (float)GaugePara.CenterY_LinU);
                            m_LineGauger4.Length = (float)GaugePara.LineU_Len;
                            m_LineGauger4.Angle = (float)GaugePara.LineU_Ang;
                            m_LineGauger4.Tolerance = (float)GaugePara.LineU_Tol;
                            m_LineGauger4.Thickness = 5;

                            if (GaugePara.bolBW_U)
                                m_LineGauger4.TransitionType = ETransitionType.Bw;
                            else
                                m_LineGauger4.TransitionType = ETransitionType.Wb;
                            if (GaugePara.bolBE_U)
                                m_LineGauger4.TransitionChoice = ETransitionChoice.NthFromBegin;
                            else
                                m_LineGauger4.TransitionChoice = ETransitionChoice.NthFromEnd;
                            EasyImage.Copy(m_Source2, m_Source);
                            // Perform the measurement
                            m_LineGauger4.Measure(m_Source);
                            if (m_LineGauger4.NumMeasuredPoints > 0)
                            {
                                measureLine = m_LineGauger4.MeasuredLine;
                                orgTemp.X = measureLine.Org.X;
                                orgTemp.Y = measureLine.Org.Y;
                                endTemp.X = measureLine.End.X;
                                endTemp.Y = measureLine.End.Y;
                                ang = measureLine.Angle;
                                Ang = ang;
                            }
                            break;
                    }
                    //EasyImage.Copy(m_Source2, m_Source);
                    //// Perform the measurement
                    //m_LineGauger1.Measure(m_Source);
                    //if (m_LineGauger1.NumMeasuredPoints > 0)
                    //{
                    //    measureLine = m_LineGauger1.MeasuredLine;
                    //    orgTemp.X = measureLine.Org.X;
                    //    orgTemp.Y = measureLine.Org.Y;
                    //    endTemp.X = measureLine.End.X;
                    //    endTemp.Y = measureLine.End.Y;
                    //    ang = measureLine.Angle;
                    //    Ang = ang;
                    //}
                }
                catch (EException exc)
                {
                    MessageBox.Show(exc.Message);
                    //return;
                }
                //Refresh
                ctrl.Refresh();
            }
            else
            {
                m_LineGauger1.Detach();
                m_LineGauger2.Detach();
                m_LineGauger3.Detach();
                m_LineGauger4.Detach();
            }
            org = orgTemp;
            end = endTemp;
            Ang = ang;
            return retErr;
        }
        public RetErr SetShowStatus(Show_Img Status)
        {
            RetErr retErr = new RetErr();
            try
            {
                show_Img = Status;
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                retErr.flag = false;
                retErr.Num = Num._SetShowStatus;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "SetGaugePara", retErr.Meg);
            }
            return retErr;
        }
        public RetErr GetShowStatus(out Show_Img Status)
        {
            RetErr retErr = new RetErr();

            try
            {
                Status = show_Img;
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                retErr.flag = false;
                retErr.Num = Num._GetShowStatus;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "GetShowStatus", retErr.Meg);
            }
            Status = show_Img;
            return retErr;
        }
        public RetErr SetGaugePara(s_MarkPara Para)
        {
            RetErr retErr = new RetErr();
            try
            {
                GaugePara = Para;
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                retErr.flag = false;
                retErr.Num = Num._SetGaugePara;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "SetGaugePara", retErr.Meg);
            }
            return retErr;
        }
        public RetErr SetRECCenter(PointF pIntersection)
        {
            RetErr retErr = new RetErr();
            PInterSection = pIntersection;
            return retErr;
        }
        public RetErr GetIntersection(PointF lineFirstStar, PointF lineFirstEnd, PointF lineSecondStar, PointF lineSecondEnd, out bool bolFind, out PointF PIntersection)
        {
            /*
             * L1，L2都存在斜率的情況：
             * 直線方程L1: ( y - y1 ) / ( y2 - y1 ) = ( x - x1 ) / ( x2 - x1 ) 
             * => y = [ ( y2 - y1 ) / ( x2 - x1 ) ]( x - x1 ) + y1
             * 令 a = ( y2 - y1 ) / ( x2 - x1 )
             * 有 y = a * x - a * x1 + y1   .........1
             * 直線方程L2: ( y - y3 ) / ( y4 - y3 ) = ( x - x3 ) / ( x4 - x3 )
             * 令 b = ( y4 - y3 ) / ( x4 - x3 )
             * 有 y = b * x - b * x3 + y3 ..........2
             * 
             * 如果 a = b，則兩直線平等，否則， 聯解方程 1,2，得:
             * x = ( a * x1 - b * x3 - y1 + y3 ) / ( a - b )
             * y = a * x - a * x1 + y1
             * 
             * L1存在斜率, L2平行Y軸的情況：
             * x = x3
             * y = a * x3 - a * x1 + y1
             * 
             * L1 平行Y軸，L2存在斜率的情況：
             * x = x1
             * y = b * x - b * x3 + y3
             * 
             * L1與L2都平行Y軸的情況：
             * 如果 x1 = x3，那麼L1與L2重合，否則平等
             * 
            */
            RetErr retrr = new RetErr();
            //Point PTemp = new Point(0, 0);
            float a = 0, b = 0;
            int state = 0;
            bolFind = false;
            if (lineFirstStar.X != lineFirstEnd.X)
            {
                a = (lineFirstEnd.Y - lineFirstStar.Y) / (lineFirstEnd.X - lineFirstStar.X);
                state |= 1;
            }
            if (lineSecondStar.X != lineSecondEnd.X)
            {
                b = (lineSecondEnd.Y - lineSecondStar.Y) / (lineSecondEnd.X - lineSecondStar.X);
                state |= 2;
            }
            switch (state)
            {
                case 0: //L1與L2都平行Y軸
                    {
                        if (lineFirstStar.X == lineSecondStar.X)
                        {
                            //throw new Exception("兩條直線互相重合，且平行於Y軸，無法計算交點。");
                            //return new PointF(0, 0);
                            //PTemp.X = 0;
                            //PTemp.Y = 0;
                            bolFind = false;

                        }
                        else
                        {
                            //throw new Exception("兩條直線互相平行，且平行於Y軸，無法計算交點。");
                            //PTemp.X = 0;
                            //PTemp.Y = 0;
                            bolFind = false;
                        }
                    }
                    break;
                case 1: //L1存在斜率, L2平行Y軸
                    {
                        float x = lineSecondStar.X;
                        float y = (lineFirstStar.X - x) * (-a) + lineFirstStar.Y;

                        //PTemp.X = 0;
                        //PTemp.Y = 0;
                        bolFind = false;
                        break;
                    }
                case 2: //L1 平行Y軸，L2存在斜率
                    {
                        float x = lineFirstStar.X;
                        //網上有相似代碼的，這一處是錯誤的。你可以對比case 1 的邏輯 進行分析
                        //源code:lineSecondStar * x + lineSecondStar * lineSecondStar.X + p3.Y;
                        float y = (lineSecondStar.X - x) * (-b) + lineSecondStar.Y;
                        PInterSection.X = (int)x;
                        PInterSection.Y = (int)y;
                        bolFind = true;
                        break;
                    }
                case 3: //L1，L2都存在斜率
                    {
                        if (a == b)
                        {
                            // throw new Exception("兩條直線平行或重合，無法計算交點。");
                            //return new PointF(0, 0);
                            //PTemp.X = 0;
                            //PTemp.Y = 0;
                            bolFind = false;
                        }
                        else
                        {
                            float x = (a * lineFirstStar.X - b * lineSecondStar.X - lineFirstStar.Y + lineSecondStar.Y) / (a - b);
                            float y = a * x - a * lineFirstStar.X + lineFirstStar.Y;
                            //return new PointF(x, y);
                            PInterSection.X = (int)x;
                            PInterSection.Y = (int)y;
                            bolFind = true;
                        }
                        break;
                    }
            }
            // throw new Exception("不可能發生的情況");
            PIntersection = PInterSection;//out 
            //PInterSection = PTemp;
            return retrr;
        }
        public RetErr GetGaugePara(out s_MarkPara Para)
        {
            RetErr retErr = new RetErr();
            s_MarkPara tamp = new s_MarkPara();
            try
            {
                tamp = GaugePara;
            }
            catch (EException exc)
            {
                MessageBox.Show(exc.Message);
                retErr.flag = false;
                retErr.Num = Num._GetGaugePara;
                retErr.Meg = exc.Message;
                Log.Pushlist(retErr.Num, "GetGaugePara", retErr.Meg);
            }
            Para = tamp;
            return retErr;
        }

    }
    enum drag
    {
        Line_L,
        Line_D,
        Line_R,
        Line_U,
        Cir
    }
}

