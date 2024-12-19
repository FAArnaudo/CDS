using System;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class PumpController
    {
        private static PumpController instance;                 // Instancia Singleton
        private int TimerProcess { get; set; }                  // Tiempo de espera entre cada procesamiento en segundos.
        private string ControllerType { get; set; }             // Tipo de controlador seleccionado
        private ControlPanel ControlPanel { get; set; }         // Panel de Control para configurar el label de estado

        private CancellationTokenSource cancellationTokenSource;// Tocken de control de procesos asincronicos
        private CancellationTokenSource watchDogTockenSource;   // Tocken de control de procesos asincronicos
        private Task mainProcess = null;                 // Hilo para manejar el proceso principal en paralelo al resto de la ejecución.
        private DateTime lastActivityTime;               // Variable que almacena el DateTime que almacena la hora actual
        private readonly int MAX_TIMER_MIN = 5;          // Humbral maximo de tiempo sin procesar (10 minutos)

        private PumpController()
        {
            cancellationTokenSource = new CancellationTokenSource();
            watchDogTockenSource = new CancellationTokenSource();
        }

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

        /// <summary>
        /// Inicializa el controlador con los datos y el panel de control proporcionados.
        /// </summary>
        /// <param name="data">Datos necesarios para la inicialización del controlador.</param>
        /// <param name="controlPanel">Panel de control asociado al controlador.</param>
        /// <returns>True si la inicialización fue exitosa; false en caso contrario.</returns>
        public bool Init(Data data, ControlPanel controlPanel)
        {
            try
            {
                // Inicializa los valores básicos y la base de datos
                TimerProcess = Convert.ToInt32(data.Timer);

                ControlPanel = controlPanel;

                // Asegura que la base de datos exista
                _ = ConnectorSQLite.Instance.CreateDatabase();

                return UpdateControllerType(data.Controller);
            }
            catch (Exception e) when (e is ArgumentNullException || e is NullReferenceException || e is FormatException)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                return false;
            }
        }

        /// <summary>
        /// Actualiza el tipo de controlador y verifica su validez.
        /// </summary>
        /// <param name="data">Nuevo tipo de controlador a configurar.</param>
        /// <returns>True si el controlador es válido y fue configurado exitosamente; false en caso contrario.</returns>
        public bool UpdateControllerType(string controller)
        {
            if (ControllerType == null || !ControllerType.Equals(controller))
            {
                cancellationTokenSource?.Cancel();

                // Verifica y actualiza el tipo de controlador
                if (!CheckController(controller))
                {
                    return false;
                }
            }

            ControllerType = controller;

            return true;
        }

        /// <summary>
        /// Verifica y configura el controlador según su tipo.
        /// </summary>
        /// <param name="controller">Tipo de controlador a verificar.</param>
        /// <returns>True si el controlador es válido; false en caso contrario.</returns>
        private bool CheckController(string controller)
        {
            // Simula un retraso basado en el temporizador
            Thread.Sleep(1500 * TimerProcess);

            // Inicializa el token de cancelación
            cancellationTokenSource = new CancellationTokenSource();

            switch (controller)
            {
                case "CEM-44":
                    // Inicia el proceso principal para el controlador CEM-44
                    mainProcess = Task.Run(() => CemProcess(cancellationTokenSource.Token));
                    break;
                case "FUSION":
                    // Inicia el proceso principal para el controlador FUSION
                    mainProcess = Task.Run(() => FusionProcess(cancellationTokenSource.Token));
                    break;
                default:
                    // Registra un error si el tipo de controlador no es reconocido
                    Log.Instance.WriteLog($"No se reconoce el controlador: {controller}", LogType.t_error);
                    return false;
            }

            _ = watchDogTockenSource.Token;

            // Inicia el watchdog con un tiempo límite predefinido
            StartWatchdog(watchDogTockenSource, TimeSpan.FromMinutes(MAX_TIMER_MIN));

            return true;
        }

        /// <summary>
        /// Metodo principal. Da inicio a la interacción con el controlador CEM-44.
        /// </summary>
        /// <param name="token"></param>
        private void CemProcess(CancellationToken token)
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
        private void FusionProcess(CancellationToken token)
        {
            Log.Instance.WriteLog($"Nuevo proceso principal iniciado: {mainProcess.Id} {mainProcess.Status}.\n", LogType.t_info);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    bool cierreDetectado = false;

                    while (!cierreDetectado && !token.IsCancellationRequested)
                    {
                        lastActivityTime = DateTime.UtcNow;     // Indica actividad

                        Thread.Sleep(1000 * TimerProcess);      // Timer in seconds

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

        /// <summary>
        /// 
        /// </summary>
        public void StopProcess()
        {
            cancellationTokenSource.Cancel();
            watchDogTockenSource.Cancel();
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

            _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConnection SET isConnected = {conexion}, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");
        }

        /// <summary>
        /// Metodo para monitorear el metodo principal. En caso de que no se detecte actividad se toman medidas.
        /// </summary>
        /// <param name="tokenSource"></param>
        /// <param name="timeout"></param>
        private void StartWatchdog(CancellationTokenSource tokenSource, TimeSpan timeout)
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