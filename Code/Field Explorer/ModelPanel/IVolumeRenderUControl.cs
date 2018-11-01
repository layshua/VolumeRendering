using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Gaea.Renderable;
using SlimDX;
using SlimDX.Direct3D9;
using FieldModel;
using FieldModel;
using Gaea;

namespace FieldExplorer.Meteorology
{
    public partial class IVolumeRenderUControl : UserControl
    {
        private RenderableObjectList _roList;
        private VolumeRenderer _volumeRenderer;
        private MeteorDataProvider _mdProvider;
        private Bitmap _tranferMap;
        float maxValue = 0.0f;
        float minValue = 255.0f;
        short modelNum = 0;

        private List<int> histog = new List<int>();

        public IVolumeRenderUControl()
        {
            InitializeComponent();
        }

        public bool IsInt(string message)
        {
            int result = 0;
            try
            {
                result = Convert.ToInt32(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SaveVarStatistics(string savePath)
        {
            int len = 0;

            if (!File.Exists(savePath))
            {
                FileStream fs1 = new FileStream(savePath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                sw.Close();
                fs1.Close();
            }
            //写xml配置文件
            XmlDocument XMLDoc = new System.Xml.XmlDocument();
            XMLDoc.LoadXml("<?xml version='1.0' encoding='UTF-8'?>" + "<StaticVar></StaticVar>");
            XmlNode root = XMLDoc.DocumentElement;

            for (int v = 0; v < _mdProvider.meteorMetaData.varNameList.Count - 5; v++)
            {
                string name = _mdProvider.meteorMetaData.varNameList[v];
                XmlElement dataEle = XMLDoc.CreateElement(name);

                XmlElement metaEleSub = null;
                _mdProvider.GetData(_mdProvider.ConfigPath, name, 4);
                len = _mdProvider.meteorData.varList[0].Length;
                float[] vec = new float[len];
                _mdProvider.meteorData.varList[0].CopyTo(vec, 0);
                float min = 50000.0f, max = 0.0f;
                for (int i = 0; i < len; i++)
                {
                    if (vec[i] > max && vec[i] < 50000) max = vec[i];
                    if (vec[i] < min && vec[i] > 0.000000001) min = vec[i];
                }
                dataEle.SetAttribute("MIN", min.ToString());
                dataEle.SetAttribute("MAX", max.ToString());
                float step = (max - min) / 400;
                float uppr = 0.0f, lower = min;
                int count = 0;
                for (int i = 0; i < 400; i++)
                {
                    count = 0;
                    lower += step;
                    uppr = lower + step;
                    for (int j = 0; j < len; j++)
                        if (vec[j] < uppr && vec[j] > lower) count++;

                    metaEleSub = XMLDoc.CreateElement("区间" + i.ToString());
                    metaEleSub.InnerText = count.ToString();
                    metaEleSub.SetAttribute("LOW", lower.ToString());
                    metaEleSub.SetAttribute("HIGH", uppr.ToString());
                    dataEle.AppendChild(metaEleSub);
                }
                root.AppendChild(dataEle);
            }
            XMLDoc.Save(savePath);
        }

        private void GetHistoData(string VarName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            XmlNodeList nodeLists = null;
            histog.Clear();
            string p = System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, "Shaders\\data.xml");
            xmlDoc.Load(p);
            string _datasetName = xmlDoc.DocumentElement.Name;
            if (_datasetName == "StaticVar")
            {
                XmlNode root = xmlDoc.SelectSingleNode("StaticVar");
                XmlNode cursor = root.FirstChild;
                while (cursor != null && cursor.Name != VarName)
                {
                    cursor = cursor.NextSibling;
                }
                if (cursor == null) return;
                minValue = float.Parse((cursor as XmlElement).GetAttribute("MIN"));
                maxValue = float.Parse((cursor as XmlElement).GetAttribute("MAX"));
                nodeLists = cursor.ChildNodes;
                for (int i = 0; i < 400; i++)
                {
                    histog.Add(int.Parse(nodeLists[i].InnerText));
                }
            }
        }

        public IVolumeRenderUControl(RenderableObjectList roList, MeteorDataProvider mdProvider)
        {
            InitializeComponent();
            _roList = roList;
            _mdProvider = mdProvider;
            _tranferMap = new Bitmap(2550, 1);
            this.dispModels.SelectedIndex = 0;
            for (int i = 0; i < _mdProvider.meteorMetaData.varNameList.Count - 5; i++)
            {
                CBoxAVariables.Properties.Items.Add(_mdProvider.meteorMetaData.varNameList[i]);
            }
            for (int i = 0; i < _mdProvider.meteorMetaData.MergeNameList.Count; i++)
            {
                CBoxAVariables.Properties.Items.Add(_mdProvider.meteorMetaData.MergeNameList[i]);
            }
            CBoxAVariables.SelectedIndex = 0;
            this.volumeRendererSymbolControl1.TransferMapChanged += new TransferMapChangedHandler(volumeRendererSymbolControl1_TransferMapChanged);
            this.volumeRendererSymbolControl1.TipInfoChanged += new TipInfoChangedHandler(volumeRendererSymbolControl1_TipInfoChanged);


            //写XML数据文档
            string path = System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, "Shaders\\data.xml");
            if (!File.Exists(path))
                SaveVarStatistics(path);
            GetHistoData((string)CBoxAVariables.SelectedItem);
            this.volumeRendererSymbolControl1.histogram = histog;
            this.volumeRendererSymbolControl1.Refresh();
        }

        void volumeRendererSymbolControl1_TipInfoChanged(int color, int value)
        {
            HSV hsv = MathUitilities.RGBtoHSV(color);
            this.rgbLabel.Text = "RGB: " + ((color >> 16) & 255).ToString() + ";" + ((color >> 8) & 255).ToString() + ";" + (color & 255).ToString();
            this.hsvLabel.Text = "HSV: " + hsv.H.ToString() + ";" + hsv.S.ToString() + ";" + hsv.V.ToString();
            this.valueLable.Text = "Value: " + ((value / 2550.0) * (maxValue - minValue)).ToString();
        }

        private void volumeRendererSymbolControl1_TransferMapChanged(Bitmap transferMap)
        {
            if (_volumeRenderer != null)
                if (_volumeRenderer.DVRDriver != null)
                {
                    _tranferMap = transferMap;
                    _volumeRenderer.DVRDriver.SetTransferMap(transferMap);
                }
        }

       

        private void isUsingLights_CheckedChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
                if (_volumeRenderer.DVRDriver != null)
                    _volumeRenderer.DVRDriver.IsUsingLight = this.isUsingLights.Checked;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            List<ControlPoint> colorCtrl = volumeRendererSymbolControl1.getColorCtrlPoints();
            List<ControlPoint> opacity = volumeRendererSymbolControl1.getOpacityCtrlPoints();

            string svePath = string.Empty;
            SaveFileDialog opF = new SaveFileDialog();
            if (opF.ShowDialog() == DialogResult.OK)
            {
                svePath = opF.FileName;
            }
            else
                return;

            if (!File.Exists(svePath))
            {
                FileStream fs1 = new FileStream(svePath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(svePath, FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                sr.Close();
                fs.Close();
            }

            if (File.Exists(svePath))
            {
                XmlDocument XMLDoc = new System.Xml.XmlDocument();
                XMLDoc.LoadXml("<?xml version='1.0' encoding='UTF-8'?>" + "<ControlPoints></ControlPoints>");
                XmlNode root = XMLDoc.DocumentElement;

                XmlElement dataEle = XMLDoc.CreateElement("ColorControlPoints");
                XmlElement metaEleSub = null;
                for (int i = 0; i < colorCtrl.Count; i++)
                {
                    metaEleSub = XMLDoc.CreateElement("点" + i.ToString());
                    metaEleSub.InnerText = colorCtrl[i].value.ToString();
                    metaEleSub.SetAttribute("X", colorCtrl[i].Point.X.ToString());
                    metaEleSub.SetAttribute("Y", colorCtrl[i].Point.Y.ToString());
                    dataEle.AppendChild(metaEleSub);
                }
                root.AppendChild(dataEle);

                dataEle = XMLDoc.CreateElement("OpacityControlPoints");
                metaEleSub = null;
                for (int i = 0; i < opacity.Count; i++)
                {
                    metaEleSub = XMLDoc.CreateElement("点" + i.ToString());
                    metaEleSub.InnerText = opacity[i].value.ToString();
                    metaEleSub.SetAttribute("X", opacity[i].Point.X.ToString());
                    metaEleSub.SetAttribute("Y", opacity[i].Point.Y.ToString());
                    dataEle.AppendChild(metaEleSub);
                }
                root.AppendChild(dataEle);
                XMLDoc.Save(svePath);
                MessageBox.Show("存储成功!");
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            List<ControlPoint> colorCtrl = new List<ControlPoint>();
            List<ControlPoint> opacity = new List<ControlPoint>();

            OpenFileDialog opF = new OpenFileDialog();
            opF.Filter = "XML文件|*.xml";
            string path = string.Empty;
            if (opF.ShowDialog() == DialogResult.OK)
            {
                path = opF.FileName;
            }
            else
                return;

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            xmlDoc.Load(path);
            string dataSetName = xmlDoc.DocumentElement.Name;
            if (dataSetName == "ControlPoints")
            {
                XmlNode root = xmlDoc.SelectSingleNode("ControlPoints");
                XmlNode cursor = root.FirstChild;

                if (cursor.Name == "ColorControlPoints")
                {
                    XmlNodeList nodeLists = cursor.ChildNodes;
                    for (int i = 0; i < nodeLists.Count; i++)
                    {
                        ControlPoint cp = new ControlPoint();
                        cp.value = Convert.ToInt32(nodeLists[i].InnerText);
                        cp.Point.X = Convert.ToInt32((nodeLists[i] as XmlElement).GetAttribute("X"));
                        cp.Point.Y = Convert.ToInt32((nodeLists[i] as XmlElement).GetAttribute("Y"));
                        colorCtrl.Add(cp);
                    }
                }
                cursor = cursor.NextSibling;
                if (cursor.Name == "OpacityControlPoints")
                {
                    XmlNodeList nodeLists = cursor.ChildNodes;
                    for (int i = 0; i < nodeLists.Count; i++)
                    {
                        ControlPoint cp = new ControlPoint();
                        cp.value = Convert.ToInt32(nodeLists[i].InnerText);
                        cp.Point.X = Convert.ToInt32((nodeLists[i] as XmlElement).GetAttribute("X"));
                        cp.Point.Y = Convert.ToInt32((nodeLists[i] as XmlElement).GetAttribute("Y"));
                        opacity.Add(cp);
                    }
                }
            }

            volumeRendererSymbolControl1.setColorCtrlPoints(colorCtrl);
            volumeRendererSymbolControl1.setOpacityCtrlPoints(opacity);
            volumeRendererSymbolControl1.Refresh();
            volumeRendererSymbolControl1.Update();
            if (_volumeRenderer != null)
                if (_volumeRenderer.DVRDriver != null)
                    _volumeRenderer.DVRDriver.SetTransferMap(volumeRendererSymbolControl1.getBitMap());
        }

        private void X_cuts_EditValueChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
                if (_volumeRenderer.DVRDriver != null)
                {
                    _volumeRenderer.DVRDriver.CullX = (float)X_cuts.Value;
                    _volumeRenderer.DVRDriver.CullDirty = true;
                }
        }

        private void Y_cuts_EditValueChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
                if (_volumeRenderer.DVRDriver != null)
                {
                    _volumeRenderer.DVRDriver.CullY = (float)Y_cuts.Value;
                    _volumeRenderer.DVRDriver.CullDirty = true;
                }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (!IsInt(leftXs.Text) || !IsInt(leftYs.Text) || !IsInt(rightXs.Text) || !IsInt(rightYs.Text))
            {
                MessageBox.Show("输入必须为数字!");
            }
            int lx = Convert.ToInt32(leftXs.Text);
            int ly = Convert.ToInt32(leftYs.Text);
            int rx = Convert.ToInt32(rightXs.Text);
            int ry = Convert.ToInt32(rightYs.Text);
            if (_volumeRenderer != null)
                _volumeRenderer.SetCorners(lx, ly, rx, ry);
            leftXs.Enabled = false;
            leftYs.Enabled = false;
            rightXs.Enabled = false;
            rightYs.Enabled = false;
        }

        private void rownumUpdowns_EditValueChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
                _volumeRenderer.SetBlockSize((int)columnumUpdowns.Value, (int)rownumUpdowns.Value);
        }

        private void columnumUpdowns_EditValueChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
                _volumeRenderer.SetBlockSize((int)columnumUpdowns.Value, (int)rownumUpdowns.Value);
        }

        private void modelHeights_EditValueChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
                _volumeRenderer.SetBlkHeight((float)modelHeights.Value);
        }

        private void zExggrates_EditValueChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
                _volumeRenderer.SetBlkExgrte((float)zExggrates.Value);
        }

        private void CBoxAVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string varName = (string)CBoxAVariables.SelectedItem;
            string path = System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, "Shaders\\data.xml");
            if (!File.Exists(path))
                SaveVarStatistics(path);
            GetHistoData((string)CBoxAVariables.SelectedItem);
            this.volumeRendererSymbolControl1.histogram = histog;
            this.volumeRendererSymbolControl1.minimun = minValue;
            this.volumeRendererSymbolControl1.medium = (minValue + maxValue) / 2;
            this.volumeRendererSymbolControl1.maximun = maxValue;
            this.volumeRendererSymbolControl1.Refresh();
            volumeRendererSymbolControl1.Update();
            Point xySize = new Point();
            xySize.Y=_mdProvider.meteorMetaData.nRow;
            xySize.X = _mdProvider.meteorMetaData.nColumn;

            _volumeRenderer = new VolumeRenderer(volumeRendererSymbolControl1.getBitMap(), varName, _mdProvider, 0, new Color4(0, 0, 0, 0), 0, 0, 0, 0, 0, 0, xySize);
            if (this.dispModels.SelectedIndex == 0)
                _volumeRenderer.renderModel = 0;
            else
                _volumeRenderer.renderModel = 1;
        }

        private void CBoxEVariables_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnDeleteROS_Click(object sender, EventArgs e)
        {
            if (_roList.ChildObjects.Contains(_volumeRenderer as RenderableObject))
            {
                _roList.Remove(_volumeRenderer);
                _volumeRenderer.Dispose();
                CBoxEVariables.Properties.Items.Remove(CBoxEVariables.SelectedItem);
                CBoxEVariables.SelectedIndex = CBoxEVariables.Properties.Items.Count - 1;
                if (CBoxEVariables.Properties.Items.Count == 0)
                    CBoxEVariables.Text = null;
                this.dispModels.Enabled = true;
            }
            xtraTabControl2.Enabled = true;
            leftXs.Enabled = true;
            leftYs.Enabled = true;
            rightXs.Enabled = true;
            rightYs.Enabled = true;
        }

        private void btnAddROS_Click(object sender, EventArgs e)
        {
            if (_roList.ChildObjects.Contains(_volumeRenderer as RenderableObject))
                return;
            else
            {
                _roList.Add(_volumeRenderer);
                CBoxAVariables.SelectedIndex = CBoxAVariables.SelectedIndex;
                CBoxEVariables.Properties.Items.Add(CBoxAVariables.SelectedItem);
                CBoxEVariables.SelectedIndex = CBoxEVariables.Properties.Items.Count - 1;
                this.dispModels.Enabled = false;
            }
            xtraTabControl2.Enabled = false;

        }

        private void dispModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_volumeRenderer != null)
            {
                if (this.dispModels.SelectedIndex == 0)
                    _volumeRenderer.renderModel = 0;
                else
                    _volumeRenderer.renderModel = 1;
            }
        }

        private void leftXs_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void sampleEdit_EditValueChanged(object sender, EventArgs e)
        {
            _volumeRenderer.DVRDriver.Samples = Convert.ToInt32(this.sampleEdit.Value);
        }

       
    }
}
