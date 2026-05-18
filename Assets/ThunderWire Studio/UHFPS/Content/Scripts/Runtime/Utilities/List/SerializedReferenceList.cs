using System;
using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    public interface ISerializedReferenceListItem
    {
        string Name { get; }
    }

    /// <summary>
    /// Provides a serializable list of reference-type items with type-based retrieval and management functionality.
    /// Draws a custom inspector in the Unity Editor.
    /// </summary>
    [Serializable]
    public abstract class SerializedReferenceList<TItem> where TItem : class, ISerializedReferenceListItem
    {
        [SerializeReference]
        public List<TItem> Items = new();

        /// <summary>
        /// Get the first item of type T, or null if none found.
        /// </summary>
        public T Get<T>() where T : class, TItem
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] is T t) 
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Remove all items of type T. Returns true if any were removed.
        /// </summary>
        public bool Remove<T>() where T : class, TItem
        {
            return Items.RemoveAll(a => a is T) > 0;
        }

        /// <summary>
        /// Clear all items.
        /// </summary>
        public void Clear() => Items.Clear();

        /// <summary>
        /// Try to get the first item of type T. Returns true if found, false otherwise.
        /// </summary>
        public bool TryGet<T>(out T item) where T : class, TItem
        {
            item = Get<T>();
            return item != null;
        }

        /// <summary>
        /// Get the first item of type T, or create and add a new one if none found.
        /// </summary>
        public T GetOrAdd<T>() where T : class, TItem, new()
        {
            if (TryGet<T>(out var found))
                return found;

            var created = new T();
            Items.Add(created);
            return created;
        }
    }
}
