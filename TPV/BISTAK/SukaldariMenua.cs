using System.Windows.Forms;
using TPV.BISTAK;

namespace TPV
{
    public partial class SukaldariMenua : Form
    {
        private readonly int langileId;
        private readonly bool itzuliDezake;

        public SukaldariMenua(int langileIdPasatua, bool itzuliDezake = false)
        {
            InitializeComponent();
            langileId = langileIdPasatua;
            this.itzuliDezake = itzuliDezake;

            btnItzuli.Visible = itzuliDezake;
            btnItzuli.Click += btnItzuli_Click;

            btnInbentarioaKudeatu.Click += (s, e) =>
            {
                using (InbentarioaKudeatu inb = new InbentarioaKudeatu())
                {
                    inb.ShowDialog();
                }
            };

            btnPlaterenEgoera.Click += (s, e) =>
            {
                using (ZerbitzuenEgoera pl = new ZerbitzuenEgoera())
                {
                    pl.ShowDialog();
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
