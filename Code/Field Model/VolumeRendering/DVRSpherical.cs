using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using Gaea;

namespace FieldModel
{
    public class DVRSpherical : DVRTextured3D
    {
        protected int _nr;
        protected float _shellWidth;
        protected int level;

        public DVRSpherical(string varName, FieldModel.MeteorDataProvider mdProvider, Bitmap bitmap, int col, int row, float modelH, float exgg, Point pLeft, Point pRight)
            :base(varName, mdProvider, bitmap, col, row, modelH, exgg, pLeft, pRight)
        {
            if (_bricks != null)
                _bricks.Clear();
            else
                _bricks = new List<VolumeBrick>();

            _varName = varName;
            _mdProvider = mdProvider;
            nTime = mdProvider.meteorMetaData.nTime;
            nLevel = mdProvider.meteorMetaData.nLevel;
            nRow = mdProvider.meteorMetaData.nRow;
            nCol = mdProvider.meteorMetaData.nColumn;
            _vectorA = new float[nRow * nCol * nLevel];
            _cacheArr = new float[nRow * nCol * nLevel];

            _mdProvider.GetData(_mdProvider.ConfigPath, varName, FieldModel.TimeController.Instance.TimeNumber);
            _mdProvider.meteorData.varList[0].CopyTo(_vectorA, 0);

            uvw = new float[nLevel, nRow, nCol];
            int idx = 0;
            for (int l = 0; l < nLevel; l++)
                for (int r = 0; r < nRow; r++)
                    for (int c = 0; c < nCol; c++)
                    {
                        uvw[l, r, c] = _vectorA[idx];
                        idx++;
                    }

            float latOri, lonOri, intervalLat, intervalLon;
            lonOri = (float)_mdProvider.meteorMetaData.lons[0].Degrees;
            latOri = (float)_mdProvider.meteorMetaData.lats[0].Degrees;
            intervalLon = (float)(_mdProvider.meteorMetaData.lons[1].Degrees - _mdProvider.meteorMetaData.lons[0].Degrees);
            intervalLat = (float)(_mdProvider.meteorMetaData.lats[1].Degrees - _mdProvider.meteorMetaData.lats[0].Degrees);
            _bricks.Add(new VolumeBrick(this, lonOri, latOri, modelH, intervalLon, intervalLat, exgg, 0, 0, 429, 267, nLevel));
        }

        public override void Initialize(Gaea.DrawArgs drawArgs)
        {
            string outerrors = null;
            if (_effect == null || _effect.Disposed)
            {
                string fileName = System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, "Shaders\\Spherical_Shader.fx");
                StreamReader tr = new StreamReader(fileName);
                string ef = tr.ReadToEnd();
                tr.Close();
                try
                {
                    _effect = Effect.FromString(
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
                {}
                if (outerrors != null && outerrors.Length > 0)
                {
                    throw new Exception(outerrors);
                }
            }
            _effect.SetTexture("TransferMap", _transferMap);
        }

        public override void RenderBricks(DrawArgs drawArgs)
        {
            drawArgs.device.SetRenderState(RenderState.ZEnable, false);

            if (_frameNow < nTime)
            {
                if (_frameNow != oldFrameTime)
                {
                    oldFrameTime = _frameNow;  //更新旧一帧的数值
                    if (!_mdProvider.StartReading)
                    {
                        if (_frameNow == nTime - 1) //当frameTime为最后一帧是，预读第一帧数据
                            _mdProvider.GetDataAsynEx(_mdProvider.ConfigPath, _varName, 0, _cacheArr);
                        else                        //其他情况则读下一帧数据
                            _mdProvider.GetDataAsynEx(_mdProvider.ConfigPath, _varName, _frameNow + 1, _cacheArr);
                    }
                    if (_mdProvider.IsEndRead)
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
                        _bricks[0].CreateVolumeTexture(uvw);
                    }
                }
            }

            if (_frameNow != 0)
                RenderBrick(drawArgs, _bricks[0]);
        }

        protected override void calculateSampling(VolumeBrick brick)
        {
            
             
            Matrix modelview = DrawArgs.Instance.WorldCamera.AbsoluteViewMatrix;
            Matrix modelviewInverse = Matrix.Invert(modelview);

            GMBoundingBox volumeBox = brick.VolumeBox.Clone() as GMBoundingBox;
            volumeBox.Transformation(modelview);
            /*
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
            */

            //_samples = 3000;
            _delta = (float)((DrawArgs.Camera.WorldRadius + 480000) / (double)_samples);
        }

        float max = 0.0f;
        float min = 3000.0f;
        int oldFrameTime = -1;

        Vector3 calculatedSliceRay = Vector3.Zero;
        List<PositionTextured3D[]> calculatedSlices = new List<PositionTextured3D[]>();

        protected override void DrawViewAlignedSlices(DrawArgs drawArgs, VolumeBrick brick, Matrix viewMatrix)
        {
            double outerplus = 480000, interplus=100000;
            double r0 = drawArgs.WorldCamera.WorldRadius + interplus;
            double r1 = drawArgs.WorldCamera.WorldRadius + outerplus;
            //drawArgs.device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);

            GMBoundingBox textureBox = brick.TextureBox;
            GMBoundingBox volumeBox = brick.VolumeBox;
            GMBoundingBox rotatedBox = volumeBox.Clone() as GMBoundingBox;
            rotatedBox.Transformation(viewMatrix);
            GMVector3D tempp = drawArgs.WorldCamera.ReferenceCenter;
            //Vector3 dir = drawArgs.WorldCamera.Position - new Vector3((float)tempp.X, (float)tempp.Y, (float)tempp.Z);
            Vector3 dir = new Vector3(drawArgs.WorldCamera.Position.x - (float)tempp.X, drawArgs.WorldCamera.Position.y - (float)tempp.Y,
                drawArgs.WorldCamera.Position.z - (float)tempp.Z);

            Vector3 slicePlaneNormal = new Vector3(-dir.X, -dir.Y, -dir.Z);
            slicePlaneNormal.Normalize();




            calculateSampling(brick);

            drawArgs.device.VertexFormat = PositionTextured3D.Format;
            _effect.Technique = "Spherical";
            _effect.SetTexture("VolumeTex", brick.Texture(0));
            if (_frameNow != nTime - 1) //当frameTime
                _effect.SetTexture("VolumeTex2", brick.Texture(1));
            else
                _effect.SetTexture("VolumeTex2", brick.Texture(0));

            _effect.Begin(FX.None);
            _effect.BeginPass(0);

            if (true||calculatedSliceRay == Vector3.Zero || calculatedSliceRay != slicePlaneNormal)
            {
                calculatedSlices.Clear();

               
                //计算两个slice间距
                Vector3 sliceDelta = slicePlaneNormal * _delta;

                Vector3 center = new Vector3(0, 0, 0);
                Vector3 slicePoint = center + ((float)r1 * slicePlaneNormal) - sliceDelta;

                Vector3 sliceOrtho;

                if (Vector3.Dot(slicePlaneNormal, new Vector3(1, 1, 0)) != 0.0)
                    sliceOrtho = Vector3.Cross(slicePlaneNormal, new Vector3(1, 1, 0));
                else
                    sliceOrtho = Vector3.Cross(slicePlaneNormal, new Vector3(0, 1, 0));
                sliceOrtho.Normalize();


                float left = (brick.MinX + 180) / 360.0f;
                float buttom = (brick.MinY + 90) / 180.0f;
                float right = (brick.MinX + 180 + brick._nx * brick.LonStep) / 360.0f;
                float upp = (brick.MinY + 90 + brick._ny * brick.LatStep) / 180.0f;
                float lowest = brick.MinZ;

                for (int i = 0; i <= _samples; i++)
                {
                    float d = (center - slicePoint).Length();
                    if (r1 > d)
                    {
                        float rg0 = 0.0f;
                        float rg1 = (float)Math.Sqrt(r1 * r1 - d * d);
                        if (r0 > d) rg0 = (float)Math.Sqrt(r0 * r0 - d * d);

                        int m = 0;
                        PositionTextured3D[] d3dVerts = new PositionTextured3D[90];
                        for (double theta = -Math.PI / 20.0; theta <= 2 * Math.PI; theta += Math.PI / 20.0)
                        {
                            Vector3 v = sliceOrtho * (float)Math.Cos(theta) + (float)Math.Sin(theta) * Vector3.Cross(slicePlaneNormal, sliceOrtho);
                            v.Normalize();

                            Vector3 v0 = slicePoint + v * rg0;
                            Vector3 v1 = slicePoint + v * rg1;

                            Vector3 LonLat0 = GMMaths.CartesianToSpherical2(v0.X, v0.Y, v0.Z);
                            Vector3 LonLat1 = GMMaths.CartesianToSpherical2(v1.X, v1.Y, v1.Z);

                            if (LonLat0.Z >= max) max = LonLat0.Z;
                            if (LonLat1.Z >= max) max = LonLat1.Z;
                            if (LonLat0.Z <= min) min = LonLat0.Z;
                            if (LonLat1.Z <= min) min = LonLat1.Z;

                            Vector3 t0 = new Vector3(0, 0, 0);
                            Vector3 t1 = new Vector3(0, 0, 0);

                            t0.X = (LonLat0.Z + 180) / 360.0f;
                            t0.Y = (LonLat0.Y + 90.0f) / 180.0f;
                            t0.Z = 1.0f - (v0.Length() - (float)r0) / (float)(outerplus - interplus);
                            t1.X = (LonLat1.Z + 180) / 360.0f;
                            t1.Y = (LonLat1.Y + 90.0f) / 180.0f;
                            t1.Z = 1.0f - (v1.Length() - (float)r0) / (float)(outerplus - interplus);

                            t0.X = (t0.X - left) / (right - left);
                            t0.Y = (t0.Y - buttom) / (upp - buttom);
                            t1.X = (t1.X - left) / (right - left);
                            t1.Y = (t1.Y - buttom) / (upp - buttom);

                            d3dVerts[m].X = v0.X;
                            d3dVerts[m].Y = v0.Y;
                            d3dVerts[m].Z = v0.Z;
                            d3dVerts[m].Tu = t0.X;
                            d3dVerts[m].Tv = t0.Y;
                            d3dVerts[m].Tw = t0.Z;
                            m++;
                            d3dVerts[m].X = v1.X;
                            d3dVerts[m].Y = v1.Y;
                            d3dVerts[m].Z = v1.Z;
                            d3dVerts[m].Tu = t1.X;
                            d3dVerts[m].Tv = t1.Y;
                            d3dVerts[m].Tw = t1.Z;
                            m++;
                        }
                        drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m - 4, d3dVerts);
                       
                        calculatedSlices.Add(d3dVerts);
                    }
                    slicePoint -= sliceDelta;
                }
            }
            else
            {
                for (int i = 0; i < calculatedSlices.Count;i++ )
                {
                    drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 80, calculatedSlices[i]);
                }

            }

           
            _effect.EndPass();
            _effect.End();

            calculatedSliceRay = slicePlaneNormal;
        }
    }
}
