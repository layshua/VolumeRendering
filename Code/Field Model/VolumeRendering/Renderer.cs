using System;
using System.Collections.Generic;
using System.Text;
using Gaea;
using Gaea.Renderable;

namespace FieldModel
{
    public abstract class VoxelRendererBase : Gaea.Renderable.RenderableObject
    {
        protected DVRBase _DVRDriver = null;
        public VoxelRendererBase(string name)
            : base(name)
        {}

        public DVRBase DVRDriver
        {
            get { return _DVRDriver; }
        }

        public override void Initialize(DrawArgs drawArgs)
        {
            base.Initialize(drawArgs);
        }

        public override void Render(DrawArgs drawArgs)
        {

        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return false;
        }

        public override void Update(DrawArgs drawArgs)
        {
            if (!_IsInitialized)
                Initialize(drawArgs);
            base.Update(drawArgs);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
