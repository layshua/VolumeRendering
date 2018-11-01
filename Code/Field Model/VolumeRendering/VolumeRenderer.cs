using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using Gaea;

namespace FieldModel
{
    public class VolumeRenderer : VoxelRendererBase
    {
        //Typhoon.NetCDFData _ncData;
        public short renderModel = 0;
        FieldModel.MeteorDataProvider _mdProvider;
        string _varName;
        int isoVlue = 50;
        Color4 isoColr = new Color4(230, 79, 157, 255);
        float ka = 0.0f, kd = 0.0f, ks = 0.0f, exp = 80.0f;
        int demson = 10;
        float transp = 0.9f;
        Bitmap transfer = null;
        static int colNum = 4;
        static int rowNum = 4;
        //zxq 修改模型modelHeight、zExaggerate（原值100000f、20000f） //water 100f 20f
        static float modelHeight = 100000f;
        static float zExaggerate = 20000f;
        static Point leftButtom = new Point(0,0);
        static Point rightUpper;

        public VolumeRenderer(Bitmap bmap,string name, FieldModel.MeteorDataProvider mdProvider, int isoValue, Color4 color,
            float _ka, float _kd, float _ks, float _exp, float _trans, int _dem, Point xySize)
            : base(name)
        {
            _mdProvider = mdProvider;
            _varName = name;
            transfer = bmap;

            this.isoVlue = isoValue;
            this.isoColr = color;
            this.ka = _ka;
            this.kd = _kd;
            this.ks = _ks;
            this.exp = _exp;
            this.transp = _trans;
            this.demson = _dem;
            rightUpper = xySize;
        }

        public void AddIsoSurface(int iso, Color4 color)
        {
            _DVRDriver.AddISO(iso, color);
        }

        public void SetBlockSize(int col, int row)
        {
            colNum = col;
            rowNum = row;
        }
        public void SetBlkHeight(float mdHeight)
        {
            modelHeight = mdHeight;
        }

        public void SetBlkExgrte(float zEx)
        {
            zExaggerate = zEx;
        }

        public void SetCorners(int lx, int ly, int rx, int ry)
        {
            leftButtom.X = lx;
            leftButtom.Y = ly;
            rightUpper.X = rx;
            rightUpper.Y = ry;
        }

        public override void Initialize(DrawArgs drawArgs)
        {
            switch (renderModel)
            {
                case 0:
                    _DVRDriver = new DVRTextured3D(_varName, _mdProvider, transfer, colNum, rowNum, modelHeight, zExaggerate, leftButtom, rightUpper);
                    break;
                case 1:
                    _DVRDriver = new DVRSpherical(_varName, _mdProvider, transfer, colNum, rowNum, modelHeight, zExaggerate, leftButtom, rightUpper);
                    break;
                
                default: break;
            }
            base.Initialize(drawArgs);
        }

        public override void Render(DrawArgs drawArgs)
        {
            _DVRDriver.Render(drawArgs);
        }

        public override void Dispose()
        {
            if (_DVRDriver != null)
                _DVRDriver.Dispose();
            base.Dispose();
        }
    }
}
