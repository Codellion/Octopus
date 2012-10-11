using System;
using System.Collections.Generic;

namespace Octopus.Management.Commons.Jobs
{
    /// <summary>
    /// Clase que gestiona el subproceso más prioritario dentro del pool de ejecución de hilos
    /// </summary>
    public class KingHill
    {
        #region Definición de Eventos y Delegados

        /// <summary>
        /// Delegado que gestiona el evento de cambio de subproceso más prioritario
        /// </summary>
        /// <param name="newKing">Identificador del proceso en segundo plano, su hashcode</param>
        public delegate void KingChange(string newKing);

        /// <summary>
        /// Evento de cambio de subproceso más prioritario
        /// </summary>
        public event KingChange KingChanged;

        #endregion

        #region Propiedades

        /// <summary>
        /// Identificador del subproceso más prioritario
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Prioridad del subproceso más prioritario 
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Lista de subprocesos secundarios ordenados descendentemente por prioridad
        /// </summary>
        public List<KeyValuePair<string, int>> WorkerRelations { get; set; }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Instancia un nuevo "Rey de la Colina"
        /// </summary>
        /// <param name="hash">Identificador del proceso en segundo plano</param>
        /// <param name="priority">Prioridad asignada al subproceso</param>
        public KingHill(string hash, int priority)
        {
            Hash = hash;
            Priority = priority;

            WorkerRelations = new List<KeyValuePair<string, int>>();
        }

        /// <summary>
        /// Notifica la ejecución de un nuevo subproceso
        /// </summary>
        /// <param name="hash">Identificador del proceso en segundo plano</param>
        /// <param name="priority">Prioridad asignada al subproceso</param>
        public void AddWorker(string hash, int priority)
        {
            string newDescendantHash = hash;
            int newDescendantPriority = priority;

            if (priority > Priority)
            {
                if(string.IsNullOrEmpty(Hash))
                {
                    //Establecemos el nuevo rey de la colina
                    Priority = priority;
                    Hash = hash;

                    return;
                }
                
                newDescendantHash = Hash;
                newDescendantPriority = Priority; 

                //Establecemos el nuevo rey de la colina
                Priority = priority;
                Hash = hash;

                if (KingChanged != null)
                    KingChanged.Invoke(Hash);

                Console.WriteLine("Nuevo rey de la colina: {0}", Hash);
            }

            //Si el rey de la colina ha cambiado necesitamos recalcular las prioridades de toda la cola
            AddNewRelation(newDescendantHash, newDescendantPriority);
        }

        /// <summary>
        /// Notifica el fin de la ejecución de un subproceso
        /// </summary>
        /// <param name="hash">Identificador del proceso en segundo plano</param>
        public void ReleasePriority(string hash)
        {
            if (hash.Equals(Hash))
            {
                if (WorkerRelations.Count == 0)
                {
                    Hash = String.Empty;
                    Priority = -1;
                }
                else
                {
                    Hash = WorkerRelations[0].Key;
                    Priority = WorkerRelations[0].Value;

                    if (KingChanged != null)
                        KingChanged.Invoke(Hash);

                    Console.WriteLine("Nuevo rey de la colina: {0}", Hash);

                    WorkerRelations.RemoveAt(0);
                }
            }
            else
            {
                WorkerRelations.RemoveAll(rel => rel.Key == hash);
            }
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Método que añade un nuevo subproceso secundario
        /// </summary>
        /// <param name="hash">Identificador del proceso en segundo plano</param>
        /// <param name="priority">Prioridad asignada al subproceso</param>
        private void AddNewRelation(string hash, int priority)
        {
            bool ordenado = false;

            var workRelation = new KeyValuePair<string, int>(hash, priority);

            for (int i = 0; i < WorkerRelations.Count && !ordenado; i++)
            {
                if (WorkerRelations[i].Value > priority)
                {
                    WorkerRelations.Insert(i, workRelation);

                    ordenado = true;
                }
            }

            if (!ordenado)
            {
                WorkerRelations.Add(workRelation);
            }
        }

        #endregion

        
    }
}
