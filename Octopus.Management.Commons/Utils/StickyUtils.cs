using System.ComponentModel;

namespace Octopus.Management.Commons.Utils
{
    /// <summary>
    /// Clase que trabaja con los planes de ejecución mediante reflexión para 
    /// facilitar el acceso a los datos de estos en secciones de código
    /// sin acceso a los datos genéricos
    /// </summary>
    public static class StickyUtils
    {
        /// <summary>
        /// Ejecuta un plan de ejecución
        /// </summary>
        /// <param name="sticky">Plan de ejecución</param>
        /// <param name="work">Proceso en segundo plano</param>
        /// <returns>Datos devueltos por el plan de ejecución</returns>
        public static object ExecuteSticky(object sticky, BackgroundWorker work = null)
        {
            return sticky.GetType().GetMethod("ExecutePlan",
                new[] { typeof(BackgroundWorker) }).Invoke(sticky,
                    new object[] { work });
        }

        /// <summary>
        /// Devuelve el identificador de un plan de ejecución
        /// </summary>
        /// <param name="sticky">Plan de ejecución</param>
        /// <returns>Identificador</returns>
        public static long GetStickyId(object sticky)
        {
            return (long) sticky.GetType().GetProperty("Id").GetValue(sticky, null);
        }

        /// <summary>
        /// Devuelve el valor de una propiedad de un plan de ejecución
        /// </summary>
        /// <typeparam name="T">Tipo de parámetro utilizado en el plan de ejecución</typeparam>
        /// <param name="sticky">Plan de ejecución</param>
        /// <param name="propName">Nombre de la propiedad</param>
        /// <returns>Valor de la propiedad</returns>
        public static T GetStickyProp<T>(object sticky, string propName)
        {
            return (T)sticky.GetType().GetProperty(propName).GetValue(sticky, null);
        }
    }
}
