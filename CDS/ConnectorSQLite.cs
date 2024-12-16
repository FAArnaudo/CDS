using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ConnectorSQLite
    {
        // Instancia estática privada
        private static ConnectorSQLite instance = null;

        // Nombre de la base de datos y cadena de conexión
        private readonly string databaseName = "cds.db";
        private readonly string connectionString = "Data Source='{0}';Version=3;";

        // Conexión a la base de datos SQLite
        private readonly SQLiteConnection connection;

        // Constructor privado
        private ConnectorSQLite()
        {
            // Iniciar la conexión
            connection = new SQLiteConnection(string.Format(connectionString, Configuration.GetConfiguration().RutaProyNuevo + "\\CDS\\" + databaseName));
        }

        /// <summary>
        /// Propiedad pública estática para obtener la instancia del Singleton
        /// </summary>
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
        /// Método para crear la base de datos si no existe
        /// </summary>
        /// <returns>retorna true si la creación fue exitosa o false en caso contrario</returns>
        public bool CreateDatabase()
        {
            try
            {
                string folderPath = Configuration.GetConfiguration().RutaProyNuevo + "\\CDS\\";
                string databasePath = Path.Combine(folderPath, databaseName);

                // Crear la carpeta si no existe
                if (!Directory.Exists(folderPath))
                {
                    _ = Directory.CreateDirectory(folderPath);
                }

                // Crear la base de datos si no existe
                if (!File.Exists(databasePath))
                {
                    SQLiteConnection.CreateFile(databasePath);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al crear la base de datos: {ex.Message}", LogType.t_error);
                return false;
            }
        }

        /// <summary>
        /// Método para ejecutar una consulta de tipo SELECT y retornar un DataTable
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Retorna el DataTable de la consulta de selección o null en caso de error</returns>
        public DataTable ExecuteSelectQuery(string query)
        {
            try
            {
                // Abrir la conexión
                OpenConnection();

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable); // Llenar el DataTable con los resultados
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al ejecutar SELECT: {ex.Message}", LogType.t_error);
                return null;
            }
            finally
            {
                // Cerrar la conexión al final de la operación
                CloseConnection();
            }
        }

        /// <summary>
        /// Método para ejecutar una consulta de tipo INSERT, UPDATE, DELETE y retornar el número de filas afectadas
        /// </summary>
        /// <param name="query"></param>
        /// <returns>retorna el numero de campos modificados o 0 si no haya modificacion. En caso de error retorna -1</returns>
        public int ExecuteNonQuery(string query)
        {
            try
            {
                // Abrir la conexión
                OpenConnection();

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    int rowsAffected = cmd.ExecuteNonQuery(); // Ejecuta el comando y devuelve el número de filas afectadas
                    return rowsAffected;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al ejecutar INSERT/UPDATE/DELETE: {ex.Message}", LogType.t_error);
                return -1; // En caso de error, retornamos -1 (ninguna fila afectada)
            }
            finally
            {
                // Cerrar la conexión al final de la operación
                CloseConnection();
            }
        }

        /// <summary>
        /// Método para ejecutar una consulta que retornen un estado
        /// </summary>
        /// <param name="query"></param>
        /// <returns>true si hubo inserción o modificación, y false en caso contrario</returns>
        public bool ExecuteStateQuery(string query)
        {
            try
            {
                // Abrir la conexión
                OpenConnection();

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    // Ejecutamos la consulta y verificamos el estado
                    int result = Convert.ToInt32(cmd.ExecuteScalar()); // Ejecuta una consulta que retorna un solo valor
                    return result == 1; // Si el resultado es 1, retornamos true, de lo contrario, false
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al ejecutar INSERT o consulta con estado: {ex.Message}", LogType.t_error);
                return false;
            }
            finally
            {
                // Cerrar la conexión al final de la operación
                CloseConnection();
            }
        }

        /// <summary>
        /// Método para abrir la conexión a la base de datos
        /// </summary>
        /// <returns></returns>
        private SQLiteConnection OpenConnection()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }

        /// <summary>
        /// Método para cerrar la conexión a la base de datos
        /// </summary>
        private void CloseConnection()
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }
}
