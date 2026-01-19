using System.Drawing;
using System.Windows.Forms;

namespace TPV.BISTAK
{
    partial class ErreserbaGestionatu
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layoutNagusia;
        private Panel headerPanel;
        private Label lblTitulo;
        private TableLayoutPanel filtroLayout;
        private DateTimePicker datePicker;
        private ComboBox hourPicker;
        private Button btnBilatu;
        private FlowLayoutPanel erreserbakPanel;
        private Label lblMensaje;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            layoutNagusia = new TableLayoutPanel();
            headerPanel = new Panel();
            lblTitulo = new Label();
            filtroLayout = new TableLayoutPanel();
            datePicker = new DateTimePicker();
            hourPicker = new ComboBox();
            btnBilatu = new Button();
            erreserbakPanel = new FlowLayoutPanel();
            lblMensaje = new Label();

            layoutNagusia.SuspendLayout();
            headerPanel.SuspendLayout();
            filtroLayout.SuspendLayout();
            SuspendLayout();

            layoutNagusia.ColumnCount = 1;
            layoutNagusia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutNagusia.Controls.Add(headerPanel, 0, 0);
            layoutNagusia.Controls.Add(filtroLayout, 0, 1);
            layoutNagusia.Controls.Add(erreserbakPanel, 0, 2);
            layoutNagusia.Dock = DockStyle.Fill;
            layoutNagusia.RowCount = 3;
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            layoutNagusia.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            headerPanel.BackColor = Color.Black;
            headerPanel.Controls.Add(lblTitulo);
            headerPanel.Dock = DockStyle.Fill;

            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.Goldenrod;
            lblTitulo.Text = "Erreserbak Kudeatu";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            filtroLayout.ColumnCount = 3;
            filtroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            filtroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            filtroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            filtroLayout.Controls.Add(datePicker, 0, 0);
            filtroLayout.Controls.Add(hourPicker, 1, 0);
            filtroLayout.Controls.Add(btnBilatu, 2, 0);
            filtroLayout.Dock = DockStyle.Fill;

            datePicker.CustomFormat = "yyyy-MM-dd";
            datePicker.Format = DateTimePickerFormat.Custom;
            datePicker.Dock = DockStyle.Fill;
            datePicker.MinDate = DateTime.Today;

            hourPicker.DropDownStyle = ComboBoxStyle.DropDownList;
            hourPicker.Dock = DockStyle.Fill;

            btnBilatu.BackColor = Color.Goldenrod;
            btnBilatu.Dock = DockStyle.Fill;
            btnBilatu.FlatStyle = FlatStyle.Flat;
            btnBilatu.FlatAppearance.BorderSize = 0;
            btnBilatu.ForeColor = Color.Black;
            btnBilatu.Text = "Bilatu";
            btnBilatu.UseVisualStyleBackColor = false;
            btnBilatu.Click += btnBilatu_Click;

            erreserbakPanel.AutoScroll = true;
            erreserbakPanel.Dock = DockStyle.Fill;
            erreserbakPanel.Location = new Point(3, 163);
            erreserbakPanel.Name = "erreserbakPanel";
            erreserbakPanel.Size = new Size(894, 434);
            erreserbakPanel.TabIndex = 2;

            lblMensaje.Dock = DockStyle.Fill;
            lblMensaje.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblMensaje.TextAlign = ContentAlignment.MiddleCenter;
            lblMensaje.Text = "Aukeratu ezazu egun eta ordu bat.";

            erreserbakPanel.Controls.Add(lblMensaje);

            BackColor = Color.White;
            ClientSize = new Size(900, 600);
            Controls.Add(layoutNagusia);
            MinimumSize = new Size(900, 600);
            Name = "ErreserbaGestionatu";
            Text = "Erreserbak";
            WindowState = FormWindowState.Maximized;

            layoutNagusia.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            filtroLayout.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
