using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TPV
{
    public partial class Login : Form
    {
        private int saiakerak = 3;
        private DB_Konexioa db;

        public Login()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(400, 350);

            db = new DB_Konexioa();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string erabiltzailea = txtErabiltzailea.Text.Trim();
            string pasahitza = txtPasahitza.Text.Trim();

            if (erabiltzailea == "" || pasahitza == "")
            {
                MessageBox.Show("Mesedez, bete bi eremuak.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = db.Conexion)
                {
                    conn.Open();

                    string sql = "SELECT COUNT(*) FROM langileak WHERE erabiltzailea=@erab AND pasahitza=@pass";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@erab", erabiltzailea);
                    cmd.Parameters.AddWithValue("@pass", pasahitza);

                    int balioa = Convert.ToInt32(cmd.ExecuteScalar());

                    if (balioa == 1)
                    {
                        MessageBox.Show("Ongi etorri!", "Login OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        saiakerak--;

                        MessageBox.Show(
                            $"Erabiltzailea edo pasahitza oker daude.\nGelditzen diren saiakerak: {saiakerak}",
                            "Errorea",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        if (saiakerak <= 0)
                        {
                            MessageBox.Show("3 saiakera gainditu dituzu.\nAplikazioa itxiko da.",
                                "Blokeoa",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Stop);

                            Application.Exit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea datu-basearekin konektatzean:\n" + ex.Message,
                    "Errorea",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
