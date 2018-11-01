//**************************************************************//
//**************************************************************//
//*******           Copyright (C) 2010-2018              *******//
//*******           Team: Wuhan University               *******//
//*******           Developer:Xuequan Zhang              *******//
//*******           EMail:brucezhang@whu.edu.cn          *******//
//*******           Address: 129 Luoyu Road, Wuhan,      *******//
//*******                    Hubei, 430079, China        *******//
//**************************************************************//
//**************************************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Helpers;
using Gaea;
using Gaea.Controls;
using Gaea.Core;

using Gaea.PluginEngine;
using Gaea.Camera;
using Gaea.Net;
using Gaea.Net.Wms;
using Gaea.Terrain;
using Gaea.Renderable;
using System.Xml;
using Gaea.GeoDatabase;
using Gaea.Display;
using Gaea.Carto;
using Gaea.Catalog;
using Gaea.Catalog.View;
using Gaea.DockControls.Forms;
using Gaea.DockControls.Docking;
using Gaea.DockControls.Controls;
using Gaea.DockControls.Common;
using Altova.Types;
using FieldModel;
using Gaea.Components;

namespace FieldExplorer
{
    public partial class Explorer : RibbonForm
    {
        WorldControl _WorldControl;
        SystemSettings _Settings;

        public WorldControl WorldControl
        {
            get
            {
                return _WorldControl;
            }
        }

        public Explorer( )
        {
            InitializeComponent( );
            using(Splash sp = new Splash())
            {
                sp.Owner = this;
                sp.Show( );
                Application.DoEvents( );
                Initialize( );
                this.Text = "Interactive 4D Field Visual Analysis System (4DFVAS)";
                sp.Close( );
            }
        }

        void Initialize( )
        {
            InitSkinGallery( );     

            InitWorldControl();

            
            CreateWorkGUI();

            MeteoModelPanel.Close();
            AnalysisPanel.Close();

            WorldManager.Settings.ShowCompass = true;
            WorldManager.Settings.ShowLogo = false;
            WorldManager.Settings.ShowGlobeClouds = false;
            WorldManager.Settings.ShowOcean = false;
            WorldManager.Settings.ShowDownloadIndicator = false;
            WorldManager.Settings.ShowStatusInfo = false;
            WorldManager.Settings.ShowScale = false;
            WorldManager.Settings.ShowUpdateInfo = true;
           
        }

        
        void InitWorldControl( )
        {
            try
            {
                _WorldControl = new WorldControl( );
                
                _WorldControl.Dock = DockStyle.Fill;
                _WorldControl.Cursor = Cursors.Default;
                _WorldControl.Location = new Point( 0 , 0 );
                _WorldControl.Name = "_WorldControl";
                _WorldControl.TabIndex = 0;
                _WorldControl.Text = "WorldControl1";
                this.panelControl1.Controls.Add(_WorldControl);

                _WorldControl.InitializeEngineSystem( );
                _WorldControl.InitializeScene( );
                _Settings = _WorldControl.Settings;

                if (_Settings.ConfigurationWizardAtStartup)
                {
                    if (!File.Exists(_Settings.FileName))
                    {
                        _Settings.ConfigurationWizardAtStartup = false;
                    }
                }

                _WorldControl.OpenWorld( WorldNames.Earth );
                _WorldControl.StartRenderingLoop( );

                
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        Meteorology.MeteorologyControl _metroControl = null;
        FieldModel.MeteorDataProvider mdp = null;

        private void CreateWorkGUI()
        {
            _metroControl = new Meteorology.MeteorologyControl(this.WorldControl);
            MeteoModelPanel.Width = _metroControl.Width;
            MeteoModelPanel.Controls.Add(_metroControl);

        }

        void InitSkinGallery( )
        {
            SkinHelper.InitSkinPopupMenu( iStyles );
            UserLookAndFeel.Default.SetSkinStyle( "Office 2010 Black" );
        }

        
        //mouse
        private void barButtonItem47_ItemClick(object sender, ItemClickEventArgs e)
        {
            _WorldControl.MousePointer = Gaea.Controls.gaeaControlsMousePointer.gaeaPointerArrow;
            DrawArgs.MouseCursor = CursorType.Arrow;
            _WorldControl.CurrentTool = null;
        }
        //cross
        private void barButtonItem48_ItemClick(object sender, ItemClickEventArgs e)
        {
            WorldManager.Settings.ShowCrosshairs = !WorldManager.Settings.ShowCrosshairs;
        }
        //north
        private void barButtonItem58_ItemClick(object sender, ItemClickEventArgs e)
        {
            DrawArgs.Instance.WorldCamera.Reset();
        }
        //grid
        private void barButtonItem49_ItemClick(object sender, ItemClickEventArgs e)
        {
            WorldManager.Settings.ShowLatLonLines = !WorldManager.Settings.ShowLatLonLines;
        }
        //globe
        private void barButtonItem59_ItemClick(object sender, ItemClickEventArgs e)
        {
            Gaea.Renderable.RenderableObject bluemarb = _WorldControl.CurrentWorld.RenderableObjects.GetObject("影像");
            if (bluemarb != null)
            { bluemarb.Visible = !bluemarb.Visible; }
        }
        //loaction
        private void barButtonItem60_ItemClick(object sender, ItemClickEventArgs e)
        {
            _WorldControl.GotoLatLonAltitude(double.Parse(barLat.EditValue.ToString()), double.Parse(barLon.EditValue.ToString()),
                                             double.Parse(barAlt.EditValue.ToString()));
        }


        //load data
        private void barButtonItem77_ItemClick(object sender,ItemClickEventArgs e)
        {
            if (mdp != null)
                if (DialogResult.Yes == MessageBox.Show("The data has been loaded, do you want to replace？", "tip", MessageBoxButtons.YesNo))
                {
                    mdp.UnloadData();
                    mdp = null;
                    _metroControl.Destructize();
                }
                else
                    return;

            string path;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "XML File(*.xml)|*.xml";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                mdp = new FieldModel.MeteorDataProvider(path);
                if (mdp.MeteoDatasetName == "MeteorologicalSet")  //如果是气象数据集文件则进入
                {
                    FieldModel.TimeController vac = new FieldModel.TimeController();
                    FieldModel.TimeController.Instance = vac;
                    FieldModel.TimeController.Instance.StartTime();
                    FieldModel.TimeController.Instance.TimeCount = mdp.meteorMetaData.nTime;
                    FieldModel.TimeController.Instance.KeyFrameInterval = 1000;
                    try
                    {
                        _metroControl.Initialize(mdp);

                       
                    }
                    catch
                    { }


                    barEditItem8.EditValue = true;
                    barEditItem2.EditValue = true;

                    this.toolStripTimeNum.EditValue = FieldModel.TimeController.Instance.TimeNumber.ToString();
                    this.toolStripTimeIntervals.EditValue = FieldModel.TimeController.Instance.KeyFrameInterval.ToString();
                }
                else
                    MessageBox.Show("The data format is not correct, please select a correct data！");
            }
        }

        //texture slicing
        private void barButtonItem73_ItemClick(object sender, ItemClickEventArgs e)
        {
            MeteoModelPanel.Show();
        }



        protected override void OnClosing(CancelEventArgs e)
        {
            this.Dispose(true);

            base.OnClosing(e);
        }


        


    }
}