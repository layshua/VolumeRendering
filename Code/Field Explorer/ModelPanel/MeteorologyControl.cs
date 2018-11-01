using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using Gaea.Renderable;
using Gaea.Controls;
using FieldModel;

namespace FieldExplorer.Meteorology
{
    public partial class MeteorologyControl : DevExpress.XtraEditors.XtraUserControl
    {
        private RenderableObjectList _renderableObjectList;  //模型列表，用来添加各种模型渲染对象
        private WorldControl _worldControl;

        private MeteorDataProvider _mdProvider;
        public MeteorologyControl(WorldControl wc)
        {
            InitializeComponent();
            _worldControl = wc;
        }

        //zxq 外部获取数据
        public MeteorDataProvider DataProvider
        {
            get
            {
                return _mdProvider;
            }
        }
     

        public bool IsInitialized()
        {
            return _mdProvider != null;
        }

        public void Initialize(MeteorDataProvider mdProvider)
        {
            _mdProvider = mdProvider;

            _renderableObjectList = new RenderableObjectList("MeteorologyRenderObjectList");
            _worldControl.CurrentWorld.Add(_renderableObjectList);

            //初始化TabControl中的各个page页面
            InitializeTabControl();
        }

        public void Destructize()
        {
            _mdProvider = null;
            _worldControl.CurrentWorld.Remove(_renderableObjectList);
            _renderableObjectList.Dispose();
            _renderableObjectList = null;
            GC.Collect();
        }

        void InitializeTabControl()
        {
            try {

                ////tabpage6
                IVolumeRenderUControl ivrUC = new IVolumeRenderUControl(_renderableObjectList, _mdProvider);
                ivrUC.Dock = DockStyle.Fill;


                this.Controls.Add(ivrUC);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

    }
}
