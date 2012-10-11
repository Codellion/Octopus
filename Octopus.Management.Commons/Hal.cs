using System.Collections.Generic;
using System.ComponentModel;

using Octopus.Management.Commons.Jobs;

namespace Octopus.Management.Commons
{
    /// <summary>
    /// Clase central encargada de gestionar los subprocesos de un sistema de forma eficiente
    /// </summary>
    public static class Hal
    {
        #region Propiedades

        /// <summary>
        /// Variable utilizada para asignar identificadores a los planes de ejecución
        /// </summary>
        private static long _ticketMaster;

        /// <summary>
        /// Número máximo de ejecuciones en paralelo
        /// </summary>
        public static int MaxParallelsProcess = 8;

        /// <summary>
        /// Lista de procesos en ejecución
        /// </summary>
        public static IDictionary<long, BackgroundWorker> Sonar { get; set; }

        /// <summary>
        /// Lista de resultados de las ejecuciones
        /// </summary>
        public static IDictionary<long, object> Results { get; set; }

        /// <summary>
        /// Elemento más prioritario dentro del contexto de ejecución
        /// </summary>
        public static KingHill KingHill { get; set; }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Para el trabajo actual y borra la lista de espera si se le indica por parámetros
        /// </summary>
        private static void StopSonar(bool deleteQueue)
        {
            lock (Sonar)
            {
                if (deleteQueue)
                {
                    WorkUtils.ProcessInQueue.Clear();
                }

                var stikiesId = new List<long>(Sonar.Keys);
                stikiesId.ForEach(WorkUtils.RemoveWork);
            }

            WaitEnd();
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Devuelve un nuevo identificador para un plan de ejecución
        /// </summary>
        /// <returns></returns>
        public static long GetTicket()
        {
            _ticketMaster++;

            return _ticketMaster;
        }
        
        /// <summary>
        /// Método que espera a la finalización de todos los procesos en ejecución y en cola
        /// </summary>
        public static void WaitEnd()
        {
            while(Sonar.Count > 0){}
        }

        /// <summary>
        /// Método que espera a la finalización del proceso pasado como parámetro
        /// </summary>
        public static void WaitEnd(long idSticky)
        {
            while (Sonar.ContainsKey(idSticky) || WorkUtils.ProcessInQueueIndex.Contains(idSticky)){}
        }

        /// <summary>
        /// Para el trabajo actual y borra la lista de espera
        /// </summary>
        public static void StopSonar()
        {
            StopSonar(false);
        }

        /// <summary>
        /// Para el trabajo actual y empieza a procesar los elementos en espera
        /// </summary>
        public static void StopCurrentSonar()
        {
            StopSonar(false);
        }

        /// <summary>
        /// Pausa todos los subprocesos en ejecución
        /// </summary>
        public static void PauseSonar()
        {
            //TODO Hal.PauseSonar()
        }

        #endregion
    }
}
