using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace CDS
{
    public class ConnectorSQLite
    {
        private static ConnectorSQLite instance = null;
        private static readonly string databaseName = "cds.db";
        private static readonly string connectionString = "Data Source='{0}';Version=3;";
        private ConnectorSQLite() { }
        public static ConnectorSQLite Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnectorSQLite();
                }
                return instance;
            }
        }
        /// <summary>
        /// Método para ejecutar una query que devuelva una dataTable (como un select)
        /// </summary>
        /// <param name="query"> Comando a ejecutar </param>
        /// <returns> Una tabla con la respuesta al comando </returns>
        public DataTable GetDataTable(string query)
        {
            DataTable dataTable = new DataTable();
            return dataTable;
        }

        /// <summary>
        /// Método para ejecutar una query que no devuelve una tabla (como un update)
        /// </summary>
        /// <param name="query"> Comando a ejecutar </param>
        /// <returns> Cantidad de registros alterados </returns>
        public int SetData(string query)
        {
            int stat = 0;
            return stat;
        }

        /// <summary>
        /// Método para obtener el estado de alguna bandera en la tabla
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Precio del producto</returns>
        public bool CheckFlag(string table, string field)
        {
            bool cierreFlag = false;
            string query = $"SELECT {table} FROM {field} LIMIT 1";

            return cierreFlag;
        }

        public void CreateBBDD(string folderPath)
        {
            string databasePath = Path.Combine(folderPath, databaseName);

            try
            {
                // Crear la carpeta si no existe
                if (!Directory.Exists(folderPath))
                {
                    _ = Directory.CreateDirectory(folderPath);
                }

                // Crear la base de datos si no existe
                if (!File.Exists(databasePath))
                {
                    SQLiteConnection.CreateFile(databasePath);
                }

                // Conectar a la base de datos
                using (SQLiteConnection connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
                {
                    connection.Open();

                    // Crear las tablas si no existen
                    string createTableQuery = "CREATE TABLE IF NOT EXISTS Configuration " +
                                              "(id_config INTEGER PRIMARY KEY CHECK (id_config = 1) razon_social TEXT, ruta_pry TEXT, bandera TEXT, " +
                                              "controlador TEXT, ip_controlador TEXT, protocolo INTEGER, " +
                                              "timer INTEGER)";
                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Excepción en CreateBBDD. Mensaje: {e.Message}\n");
            }
        }
    }
}
