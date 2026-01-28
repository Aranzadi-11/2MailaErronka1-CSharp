using System.Drawing;
using System.Windows.Forms;

namespace TPV
{
    partial class TxatKontrola
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layoutNagusia;
        private Panel headerPanel;
        private Label lblIzenburua;
        private Label lblEgoera;
        private Panel edukiaPanel;
        private FlowLayoutPanel mezuakPanel;
        private TableLayoutPanel layoutBehea;
        private TextBox txtMezua;
        private Button btnBidali;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            layoutNagusia = new TableLayoutPanel();
            headerPanel = new Panel();
            lblIzenburua = new Label();
            lblEgoera = new Label();
            edukiaPanel = new Panel();
            mezuakPanel = new FlowLayoutPanel();
            layoutBehea = new TableLayoutPanel();
            txtMezua = new TextBox();
            btnBidali = new Button();

            layoutNagusia.SuspendLayout();
            headerPanel.SuspendLayout();
            edukiaPanel.SuspendLayout();
            layoutBehea.SuspendLayout();
            SuspendLayout();

            layoutNagusia.ColumnCount = 1;
            layoutNagusia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutNagusia.Dock = DockStyle.Fill;
            layoutNagusia.RowCount = 2;
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            headerPanel.BackColor = Color.Black;
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.Controls.Add(lblEgoera);
            headerPanel.Controls.Add(lblIzenburua);

            lblIzenburua.AutoSize = false;
            lblIzenburua.Dock = DockStyle.Fill;
            lblIzenburua.TextAlign = ContentAlignment.MiddleLeft;
            lblIzenburua.Padding = new Padding(16, 0, 16, 0);
            lblIzenburua.Text = "Txata";
            lblIzenburua.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            lblIzenburua.ForeColor = Color.Goldenrod;

            lblEgoera.AutoSize = false;
            lblEgoera.Dock = DockStyle.Right;
            lblEgoera.Width = 160;
            lblEgoera.TextAlign = ContentAlignment.MiddleRight;
            lblEgoera.Padding = new Padding(0, 0, 16, 0);
            lblEgoera.Text = "Deskonektatuta";
            lblEgoera.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblEgoera.ForeColor = Color.Goldenrod;

            edukiaPanel.Dock = DockStyle.Fill;
            edukiaPanel.BackColor = Color.White;
            edukiaPanel.Padding = new Padding(12);

            mezuakPanel.Dock = DockStyle.Fill;
            mezuakPanel.FlowDirection = FlowDirection.TopDown;
            mezuakPanel.WrapContents = false;
            mezuakPanel.AutoScroll = true;
            mezuakPanel.BackColor = Color.White;

            layoutBehea.Dock = DockStyle.Bottom;
            layoutBehea.Height = 56;
            layoutBehea.ColumnCount = 2;
            layoutBehea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutBehea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            layoutBehea.RowCount = 1;
            layoutBehea.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutBehea.Padding = new Padding(0, 12, 0, 0);

            txtMezua.Dock = DockStyle.Fill;
            txtMezua.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            txtMezua.BorderStyle = BorderStyle.FixedSingle;
            txtMezua.KeyDown += txtMezua_KeyDown;

            btnBidali.Dock = DockStyle.Fill;
            btnBidali.Text = "Bidali";
            btnBidali.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnBidali.BackColor = Color.Goldenrod;
            btnBidali.ForeColor = Color.Black;
            btnBidali.FlatStyle = FlatStyle.Flat;
            btnBidali.FlatAppearance.BorderSize = 0;
            btnBidali.Click += btnBidali_Click;

            layoutBehea.Controls.Add(txtMezua, 0, 0);
            layoutBehea.Controls.Add(btnBidali, 1, 0);

            edukiaPanel.Controls.Add(layoutBehea);
            edukiaPanel.Controls.Add(mezuakPanel);

            layoutNagusia.Controls.Add(headerPanel, 0, 0);
            layoutNagusia.Controls.Add(edukiaPanel, 0, 1);

            Controls.Add(layoutNagusia);
            Name = "TxatKontrola";
            Size = new Size(380, 650);

            layoutNagusia.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            edukiaPanel.ResumeLayout(false);
            layoutBehea.ResumeLayout(false);
            layoutBehea.PerformLayout();
            ResumeLayout(false);
        }
    }
}
