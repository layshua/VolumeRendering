namespace FieldExplorer.Meteorology
{
    partial class IVolumeRenderUControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel3 = new System.Windows.Forms.Panel();
            this.volumeRendererSymbolControl1 = new FieldExplorer.Meteorology.VolumeRendererSymbolControl();
            this.panel2 = new System.Windows.Forms.Panel();
            this.sampleEdit = new DevExpress.XtraEditors.SpinEdit();
            this.valueLable = new System.Windows.Forms.Label();
            this.hsvLabel = new System.Windows.Forms.Label();
            this.rgbLabel = new System.Windows.Forms.Label();
            this.isUsingLights = new DevExpress.XtraEditors.CheckEdit();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.X_cuts = new DevExpress.XtraEditors.TrackBarControl();
            this.Y_cuts = new DevExpress.XtraEditors.TrackBarControl();
            this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            this.rightYs = new DevExpress.XtraEditors.TextEdit();
            this.rightXs = new DevExpress.XtraEditors.TextEdit();
            this.leftYs = new DevExpress.XtraEditors.TextEdit();
            this.leftXs = new DevExpress.XtraEditors.TextEdit();
            this.simpleButton3 = new DevExpress.XtraEditors.SimpleButton();
            this.leftBtm = new System.Windows.Forms.Label();
            this.rightUpr = new System.Windows.Forms.Label();
            this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
            this.zExggrates = new DevExpress.XtraEditors.SpinEdit();
            this.modelHeights = new DevExpress.XtraEditors.SpinEdit();
            this.columnumUpdowns = new DevExpress.XtraEditors.SpinEdit();
            this.rownumUpdowns = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.dispModels = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnDeleteROS = new DevExpress.XtraEditors.SimpleButton();
            this.btnAddROS = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.CBoxEVariables = new DevExpress.XtraEditors.ComboBoxEdit();
            this.CBoxAVariables = new DevExpress.XtraEditors.ComboBoxEdit();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.isUsingLights.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.X_cuts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.X_cuts.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Y_cuts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Y_cuts.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl2)).BeginInit();
            this.xtraTabControl2.SuspendLayout();
            this.xtraTabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rightYs.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightXs.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftYs.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftXs.Properties)).BeginInit();
            this.xtraTabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zExggrates.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.modelHeights.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnumUpdowns.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rownumUpdowns.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dispModels.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CBoxEVariables.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CBoxAVariables.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.volumeRendererSymbolControl1);
            this.panel3.Location = new System.Drawing.Point(1, 100);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(260, 143);
            this.panel3.TabIndex = 5;
            // 
            // volumeRendererSymbolControl1
            // 
            this.volumeRendererSymbolControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.volumeRendererSymbolControl1.Location = new System.Drawing.Point(0, 0);
            this.volumeRendererSymbolControl1.Name = "volumeRendererSymbolControl1";
            this.volumeRendererSymbolControl1.Size = new System.Drawing.Size(256, 139);
            this.volumeRendererSymbolControl1.TabIndex = 0;
            this.volumeRendererSymbolControl1.Text = "volumeRendererSymbolControl1";
            this.volumeRendererSymbolControl1.TransferMapChanged += new FieldExplorer.Meteorology.TransferMapChangedHandler(this.volumeRendererSymbolControl1_TransferMapChanged);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.sampleEdit);
            this.panel2.Controls.Add(this.valueLable);
            this.panel2.Controls.Add(this.hsvLabel);
            this.panel2.Controls.Add(this.rgbLabel);
            this.panel2.Controls.Add(this.isUsingLights);
            this.panel2.Location = new System.Drawing.Point(1, 250);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(260, 77);
            this.panel2.TabIndex = 6;
            // 
            // sampleEdit
            // 
            this.sampleEdit.EditValue = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.sampleEdit.Location = new System.Drawing.Point(181, 5);
            this.sampleEdit.Name = "sampleEdit";
            this.sampleEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.sampleEdit.Properties.Increment = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.sampleEdit.Properties.MaxValue = new decimal(new int[] {
            3000000,
            0,
            0,
            0});
            this.sampleEdit.Size = new System.Drawing.Size(67, 20);
            this.sampleEdit.TabIndex = 94;
            this.sampleEdit.EditValueChanged += new System.EventHandler(this.sampleEdit_EditValueChanged);
            // 
            // valueLable
            // 
            this.valueLable.AutoSize = true;
            this.valueLable.Location = new System.Drawing.Point(15, 52);
            this.valueLable.Name = "valueLable";
            this.valueLable.Size = new System.Drawing.Size(41, 12);
            this.valueLable.TabIndex = 2;
            this.valueLable.Text = "Value:";
            // 
            // hsvLabel
            // 
            this.hsvLabel.AutoSize = true;
            this.hsvLabel.Location = new System.Drawing.Point(15, 29);
            this.hsvLabel.Name = "hsvLabel";
            this.hsvLabel.Size = new System.Drawing.Size(29, 12);
            this.hsvLabel.TabIndex = 1;
            this.hsvLabel.Text = "HSV:";
            // 
            // rgbLabel
            // 
            this.rgbLabel.AutoSize = true;
            this.rgbLabel.Location = new System.Drawing.Point(15, 8);
            this.rgbLabel.Name = "rgbLabel";
            this.rgbLabel.Size = new System.Drawing.Size(29, 12);
            this.rgbLabel.TabIndex = 0;
            this.rgbLabel.Text = "RGB:";
            // 
            // isUsingLights
            // 
            this.isUsingLights.Location = new System.Drawing.Point(163, 50);
            this.isUsingLights.Name = "isUsingLights";
            this.isUsingLights.Properties.Caption = "Light";
            this.isUsingLights.Size = new System.Drawing.Size(87, 19);
            this.isUsingLights.TabIndex = 92;
            this.isUsingLights.CheckedChanged += new System.EventHandler(this.isUsingLights_CheckedChanged);
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(154, 530);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 23);
            this.simpleButton2.TabIndex = 94;
            this.simpleButton2.Text = "Load Config";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(33, 530);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 23);
            this.simpleButton1.TabIndex = 93;
            this.simpleButton1.Text = "Save Config";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // X_cuts
            // 
            this.X_cuts.EditValue = 429;
            this.X_cuts.Location = new System.Drawing.Point(3, 330);
            this.X_cuts.Name = "X_cuts";
            this.X_cuts.Properties.Maximum = 429;
            this.X_cuts.Size = new System.Drawing.Size(256, 45);
            this.X_cuts.TabIndex = 95;
            this.X_cuts.Value = 429;
            this.X_cuts.EditValueChanged += new System.EventHandler(this.X_cuts_EditValueChanged);
            // 
            // Y_cuts
            // 
            this.Y_cuts.EditValue = null;
            this.Y_cuts.Location = new System.Drawing.Point(3, 361);
            this.Y_cuts.Name = "Y_cuts";
            this.Y_cuts.Properties.Maximum = 267;
            this.Y_cuts.Size = new System.Drawing.Size(258, 45);
            this.Y_cuts.TabIndex = 96;
            this.Y_cuts.EditValueChanged += new System.EventHandler(this.Y_cuts_EditValueChanged);
            // 
            // xtraTabControl2
            // 
            this.xtraTabControl2.Location = new System.Drawing.Point(6, 405);
            this.xtraTabControl2.Name = "xtraTabControl2";
            this.xtraTabControl2.SelectedTabPage = this.xtraTabPage2;
            this.xtraTabControl2.Size = new System.Drawing.Size(255, 117);
            this.xtraTabControl2.TabIndex = 97;
            this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage2,
            this.xtraTabPage3});
            // 
            // xtraTabPage2
            // 
            this.xtraTabPage2.Controls.Add(this.rightYs);
            this.xtraTabPage2.Controls.Add(this.rightXs);
            this.xtraTabPage2.Controls.Add(this.leftYs);
            this.xtraTabPage2.Controls.Add(this.leftXs);
            this.xtraTabPage2.Controls.Add(this.simpleButton3);
            this.xtraTabPage2.Controls.Add(this.leftBtm);
            this.xtraTabPage2.Controls.Add(this.rightUpr);
            this.xtraTabPage2.Name = "xtraTabPage2";
            this.xtraTabPage2.Size = new System.Drawing.Size(249, 88);
            this.xtraTabPage2.Text = "Region";
            // 
            // rightYs
            // 
            this.rightYs.EditValue = "267";
            this.rightYs.Location = new System.Drawing.Point(161, 33);
            this.rightYs.Name = "rightYs";
            this.rightYs.Size = new System.Drawing.Size(52, 20);
            this.rightYs.TabIndex = 88;
            // 
            // rightXs
            // 
            this.rightXs.EditValue = "429";
            this.rightXs.Location = new System.Drawing.Point(93, 33);
            this.rightXs.Name = "rightXs";
            this.rightXs.Size = new System.Drawing.Size(52, 20);
            this.rightXs.TabIndex = 87;
            // 
            // leftYs
            // 
            this.leftYs.EditValue = "0";
            this.leftYs.Location = new System.Drawing.Point(161, 8);
            this.leftYs.Name = "leftYs";
            this.leftYs.Size = new System.Drawing.Size(52, 20);
            this.leftYs.TabIndex = 86;
            // 
            // leftXs
            // 
            this.leftXs.EditValue = "0";
            this.leftXs.Location = new System.Drawing.Point(91, 8);
            this.leftXs.Name = "leftXs";
            this.leftXs.Size = new System.Drawing.Size(52, 20);
            this.leftXs.TabIndex = 85;
            this.leftXs.EditValueChanged += new System.EventHandler(this.leftXs_EditValueChanged);
            // 
            // simpleButton3
            // 
            this.simpleButton3.Location = new System.Drawing.Point(184, 59);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(50, 23);
            this.simpleButton3.TabIndex = 84;
            this.simpleButton3.Text = "OK";
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);
            // 
            // leftBtm
            // 
            this.leftBtm.AutoSize = true;
            this.leftBtm.Location = new System.Drawing.Point(14, 13);
            this.leftBtm.Name = "leftBtm";
            this.leftBtm.Size = new System.Drawing.Size(77, 12);
            this.leftBtm.TabIndex = 72;
            this.leftBtm.Text = "Lower Left: ";
            // 
            // rightUpr
            // 
            this.rightUpr.AutoSize = true;
            this.rightUpr.Location = new System.Drawing.Point(14, 38);
            this.rightUpr.Name = "rightUpr";
            this.rightUpr.Size = new System.Drawing.Size(65, 12);
            this.rightUpr.TabIndex = 73;
            this.rightUpr.Text = "Up Right: ";
            // 
            // xtraTabPage3
            // 
            this.xtraTabPage3.Controls.Add(this.zExggrates);
            this.xtraTabPage3.Controls.Add(this.modelHeights);
            this.xtraTabPage3.Controls.Add(this.columnumUpdowns);
            this.xtraTabPage3.Controls.Add(this.rownumUpdowns);
            this.xtraTabPage3.Controls.Add(this.labelControl11);
            this.xtraTabPage3.Controls.Add(this.labelControl10);
            this.xtraTabPage3.Controls.Add(this.labelControl9);
            this.xtraTabPage3.Controls.Add(this.labelControl8);
            this.xtraTabPage3.Name = "xtraTabPage3";
            this.xtraTabPage3.Size = new System.Drawing.Size(249, 88);
            this.xtraTabPage3.Text = "Brick";
            // 
            // zExggrates
            // 
            this.zExggrates.EditValue = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.zExggrates.Location = new System.Drawing.Point(158, 64);
            this.zExggrates.Name = "zExggrates";
            this.zExggrates.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.zExggrates.Properties.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.zExggrates.Properties.MaxValue = new decimal(new int[] {
            200000,
            0,
            0,
            0});
            this.zExggrates.Size = new System.Drawing.Size(77, 20);
            this.zExggrates.TabIndex = 94;
            this.zExggrates.EditValueChanged += new System.EventHandler(this.zExggrates_EditValueChanged);
            // 
            // modelHeights
            // 
            this.modelHeights.EditValue = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.modelHeights.Location = new System.Drawing.Point(158, 36);
            this.modelHeights.Name = "modelHeights";
            this.modelHeights.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.modelHeights.Properties.Increment = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.modelHeights.Properties.MaxValue = new decimal(new int[] {
            3000000,
            0,
            0,
            0});
            this.modelHeights.Size = new System.Drawing.Size(77, 20);
            this.modelHeights.TabIndex = 93;
            this.modelHeights.EditValueChanged += new System.EventHandler(this.modelHeights_EditValueChanged);
            // 
            // columnumUpdowns
            // 
            this.columnumUpdowns.EditValue = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.columnumUpdowns.Location = new System.Drawing.Point(192, 10);
            this.columnumUpdowns.Name = "columnumUpdowns";
            this.columnumUpdowns.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.columnumUpdowns.Properties.MaxValue = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.columnumUpdowns.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.columnumUpdowns.Size = new System.Drawing.Size(43, 20);
            this.columnumUpdowns.TabIndex = 92;
            this.columnumUpdowns.EditValueChanged += new System.EventHandler(this.columnumUpdowns_EditValueChanged);
            // 
            // rownumUpdowns
            // 
            this.rownumUpdowns.EditValue = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.rownumUpdowns.Location = new System.Drawing.Point(69, 10);
            this.rownumUpdowns.Name = "rownumUpdowns";
            this.rownumUpdowns.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.rownumUpdowns.Properties.MaxValue = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.rownumUpdowns.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.rownumUpdowns.Size = new System.Drawing.Size(43, 20);
            this.rownumUpdowns.TabIndex = 91;
            this.rownumUpdowns.EditValueChanged += new System.EventHandler(this.rownumUpdowns_EditValueChanged);
            // 
            // labelControl11
            // 
            this.labelControl11.Location = new System.Drawing.Point(15, 67);
            this.labelControl11.Name = "labelControl11";
            this.labelControl11.Size = new System.Drawing.Size(119, 14);
            this.labelControl11.TabIndex = 90;
            this.labelControl11.Text = "Elevation Exaggerate:";
            // 
            // labelControl10
            // 
            this.labelControl10.Location = new System.Drawing.Point(15, 39);
            this.labelControl10.Name = "labelControl10";
            this.labelControl10.Size = new System.Drawing.Size(57, 14);
            this.labelControl10.TabIndex = 89;
            this.labelControl10.Text = "Elevation: ";
            // 
            // labelControl9
            // 
            this.labelControl9.Location = new System.Drawing.Point(138, 13);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(48, 14);
            this.labelControl9.TabIndex = 88;
            this.labelControl9.Text = "Column: ";
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(15, 13);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(32, 14);
            this.labelControl8.TabIndex = 87;
            this.labelControl8.Text = "Row: ";
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.labelControl3);
            this.panelControl1.Controls.Add(this.dispModels);
            this.panelControl1.Controls.Add(this.btnDeleteROS);
            this.panelControl1.Controls.Add(this.btnAddROS);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Controls.Add(this.labelControl2);
            this.panelControl1.Controls.Add(this.CBoxEVariables);
            this.panelControl1.Controls.Add(this.CBoxAVariables);
            this.panelControl1.Location = new System.Drawing.Point(1, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(260, 91);
            this.panelControl1.TabIndex = 98;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(12, 11);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(40, 14);
            this.labelControl3.TabIndex = 5;
            this.labelControl3.Text = "Model: ";
            // 
            // dispModels
            // 
            this.dispModels.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.dispModels.Location = new System.Drawing.Point(75, 8);
            this.dispModels.Name = "dispModels";
            this.dispModels.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dispModels.Properties.Items.AddRange(new object[] {
            "3DTexture",
            "Spherical"});
            this.dispModels.Size = new System.Drawing.Size(84, 20);
            this.dispModels.TabIndex = 4;
            this.dispModels.SelectedIndexChanged += new System.EventHandler(this.dispModels_SelectedIndexChanged);
            // 
            // btnDeleteROS
            // 
            this.btnDeleteROS.Location = new System.Drawing.Point(178, 37);
            this.btnDeleteROS.Name = "btnDeleteROS";
            this.btnDeleteROS.Size = new System.Drawing.Size(72, 23);
            this.btnDeleteROS.TabIndex = 3;
            this.btnDeleteROS.Text = "Delete";
            this.btnDeleteROS.Click += new System.EventHandler(this.btnDeleteROS_Click);
            // 
            // btnAddROS
            // 
            this.btnAddROS.Location = new System.Drawing.Point(178, 9);
            this.btnAddROS.Name = "btnAddROS";
            this.btnAddROS.Size = new System.Drawing.Size(72, 23);
            this.btnAddROS.TabIndex = 2;
            this.btnAddROS.Text = "Instancing";
            this.btnAddROS.Click += new System.EventHandler(this.btnAddROS_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 37);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(50, 14);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "Variable: ";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(12, 65);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(55, 14);
            this.labelControl2.TabIndex = 3;
            this.labelControl2.Text = "Instance: ";
            // 
            // CBoxEVariables
            // 
            this.CBoxEVariables.Location = new System.Drawing.Point(75, 62);
            this.CBoxEVariables.Name = "CBoxEVariables";
            this.CBoxEVariables.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.CBoxEVariables.Size = new System.Drawing.Size(84, 20);
            this.CBoxEVariables.TabIndex = 3;
            this.CBoxEVariables.SelectedIndexChanged += new System.EventHandler(this.CBoxEVariables_SelectedIndexChanged);
            // 
            // CBoxAVariables
            // 
            this.CBoxAVariables.Location = new System.Drawing.Point(75, 34);
            this.CBoxAVariables.Name = "CBoxAVariables";
            this.CBoxAVariables.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.CBoxAVariables.Size = new System.Drawing.Size(84, 20);
            this.CBoxAVariables.TabIndex = 2;
            this.CBoxAVariables.SelectedIndexChanged += new System.EventHandler(this.CBoxAVariables_SelectedIndexChanged);
            // 
            // IVolumeRenderUControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.xtraTabControl2);
            this.Controls.Add(this.Y_cuts);
            this.Controls.Add(this.X_cuts);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Name = "IVolumeRenderUControl";
            this.Size = new System.Drawing.Size(270, 559);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sampleEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.isUsingLights.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.X_cuts.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.X_cuts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Y_cuts.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Y_cuts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl2)).EndInit();
            this.xtraTabControl2.ResumeLayout(false);
            this.xtraTabPage2.ResumeLayout(false);
            this.xtraTabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rightYs.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightXs.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftYs.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftXs.Properties)).EndInit();
            this.xtraTabPage3.ResumeLayout(false);
            this.xtraTabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zExggrates.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.modelHeights.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnumUpdowns.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rownumUpdowns.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dispModels.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CBoxEVariables.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CBoxAVariables.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private Meteorology.VolumeRendererSymbolControl volumeRendererSymbolControl1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label hsvLabel;
        private System.Windows.Forms.Label rgbLabel;
        private System.Windows.Forms.Label valueLable;
        private DevExpress.XtraEditors.CheckEdit isUsingLights;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TrackBarControl X_cuts;
        private DevExpress.XtraEditors.TrackBarControl Y_cuts;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl2;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
        private DevExpress.XtraEditors.TextEdit rightYs;
        private DevExpress.XtraEditors.TextEdit rightXs;
        private DevExpress.XtraEditors.TextEdit leftYs;
        private DevExpress.XtraEditors.TextEdit leftXs;
        private DevExpress.XtraEditors.SimpleButton simpleButton3;
        private System.Windows.Forms.Label leftBtm;
        private System.Windows.Forms.Label rightUpr;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage3;
        private DevExpress.XtraEditors.SpinEdit zExggrates;
        private DevExpress.XtraEditors.SpinEdit modelHeights;
        private DevExpress.XtraEditors.SpinEdit columnumUpdowns;
        private DevExpress.XtraEditors.SpinEdit rownumUpdowns;
        private DevExpress.XtraEditors.LabelControl labelControl11;
        private DevExpress.XtraEditors.LabelControl labelControl10;
        private DevExpress.XtraEditors.LabelControl labelControl9;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnAddROS;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.ComboBoxEdit CBoxEVariables;
        private DevExpress.XtraEditors.ComboBoxEdit CBoxAVariables;
        private DevExpress.XtraEditors.SimpleButton btnDeleteROS;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.ComboBoxEdit dispModels;
        private DevExpress.XtraEditors.SpinEdit sampleEdit;
    }
}
