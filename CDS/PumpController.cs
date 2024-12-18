using System;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class PumpController
    {
        private static PumpController instance;                 // Instancia Singleton
        private static int TimerProcess { get; set; }           // Tiempo de espera entre cada procesamiento en segundos.
        private static string ControllerType { get; set; }      // Tipo de controlador seleccionado
        private ControlPanel ControlPanel { get; set; }         // Panel de Control para configurar el label de estado
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();     // Tocken de control de procesos asincronicos
        private static readonly CancellationTokenSource tokenSource = new CancellationTokenSource();        // Tocken de control de procesos asincronicos
        private static Task mainProcess = null;                 // Hilo para manejar el proceso principal en paralelo al resto de la ejecución.
        private static DateTime lastActivityTime;               // Variable que almacena el DateTime que almacena la hora actual
        private static readonly int MAX_TIMER_MIN = 5;          // Humbral maximo de tiempo sin procesar (10 minutos)

        private PumpController() { }

        /// <summary>
        /// Instancia Singleton de PumpController.
        /// </summary>
        public static PumpController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PumpController();
                }

                return instance;
            }
        }

        public bool Init(Data data, ControlPanel controlPanel)
        {
            try
            {
                TimerProcess = Convert.ToInt32(data.Timer);

                ControlPanel = controlPanel;

                // Creamos la base de datos, si no existe
                if (ConnectorSQLite.Instance.CreateDatabase())
                {
                    // Creamos las tablas si no existen
                    ConnectorSQLite.Instance.CreateTables();
                }

                // Si el controlador no fue asignado se inicia el proceso
                if (ControllerType == null)
                {
                    ControllerType = data.Controller;

                    if (!CheckController(ControllerType))
                    {
                        return false;
                    }
                }
                // Si el controlador fue asignado y se cambia por uno diferente, se inicia el proceso nuevo
                else if (!ControllerType.Equals(data.Controller))
                {
                    ControllerType = data.Controller;

                    cancellationTokenSource.Cancel();

                    if (!CheckController(ControllerType))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (ArgumentNullException e)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                return false;
            }
            catch (NullReferenceException e)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                return false;
            }
            catch (FormatException e)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                return false;
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                return false;
            }
        }

        /// <summary>
        /// Metodo principal. Da inicio a la interacción con el controlador CEM-44.
        /// </summary>
        /// <param name="token"></param>
        private static void CemProcess(CancellationToken token)
        {
            Log.Instance.WriteLog($"Nuevo proceso principal iniciado: {mainProcess.Id} {mainProcess.Status}.\n", LogType.t_info);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    bool cierreDetectado = false;

                    while (!cierreDetectado && !token.IsCancellationRequested)
                    {
                        lastActivityTime = DateTime.UtcNow;  // Indica actividad

                        Thread.Sleep(1000 * TimerProcess);          // Timer in seconds

                        Instance.CheckConexion(1);

                        cierreDetectado = ConnectorSQLite.Instance.ExecuteStateQuery($"SELECT hacerCierre FROM cierreBandera LIMIT 1");
                    }

                    if (cierreDetectado)
                    {
                        Log.Instance.WriteLog($"Pedido de Cierre detectado...\n", LogType.t_info);
                    }

                    Thread.Sleep(1000 * TimerProcess);          // Timer in seconds
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                }
            }
        }

        /// <summary>
        /// Metodo principal. Da inicio a la interacción con el controlador FUSION.
        /// </summary>
        /// <param name="token"></param>
        private static void FusionProcess(CancellationToken token)
        {
            Log.Instance.WriteLog($"Nuevo proceso principal iniciado: {mainProcess.Id} {mainProcess.Status}.\n", LogType.t_info);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    bool cierreDetectado = false;

                    while (!cierreDetectado && !token.IsCancellationRequested)
                    {
                        lastActivityTime = DateTime.UtcNow;  // Indica actividad

                        Thread.Sleep(1000 * TimerProcess);          // Timer in seconds

                        Instance.CheckConexion(0);

                        cierreDetectado = ConnectorSQLite.Instance.ExecuteStateQuery($"SELECT hacerCierre FROM cierreBandera LIMIT 1");
                    }

                    if (cierreDetectado)
                    {
                        Log.Instance.WriteLog($"Pedido de Cierre detectado...\n", LogType.t_info);
                    }

                    Thread.Sleep(1000 * TimerProcess);          // Timer in seconds
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                }
            }
        }

        public void StopProcess()
        {
            cancellationTokenSource.Cancel();
            tokenSource.Cancel();
        }

        /// <summary>
        /// Método para verificar cual es el controlador que debe asignarce.
        /// </summary>
        /// <param name="controller"></param>
        private static bool CheckController(string controller)
        {
            Thread.Sleep(2000 * TimerProcess);          // Timer in seconds

            cancellationTokenSource = new CancellationTokenSource();

            switch (controller)
            {
                case "CEM-44":
                    mainProcess = Task.Run(() => CemProcess(cancellationTokenSource.Token));
                    break;
                case "FUSION":
                    mainProcess = Task.Run(() => FusionProcess(cancellationTokenSource.Token));
                    break;
                default:
                    Log.Instance.WriteLog($"No se reconoce el controlador: {controller}", LogType.t_error);
                    return false;
            }

            _ = tokenSource.Token;

            // Inicia el watchdog con un tiempo límite en segundos
            StartWatchdog(tokenSource, TimeSpan.FromMinutes(MAX_TIMER_MIN));

            return true;
        }

        /// <summary>
        /// Método para actualizar el estado de la conexión con el controlador.
        /// Un estado en Verde indica una conexión estable.
        /// Un estado en Rojo indica un controlador desconectado y debe ser revisado.
        /// </summary>
        /// <param name="conexion"></param>
        public void CheckConexion(int conexion)
        {
            if (conexion == 1)// conexion => exitosa
            {
                ControlPanel.Dispatcher.Invoke(new Action(() =>
                {
                    ControlPanel.CreateCustomLabel("Controlador Online", "#00FF00");
                }));
            }
            else// conexion => fallida
            {
                ControlPanel.Dispatcher.Invoke(new Action(() =>
                {
                    ControlPanel.CreateCustomLabel("Controlador Offline", "#FF0000");
                }));
            }

            _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConexion SET isConnected = {conexion}, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConexion = 1");
        }

        /// <summary>
        /// Metodo para monitorear el metodo principal. En caso de que no se detecte actividad se toman medidas.
        /// </summary>
        /// <param name="tokenSource"></param>
        /// <param name="timeout"></param>
        private static void StartWatchdog(CancellationTokenSource tokenSource, TimeSpan timeout)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000 * TimerProcess); // Check interval

                while (!tokenSource.Token.IsCancellationRequested)
                {
                    // Suponiendo que actualizas `lastActivityTime` en cada iteración del bucle principal
                    TimeSpan timeDifference = DateTime.UtcNow - lastActivityTime;
                    if (timeDifference > timeout)
                    {
                        // Log de que el hilo principal parece haberse detenido
                        Log.Instance.WriteLog("Advertencia: El hilo principal ha dejado de responder.\n", LogType.t_error);

                        Log.Instance.WriteLog($"\n\tTiempo del proceso principal: {timeDifference}" +
                                              $"\n\tTiempo de humbral maximo: {MAX_TIMER_MIN}", LogType.t_debug);

                        // Liberar el tocken del MainProcess
                        cancellationTokenSource.Cancel();
                        // Reiniciar el proceso principal con un nuevo token
                        cancellationTokenSource = new CancellationTokenSource();
                        mainProcess = null;
                        _ = CheckController(ControllerType);
                    }
                }
            });
        }
    }
}
