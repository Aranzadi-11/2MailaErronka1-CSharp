using System.Drawing;
using System.Windows.Forms;

namespace TPV.BISTAK
{
    partial class ZerbitzuaKudeatu
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layoutNagusia;
        private Panel headerPanel;
        private Label lblTitulo;
        private FlowLayoutPanel categoriasPanel;
        private Button btnEskaeraAmaitu;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            layoutNagusia = new TableLayoutPanel();
            headerPanel = new Panel();
            lblTitulo = new Label();
            categoriasPanel = new FlowLayoutPanel();
            btnEskaeraAmaitu = new Button();

            layoutNagusia.SuspendLayout();
            headerPanel.SuspendLayout();
            SuspendLayout();

            // layoutNagusia
            layoutNagusia.ColumnCount = 1;
            layoutNagusia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutNagusia.Controls.Add(headerPanel, 0, 0);
            layoutNagusia.Controls.Add(categoriasPanel, 0, 1);
            layoutNagusia.Controls.Add(btnEskaeraAmaitu, 0, 2);
            layoutNagusia.Dock = DockStyle.Fill;
            layoutNagusia.RowCount = 3;
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            // headerPanel
            headerPanel.BackColor = Color.Black;
            headerPanel.Controls.Add(lblTitulo);
            headerPanel.Dock = DockStyle.Fill;

            // lblTitulo
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.ForeColor = Color.Goldenrod;
            lblTitulo.Text = "Zerbitzua Kudeatu";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            // categoriasPanel
            categoriasPanel.Dock = DockStyle.Fill;
            categoriasPanel.AutoScroll = true;
            categoriasPanel.FlowDirection = FlowDirection.TopDown;
            categoriasPanel.WrapContents = false;

            // btnEskaeraAmaitu
            btnEskaeraAmaitu.Text = "Eskaera amaitu";
            btnEskaeraAmaitu.BackColor = Color.Goldenrod;
            btnEskaeraAmaitu.ForeColor = Color.Black;
            btnEskaeraAmaitu.FlatStyle = FlatStyle.Flat;
            btnEskaeraAmaitu.Dock = DockStyle.Fill;

            // Form
            ClientSize = new Size(1000, 700);
            Controls.Add(layoutNagusia);
            Text = "Zerbitzua Kudeatu";
            WindowState = FormWindowState.Maximized;

            layoutNagusia.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
