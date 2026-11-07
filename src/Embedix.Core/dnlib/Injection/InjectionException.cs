using dnlib.DotNet;
using System;

namespace Embedix.Core.dnlib.Injection
{
    /// <summary>
    /// Thrown by <see cref="Injector.Inject"/> when a <see cref="TypeDef"/> fails to be
    /// injected into the target module. Replaces the plain <see cref="Exception"/> wrapper
    /// used by the original implementation so callers can catch injection failures
    /// specifically and still inspect <see cref="Exception.InnerException"/> for the root cause.
    /// </summary>
    [Serializable]
    public class InjectionException : Exception
    {
        /// <summary>Full name of the source type that failed to inject, if known.</summary>
        public string SourceTypeFullName { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InjectionException"/> class with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InjectionException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InjectionException"/> class with a message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that caused this one.</param>
        public InjectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InjectionException"/> class for a type that failed to inject.
        /// </summary>
        /// <param name="source">The type that failed to inject.</param>
        /// <param name="innerException">The exception that caused the failure.</param>
        public InjectionException(TypeDef source, Exception innerException)
            : base($"Failed to inject type '{source?.FullName}'.", innerException)
        {
            SourceTypeFullName = source?.FullName;
        }
    }
}
