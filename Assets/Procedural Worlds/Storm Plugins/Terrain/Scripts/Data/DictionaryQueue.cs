using System;
using System.Collections.Generic;

namespace ProceduralWorlds.Storm.Terrain
{

    [Serializable]
    public class DictionaryQueue<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private Queue<TKey> keyQueue = new Queue<TKey>();

        // Enqueue: Add an item to the end of the dictionary and queue its key
        public void Enqueue(TKey key, TValue value)
        {
            Add(key, value);
            keyQueue.Enqueue(key);
        }

        // Dequeue: Remove and return the first item (key-value pair) from the dictionary
        public KeyValuePair<TKey, TValue> Dequeue()
        {
            if (keyQueue.Count == 0)
            {
                throw new InvalidOperationException("The DictionaryQueue is empty.");
            }

            TKey key = keyQueue.Dequeue();
            TValue value = this[key];
            Remove(key);
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        // Peek: View the first item (key-value pair) without removing it
        public KeyValuePair<TKey, TValue> Peek()
        {
            if (keyQueue.Count == 0)
            {
                throw new InvalidOperationException("The DictionaryQueue is empty.");
            }

            TKey key = keyQueue.Peek();
            TValue value = this[key];
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        // Remove: Remove a specific key-value pair
        public new bool Remove(TKey key)
        {
            if (!ContainsKey(key))
            {
                return false;
            }

            base.Remove(key);
            Queue<TKey> newQueue = new Queue<TKey>();

            foreach (var existingKey in keyQueue)
            {
                if (!existingKey.Equals(key))
                {
                    newQueue.Enqueue(existingKey);
                }
            }

            keyQueue = newQueue;
            return true;
        }
    }
}
