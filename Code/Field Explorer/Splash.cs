using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Resources;
using System.Windows.Forms;
using System.Xml;

namespace Gaea
{
	/// <summary>
	/// The splash screen displayed while Gaea is loading.
	/// </summary>
    public class Splash : Gaea.Components.SplashFormBase
	{
		private bool wasClicked;
		private bool hasError;
		private DateTime startTime = DateTime.Now;
		private TimeSpan timeOut = TimeSpan.FromSeconds(1);
        private int defaultHeight;
        public PictureBox PictureBox;
        public static String AdoConnectionString = String.Empty;
        public static bool IsLogin = true;
        public static bool IsChangeUser = false;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:Gaea.Splash"/> class.
		/// </summary>
		public Splash()
		{
			InitializeComponent();

			//pictureBox.Image = GetStartupImage();
			defaultHeight = this.Height;

            //FileInfo splashImageFile = new FileInfo(
            //    Path.GetDirectoryName(Application.ExecutablePath) + "\\Data\\Icons\\Interface\\splash.png");

            //if(splashImageFile.Exists)
            //{
            //    pictureBox.Image = Image.FromFile(splashImageFile.FullName);
            //}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set if an error is displayed.
		/// </summary>
		public bool HasError
		{
			get
			{
				return hasError;
			}

			set
			{
				hasError = value;
				if(hasError)
				{
					// Make space for error messages
					Height = defaultHeight + 80;
				}
				else
				{
					Height = defaultHeight;
				}
			}
		}
		/// <summary>
		/// Display normal message on splash screen.
		/// </summary>
		/// <param name="message">Message to display on the splash screen</param>
		public override void SetText(string message)
		{
			if(hasError)
				Wait();
			HasError = false;
			this.Invalidate();
			Application.DoEvents();
		}

		/// <summary>
		/// Display an error message on splash.  Splash will stay visible longer to alert the user.
		/// </summary>
		/// <param name="message">Message to display on the splash screen</param>
		public override void SetError(string message)
		{
			if(hasError)
				Wait();
			HasError = true;
			wasClicked = false;
			this.timeOut = TimeSpan.FromSeconds(30);
			this.Invalidate();
			Application.DoEvents();
		}

		/// <summary>
		/// True when splash is done displaying (timed out or user intervention)
		/// </summary>
		public override bool IsDone 
		{
			get
			{
				Application.DoEvents();
				// Remove splash if user got tired, else wait preset time
				if (wasClicked)
					return true;
				TimeSpan timeElapsed = System.DateTime.Now - this.startTime;
				return (timeElapsed >= this.timeOut);
			}
		}

		protected void Wait()
		{
			while(!IsDone)
				Thread.Sleep(100);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			wasClicked = true;
			base.OnKeyUp(e);
		}

		private void Splash_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			wasClicked = true;
		}

		/// <summary>
		/// Creates the splash/about box picture with version number.
		/// </summary>
		/// <returns></returns>
		public static Image GetStartupImage()
		{
            return Image.FromFile(WorldManager.SystemSettings.RootPath + "\\Data\\Icons\\Interface\\splash.png");
		}

		private static Font LoadFont( string familyName, float emSize, FontStyle newStyle )
		{
			try
			{
				return new Font(familyName, emSize, newStyle );
			}
			catch(ArgumentException)
			{
				// Font load failed.
			}
			// Fall back to default font
			return new Font("", emSize, newStyle);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Splash));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBox
            // 
            this.PictureBox.BackColor = System.Drawing.Color.White;
            this.PictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox.Image")));
            this.PictureBox.InitialImage = null;
            this.PictureBox.Location = new System.Drawing.Point(0, 0);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(661, 455);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox.TabIndex = 2;
            this.PictureBox.TabStop = false;
            this.PictureBox.Click += new System.EventHandler(this.pictureBox_Click);
            this.PictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Splash_MouseDown);
            // 
            // Splash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(661, 455);
            this.ControlBox = false;
            this.Controls.Add(this.PictureBox);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "Splash";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Splash";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Load += new System.EventHandler(this.Splash_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Splash_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			ControlPaint.DrawBorder3D(e.Graphics, 0,0,(int)e.Graphics.VisibleClipBounds.Width, (int)e.Graphics.VisibleClipBounds.Height, Border3DStyle.Raised);
		}
        private bool Authentication(String uName,String Psd)
        {
            //OaWorkSpace oas = new OaWorkSpace();
            //MainApplication.SystemOA = oas;
            //MainApplication.SystemUserName = uName;
            //OAEngineEX.OAEx oaex = new OAEngineEX.OAEx();
            //String strAllRights = "";
            ////string cnstr = "Provider=SQLOLEDB.1;Password=123;Persist Security Info=True;User ID=sa;Initial Catalog=gljc;Data Source=192.168.0.82;Use Procedure for Prepare=1;Auto Translate=True;Packet Size=4096;Workstation ID=OFFICE;Use Encryption for Data=False;Tag with column collation when possible=False";
            //try
            //{
            //    ADODB.Connection cn1 = oaex.GetAdoConnection(AdoConnectionString);
            //    if (cn1 == null)
            //    {
            //        DevExpress.XtraEditors.XtraMessageBox.Show("登录失败,请检查网络连接以及用户数据库是否安装正确!");
            //        return false;
            //    }
            //    oas.AdoConnection = cn1;
            //    oas.DBType = 2;
            //    String strPsd = oaex.GetAdminPassword(ref cn1);
            //    if (oas.Login("Admin", strPsd.Trim()))
            //    {

            //        if (oaex.CheckPassWord(ref oas, uName, Psd) == 0)
            //        {
               
            //            IOaRight right1 = null;
            //            IOaRole role1 = null;
            //            IOaUser user1 = (IOaUser)oas.Users.FindByName(uName);
            //            if (user1 == null)
            //            {
            //                DevExpress.XtraEditors.XtraMessageBox.Show("用户名区分大小写！");
            //                return false;
            //            }
            //            int i, j, count, count2;
            //            count = user1.Rights.Count;
            //            for (i = 0; i < count; i++)
            //            {
            //                right1 = (IOaRight)user1.Rights.get_Item(i);
            //                strAllRights += right1.Code + ";";
            //            }
            //            count = user1.Roles.Count;
            //            for (i = 0; i < count; i++)
            //            {
            //                role1 = (IOaRole)user1.Roles.get_Item(i);
            //                count2 = role1.OaRights.Count;
            //                for (j = 0; j < count2; j++)
            //                {
            //                    right1 = (IOaRight)role1.OaRights.get_Item(j);
            //                    strAllRights += right1.Code + ";";
            //                }

            //            }
            //            MainApplication.UserCompetence = strAllRights;
            //            MainApplication.SystemUserPSD = Psd;
            //            return true;
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    }
            //    else
            //    {

            //        DevExpress.XtraEditors.XtraMessageBox.Show("登录失败,请检查网络连接以及用户数据库是否安装正确!");
            //        return false;
            //    }
            //}
            //catch(Exception ex)
            //{
            //    DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message.ToString());
            //    return false;
            //}
            return true;
        }
  
        private void bt_Login_Click(object sender, EventArgs e)
        {
            //if(Authentication(this.tbUserName.Text.Trim(),this.tbPassword.Text.Trim()))
            //{
            //    XmlDocument xdoc = new XmlDocument();
            //    xdoc.Load(WorldManager.SystemSettings.BinPath + @"\Config\Server.config");
            //    XmlNode xNode = xdoc.SelectSingleNode("Server/login");
            //    xNode.Attributes["UserName"].Value =WP_Encoder.EncryptString(this.tbUserName.Text.Trim());
            //    if (this.cbAutoLogin.Checked == true)
            //    {
            //        xNode.Attributes["Password"].Value = WP_Encoder.EncryptString(this.tbPassword.Text.Trim());
            //        xNode.InnerText = "true";
            //    }
            //    else
            //    {
            //        xNode.InnerText = "false";
            //    }
            //    xdoc.Save(WorldManager.SystemSettings.BinPath + @"\Config\Server.config");
            //        this.Dispose();
            //}
            //else
            //{
            //    DevExpress.XtraEditors.XtraMessageBox.Show("您无法认证通过！"); 
            //}
        }

        private void bt_Quit_Click(object sender, EventArgs e)
        {
            //this.Dispose();
            //if (Splash.IsChangeUser == false)
            //{
            //    Application.Exit();
            //}
            //Splash.IsChangeUser = false;
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }

        private void Splash_Load(object sender, EventArgs e)
        {
            //if (Splash.IsLogin == true)
            //{
                
            //    XmlDocument xdoc = new XmlDocument();
            //    xdoc.Load(WorldManager.SystemSettings.BinPath + @"\Config\Server.config");
            //    XmlNode xNode = xdoc.SelectSingleNode("Server/login");
            //    if (xNode.InnerText.Trim() == "true")
            //    {
            //        this.panelLogin.Visible = false;
            //        if (Authentication(WP_Encoder.DecryptString(xNode.Attributes["UserName"].Value.Trim()),
            //                           WP_Encoder.DecryptString(xNode.Attributes["Password"].Value.Trim())))
            //        {
            //            Splash.IsLogin = false;
            //            this.Dispose();
            //            return;
            //        }
            //        else
            //        {
            //            this.panelLogin.Visible = true;
            //            this.tbUserName.Text =WP_Encoder.DecryptString(xNode.Attributes["UserName"].Value.Trim());
            //        }
            //    }
            //    else
            //    {
            //        this.tbUserName.Text =WP_Encoder.DecryptString(xNode.Attributes["UserName"].Value.Trim());
            //    }
            //}
            //Splash.IsLogin = false;
        }

        public override Image SplashImage
        {
            get
            {
                return this.PictureBox.Image;
            }
            set
            {
                this.PictureBox.Image = value;
            }
        }

    }
}
