using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace Octopus.Management.Commons.Jobs
{
    /// <summary>
    /// Clase que gestiona los parámetros de entrada y la prioridad de ejecución de los subprocesos de Octopus
    /// </summary>
    /// <typeparam name="T">Tipo de parámetro del plan de ejecución</typeparam>
    public class Token<T>
    {
        #region Propiedades

        /// <summary>
        /// Identificador del plan de ejecución al que pertenece el token
        /// </summary>
        public long StickyId { get; set; }

        /// <summary>
        /// Proceso en segundo plano
        /// </summary>
        public BackgroundWorker Worker { get; set; }

        /// <summary>
        /// Parámetro de entrada del plan de ejecución
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Identificador del proceso en segundo plano
        /// </summary>
        private readonly string _hash;

        /// <summary>
        /// Progreso del progreso actual
        /// </summary>
        private int _currentProgress;

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Instance un nuevo Token con los datos asociados al proceso en
        /// segundo plano pasados por parámetros
        /// </summary>
        /// <param name="stickyId">Identificador del plan de ejecución</param>
        /// <param name="data">Datos asociados al plan de ejecución</param>
        /// <param name="worker">Proceso en segundo plano</param>
        public Token(long stickyId, T data, BackgroundWorker worker)
        {
            StickyId = stickyId;
            Data = data;
            Worker = worker;
            _hash = worker.GetHashCode().ToString(CultureInfo.InvariantCulture);

            Hal.KingHill.KingChanged += AdjustPriorityKing;

            if(_hash.Equals(Hal.KingHill.Hash))
            {
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            }
        }

        /// <summary>
        /// Método que sirve para informar al sistema del grado de avance de una tarea en segundo plano
        /// e informa de la posibilidad de seguir ejecutandose
        /// </summary>
        /// <param name="progress">Progreso actual del subproceso</param>
        /// <returns>Indica si puede continuar ejecutandose el subproceso</returns>
        public bool Signal(int progress)
        {
            if(Worker.CancellationPending)
            {
                return false;
            }

            if(progress > 0 && progress <= 100)
            {
                if(progress != _currentProgress)
                {
                    _currentProgress = progress;
                    Worker.ReportProgress(progress, StickyId);        
                }
            }

            return true;
        }

        #endregion

        #region Eventos

        /// <summary>
        /// Métodos que gestiona el evento de cambio de "rey de la colina" (Subproceso dominante)
        /// para reaujustar las prioridades de los demás
        /// </summary>
        /// <param name="hash"></param>
        private void AdjustPriorityKing(string hash)
        {
            Thread.CurrentThread.Priority =
                _hash.Equals(hash) ? ThreadPriority.AboveNormal : ThreadPriority.Normal;
        }

        #endregion
    }
}
