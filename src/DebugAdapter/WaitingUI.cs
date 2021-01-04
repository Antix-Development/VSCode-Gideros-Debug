using System;
using System.Windows.Forms;

namespace VSCodeDebug {
  class WaitingUI : Form {

    private Label label1;

    public WaitingUI() {
      InitializeComponent();
      CenterToScreen();

      Shown += WaitingUI_Shown;
      FormClosing += WaitingUI_FormClosing;
    }

    private void WaitingUI_FormClosing(object sender, FormClosingEventArgs e) {
      Program.Stop();
      Environment.Exit(0);
    }

    private void WaitingUI_Shown(object sender, EventArgs e) {
      new System.Threading.Thread(Program.DebugSessionLoop).Start();
    }

    // Set text of main label in ui.
    public void SetLabelText(string s) {
      BeginInvoke(new Action(() => {
        label1.Text = s;
      }));
    }

    private void InitializeComponent() {
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(392, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "label1";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // WaitingUI
      // 
      this.ClientSize = new System.Drawing.Size(416, 31);
      this.Controls.Add(this.label1);
      this.Location = new System.Drawing.Point(32, 32);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "WaitingUI";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Gideros Debug";
      this.ResumeLayout(false);

    }
  }
}
