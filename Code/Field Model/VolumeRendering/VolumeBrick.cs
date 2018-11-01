using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;
using Gaea;

namespace FieldModel
{
    public class VolumeBrick : IDisposable
    {
        private List<VolumeTexture> _texture = new List<VolumeTexture>();
        private int _textureCount = 0;
        private GMBoundingBox _textureBox = null;
        private GMBoundingBox _volumeBox = null;
        private Vector3 _center;
        private DVRBase _dvr;

        public GMBoundingBox TextureBox
        {
            get
            {
                return _textureBox;
            }
        }

        public GMBoundingBox VolumeBox
        {
            get
            {
                return _volumeBox;
            }
        }

        public Vector3 Center
        {
            get
            {
                return _center;
            }
        }

        public VolumeTexture Texture(int i)
        {
            return _texture[i];
        }

        public int ColNum
        {
            get
            {
                return _nx;
            }
        }

        public int RowNum
        {
            get
            {
                return _ny;
            }
        }

        public float MinX
        {
            get
            {
                return _minX;
            }
        }

        public float MinY
        {
            get
            {
                return _minY;
            }
        }

        public float MinZ
        {
            get
            {
                return _minZ;
            }
        }

        public float LonStep
        {
            get
            {
                return _sepDisX;
            }
        }

        public float LatStep
        {
            get
            {
                return _sepDisY;
            }
        }

        public int TextureCount
        {
            get { return _texture.Count; }
        }

        float _minX, _minY, _minZ, _sepDisX, _sepDisY, _sepDisZ;
        public int _ix, _iy, _nx, _ny;
        public float _cullx, _cully;
        public VolumeBrick(DVRBase dvr, float minX, float minY, float minZ, float sepDisX, float sepDisY, float sepDisZ, int ix, int iy, int nx, int ny, int nz)
        {
            _dvr = dvr;
            _minX = minX;
            _minY = minY;
            _minZ = minZ;
            _sepDisX = sepDisX;
            _sepDisY = sepDisY;
            _sepDisZ = sepDisZ;
            _ix = ix;
            _iy = iy;
            _nx = nx;
            _ny = ny;

            VolumeTexture textTure1 = null;
            VolumeTexture textTure2 = null;

            textTure1 = new VolumeTexture(DrawArgs.Device, nx, ny, nz, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            textTure2 = new VolumeTexture(DrawArgs.Device, nx, ny, nz, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            _texture.Add(textTure1);
            _texture.Add(textTure2);

            #region create texture box
            _textureBox = new GMBoundingBox(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                                          new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1));

            minX += sepDisX * ix;
            minY += sepDisY * iy;

            //create volume box
            float vbXWidth = 0f;
            float vbYWidth = 0f;
            float vbZWidth = 0f;
            vbXWidth = sepDisX * nx;
            vbYWidth = sepDisY * ny;
            vbZWidth = sepDisZ * nz;
            Vector3 vb0 = GMMaths.SphericalToCartesian(minY, minX, DrawArgs.World.EquatorialRadius + (double)minZ);
            Vector3 vb1 = GMMaths.SphericalToCartesian(minY, minX + vbXWidth, DrawArgs.World.EquatorialRadius + (double)minZ);
            Vector3 vb2 = GMMaths.SphericalToCartesian(minY + vbYWidth, minX + vbXWidth, DrawArgs.World.EquatorialRadius + (double)minZ);
            Vector3 vb3 = GMMaths.SphericalToCartesian(minY + vbYWidth, minX, DrawArgs.World.EquatorialRadius + (double)minZ);
            Vector3 vb4 = GMMaths.SphericalToCartesian(minY, minX, DrawArgs.World.EquatorialRadius + (double)minZ + (double)vbZWidth);
            Vector3 vb5 = GMMaths.SphericalToCartesian(minY, minX + vbXWidth, DrawArgs.World.EquatorialRadius + (double)minZ + (double)vbZWidth);
            Vector3 vb6 = GMMaths.SphericalToCartesian(minY + vbYWidth, minX + vbXWidth, DrawArgs.World.EquatorialRadius + (double)minZ + (double)vbZWidth);
            Vector3 vb7 = GMMaths.SphericalToCartesian(minY + vbYWidth, minX, DrawArgs.World.EquatorialRadius + (double)minZ + (double)vbZWidth);
            //       7*--------*6
            //       /|       /|
            //      / |      / |
            //     /  |     /  |
            //    /  3*----/---*2
            //  4*--------*5  /
            //   |  /     |  /
            //   | /      | /
            //   |/       |/
            //  0*--------*1
            _volumeBox = new GMBoundingBox(vb0, vb1, vb2, vb3, vb4, vb5, vb6, vb7);
            _center = _volumeBox.CalculateCenter();
             
            #endregion
        }

        //内存泄露问题
        public void CreateVolumeTexture(float[, ,] volumeData)
        {
            int temp = 0;
            int nz = volumeData.GetLength(0);
            VolumeTexture texture = new VolumeTexture(DrawArgs.Device, _nx, _ny, nz, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            DataBox data = texture.LockBox(0, LockFlags.None);
            BinaryWriter bw = new BinaryWriter(data.Data);
            bw.Seek(0, SeekOrigin.Begin);
            for (int l = 0; l < nz; l++)
            {
                for (int r = 0; r < _ny; r++)
                {
                    for (int c = 0; c < _nx; c++)
                    {
                        temp = _dvr.ActualValueToTransferMapCoord(volumeData[l, r + _iy, c + _ix]);
                        bw.Write(temp << 16);
                    }
                }
            }
            texture.UnlockBox(0);

            VolumeTexture tmp = _texture[0];
            _texture[0] = _texture[1];
            _texture[1] = texture;
            tmp.Dispose();
        }

        VolumeTexture texture = null;
        public void UpdateVolumeTexture(float[, ,] volumeData)
        {
            VolumeTexture tmp = _texture[0];
            _texture[0] = _texture[1];

            int temp = 0;
            int nz = volumeData.GetLength(0);
            if (texture != null)
                texture.Dispose();
            texture = new VolumeTexture(DrawArgs.Device, _nx, _ny, nz, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            DataBox db = texture.LockBox(0,LockFlags.None);
            BinaryWriter bw = new BinaryWriter(db.Data);
            bw.Seek(0, SeekOrigin.Begin);
            for (int l = 0; l < nz; l++)
            {
                for (int r = 0; r < _ny; r++)
                {
                    for (int c = 0; c < _nx; c++)
                    {
                        temp = _dvr.ActualValueToTransferMapCoord(volumeData[l, r + _iy, c + _ix]);
                        bw.Write(temp << 16);
                    }
                }
            }
            texture.UnlockBox(0);

            _texture[1] = texture;
            tmp.Dispose();
            GC.Collect();
        }

        public VolumeTexture GetBrickTexture(int index)
        {
            return _texture[index];
        }

        #region IDisposable 成员

        public void Dispose()
        {
            foreach (var v in _texture)
                v.Dispose();
        }

        #endregion
    }
}
