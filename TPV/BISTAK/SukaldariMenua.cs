using System.Drawing;
using System.Windows.Forms;
using TPV.BISTAK;

namespace TPV
{
    public partial class SukaldariMenua : Form
    {
        private readonly int langileId;
        private bool itzuliDezake;
        private Button btnItzuli;

        public SukaldariMenua(int langileIdPasatua, bool itzuliDezake = false)
        {
            InitializeComponent();
            langileId = langileIdPasatua;
            this.itzuliDezake = itzuliDezake;

            if (itzuliDezake)
            {
                btnItzuli = new Button
                {
                    Text = "Zerbitzari Menuara",
                    Font = new Font("Segoe UI", 12F),
                    Size = new Size(220, 40),
                    Location = new Point(10, 10)
                };

                btnItzuli.Click += (s, e) =>
                {
                    new ZerbitzariMenua(langileId).Show();
                    this.Close();
                };

                Controls.Add(btnItzuli);
            }
        }
    }
}
