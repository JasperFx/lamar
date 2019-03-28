using System;

namespace LamarCodeGeneration.Util
{
    internal static class DisposableExtensions
    {
        /// <summary>
        /// Attempts to call Dispose(), but swallows and discards any
        /// exceptions thrown
        /// </summary>
        /// <param name="disposable"></param>
        public static void SafeDispose(this IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception)
            {
                // That's right, swallow that exception
            }
        }

    }
}