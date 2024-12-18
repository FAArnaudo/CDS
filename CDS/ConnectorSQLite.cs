﻿using System;
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
                throw new Exception($"Error al crear la base de datos: {ex.Message}");
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
                _ = OpenConnection();

                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        _ = dataAdapter.Fill(dataTable); // Llenar el DataTable con los resultados
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
                _ = OpenConnection();

                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
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
                _ = OpenConnection();
                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                {
                    int result = Convert.ToInt32(cmd.ExecuteScalar());
                    return result == 1;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al ejecutar una consulta de estados: {ex.Message}", LogType.t_error);
                return false;
            }
            finally
            {
                // Cerrar la conexión al final de la operación
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea todas las tablas necesarias para que el sistema pueda almacenar la informacón
        /// </summary>
        public void CreateTables()
        {
            try
            {
                _ = OpenConnection();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Surtidores " +
                                          "(IdSurtidor INTEGER, Manguera  INTEGER, Producto  INTEGER, " +
                                          "Precio REAL, DescProd TEXT)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Tanques (id INTEGER PRIMARY KEY, " +
                                   "volumen REAL NOT NULL, total REAL NOT NULL)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Despachos " +
                                   "(id INTEGER NOT NULL, surtidor INTEGER NOT NULL, " +
                                   "manguera INTEGER, producto TEXT NOT NULL, " +
                                   "PPU REAL NOT NULL, volumen REAL NOT NULL, " +
                                   "monto REAL NOT NULL, descripcion TEXT, " +
                                   "despacho_pedido BLOB, fecha TEXT DEFAULT(datetime('now', 'localtime')), " +
                                   "AUC TEXT DEFAULT '0', DCA REAL, DCP TEXT, " +
                                   "DPN TEXT, TXTD TEXT, cod_auto TEXT, glosa_auto TEXT, " +
                                   "valor_auto REAL, PRIMARY KEY(id,surtidor))";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS cierreBandera " +
                                   "(hacerCierre INTEGER NOT NULL);" +
                                   "\nINSERT INTO cierreBandera (hacerCierre) " +
                                   "SELECT 0 WHERE NOT EXISTS (SELECT 1 FROM cierreBandera)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Cierres " +
                                   "(id INTEGER, id_cierre INTEGER, fecha TEXT DEFAULT(datetime('now', 'localtime')), " +
                                   "monto_contado TEXT, volumen_contado TEXT, " +
                                   "monto_YPFruta TEXT, volumen_YPFruta TEXT, state TEXT, PRIMARY KEY(id AUTOINCREMENT))";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorManguera " +
                                   "(id INTEGER NOT NULL, surtidor INTEGER NOT NULL, " +
                                   "manguera INTEGER NOT NULL, monto REAL, " +
                                   "volumen REAL, monto_acumulado REAL, volumen_acumulado REAL)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorProducto " +
                                   "(id INTEGER NOT NULL, producto INTEGER NOT NULL, " +
                                   "monto REAL NOT NULL, volumen REAL NOT NULL)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Productos " +
                                   "(id_producto INTEGER PRIMARY KEY, numero_producto INTEGER NOT NULL, numero_despacho INTEGER NOT NULL," +
                                   "producto TEXT NOT NULL, precio REAL NOT NULL)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CheckConexion " +
                                  "(idConexion INTEGER PRIMARY KEY, isConnected INTEGER, fecha date DEFAULT(datetime('now', 'localtime')));" +
                                  "\nINSERT INTO CheckConexion (idConexion, isConnected)" +
                                  "\nSELECT 1, 0 " +
                                  "\nWHERE NOT EXISTS (SELECT 1 FROM CheckConexion WHERE idConexion = 1)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Datos_CIO " +
                                   "(id_cio INTEGER PRIMARY KEY, ip_vox TEXT NOT NULL, ip_bridge TEXT NOT NULL, " +
                                   "ip_server TEXT NOT NULL, ip_libre TEXT NOT NULL, " +
                                   "ruteo_estatico TEXT, fecha date DEFAULT(datetime('now', 'localtime')));" +
                                   "\nINSERT INTO Datos_CIO (id_cio, ip_vox, ip_bridge, ip_server, ip_libre, ruteo_estatico)" +
                                   "\nSELECT 1, '', '', '', '', '' " +
                                   "\nWHERE NOT EXISTS (SELECT 1 FROM Datos_CIO WHERE id_cio = 1);";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al crear tabla: {ex.Message}", LogType.t_error);
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
