using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class PumpController
    {
        private static PumpController instance;         // Instancia Singleton
        private int TimerProcess { get; set; }          // Tiempo de espera entre cada procesamiento en segundos.
        private string ControllerType { get; set; }     // Tipo de controlador seleccionado
        private ControlPanel ControlPanel { get; set; } // Panel de Control para configurar el label de estado
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task mainProcess = null;     // Hilo para manejar el proceso principal en paralelo al resto de la ejecución.

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
            bool isInitOk = false;
            try
            {
                TimerProcess = Convert.ToInt32(data.Timer);

                ControlPanel = controlPanel;

                if (ControllerType == null || !ControllerType.Equals(data.Controller))
                {
                    ControllerType = data.Controller;

                    if (CheckController(ControllerType))
                    {
                        isInitOk = true;
                    }
                }
                else
                {
                    isInitOk = true;
                }
            }
            catch (ArgumentNullException e)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                isInitOk = false;
            }
            catch (NullReferenceException e)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                isInitOk = false;
            }
            catch (FormatException e)
            {
                Log.Instance.WriteLog($"Error en el metodo PumpController.Init. Exception: {e.Message}", LogType.t_error);
                isInitOk = false;
            }

            return isInitOk;
        }

        private void CemProcess(CancellationToken token)
        {
            Thread.Sleep(1000 * TimerProcess);          // Timer in seconds
            CheckConexion(1);
        }

        private void FusionProcess(CancellationToken token)
        {
            Thread.Sleep(1000 * TimerProcess);          // Timer in seconds
            CheckConexion(0);
        }

        /// <summary>
        /// Método para verificar cual es el controlador que debe asignarce.
        /// </summary>
        /// <param name="controller"></param>
        private bool CheckController(string controller)
        {
            switch (controller)
            {
                case "CEM-44":
                    mainProcess = Task.Run(() => CemProcess(cancellationTokenSource.Token));
                    break;
                case "FUSION":
                    mainProcess = Task.Run(() => FusionProcess(cancellationTokenSource.Token));
                    break;
                default:
                    return false;
            }

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

            //_ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConexion SET isConnected = {conexion}, fecha = datetime('now', 'localtime') WHERE idConexion = 1");
        }
    }
}
