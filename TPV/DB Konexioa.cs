
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TPV
{

    public class DB_Konexioa
    {
        private readonly string connectionString =
            "Server=192.168.115.153;Database=jatetxea;Uid=admin;Pwd=abc123ABC@;";


        public MySqlConnection Conexion { get; }

        public DB_Konexioa()
        {
            Conexion = new MySqlConnection(connectionString);
        }
    }
}