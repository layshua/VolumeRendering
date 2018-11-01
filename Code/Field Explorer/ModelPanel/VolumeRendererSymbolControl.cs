using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace FieldExplorer.Meteorology
{
    public class ControlPoint
    {
        public Point Point;
        public int value;
    }

    public delegate void TransferMapChangedHandler(Bitmap transferMap);

    public delegate void TipInfoChangedHandler(int color, int value);

    public partial class VolumeRendererSymbolControl : Control
    {
        public enum ControlPointTypes
        {
            NullType,
            OpacityControlPoint,
            ColorControlPoint
        }
        public struct FocusControlPointInfo
        {
            public ControlPointTypes ControlPointType;
            public ControlPoint FocusControlPoint;
        }
        #region Member
        private List<ControlPoint> _colorControlPoints = null;
        private List<ControlPoint> _opacityControlPoints = null;
        private Pen _pen = null;
        private SolidBrush _brush = null;
        private ColorDialog _colorDialog = null;
        private Bitmap _colorBar = null;
        private Bitmap _transferMap = null;

        private const int COLOR_CONTROL_POINT_WIDTH = 2;
        private const int COLOR_CONTROL_POINT_HIGHT = 8;
        private const int OPACITY_CONTROL_POINT_WIDTH = 3;
        private const int OPACITY_CONTROL_POINT_HIGHT = 3;
        private const int ViewMargin = 10;
        private const int ColorBarRegionHeigt = 20;
        public float XMin = 0;
        public float YMin = 0;
        public float XMax = 2550;
        public float YMax = 255;

        public List<int> histogram = null;
        public float minimun = 0.0f;
        public float medium = 127.5f;
        public float maximun = 255.0f;
        //控制点
        private FocusControlPointInfo _focusControlPoint = new FocusControlPointInfo();
        private FocusControlPointInfo _lastPointInfo = new FocusControlPointInfo();
        private Point _mouseDowmPoint = new Point();

        //右键菜单项
        private ContextMenuStrip _cms = new ContextMenuStrip();
        private ToolStripMenuItem _addColorControlPoint, _addOpacityControlPoint, _deleteColorControlPoint, _deleteOpacityControlPoint, _changeColor;

        public event TransferMapChangedHandler TransferMapChanged;
        public event TipInfoChangedHandler TipInfoChanged;
        #endregion

        #region public methods
        public VolumeRendererSymbolControl()
        {
            this.SetStyle(ControlStyles.DoubleBuffer, true);//设置双缓冲，防止图像抖动 
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);//   忽略系统消息，防止图像闪烁 
            this.SetStyle(ControlStyles.UserMouse, true);//控制鼠标完成事件 

            InitializeComponent();

            InitializeGraphControl();  //设置默认图形的
            CreateContextMenuStrip();

            _pen = new Pen(Color.Black);
            _brush = new SolidBrush(Color.Red);
            _colorBar = new Bitmap(2550, 10);
            _transferMap = new Bitmap(2550, 1);
        }
        #endregion

        #region public methods

        public List<ControlPoint> getColorCtrlPoints()
        {
            return _colorControlPoints;
        }

        public List<ControlPoint> getOpacityCtrlPoints()
        {
            return _opacityControlPoints;
        }

        public Bitmap getBitMap()
        {
            return _transferMap;
        }

        public void setColorCtrlPoints(List<ControlPoint> colorP)
        {
            if (colorP != null)
                _colorControlPoints = colorP;
        }

        public void setOpacityCtrlPoints(List<ControlPoint> opacityP)
        {
            if (opacityP != null)
                _opacityControlPoints = opacityP;
        }

        public void Update()
        {
            CreateBitmap();
        }

        #endregion

        #region protected methods

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.Clear(Color.White);
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            DrawAxis(pe.Graphics);
            //draw color bar
            DrawColorBar(pe.Graphics);
            //draw histogram
            DrawHistogram(pe.Graphics);
            //draw opacity lines
            DrawLines(pe.Graphics);
            //draw opacity control points
            foreach (var o in _opacityControlPoints)
                DrawRectangle(pe.Graphics, ViewPointToControlPoint(o.Point), new Size(OPACITY_CONTROL_POINT_WIDTH, OPACITY_CONTROL_POINT_HIGHT), Color.Red);

            //draw color control points
            foreach (var o in _colorControlPoints)
            {
                //Point p = ViewPointToControlPoint(o.Point);
                //p.Y= this.Height - ViewMargin;  //set the point's Y value,let it lie on the button of the map
                DrawRectangle(pe.Graphics, ViewPointToControlPoint(o.Point), new Size(COLOR_CONTROL_POINT_WIDTH, COLOR_CONTROL_POINT_HIGHT), Color.Yellow);
            }
            base.OnPaint(pe);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            Point p = new Point(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                _focusControlPoint = GetFocusControlPoint(e.X, e.Y);
                if (_focusControlPoint.ControlPointType == ControlPointTypes.ColorControlPoint)
                {
                    if (_colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        _focusControlPoint.FocusControlPoint.value = _colorDialog.Color.ToArgb();
                    }
                }
            }
            else
            { }

            this.Refresh();

            CreateBitmap();
            if (TransferMapChanged != null)
            {
                TransferMapChanged(_transferMap);
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _mouseDowmPoint.X = e.X;
            _mouseDowmPoint.Y = e.Y;
            _focusControlPoint = GetFocusControlPoint(e.X, e.Y);
            _lastPointInfo = _focusControlPoint;
            if (e.Button == MouseButtons.Right)
            {
                _cms.Items.Clear();
                _cms.Items.Add(_addColorControlPoint);
                _cms.Items.Add(_addOpacityControlPoint);
                if (_focusControlPoint.ControlPointType == ControlPointTypes.ColorControlPoint)
                {
                    _cms.Items.Add(_changeColor);
                    _cms.Items.Add(_deleteColorControlPoint);
                }
                else if (_focusControlPoint.ControlPointType == ControlPointTypes.OpacityControlPoint)
                {
                    _cms.Items.Add(_deleteOpacityControlPoint);
                }
                else
                { }
                _cms.Show(this, e.X, e.Y);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _focusControlPoint.FocusControlPoint = null;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_focusControlPoint.FocusControlPoint != null)
            {
                int x = ControlPointToViewPoint(new Point(e.X, e.Y)).X;
                int y = ControlPointToViewPoint(new Point(e.X, e.Y)).Y;
                if (_focusControlPoint.ControlPointType == ControlPointTypes.ColorControlPoint)
                {
                    int index = _colorControlPoints.IndexOf(_focusControlPoint.FocusControlPoint);
                    if (index == 0)
                    {
                        if (x < XMin)
                            x = (int)XMin;
                        else if (x > _colorControlPoints[index + 1].Point.X)
                            x = _colorControlPoints[index + 1].Point.X - 1;
                    }
                    else if (index == _colorControlPoints.Count - 1)
                    {
                        if (x < _colorControlPoints[index - 1].Point.X)
                            x = _colorControlPoints[index - 1].Point.X + 1;
                        else if (x > XMax)
                            x = (int)XMax;
                    }
                    else
                    {
                        if (x < _colorControlPoints[index - 1].Point.X)
                            x = _colorControlPoints[index - 1].Point.X + 1;
                        else if (x > _colorControlPoints[index + 1].Point.X)
                            x = _colorControlPoints[index + 1].Point.X - 1;
                    }
                    //if (y < YMin)
                    //    y = (int)YMin;
                    //if (y > YMax)
                    //    y = (int)YMax;
                    _focusControlPoint.FocusControlPoint.Point.X = x; ;
                    //_focusControlPoint.FocusControlPoint.value = _colorBar.GetPixel(e.X, 0).ToArgb();
                }
                else if (_focusControlPoint.ControlPointType == ControlPointTypes.OpacityControlPoint)
                {
                    int index = _opacityControlPoints.IndexOf(_focusControlPoint.FocusControlPoint);
                    if (index == 0)
                    {
                        if (x < XMin)
                            x = (int)XMin;
                        else if (x > _opacityControlPoints[index + 1].Point.X)
                            x = _opacityControlPoints[index + 1].Point.X - 1;
                    }
                    else if (index == _opacityControlPoints.Count - 1)
                    {
                        if (x < _opacityControlPoints[index - 1].Point.X)
                            x = _opacityControlPoints[index - 1].Point.X + 1;
                        else if (x > XMax)
                            x = (int)XMax;
                    }
                    else
                    {
                        if (x < _opacityControlPoints[index - 1].Point.X)
                            x = _opacityControlPoints[index - 1].Point.X + 1;
                        else if (x > _opacityControlPoints[index + 1].Point.X)
                            x = _opacityControlPoints[index + 1].Point.X - 1;
                    }
                    if (y < YMin)
                        y = (int)YMin;
                    if (y > YMax)
                        y = (int)YMax;
                    _focusControlPoint.FocusControlPoint.Point = new Point(x, y);
                    _focusControlPoint.FocusControlPoint.value = y;
                }
                this.Refresh();

                CreateBitmap();
                if (TransferMapChanged != null)
                {
                    TransferMapChanged(_transferMap);
                }
            }

            if (TipInfoChanged != null)
            {
                int wx = ControlPointToViewPoint(new Point(e.X, e.Y)).X;
                if (wx < 0)
                    wx = 0;
                if (wx >= _colorBar.Width)
                    wx = _colorBar.Width - 1;
                TipInfoChanged(_colorBar.GetPixel(wx, 0).ToArgb(), wx);
            }
            base.OnMouseMove(e);
        }
        #endregion

        #region private methods

        //初始化控制点信息
        private void InitializeGraphControl()
        {
            //颜色控制点
            _colorDialog = new ColorDialog();
            _colorControlPoints = new List<ControlPoint>();
            ControlPoint coP1 = new ControlPoint();
            coP1.Point = new Point(0, -65);
            coP1.value = Color.Red.ToArgb();
            _colorControlPoints.Add(coP1);
            ControlPoint coP2 = new ControlPoint();
            coP2.Point = new Point(850, -65);
            coP2.value = 255 << 24 | 255 << 8;  //绿原色
            _colorControlPoints.Add(coP2);
            ControlPoint coP3 = new ControlPoint();
            coP3.Point = new Point(1700, -65);
            coP3.value = 255 << 24 | 255;
            _colorControlPoints.Add(coP3);
            ControlPoint coP4 = new ControlPoint();
            coP4.Point = new Point(2550, -65);
            coP4.value = Color.Pink.ToArgb();
            _colorControlPoints.Add(coP4);

            _opacityControlPoints = new List<ControlPoint>();
            ControlPoint opP1 = new ControlPoint();
            opP1.Point = new Point(0, 0);
            opP1.value = 0;
            _opacityControlPoints.Add(opP1);
            ControlPoint opP2 = new ControlPoint();
            opP2.Point = new Point(850, 85);
            opP2.value = 85;
            _opacityControlPoints.Add(opP2);
            ControlPoint opP3 = new ControlPoint();
            opP3.Point = new Point(1700, 170);
            opP3.value = 170;
            _opacityControlPoints.Add(opP3);
            ControlPoint opP4 = new ControlPoint();
            opP4.Point = new Point(2550, 255);
            opP4.value = 255;
            _opacityControlPoints.Add(opP4);
        }

        //上下文菜单
        private void CreateContextMenuStrip()
        {
            _addColorControlPoint = new ToolStripMenuItem();
            _addColorControlPoint.Text = "增加颜色节点";
            _addColorControlPoint.Click += new EventHandler(addColorControlPoint_Click);

            _addOpacityControlPoint = new ToolStripMenuItem();
            _addOpacityControlPoint.Text = "增加透明度节点";
            _addOpacityControlPoint.Click += new EventHandler(addOpacityControlPoint_Click);

            _changeColor = new ToolStripMenuItem();
            _changeColor.Text = "修改控制点颜色";
            _changeColor.Click += new EventHandler(changeColor_Click);

            _deleteColorControlPoint = new ToolStripMenuItem();
            _deleteColorControlPoint.Text = "删除颜色节点";
            _deleteColorControlPoint.Click += new EventHandler(deleteColorControlPoint_Click);

            _deleteOpacityControlPoint = new ToolStripMenuItem();
            _deleteOpacityControlPoint.Text = "删除透明度节点";
            _deleteOpacityControlPoint.Click += new EventHandler(deleteOpacityControlPoint_Click);
        }

        void addOpacityControlPoint_Click(object sender, EventArgs e)
        {
            ControlPoint cp = new ControlPoint();
            cp.Point = ControlPointToViewPoint(_mouseDowmPoint);
            cp.value = cp.Point.Y;
            int idx = ComputeOpacityControlPointIndex(_mouseDowmPoint);
            if (idx == -2)
            {
                _opacityControlPoints.Add(cp);
            }
            else if (idx == -1)
            {
                //do anything
            }
            else
            {
                _opacityControlPoints.Insert(idx, cp);
            }

            this.Refresh();

            CreateBitmap();
            if (TransferMapChanged != null)
            {
                TransferMapChanged(_transferMap);
            }
        }

        void addColorControlPoint_Click(object sender, EventArgs e)
        {
            ControlPoint cp = new ControlPoint();
            cp.Point = ControlPointToViewPoint(new Point(_mouseDowmPoint.X, this.Height - 10));
            cp.value = _colorBar.GetPixel(cp.Point.X, 0).ToArgb();
            int idx = ComputeColorControlPointIndex(_mouseDowmPoint);
            if (idx == -2)
            {
                _colorControlPoints.Add(cp);
            }
            else if (idx == -1)
            {
                //do anything
            }
            else
            {
                _colorControlPoints.Insert(idx, cp);
            }

            this.Refresh();

            CreateBitmap();
            if (TransferMapChanged != null)
            {
                TransferMapChanged(_transferMap);
            }
        }

        void changeColor_Click(object sender, EventArgs e)
        {
            if (_lastPointInfo.FocusControlPoint != null)
            {
                if (_lastPointInfo.ControlPointType == ControlPointTypes.ColorControlPoint)
                {
                    if (_colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        _lastPointInfo.FocusControlPoint.value = _colorDialog.Color.ToArgb();
                    }
                }
            }
            this.Refresh();

            CreateBitmap();
            if (TransferMapChanged != null)
            {
                TransferMapChanged(_transferMap);
            }
        }

        void deleteOpacityControlPoint_Click(object sender, EventArgs e)
        {
            int idx = ComputeOpacityControlPointIndex(_mouseDowmPoint);
            if (_opacityControlPoints.Count > 2)
            {
                if (idx == -1)
                {
                    for (int i = 0; i < _opacityControlPoints.Count; i++)
                    {
                        if (_mouseDowmPoint.X == ViewPointToControlPoint(_opacityControlPoints[i].Point).X)
                            if (i == 0 || i == _opacityControlPoints.Count - 1)
                            { }
                            else
                                _opacityControlPoints.RemoveAt(i);
                    }
                }
                else if (idx > -1)
                {
                    if (idx == 0 || idx == _opacityControlPoints.Count - 1)
                    { }
                    else
                        _opacityControlPoints.RemoveAt(idx);
                }
                this.Refresh();

                CreateBitmap();
                if (TransferMapChanged != null)
                    TransferMapChanged(_transferMap);
            }
        }

        void deleteColorControlPoint_Click(object sender, EventArgs e)
        {
            int idx = ComputeColorControlPointIndex(_mouseDowmPoint);
            if (_colorControlPoints.Count > 2)
            {
                if (idx == -1)
                {
                    for (int i = 0; i < _colorControlPoints.Count; i++)
                    {
                        if (_mouseDowmPoint.X == ViewPointToControlPoint(_colorControlPoints[i].Point).X)
                            if (i == 0 || i == _colorControlPoints.Count - 1)
                            { }
                            else
                                _colorControlPoints.RemoveAt(i);
                    }
                }
                else if (idx > -1)
                {
                    if (idx == 0 || idx == _colorControlPoints.Count - 1)
                    { }
                    else
                        _colorControlPoints.RemoveAt(idx);
                }
                this.Refresh();

                CreateBitmap();
                if (TransferMapChanged != null)
                    TransferMapChanged(_transferMap);
            }
        }

        private void DrawAxis(Graphics g)
        {
            Rectangle rect = new Rectangle(new Point(ViewMargin, ViewMargin),
                new Size(this.Width - 2 * ViewMargin, this.Height - 2 * ViewMargin - ColorBarRegionHeigt));
            _brush.Color = Color.Black;
            g.FillRectangle(_brush, rect);
            g.DrawString("|" + minimun.ToString("0.000"), new Font("Verdana", 8), new SolidBrush(Color.Red), 0, this.Height - ViewMargin - ColorBarRegionHeigt + 3);
            g.DrawString("|" + medium.ToString("0.000"), new Font("Verdana", 8), new SolidBrush(Color.Red), this.Width / 2 - 2 * ViewMargin, this.Height - ViewMargin - ColorBarRegionHeigt + 3);
            g.DrawString("|" + maximun.ToString("0.000"), new Font("Verdana", 8), new SolidBrush(Color.Red), this.Width - 5 * ViewMargin, this.Height - ViewMargin - ColorBarRegionHeigt + 3);
        }

        private void DrawRectangle(Graphics g, Point p, Size halfSize, Color c)
        {
            Rectangle rect = new Rectangle(new Point(p.X - halfSize.Width, p.Y - halfSize.Height), new Size(halfSize.Width * 2, halfSize.Height * 2));
            _brush.Color = c;
            g.FillRectangle(_brush, rect);
            _pen.Width = 1f;
            _pen.Color = Color.Black;
            g.DrawRectangle(_pen, rect);
        }

        private void DrawLines(Graphics g)
        {
            if (_opacityControlPoints.Count < 2) return;
            List<Point> ps = new List<Point>();
            foreach (var o in _opacityControlPoints)
                ps.Add(ViewPointToControlPoint(o.Point));
            _pen.Width = 2f;
            _pen.Color = Color.Green;
            g.DrawLines(_pen, ps.ToArray());
        }

        private void DrawColorBar(Graphics g)
        {
            System.Drawing.Imaging.BitmapData bmpData = null;
            bmpData = _colorBar.LockBits(new Rectangle(0, 0, _colorBar.Width, _colorBar.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, _colorBar.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bLen = _colorBar.Width * _colorBar.Height * 4;
            byte[] rgbValues = new byte[bLen];
            for (int i = 0; i < _colorControlPoints.Count - 1; i++)
            {
                int width = (_colorControlPoints[i + 1].Point).X - (_colorControlPoints[i].Point).X;
                for (int j = 0; j < width; j++)
                {
                    for (int h = 0; h < _colorBar.Height; h++)
                    {
                        rgbValues[(_colorBar.Width * h + ((_colorControlPoints[i].Point).X + j)) * 4] = (byte)(HSVColorInterpolation(_colorControlPoints[i].value, _colorControlPoints[i + 1].value, (float)j / (float)width) & 255);              //b
                        rgbValues[(_colorBar.Width * h + ((_colorControlPoints[i].Point).X + j)) * 4 + 1] = (byte)((HSVColorInterpolation(_colorControlPoints[i].value, _colorControlPoints[i + 1].value, (float)j / (float)width) >> 8) & 255);   //g
                        rgbValues[(_colorBar.Width * h + ((_colorControlPoints[i].Point).X + j)) * 4 + 2] = (byte)((HSVColorInterpolation(_colorControlPoints[i].value, _colorControlPoints[i + 1].value, (float)j / (float)width) >> 16) & 255);  //r
                        rgbValues[(_colorBar.Width * h + ((_colorControlPoints[i].Point).X + j)) * 4 + 3] = (byte)255;  //透明度设为255
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bLen);
            _colorBar.UnlockBits(bmpData);
            Point p = new Point(0, -50);
            p = ViewPointToControlPoint(p);
            //p.Y = this.Height - ViewMargin - _colorBar.Height / 2;
            Rectangle rect = new Rectangle(p, new Size(this.Width - 2 * ViewMargin, _colorBar.Height));
            g.DrawImage(_colorBar, rect);

        }

        void CreateBitmap()
        {
            System.Drawing.Imaging.BitmapData bmpData = null;
            bmpData = _transferMap.LockBits(new Rectangle(0, 0, _transferMap.Width, _transferMap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, _transferMap.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bLen = _transferMap.Width * _transferMap.Height * 4;
            byte[] rgbValues = new byte[bLen];
            //set rgb
            for (int i = 0; i < _colorControlPoints.Count - 1; i++)
            {
                int width = (_colorControlPoints[i + 1].Point).X - (_colorControlPoints[i].Point).X;
                for (int j = 0; j < width; j++)
                {
                    rgbValues[(((_colorControlPoints[i].Point).X + j)) * 4] = (byte)(HSVColorInterpolation(_colorControlPoints[i].value, _colorControlPoints[i + 1].value, (float)j / (float)width) & 255);              //b
                    rgbValues[(((_colorControlPoints[i].Point).X + j)) * 4 + 1] = (byte)((HSVColorInterpolation(_colorControlPoints[i].value, _colorControlPoints[i + 1].value, (float)j / (float)width) >> 8) & 255);   //g
                    rgbValues[(((_colorControlPoints[i].Point).X + j)) * 4 + 2] = (byte)((HSVColorInterpolation(_colorControlPoints[i].value, _colorControlPoints[i + 1].value, (float)j / (float)width) >> 16) & 255);  //r
                }
            }
            //set alpha
            for (int i = 0; i < _opacityControlPoints.Count - 1; i++)
            {
                int width = (_opacityControlPoints[i + 1].Point).X - (_opacityControlPoints[i].Point).X;
                for (int j = 0; j < width; j++)
                {
                    rgbValues[((_opacityControlPoints[i].Point).X + j) * 4 + 3] = (byte)OpacityInterpolation(_opacityControlPoints[i].value, _opacityControlPoints[i + 1].value, (float)j / (float)width);
                } 
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bLen);
            _transferMap.UnlockBits(bmpData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="ratio">0--1之间</param>
        /// <returns></returns>
        private int HSVColorInterpolation(int color1, int color2, float ratio)
        {
            if (Color.Red.ToArgb() == color1 && Color.White.ToArgb() == color2)
            {
                if (ratio > 0)
                { }
            }
            FieldModel.HSV hsv1 = FieldModel.MathUitilities.RGBtoHSV(color1);
            FieldModel.HSV hsv2 = FieldModel.MathUitilities.RGBtoHSV(color2);

            float h = hsv1.H + (hsv2.H - hsv1.H) * ratio;
            float s = hsv1.S + (hsv2.S - hsv1.S) * ratio;
            float v = hsv1.V + (hsv2.V - hsv1.V) * ratio;

            hsv1.H = h;
            hsv1.S = s;
            hsv1.V = v;
            return FieldModel.MathUitilities.HSVtoRGB(hsv1);  //return rgb int
        }

        private int OpacityInterpolation(int opacity1, int opacity2, float ratio)
        {
            return (int)(opacity1 + (opacity2 - opacity1) * ratio);
        }

        private void DrawHistogram(Graphics g)
        {
            if (histogram == null) return;
            _pen.Width = 0.5f;
            _pen.Color = Color.Blue;
            int width = this.Width - 2 * ViewMargin;
            int length = this.Height - 2 * ViewMargin - ColorBarRegionHeigt;
            double step = (double)width / histogram.Count;
            int sum = 0;
            for (int i = 0; i < histogram.Count; i++)
                sum += histogram[i];
            for (int i = 0; i < histogram.Count; i++)
            {
                int h = Convert.ToInt32(length * Math.Sqrt((80 * histogram[i] * 1.0 / sum)));
                if (h > length) h = length;
                g.DrawLine(_pen, new Point(Convert.ToInt32(ViewMargin + i * step), length + ViewMargin),
                    new Point(Convert.ToInt32(ViewMargin + i * step), length + ViewMargin - h));
            }
        }

        private FocusControlPointInfo GetFocusControlPoint(int x, int y)
        {
            FocusControlPointInfo info = new FocusControlPointInfo();
            info.ControlPointType = ControlPointTypes.NullType;  //默认情况下设为null类型
            //draw opacity control points
            foreach (var o in _opacityControlPoints)
            {
                Point p = ViewPointToControlPoint(o.Point);
                if (Math.Abs(x - p.X) < OPACITY_CONTROL_POINT_WIDTH && Math.Abs(y - p.Y) < OPACITY_CONTROL_POINT_HIGHT)
                {
                    info.FocusControlPoint = o;
                    info.ControlPointType = ControlPointTypes.OpacityControlPoint;
                    return info;
                }
            }
            //draw color control points
            foreach (var o in _colorControlPoints)
            {
                Point p = ViewPointToControlPoint(o.Point);
                if (Math.Abs(x - p.X) < COLOR_CONTROL_POINT_WIDTH && Math.Abs(y - p.Y) < COLOR_CONTROL_POINT_HIGHT)
                {
                    info.FocusControlPoint = o;
                    info.ControlPointType = ControlPointTypes.ColorControlPoint;
                    return info;
                }
            }
            return info;
        }

        private int ComputeOpacityControlPointIndex(Point p)
        {
            for (int i = 0; i < _opacityControlPoints.Count; i++)
            {
                if (p.X < ViewPointToControlPoint(_opacityControlPoints[i].Point).X)
                    return i;//means insert
                else if (p.X == ViewPointToControlPoint(_opacityControlPoints[i].Point).X)
                    return -1;// -1 means same x value is not allowd.
            }
            return -2;//-2 means append
        }

        private int ComputeColorControlPointIndex(Point p)
        {
            for (int i = 0; i < _colorControlPoints.Count; i++)
            {
                if (p.X < ViewPointToControlPoint(_colorControlPoints[i].Point).X)
                    return i;//means insert
                else if (p.X == ViewPointToControlPoint(_colorControlPoints[i].Point).X)
                    return -1;// -1 means same x value is not allowd.
            }
            return -2;//-2 means append
        }

        private Point ViewPointToControlPoint(Point point)
        {
            int xc = (int)(point.X * (this.Width - 2 * ViewMargin) / (XMax - XMin) + ViewMargin);
            int yc = (int)(((YMax - YMin) - point.Y) * (this.Height - 2 * ViewMargin - ColorBarRegionHeigt) / (YMax - YMin) + ViewMargin);
            return new Point(xc, yc);
        }

        private Point ControlPointToViewPoint(Point point)
        {
            int xv = (int)((point.X - ViewMargin) * (XMax - XMin) / (this.Width - 2 * ViewMargin));
            int yv = (int)((YMax - YMin) - (point.Y - ViewMargin) * (YMax - YMin) / (this.Height - 2 * ViewMargin - ColorBarRegionHeigt));
            return new Point(xv, yv);
        }
        #endregion
    }
}
