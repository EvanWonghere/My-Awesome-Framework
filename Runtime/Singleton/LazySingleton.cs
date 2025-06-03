// File: LazySingleton.cs
using System;

namespace MAF.Singleton
{
    /// <summary>
    /// A generic base class for creating lazy-initialized, thread-safe singletons for non-MonoBehaviour classes.
    /// The instance is created only when it's first accessed.
    /// </summary>
    /// <typeparam name="T">The type of the class to be a singleton. Must have a parameterless constructor.</typeparam>
    public class LazySingleton<T> where T : class, new()
    {
        /// <summary>
        /// The lazily initialized instance of the singleton.
        /// </summary>
        private static readonly Lazy<T> LazyInstance = new Lazy<T>(() => new T());

        /// <summary>
        /// Gets the singleton instance of the class.
        /// The instance is created on first access in a thread-safe manner.
        /// </summary>
        public static T Instance => LazyInstance.Value;

        /// <summary>
        /// Protected constructor to prevent direct instantiation.
        /// Ensures that the class can only be instantiated via the singleton accessor.
        /// Allows derived classes to call this constructor.
        /// </summary>
        protected LazySingleton() { }
    }
}