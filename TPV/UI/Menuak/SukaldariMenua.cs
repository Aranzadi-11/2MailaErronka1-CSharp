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

            _ = TxataHasieratu();

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

        async System.Threading.Tasks.Task TxataHasieratu()
        {
            if (SaioGlobala.RolaId == 1)
            {
                txatPanela.Visible = false;
                return;
            }

            try
            {
                txatPanela.Visible = true;
                await TxatBezeroa.Instantzia.KonektatuAsync("192.168.1.117", 5000, SaioGlobala.LangileId, SaioGlobala.RolaId, SaioGlobala.ErabiltzaileIzena, SaioGlobala.Tokena);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Txat konexio errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txatPanela.Visible = false;
            }

        }

        private void btnItzuli_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
