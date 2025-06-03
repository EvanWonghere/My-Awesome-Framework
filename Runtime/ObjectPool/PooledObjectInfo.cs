// File: PooledObjectInfo.cs
using UnityEngine;

namespace MAF.ObjectPool
{
    /// <summary>
    /// A component attached to pooled GameObjects to help identify their original pool.
    /// This allows the <see cref="ObjectPoolManager"/> to return the object to the correct
    /// <see cref="ObjectPoolInstance"/> without needing to pass the prefab or pool ID explicitly on return.
    /// </summary>
    public class PooledObjectInfo : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the <see cref="ObjectPoolInstance"/> that this GameObject belongs to.
        /// This is set internally by the pooling system when the object is retrieved.
        /// </summary>
        public ObjectPoolInstance OwningPool { get; set; }
    }
}