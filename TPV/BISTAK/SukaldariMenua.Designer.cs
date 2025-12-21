namespace TPV
{
    partial class SukaldariMenua
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(800, 600);
            this.Text = "Sukaldarien Menua";

            this.ResumeLayout(false);
        }
    }
}
