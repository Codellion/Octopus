using System;
using System.Collections.Generic;

using Octopus.Management.Commons.Jobs;

namespace Octopus.Management.Commons
{
    /// <summary>
    /// Clase que crea un contexto de ejecución de tareas dependientes en segundo plano
    /// como pueden ser procesos transaccionales
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExecutionContext<T> : IDisposable
    {
        #region Propiedades

        /// <summary>
        /// Cola de planes de ejecución por procesar
        /// </summary>
        private Queue<Sticky<T>> _workers;

        /// <summary>
        /// Cola que indica el fuerze de la ejecución de cada plan
        /// </summary>
        private Queue<bool> _workersForceExecutions;

        #endregion

        #region Métodos Internos

        /// <summary>
        /// Agrega un plan de ejecución al final de la cola
        /// </summary>
        /// <param name="sticky">Plan de ejecución</param>
        /// <param name="forceExecution">Indica si es necesario forzar su inmediata ejecución</param>
        internal void AttachWork(Sticky<T> sticky, bool forceExecution = false)
        {
            if(_workers == null)
            {
                _workers = new Queue<Sticky<T>>();
                _workersForceExecutions = new Queue<bool>();
            }

            _workers.Enqueue(sticky);
            _workersForceExecutions.Enqueue(forceExecution);
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Método que se ejecuta en segundo plano para procesar la cola
        /// </summary>
        /// <param name="token">Token con los datos de la ejecución</param>
        /// <returns>Nulo</returns>
        private object RunProcessContext(Token<int> token)
        {
            while (_workers.Count > 0)
            {
                long idProcess = WorkUtils.AddWork(
                    _workers.Dequeue(), _workersForceExecutions.Dequeue());

                Hal.WaitEnd(idProcess);
            }

            return null;
        }

        #endregion

        #region Miembros de IDisposable

        /// <summary>
        /// Método que lanza el procesamiento de la cola al final del bloque USING
        /// </summary>
        public void Dispose()
        {
            var work = new Sticky<int>(RunProcessContext, 0);

            WorkUtils.AddWork(work, true);
        }

        #endregion
    }
}
