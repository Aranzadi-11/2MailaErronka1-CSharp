using System.Drawing;
using System.Windows.Forms;
using TPV.BISTAK;

namespace TPV
{
    public partial class ZerbitzariMenua : Form
    {
        private readonly int langileId;

        public ZerbitzariMenua(int langileIdPasatua)
        {
            InitializeComponent();
            langileId = langileIdPasatua;

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
    }
}
