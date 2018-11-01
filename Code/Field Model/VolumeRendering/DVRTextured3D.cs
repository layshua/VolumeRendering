using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using System.IO;
using Gaea;

namespace FieldModel
{
    public class DVRTextured3D : DVRBase
    {
        public static VertexFormat TexCoordSize3(int index)
        {
            return (VertexFormat)(1 << (index * 2 + 16));
        }

        public static VertexFormat TexCoordSize4(int index)
        {
            return (VertexFormat)(2 << (index * 2 + 16));
        }

        public struct PositionTextured3D
        {
            public float X;
            public float Y;
            public float Z;
            public float Tu;
            public float Tv;
            public float Tw;

            public static VertexFormat Format = VertexFormat.Position | VertexFormat.Texture1 | TexCoordSize3(0);

            public static int StrideSize
            {
                get
                {
                    return System.Runtime.InteropServices.Marshal.SizeOf(typeof(PositionTextured3D));
                }
            }
        }
        /// <summary>
        /// slice间距系数
        /// </summary>
        protected float _delta;
        
        protected double cutting = 0.0;
        protected List<VolumeBrick> _bricks = new List<VolumeBrick>();
        protected static Effect _effect = null;
        private static Effect _effectStored = null;
        protected int _frameNow = 0;
        protected string _varName;
        protected int _blockColum;
        protected int _blockRow;
        protected bool isVector = false;
        protected FieldModel.MeteorDataProvider _mdProvider;
        protected int nTime, nLevel, nRow, nCol;
        protected float[] _vectorA, _cacheArr;  //存储三帧变量
        protected Vector3[] _vectorB, _cacheBArr;  //用于处理矢量
        protected float[, ,] uvw;
        protected float maxValue = 0.0f;
        protected float minValue = 0.0f;

        bool isUsingLight = false;

        public int BlockRow
        {
            get { return _blockRow; }
            set { _blockRow = value; }
        }

        public int BlockColum
        {
            get { return _blockColum; }
            set { _blockColum = value; }
        }

        public override bool IsUsingLight
        {
            get { return isUsingLight; }
            set { isUsingLight = value; }
        }

       

        protected Texture _transferMap = null;

        public override void SetTransferMap(System.Drawing.Bitmap transferBMP)
        {
                if (_transferMap != null)
                    _transferMap.Dispose();
                _transferMap = ImageHelper.LoadTexture(transferBMP, System.Drawing.Imaging.ImageFormat.Png);
                if (_transferMap != null && _effect!=null) _effect.SetTexture("TransferMap", _transferMap);
        }

        public override int ActualValueToTransferMapCoord(float actualValue)
        {
            if (actualValue < minValue || actualValue > maxValue) return 0;
            float maxWind = maxValue;
            float minWind = minValue;
            float r = (Math.Abs(actualValue) - minWind) / (maxWind - minWind);
            return (int)(r * 255);
        }

        public DVRTextured3D(string varName, FieldModel.MeteorDataProvider mdProvider, Bitmap bitmap, int col, int row, float modelH, float exgg, Point pLeft, Point pRight)
        {
            _varName = varName;
            _transferMap = ImageHelper.LoadTexture(bitmap, System.Drawing.Imaging.ImageFormat.Png);
            if (_varName == "UVW") isVector = true;
            else isVector = false;
            _mdProvider = mdProvider;
            _blockRow = row;
            _blockColum = col;
            nTime = mdProvider.meteorMetaData.nTime;
            nLevel = mdProvider.meteorMetaData.nLevel;
            nRow = mdProvider.meteorMetaData.nRow;
            nCol = mdProvider.meteorMetaData.nColumn;
            if (isVector)
            {
                _vectorB = new Vector3[nRow * nCol * nLevel];
                _cacheBArr = new Vector3[nRow * nCol * nLevel];
            }
            else
            {
                _vectorA = new float[nRow * nCol * nLevel];
                _cacheArr = new float[nRow * nCol * nLevel];
            }

            _mdProvider.GetData(_mdProvider.ConfigPath, varName, FieldModel.TimeController.Instance.TimeNumber);
            if (isVector)
            {
                _mdProvider.meteorData.mergeList[0].CopyTo(_vectorB, 0);
                maxValue = 0.0f;
                for (int i = 0; i < _vectorB.Length; i++)
                    if (_vectorB[i].Length() < 10000000 && _vectorB[i].Length() > maxValue) maxValue = _vectorB[i].Length();

                minValue = maxValue;
                for (int i = 0; i < _vectorB.Length; i++)
                    if (_vectorB[i].Length() < minValue) minValue = _vectorB[i].Length();
            }
            else
            {
                _mdProvider.meteorData.varList[0].CopyTo(_vectorA, 0);
                maxValue = 0.0f;
                for (int i = 0; i < _vectorA.Length; i++)
                    if (_vectorA[i] < 10000000 && _vectorA[i] > maxValue) maxValue = _vectorA[i];

                minValue = maxValue;
                for (int i = 0; i < _vectorA.Length; i++)
                    if (_vectorA[i] < minValue) minValue = _vectorA[i];
            }

            uvw = new float[nLevel, nRow, nCol];
            int idx = 0;
            if (isVector)
            {
                for (int l = 0; l < nLevel; l++)
                    for (int r = 0; r < nRow; r++)
                        for (int c = 0; c < nCol; c++)
                        {
                            uvw[l, r, c] = _vectorB[idx].Length();
                            idx++;
                        }
            }
            else
            {
                for (int l = 0; l < nLevel; l++)
                    for (int r = 0; r < nRow; r++)
                        for (int c = 0; c < nCol; c++)
                        {
                            uvw[l, r, c] = _vectorA[idx];
                            idx++;
                        }
            }

            float latOri, lonOri, intervalLat, intervalLon;
            lonOri = (float)_mdProvider.meteorMetaData.lons[0].Degrees;
            latOri = (float)_mdProvider.meteorMetaData.lats[0].Degrees;
            intervalLon = (float)(_mdProvider.meteorMetaData.lons[1].Degrees - _mdProvider.meteorMetaData.lons[0].Degrees);
            intervalLat = (float)(_mdProvider.meteorMetaData.lats[1].Degrees - _mdProvider.meteorMetaData.lats[0].Degrees);

            int colStep = (pRight.X - pLeft.X) / _blockColum;
            int rowStep = (pRight.Y - pLeft.Y) / _blockRow;
            //修改模式
            try
            {
                for (int i = 0; i < _blockRow; i++)
                    for (int j = 0; j < _blockColum; j++)
                    {
                        _bricks.Add(new VolumeBrick(this, lonOri, latOri, modelH, intervalLon, intervalLat, exgg, pLeft.X + j * colStep, pLeft.Y + i * rowStep, colStep, rowStep, nLevel));
                    }
            }
            catch (Exception EX)
            { }

            FieldModel.TimeController.Instance.TimeCount = nTime;

            this.Initialize(DrawArgs.Instance);
        }

        public override void Initialize(DrawArgs drawArgs)
        {
            if (_effectStored == null || _effectStored.Disposed)
            {
                string outerrors = null;
                string fileName = System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, "Shaders\\3DTextureShader.fx");
                StreamReader tr = new StreamReader(fileName);
                string ef = tr.ReadToEnd();
                tr.Close();

                try
                {
                    _effectStored = Effect.FromString(
                                       drawArgs.device,
                                           ef,
                                           null,
                                           null,
                                           null,
                                           ShaderFlags.EnableBackwardsCompatibility,
                                           null,
                                           out outerrors);

                }
                catch (Exception e)
                {
                }

                //string fileName = System.IO.Path.Combine(WorldManager.SystemSettings.BinPath, "Shaders\\TextureBinary.fxo");
                //FileStream fsr = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read);
                //BinaryReader br = new BinaryReader(fsr);
                //byte[] buff = new byte[fsr.Length];
                //br.Read(buff, 0, (int)fsr.Length);
                //br.Close();
                //fsr.Close();
                //try
                //{
                //    _effectStored = Effect.FromMemory(drawArgs.device,
                //        buff,
                //        null,
                //        null,
                //        null,
                //        ShaderFlags.EnableBackwardsCompatibility,
                //        null,
                //        out outerrors);
                //}
                //catch (Exception e)
                //{
                //}


                if (outerrors != null && outerrors.Length > 0)
                {
                    throw new Exception(outerrors);
                }
            }
            _effect = _effectStored;

            GMVector3D sunPosition = SunCalculator.GetGeocentricPosition(TimeKeeper.CurrentTimeUtc);
            Vector4 sunVec = new Vector4((float)sunPosition.X, (float)sunPosition.Y, (float)sunPosition.Z, 0.0f);
            _effect.SetValue("lightDirection", sunVec);

            _effect.SetValue("dimensions", new Vector4(100, 100, 20, 0));
            _effect.SetTexture("TransferMap", _transferMap);
        }

        public override void Render(DrawArgs drawArgs)
        {
            //compute key frame.
            _frameNow = FieldModel.TimeController.Instance.TimeNumber;
            _effect.SetValue("FramePercentage", FieldModel.TimeController.Instance.Timefloat);

            int oldZbuffer = drawArgs.device.GetRenderState(RenderState.ZEnable);
            Matrix oldWorldMatrix = drawArgs.device.GetTransform(TransformState.World);
            Matrix oldProjMatrix = drawArgs.device.GetTransform(TransformState.Projection);
            drawArgs.device.SetTransform(TransformState.World,
                Matrix.Translation(-(float)drawArgs.WorldCamera.ReferenceCenter.X,
                -(float)drawArgs.WorldCamera.ReferenceCenter.Y,
                -(float)drawArgs.WorldCamera.ReferenceCenter.Z));

            // drawArgs.device.Transform.Projection = Matrix.PerspectiveFovRH((float)Math.PI * 0.25f, (float)drawArgs.device.Viewport.Width / (float)drawArgs.device.Viewport.Height, 1f, 4f*(float)drawArgs.WorldCamera.WorldRadius);
            drawArgs.device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Border);
            drawArgs.device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Border);
            drawArgs.device.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Border);

            VertexFormat oldVertexFormat = drawArgs.device.VertexFormat;
            int oldTextureStateColorOperation0 = drawArgs.device.GetTextureStageState(0, TextureStage.ColorOperation);

            RenderBricks(drawArgs);
            base.Render(drawArgs);

            drawArgs.device.SetTextureStageState(0, TextureStage.ColorOperation, oldTextureStateColorOperation0);
            drawArgs.device.VertexFormat = oldVertexFormat;
            drawArgs.device.SetRenderState(RenderState.ZEnable, oldZbuffer);
            drawArgs.device.SetTransform(TransformState.World, oldWorldMatrix);
            drawArgs.device.SetTransform(TransformState.Projection, oldProjMatrix);
        }

        public override void Dispose()
        {
            foreach (var b in _bricks)
                b.Dispose();

            if (_transferMap != null)
                _transferMap.Dispose();
            if (_effect != null)
            {
                _effect.Dispose();
                _effect = null;
            }
            base.Dispose();
        }

        public virtual void RenderBricks(DrawArgs drawArgs)
        {
            SortBricks(drawArgs.WorldCamera.AbsoluteViewMatrix);

            if (_frameNow < nTime)
            {
                if (_frameNow != oldFrameTime)
                {
                    oldFrameTime = _frameNow;  //更新旧一帧的数值
                    if (!_mdProvider.StartReading)
                    {
                        if (isVector)
                        {
                            if (_frameNow == nTime - 1) //当frameTime为最后一帧是，预读第一帧数据
                                _mdProvider.GetDataAsyn(_mdProvider.ConfigPath, _varName, 0, _cacheBArr);
                            else                        //其他情况则读下一帧数据
                                _mdProvider.GetDataAsyn(_mdProvider.ConfigPath, _varName, _frameNow + 1, _cacheBArr);
                        }
                        else
                        {
                            if (_frameNow == nTime - 1) //当frameTime为最后一帧是，预读第一帧数据
                                _mdProvider.GetDataAsynEx(_mdProvider.ConfigPath, _varName, 0, _cacheArr);
                            else                        //其他情况则读下一帧数据
                                _mdProvider.GetDataAsynEx(_mdProvider.ConfigPath, _varName, _frameNow + 1, _cacheArr);
                        }
                    }
                    if (_mdProvider.IsEndRead)
                    {
                        //_vectorB.CopyTo(_vectorA, 0);
                        if (isVector)
                        {
                            _mdProvider.meteorData.mergeList[0].CopyTo(_vectorB, 0);

                            int idx = 0;
                            for (int l = 0; l < nLevel; l++)
                                for (int r = 0; r < nRow; r++)
                                    for (int c = 0; c < nCol; c++)
                                    {
                                        uvw[l, r, c] = _vectorB[idx].Length();
                                        idx++;
                                    }
                        }
                        else
                        {
                            _mdProvider.meteorData.varList[0].CopyTo(_vectorA, 0);

                            int idx = 0;
                            for (int l = 0; l < nLevel; l++)
                                for (int r = 0; r < nRow; r++)
                                    for (int c = 0; c < nCol; c++)
                                    {
                                        uvw[l, r, c] = _vectorA[idx];
                                        idx++;
                                    }
                        }
                        foreach (var brick in _bricks)
                        {
                            brick.CreateVolumeTexture(uvw);
                        }
                    }
                }
            }

            //foreach brick
            if (_IsCullDirty)
            {
                foreach (var brick in _bricks)
                {
                    if (brick._ix >= _CullX) brick._cullx = 0.0f;
                    else if (brick._ix < _CullX && brick._ix + brick._nx > _CullX)
                    {
                        brick._cullx = (_CullX - ((int)_CullX / brick._nx) * brick._nx) / brick._nx;
                    }
                    else
                        brick._cullx = 1.0f;

                    if (brick._iy >= _CullY) brick._cully = 0.0f;
                    else if (brick._iy < _CullY && brick._iy + brick._ny > _CullY)
                    {
                        brick._cully = (_CullY - ((int)_CullY / brick._ny) * brick._ny) / brick._ny;
                    }
                    else
                        brick._cully = 1.0f;

                }
            }

            if (_frameNow != 0)
                foreach (var brick in _bricks)
                {
                    if (_IsCullDirty)
                    {
                        _effect.SetValue("xcull", brick._cullx);
                        _effect.SetValue("ycull", brick._cully);
                    }
                    else
                    {
                        _effect.SetValue("xcull", 1.0f);
                        _effect.SetValue("ycull", 0.0f);
                    }

                    RenderBrick(drawArgs, brick);
                }
        }

        public void SortBricks(Matrix viewMatrix)
        {
            if (_bricks.Count > 1)
            {
                List<double> sortvals = new List<double>();

                for (int i = 0; i < _bricks.Count; i++)
                {
                    Vector3 center = Vector3.TransformCoordinate(_bricks[i].Center, viewMatrix);

                    sortvals.Add(center.LengthSquared());
                }

                for (int i = 0; i < _bricks.Count; i++)
                {
                    for (int j = i + 1; j < _bricks.Count; ++j)
                    {
                        if (sortvals[i] < sortvals[j])
                        {
                            double valtmp = sortvals[i];
                            VolumeBrick bricktmp = _bricks[i];

                            sortvals[i] = sortvals[j];
                            sortvals[j] = valtmp;

                            _bricks[i] = _bricks[j];
                            _bricks[j] = bricktmp;
                        }
                    }
                }
            }
        }

        public virtual void RenderBrick(DrawArgs drawArgs, VolumeBrick brick)
        {
            DrawViewAlignedSlices(drawArgs, brick, drawArgs.WorldCamera.AbsoluteViewMatrix);
        }

        /// <summary>
        /// Calculate the sampling distance
        /// </summary>
        protected virtual void calculateSampling(VolumeBrick brick)
        {
            Matrix modelview = DrawArgs.Instance.WorldCamera.AbsoluteViewMatrix;
            Matrix modelviewInverse = Matrix.Invert(modelview);

            GMBoundingBox volumeBox = brick.VolumeBox.Clone() as GMBoundingBox;

            volumeBox.Transformation(modelview);

            //计算在视觉矩阵下离视点最远与最近的角点
            int minIndex;
            int maxIndex;
            minIndex = 0;
            maxIndex = 7;
            for (int i = 0; i < 8; i++)
            {
                if (volumeBox.Corners[minIndex].Z > volumeBox.Corners[i].Z) minIndex = i;
                if (volumeBox.Corners[maxIndex].Z < volumeBox.Corners[i].Z) maxIndex = i;
            }

            Vector3 maxv = new Vector3(modelviewInverse.M31 * volumeBox.Corners[maxIndex].Z,
                                       modelviewInverse.M32 * volumeBox.Corners[maxIndex].Z,
                                       modelviewInverse.M33 * volumeBox.Corners[maxIndex].Z);
            Vector3 minv = new Vector3(modelviewInverse.M31 * volumeBox.Corners[minIndex].Z,
                                       modelviewInverse.M32 * volumeBox.Corners[minIndex].Z,
                                       modelviewInverse.M33 * volumeBox.Corners[minIndex].Z);

            float distance = (maxv - minv).Length();
            //_samples = 400;
            _delta = distance / _samples;
        }

        int oldFrameTime = -1;
        protected virtual void DrawViewAlignedSlices(DrawArgs drawArgs, VolumeBrick brick, Matrix viewMatrix)
        {
            try
            {
                GMBoundingBox textureBox = brick.TextureBox;
                GMBoundingBox volumeBox = brick.VolumeBox;
                GMBoundingBox rotatedBox = volumeBox.Clone() as GMBoundingBox;
                rotatedBox.Transformation(viewMatrix);

                //计算在视觉矩阵下离视点最远与最近的角点
                int minIndex;
                int maxIndex;
                minIndex = 0;
                maxIndex = 7;
                for (int i = 0; i < 8; i++)
                {
                    // z
                    if (rotatedBox.Corners[minIndex].Z > rotatedBox.Corners[i].Z) minIndex = i;
                    if (rotatedBox.Corners[maxIndex].Z < rotatedBox.Corners[i].Z) maxIndex = i;
                }

                //计算切割起算点
                Vector3 slicePoint = volumeBox.Corners[minIndex];

                //计算slicePlaneNormal = Vector(0,1,0)  * InvertedViewMatrix;
                Matrix invertedViewMatrix = Matrix.Invert(viewMatrix);
                Vector3 slicePlaneNormal = new Vector3(invertedViewMatrix.M31, invertedViewMatrix.M32, invertedViewMatrix.M33);
                slicePlaneNormal.Normalize();

                calculateSampling(brick);
                //计算两个slice间距
                Vector3 sliceDelta = slicePlaneNormal * _delta;

                //
                // Calculate edge intersections between the plane and the boxes
                //
                Vector3[] verts = new Vector3[6];   // for edge intersections
                Vector3[] tverts = new Vector3[6];  // for texture intersections
                Vector3[] rverts = new Vector3[6];  // for transformed edge intersections

                drawArgs.device.VertexFormat = PositionTextured3D.Format;

                if (isUsingLight)
                    _effect.Technique = "TextureTD_Lighting";
                else
                    _effect.Technique = "TextureTD";

                _effect.SetValue("worldMatrix", drawArgs.device.GetTransform(TransformState.World));
                _effect.SetValue("viewMatrix", drawArgs.device.GetTransform(TransformState.View));
                _effect.SetValue("WorldViewProj", drawArgs.device.GetTransform(TransformState.World) * drawArgs.device.GetTransform(TransformState.View) * drawArgs.device.GetTransform(TransformState.Projection));

                Matrix mi = drawArgs.device.GetTransform(TransformState.World) * drawArgs.device.GetTransform(TransformState.View);
                mi.Invert();
                mi = Matrix.Transpose(mi);
                _effect.SetValue("normalMat", mi);

                _effect.SetTexture("VolumeTex", brick.Texture(0));
                if (_frameNow != nTime - 1) //当frameTime
                    _effect.SetTexture("VolumeTex2", brick.Texture(1));
                else
                    _effect.SetTexture("VolumeTex2", brick.Texture(0));
                //zxq 修改库后变化
                int oldAEnable = drawArgs.device.GetRenderState(RenderState.AlphaBlendEnable);
                int oldSourceBlend = drawArgs.device.GetRenderState(RenderState.SourceBlend);
                int oldDesBlend = drawArgs.device.GetRenderState(RenderState.DestinationBlend);
                drawArgs.device.SetRenderState(RenderState.AlphaBlendEnable, true);
                drawArgs.device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                drawArgs.device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                
                _effect.Begin(FX.None);
                _effect.BeginPass(0);

                for (int i = 0; i <= _samples; i++)
                {
                    int[] order = new int[6] { 0, 1, 2, 3, 4, 5 };
                    int size = 0;
                    //if (i < 0 | i > 100) continue;
                    //
                    // 计算slice面片和box的交点
                    //
                    size = intersect(slicePoint, slicePlaneNormal, volumeBox, verts, textureBox, tverts, rotatedBox, rverts, minIndex, maxIndex);
                    //
                    // 计算交点的凸壳
                    //
                    findVertexOrder(rverts, order, size);

                    //至少有三个点，绘制面
                    if (size >= 3)
                    {
                        PositionTextured3D[] d3dVerts = new PositionTextured3D[size];
                        for (int fori = 0; fori < size; fori++)
                        {
                            d3dVerts[fori].X = verts[order[fori]].X;
                            d3dVerts[fori].Y = verts[order[fori]].Y;
                            d3dVerts[fori].Z = verts[order[fori]].Z;
                            d3dVerts[fori].Tu = tverts[order[fori]].X;
                            d3dVerts[fori].Tv = tverts[order[fori]].Y;
                            d3dVerts[fori].Tw = tverts[order[fori]].Z;
                        }

                        drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleFan, size - 2, d3dVerts);

                    }

                    //下一slice平面
                    slicePoint += sliceDelta;
                }
                _effect.EndPass();
                _effect.End();
                //zxq 修改库后变化
                drawArgs.device.SetRenderState(RenderState.AlphaBlendEnable, oldAEnable);
                drawArgs.device.SetRenderState(RenderState.SourceBlend, oldSourceBlend);
                drawArgs.device.SetRenderState(RenderState.DestinationBlend, oldDesBlend);

            }
            catch (Exception ex)
            { }
        }

        int[,] edges = new int[12, 2] { {0, 1}, // front bottom edge
                                              {1, 2}, // front left edge
                                              {2, 3}, // front right edge
                                              {3, 0}, // left bottom edge
                                              {0, 4}, // right bottom edge
                                              {1, 5}, // front top edge
                                              {2, 6}, // back bottom edge
                                              {3, 7}, // back left edge
                                              {4, 5}, // back right edge
                                              {5, 6}, // back top edge
                                              {6, 7}, // left top edge
                                              {7, 4}  // right top edge
                                            };
        /// <summary>
        /// 计算slice面与box的相交结果
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="spn"></param>
        /// <param name="volumeBox"></param>
        /// <param name="verts"></param>
        /// <param name="textureBox"></param>
        /// <param name="tverts"></param>
        /// <param name="rotatedBox"></param>
        /// <param name="rverts"></param>
        /// <returns></returns>
        public int intersect(Vector3 sp, Vector3 spn, GMBoundingBox volumeBox, Vector3[] verts, GMBoundingBox textureBox,
                      Vector3[] tverts, GMBoundingBox rotatedBox, Vector3[] rverts, int minIndex, int maxIndex)
        {
            int intersections = 0;
            for (int i = 0; i < 12; i++)
            {
                Vector3 p0 = volumeBox.Corners[edges[i, 0]];
                Vector3 p1 = volumeBox.Corners[edges[i, 1]];
                float t = Vector3.Dot(spn, (sp - p0)) / Vector3.Dot(spn, (p1 - p0));

                if ((t >= 0) && (t <= 1))
                {
                    Vector3 t0 = textureBox.Corners[edges[i, 0]];
                    Vector3 t1 = textureBox.Corners[edges[i, 1]];

                    Vector3 r0 = rotatedBox.Corners[edges[i, 0]];
                    Vector3 r1 = rotatedBox.Corners[edges[i, 1]];

                    // Compute the line intersection
                    verts[intersections].X = (p0.X + t * (p1.X - p0.X));
                    verts[intersections].Y = (p0.Y + t * (p1.Y - p0.Y));
                    verts[intersections].Z = (p0.Z + t * (p1.Z - p0.Z));

                    // Compute the texture interseciton
                    tverts[intersections].X = (clamp(t0.X + t * (t1.X - t0.X), 0f, 1f));
                    tverts[intersections].Y = (clamp(t0.Y + t * (t1.Y - t0.Y), 0f, 1f));
                    tverts[intersections].Z = (clamp(t0.Z + t * (t1.Z - t0.Z), 0f, 1f));

                    // Compute view coordinate intersection
                    rverts[intersections].X = (r0.X + t * (r1.X - r0.X));
                    rverts[intersections].Y = (r0.Y + t * (r1.Y - r0.Y));
                    rverts[intersections].Z = (r0.Z + t * (r1.Z - r0.Z));

                    intersections++;
                }
            }

            return intersections;
        }

        float clamp(float x, float min, float max)
        {
            System.Diagnostics.Trace.Assert(min <= max);
            if (x < min) return min;
            if (x > max) return max;
            return (x);
        }

        /// <summary>
        /// 计算凸壳
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="order"></param>
        /// <param name="degree"></param>
        public void findVertexOrder(Vector3[] verts, int[] order, int degree)
        {
            //
            // Find the center of the polygon
            //
            float[] centroid = new float[2] { 0.0f, 0.0f };

            for (int i = 0; i < degree; i++)
            {
                centroid[0] += verts[i].X;
                centroid[1] += verts[i].Y;
            }

            centroid[0] /= degree;
            centroid[1] /= degree;

            //
            // Compute the angles from the centroid
            //
            float[] angle = new float[6];
            float dx;
            float dy;

            for (int i = 0; i < degree; i++)
            {
                dx = verts[i].X - centroid[0];
                dy = verts[i].Y - centroid[1];

                angle[i] = dy / (Math.Abs(dx) + Math.Abs(dy));

                if (dx < 0.0f)  // quadrants 2&3
                {
                    angle[i] = 2.0f - angle[i];
                }
                else if (dy < 0.0f) // quadrant 4
                {
                    angle[i] = 4.0f + angle[i];
                }
            }

            //
            // Sort the angles (bubble sort)
            //
            for (int i = 0; i < degree; i++)
            {
                for (int j = i + 1; j < degree; j++)
                {
                    if (angle[j] < angle[i])
                    {
                        float tmpd = angle[i];
                        angle[i] = angle[j];
                        angle[j] = tmpd;

                        int tmpi = order[i];
                        order[i] = order[j];
                        order[j] = tmpi;
                    }
                }
            }
        }
    }
}
