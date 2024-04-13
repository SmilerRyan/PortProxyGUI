namespace PortProxyGUI {
    partial class About {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.label_version = new System.Windows.Forms.Label();
            this.label_Star = new System.Windows.Forms.Label();
            this.SuspendLayout();
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Click += new System.EventHandler(this.linkLabel1_Click);
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            resources.ApplyResources(this.label_version, "label_version");
            this.label_version.Name = "label_version";
            resources.ApplyResources(this.label_Star, "label_Star");
            this.label_Star.Name = "label_Star";
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_Star);
            this.Controls.Add(this.label_version);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkLabel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.About_FormClosing);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_version;
        private System.Windows.Forms.Label label_Star;
    }
}