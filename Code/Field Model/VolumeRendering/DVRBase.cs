using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using SlimDX;
using SlimDX.Direct3D9;
using Gaea;

namespace FieldModel
{
    public abstract class DVRBase : IDisposable
    {
        protected bool _IsCullDirty = false;
        protected float _CullX = 429.0f;
        protected float _CullY = 0.0f;

        protected int _samples = 400;

        public abstract void SetTransferMap(System.Drawing.Bitmap transferBMP);
        public abstract int ActualValueToTransferMapCoord(float actualValue);
        public virtual void Initialize(DrawArgs drawArgs) { }
        public virtual void Render(DrawArgs drawArgs) { }
        public virtual void AddISO(int isoV, Color4 colorV) { }
        public virtual void Dispose() { }
        public virtual bool IsUsingLight { get; set; }
        public virtual float isoValue { get; set;}
        public virtual Color4  isoColor {get; set;}

        public int Samples
        {
            get { return _samples; }
            set { _samples = value; }
        }

        public float CullX
        {
            get
            {
                return _CullX;
            }
            set
            {
                _CullX = value;
            }
        }
        public float CullY
        {
            get
            {
                return _CullY;
            }
            set
            {
                _CullY = value;
            }
        }
        public bool CullDirty
        {
            get
            {
                return _IsCullDirty;
            }
            set
            {
                _IsCullDirty = value;
            }
        }
        public virtual int BlockRow { get;set; }
        public virtual int BlockColum { get; set; }
        public virtual int selctIndex { get; set; }
        public virtual float Ka { get; set; }
        public virtual float Kd { get; set; }
        public virtual int demn { get; set; }
        public virtual float Transparen { get; set; }
    }
}
