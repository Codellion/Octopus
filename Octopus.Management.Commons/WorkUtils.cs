using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

using Octopus.Management.Commons.Jobs;
using Octopus.Management.Commons.Utils;

namespace Octopus.Management.Commons
{
    /// <summary>
    /// Clase encargada de gestionar los procesos en segundo plano
    /// </summary>
    public static class WorkUtils
    {
        #region Propiedades

        /// <summary>
        /// Lista con el nivel de progreso de cada tarea
        /// </summary>
        public static IDictionary<long, int> SonarProgress { get; set; }

        /// <summary>
        /// Cache de ejecución
        /// </summary>
        private static IDictionary<string, object> SonarCache { get; set; }

        /// <summary>
        /// Indice de hilos maestros cacheables
        /// </summary>
        private static IDictionary<string, string> IndexDependencies { get; set; }

        /// <summary>
        /// Lista de hilos cacheables
        /// </summary>
        private static IDictionary<string, IList<long>> CacheDependencies { get; set; }

        /// <summary>
        /// Procesos en cola
        /// </summary>
        public static Queue<object> ProcessInQueue { get; private set; }

        /// <summary>
        /// Lista de Procesos en cola
        /// </summary>
        public static IList<long> ProcessInQueueIndex { get; private set; }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Crea un background worker con los eventos asociados
        /// </summary>
        /// <returns></returns>
        private static BackgroundWorker CreateWork()
        {
            var work = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            work.DoWork += BwDoWork;
            work.RunWorkerCompleted += BwRunWorkerCompleted;
            work.ProgressChanged += BwRunWorkerProgressChanged;

            return work;
        }

        /// <summary>
        /// Ejecuta un proceso en segundo plano
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro del delegado a invocar</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        private static void RunWork<T>(Sticky<T> sticky)
        {
            var work = CreateWork();
            string wHash = work.GetHashCode().ToString(CultureInfo.InvariantCulture);

            Hal.Sonar.Add(sticky.Id, work);

            //Se comprueba si en el plan de ejecución se tiene en cuenta el cacheo
            if (sticky.IsCacheable)
            {
                lock (CacheDependencies)
                {
                    if (!CacheDependencies.ContainsKey(sticky.Hash))
                    {
                        //Creamos el proceso maestro que servirá de referencia para los demás
                        CacheDependencies[sticky.Hash] = new List<long>();
                        IndexDependencies[wHash]
                            = sticky.Hash;
                    }
                    else
                    {
                        CacheDependencies[sticky.Hash].Add(sticky.Id);

                        return;
                    }
                }
            }

            //Comprobamos si el nuevo proceso es el más prioritario
            if (Hal.KingHill == null)
            {
                Hal.KingHill = new KingHill(wHash, sticky.PriorityPercent);
            }
            else
            {
                lock (Hal.KingHill)
                {
                    Hal.KingHill.AddWorker(wHash, sticky.PriorityPercent);
                }
            }


            //Ejecutamos el trabajo
            work.RunWorkerAsync(sticky);
        }

        /// <summary>
        /// Método que controla el fin de la ejecución de un proceso en segundo plano
        /// </summary>
        /// <param name="stickyId">Identificador del plan de ejecución</param>
        /// <param name="e">Argumentos del evento de fin de ejecución</param>
        /// <param name="work">Proceso en segundo plano</param>
        private static void EndWorkExecution(long stickyId, DoWorkEventArgs e, BackgroundWorker work = null)
        {
            Hal.Results[stickyId] = e.Result;

            //Comprobamos si el trabajo es periodico
            if (!StickyUtils.GetStickyProp<bool>(e.Argument, "IsPeriodic"))
            {
                //Finalizamos la ejecución del trabajo
                RemoveWork(stickyId);

                if (work != null)
                {
                    lock (Hal.KingHill)
                    {
                        Hal.KingHill.ReleasePriority(work.GetHashCode().ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            else
            {
                //Pausamos el trabajo y lo volvemos a ejecutar de nuevo
                Thread.Sleep(StickyUtils.GetStickyProp<int>(e.Argument, "MillisecondsDelay"));

                var newWork = CreateWork();
                newWork.RunWorkerAsync(e.Argument);
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Ejecuta un proceso en segundo plano o lo coloca en la cola de ejecución
        /// en caso de no quedar ningún hilo libre
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro del delegado a invocar</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        /// <returns>Identificador del plan de ejecución</returns>
        public static long AddWork<T>(Sticky<T> sticky)
        {
            return AddWork(sticky, false, null);
        }

        /// <summary>
        /// Ejecuta un proceso en segundo plano o lo coloca en la cola de ejecución
        /// en caso de no quedar ningún hilo libre
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro del delegado a invocar</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        /// <param name="exeCxt">Contexto de ejecución </param>
        /// <returns>Identificador del plan de ejecución</returns>
        public static long AddWork<T>(Sticky<T> sticky, ExecutionContext<T> exeCxt)
        {
            return AddWork(sticky, false, exeCxt);
        }


        /// <summary>
        /// Ejecuta un proceso en segundo plano o lo coloca en la cola de ejecución
        /// en caso de no quedar ningún hilo libre
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro del delegado a invocar</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        /// <param name="forceExecution">Indica que se ejecuta sin tener en cuenta el número de procesos en ejecución</param>
        /// <returns>Identificador del plan de ejecución</returns>
        public static long AddWork<T>(Sticky<T> sticky, bool forceExecution)
        {
            return AddWork(sticky, forceExecution, null);
        }

        /// <summary>s
        /// Ejecuta un proceso en segundo plano o lo coloca en la cola de ejecución
        /// en caso de no quedar ningún hilo libre
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro del delegado a invocar</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        /// <param name="forceExecution">Indica que se ejecuta sin tener en cuenta el número de procesos en ejecución</param>
        /// <param name="exeCxt">Contexto de ejecución </param>
        /// <returns>Identificador del plan de ejecución</returns>
        public static long AddWork<T>(Sticky<T> sticky, bool forceExecution, ExecutionContext<T> exeCxt)
        {
            //Iniciamos el sonar si no estaba activo
            if (Hal.Sonar == null)
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

                Hal.Sonar = new Dictionary<long, BackgroundWorker>(Hal.MaxParallelsProcess + 5);
                Hal.Results = new Dictionary<long, object>(Hal.MaxParallelsProcess + 5);

                SonarProgress = new Dictionary<long, int>();
                SonarCache = new Dictionary<string, object>();
                
                ProcessInQueue = new Queue<object>();
                ProcessInQueueIndex = new List<long>();

                CacheDependencies = new Dictionary<string, IList<long>>();
                IndexDependencies = new Dictionary<string, string>();
            }

            if(exeCxt != null)
            {
                exeCxt.AttachWork(sticky, forceExecution);
                return -1;
            }

            //Asignamos un ticket de ejecución al trabajo
            sticky.Id = Hal.GetTicket();
            
            //Creamos el trabajo y lo ejecutamos si no excede
            //el limite de hilos en ejecución o es prioritario
            if (Hal.Sonar.Count < Hal.MaxParallelsProcess || forceExecution)
            {
                RunWork(sticky);
            }
            else
            {
                //Si hay demasiados trabajos en paralelo 
                //se encola para su futura ejecución
                ProcessInQueue.Enqueue(sticky);
                ProcessInQueueIndex.Add(sticky.Id);
            }

            return sticky.Id;
        }

        /// <summary>
        /// Elimina un proceso en segundo plano
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro del delegado a invocar</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        public static void RemoveWork<T>(Sticky<T> sticky)
        {
            RemoveWork(sticky.Id);
        }

        /// <summary>
        /// Elimina un proceso en segundo plano por su identificador
        /// </summary>
        /// <param name="stickyId">Identificador del plan de ejecución</param>
        public static void RemoveWork(long stickyId)
        {
            //Comprobamos si aún existe en el contexto de ejecución
            if (!Hal.Sonar.ContainsKey(stickyId))
            {
                return;
            }

            //Obtenemos el trabajo y lo borramos
            var work = Hal.Sonar[stickyId];
            work.CancelAsync();

            Hal.Sonar.Remove(stickyId);

            //Obtenemos el próximo trabajo en cola y lo ejecutamos

            object sticky = null;

            //Bloqueamos el acceso a la cola para impedir que los
            //diferentes hilos inicien el mismo trabajo
            lock (ProcessInQueue)
            {
                if (ProcessInQueue.Count > 0)
                {
                    sticky = ProcessInQueue.Dequeue();
                }
            }

            //Si existe algún trabajo en cola lo ejecutamos en el sonar
            if (sticky != null)
            {
                long id = StickyUtils.GetStickyId(sticky);
                var hash = StickyUtils.GetStickyProp<string>(sticky, "Hash");

                work = CreateWork();
                string wHash = work.GetHashCode().ToString(CultureInfo.InvariantCulture);

                Hal.Sonar.Add(id, work);

                if (StickyUtils.GetStickyProp<bool>(sticky, "IsCacheable"))
                {
                    lock (CacheDependencies)
                    {
                        if (!CacheDependencies.ContainsKey(hash))
                        {
                            CacheDependencies[hash] = new List<long>();
                            IndexDependencies[wHash]
                                = hash;
                        }
                        else
                        {
                            CacheDependencies[hash].Add(id);

                            return;
                        }
                    }
                }

                var priorPer = StickyUtils.GetStickyProp<int>(sticky, "PriorityPercent");

                if (Hal.KingHill == null)
                {
                    Hal.KingHill = new KingHill(wHash, priorPer);
                }
                else
                {
                    lock (Hal.KingHill)
                    {
                        Hal.KingHill.AddWorker(wHash, priorPer);
                    }
                }

                //Ejecutamos el trabajo
                work.RunWorkerAsync(sticky);
            }
        }

        /// <summary>
        /// Actualiza los datos de ejecución de un proceso en segundo plano
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro del delegado a invocar</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        public static void UpdateWork<T>(Sticky<T> sticky)
        {
            RemoveWork(sticky);
            AddWork(sticky);
        }

        #endregion

        #region Eventos

        /// <summary>
        /// Método que gestiona el evento de ejecución de un proceso en segundo plano
        /// </summary>
        /// <param name="sender">Proceso en segundo plano</param>
        /// <param name="e">Argumentos del evento</param>
        private static void BwDoWork(object sender, DoWorkEventArgs e)
        {
            //Obtenemos los datos del trabajo
            long id = StickyUtils.GetStickyId(e.Argument);

            var worker = sender as BackgroundWorker;

            //Comprobamos si hay una cancelación pendiente del trabajo
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
                RemoveWork(id);

                return;
            }

            var isCacheable = StickyUtils.GetStickyProp<bool>(e.Argument, "IsCacheable");

            //Comprobamos si se quiere utilizar la cache
            if (isCacheable)
            {
                var hash = StickyUtils.GetStickyProp<string>(e.Argument, "Hash");

                //Comprobamos si el resultado ya se encuentra en cache
                if (SonarCache.ContainsKey(hash))
                {
                    e.Result = SonarCache[hash];
                }
                else
                {
                    //En caso contrario calculamos y guardamos en cache
                    e.Result = StickyUtils.ExecuteSticky(e.Argument, worker);

                    SonarCache[hash] = e.Result;
                }
            }
            else
            {
                //Ejecución normal de trabajos sin cache
                e.Result = StickyUtils.ExecuteSticky(e.Argument, worker);
            }

            EndWorkExecution(id, e, worker);
        }

        /// <summary>
        /// Método que gestiona el evento de fin de ejecución de un proceso en segundo plano
        /// </summary>
        /// <param name="sender">Origen del evento</param>
        /// <param name="e">Resultado de la ejecución del proceso en segundo plano</param>
        private static void BwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var work = sender as BackgroundWorker;

            if (work != null)
            {
                string wHash = work.GetHashCode().ToString(CultureInfo.InvariantCulture);

                //Comprobamos si el proceso tenía otros subprocesos cacheables pendientes de su ejecución
                //Y en caso de ser así asignamos a todos el mismo resultado
                if (IndexDependencies.ContainsKey(wHash))
                {
                    string stHash = IndexDependencies[wHash];

                    lock (CacheDependencies)
                    {
                        IList<long> stickiesDep = CacheDependencies[stHash];

                        foreach (var stickyId in stickiesDep)
                        {
                            RemoveWork(stickyId);

                            if (!e.Cancelled && e.Error == null)
                            {
                                Hal.Results[stickyId] = e.Result;
                            }
                        }

                        CacheDependencies.Remove(stHash);
                        IndexDependencies.Remove(wHash);
                    }
                }
            }
        }

        /// <summary>
        /// Método que gestiona el evento de progreso de la ejecución de un proceso en segundo plano
        /// </summary>
        /// <param name="sender">Origen del evento</param>
        /// <param name="e">Argumentos del evento</param>s
        private static void BwRunWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var stickyId = (long)e.UserState;

            SonarProgress[stickyId] = e.ProgressPercentage;
        }

        #endregion
    }
}
