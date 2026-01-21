using System.Drawing;
using System.Windows.Forms;
using TPV.BISTAK;

namespace TPV
{
    public partial class ZerbitzariMenua : Form
    {
        private readonly int langileId;
        private readonly bool itzuliDezake;

        public ZerbitzariMenua(int langileIdPasatua, bool itzuliDezake = false)
        {
            InitializeComponent();
            langileId = langileIdPasatua;
            this.itzuliDezake = itzuliDezake;

            btnItzuli.Visible = itzuliDezake;
            btnItzuli.Click += btnItzuli_Click;

            btnErreserbaSortu.Click += (s, e) =>
            {
                using (ErreserbaSortu erreserba = new ErreserbaSortu())
                {
                    erreserba.ShowDialog();
                }
            };

            btnErreserbaGestionatu.Click += (s, e) =>
            {
                using (ErreserbaGestionatu gestionatu = new ErreserbaGestionatu(langileId))
                {
                    gestionatu.ShowDialog();
                }
            };

            btnZerbitzuaHasi.Click += (s, e) =>
            {
                using (ZerbitzuaHasi zerbitzua = new ZerbitzuaHasi(langileId))
                {
                    zerbitzua.ShowDialog();
                }
            };
        }

        private void btnItzuli_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
