using System;
using System.ComponentModel;
using System.Text;

namespace Octopus.Management.Commons.Jobs
{
    /// <summary>
    /// Delegado que representa el método que ejecuta el subproceso
    /// </summary>
    /// <typeparam name="T">Tipo de parámetro que recibe el subproceso que se ejecuta</typeparam>
    /// <param name="token">Token de la ejecución</param>
    /// <returns></returns>
    public delegate object DelegateWork<T>(Token<T> token);

    /// <summary>
    /// Clase que representa un plan de ejecución de un subproceso en segundo plano
    /// </summary>
    /// <typeparam name="T">Tipo de parámetro que recibe el subproceso que se ejecuta</typeparam>
    [Serializable]
    public class Sticky<T>
    {
        #region Propiedades

        /// <summary>
        /// Indica si el resultado del proceso se puede cachear
        /// </summary>
        private bool _isCacheable;

        /// <summary>
        /// Identificador del plan de ejecución
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Parámetros de entrada del proceso
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Delegado que representa el proceso que se ejecuta en segundo plano
        /// </summary>
        public DelegateWork<T> Method { get; set; }

        /// <summary>
        /// Identificador del proceso en segundo plano
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// Periodicidad de ejecución del subproceso en milisegundos 
        /// </summary>
        public int MillisecondsDelay { get; set; }

        /// <summary>
        /// Indica si el proceso se ejecuta periodicamente
        /// </summary>
        public bool IsPeriodic
        {
            get { return MillisecondsDelay > 0; }
        } 

        /// <summary>
        /// Progreso actual del subproceso
        /// </summary>
        public int CurrentProgress { get; set; }

        /// <summary>
        /// Prioridad
        /// </summary>
        public int PriorityPercent { get; set; }

        /// <summary>
        /// Indica si el resultado del proceso se puede cachear
        /// </summary>
        public bool IsCacheable
        {
            get { return _isCacheable; } 
            set
            {
                _isCacheable = value;

                if(_isCacheable)
                {
                    var sb = new StringBuilder();

                    InspectObject(Data, ref sb);

                    Hash = sb.ToString();    
                }
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Crea un nuevo plan de ejecución con el método y los parámetros de entrada a invocar
        /// </summary>
        /// <param name="method">Método a ejecutar</param>
        /// <param name="data">Parámetros de entrada del proceso</param>
        public Sticky(DelegateWork<T> method, T data)
        {
            Data = data;
            Method = method;
        }

        /// <summary>
        /// Ejecuta el subproceso
        /// </summary>
        /// <returns>Los datos devueltos por el subproceso</returns>
        public object ExecutePlan(BackgroundWorker work)
        {
            var token = new Token<T>(Id, Data, work);

            return Method.Invoke(token);
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Método Hashing para crear un código a partir del contenido de la clase, así podemos
        /// identificar dos procesos cacheables que puedan generar el mismo resultado
        /// </summary>
        /// <typeparam name="TV">Tipo del objeto</typeparam>
        /// <param name="valor">Valor</param>
        /// <param name="stringBuilder">Codigo Hash que se genera</param>
        private static void InspectObject<TV>(TV valor, ref StringBuilder stringBuilder)
        {
            var tipoTv = valor.GetType();

            if (tipoTv.IsSealed)
            {
                stringBuilder.Append(valor);

                return;
            }

            foreach (var propInfo in tipoTv.GetProperties())
            {
                bool tipoComplejo = false;
                object vProp = string.Empty;
                object valorProp = propInfo.GetValue(valor, null);

                if (valorProp != null)
                {
                    Type varType = valorProp.GetType();

                    //Comprobamos si es un tipo basico
                    //Los tipos básico están "sellados" en .NET incluidos los arrays
                    if (varType.IsSealed)
                    {
                        if (varType.IsArray)
                        {
                            tipoComplejo = true;

                            //Si es un array recorremos todos sus elementos
                            var arrayProp = valorProp as object[];

                            if (arrayProp != null)
                            {
                                foreach (var elto in arrayProp)
                                {
                                    InspectObject(elto, ref stringBuilder);
                                }
                            }
                        }
                        else
                        {
                            vProp = valorProp;
                        }
                    }
                    else
                    {
                        //Si es un tipo complejo lo inspeccionamos
                        tipoComplejo = true;

                        InspectObject(valorProp, ref stringBuilder);
                    }
                }

                if (!tipoComplejo)
                {
                    stringBuilder.Append(vProp);
                }

            }
        }

        #endregion
    }
}
