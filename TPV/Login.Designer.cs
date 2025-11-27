namespace TPV
    {
        partial class Login
        {
            private System.ComponentModel.IContainer components = null;

            private System.Windows.Forms.Label lblTitulua;
            private System.Windows.Forms.Label lblErabiltzailea;
            private System.Windows.Forms.Label lblPasahitza;
            private System.Windows.Forms.TextBox txtErabiltzailea;
            private System.Windows.Forms.TextBox txtPasahitza;
            private System.Windows.Forms.Button btnLogin;
            private System.Windows.Forms.Panel headerPanel;

            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            private void InitializeComponent()
            {
                this.lblTitulua = new System.Windows.Forms.Label();
                this.lblErabiltzailea = new System.Windows.Forms.Label();
                this.lblPasahitza = new System.Windows.Forms.Label();
                this.txtErabiltzailea = new System.Windows.Forms.TextBox();
                this.txtPasahitza = new System.Windows.Forms.TextBox();
                this.btnLogin = new System.Windows.Forms.Button();
                this.headerPanel = new System.Windows.Forms.Panel();

                this.SuspendLayout();

                // HEADER PANELA
                this.headerPanel.BackColor = System.Drawing.Color.Black;
                this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
                this.headerPanel.Height = 90;

                // IZENBURUA 
                this.lblTitulua.AutoSize = false;
                this.lblTitulua.Text = "ABEJ - Saioa Hasi";
                this.lblTitulua.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
                this.lblTitulua.ForeColor = System.Drawing.Color.Goldenrod;
                this.lblTitulua.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                this.lblTitulua.Dock = System.Windows.Forms.DockStyle.Top;
                this.lblTitulua.Height = 70;

                // ERABILTZAILE LABELA
                this.lblErabiltzailea.AutoSize = true;
                this.lblErabiltzailea.Text = "Erabiltzailea:";
                this.lblErabiltzailea.Font = new System.Drawing.Font("Segoe UI", 12F);
                this.lblErabiltzailea.Location = new System.Drawing.Point(40, 150);
                this.lblErabiltzailea.ForeColor = System.Drawing.Color.Black;

                // PASAHITZA LABELA
                this.lblPasahitza.AutoSize = true;
                this.lblPasahitza.Text = "Pasahitza:";
                this.lblPasahitza.Font = new System.Drawing.Font("Segoe UI", 12F);
                this.lblPasahitza.Location = new System.Drawing.Point(40, 205);
                this.lblPasahitza.ForeColor = System.Drawing.Color.Black;

                // ERABILTZAILE TEXTBOX
                this.txtErabiltzailea.Font = new System.Drawing.Font("Segoe UI", 12F);
                this.txtErabiltzailea.Location = new System.Drawing.Point(160, 145);
                this.txtErabiltzailea.Width = 180;
                this.txtErabiltzailea.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);

                // PASAHITZA TEXTBOX
                this.txtPasahitza.Font = new System.Drawing.Font("Segoe UI", 12F);
                this.txtPasahitza.Location = new System.Drawing.Point(160, 200);
                this.txtPasahitza.Width = 180;
                this.txtPasahitza.PasswordChar = '•';
                this.txtPasahitza.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);

                // LOGIN BUTTON 
                this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
                this.btnLogin.Text = "SARTU";
                this.btnLogin.BackColor = System.Drawing.Color.Goldenrod;
                this.btnLogin.ForeColor = System.Drawing.Color.Black;
                this.btnLogin.FlatStyle = FlatStyle.Flat;
                this.btnLogin.FlatAppearance.BorderSize = 0;
                this.btnLogin.Location = new System.Drawing.Point(160, 260);
                this.btnLogin.Width = 140;
                this.btnLogin.Height = 42;
                this.btnLogin.Anchor = AnchorStyles.Top;
                this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

                // FORM CONFIGURAZOIOA
                this.ClientSize = new System.Drawing.Size(450, 360);
                this.Controls.Add(this.btnLogin);
                this.Controls.Add(this.txtPasahitza);
                this.Controls.Add(this.txtErabiltzailea);
                this.Controls.Add(this.lblPasahitza);
                this.Controls.Add(this.lblErabiltzailea);
                this.Controls.Add(this.lblTitulua);
                this.Controls.Add(this.headerPanel);

                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.Text = "Login";
                this.BackColor = System.Drawing.Color.White;

                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }
}