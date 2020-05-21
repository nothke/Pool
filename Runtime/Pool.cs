/*
 *  Pool<IPoolable> is a fixed size collection whose elements are preallocated on creation.
 *  Getting and releasing therefore does not heap alloc every time and call the garbage collector.
 *  Objects become "alive" with Get() or "die" with Release();
 *
 *  It is explicitely fixed-size to prevent inadvertent memory leaks, 
 *  so you cannot get more objects than the pool's capacity, otherwise an exception will be thrown.
 *
 *  Pool requires an object that inherits IPoolable.
 *  When Pool is constructed, it creates the element and calls Allocate().
 *  When you Get() an element, it calls Obtain() on it, and Release() when object is released.
 *  Therefore use Release() or Obtain() on IPoolable to its data.
 *
 *  Clear() releases all objects, but does not actually destroy them (they stay in memory).
 *  Objects will be garbage collected only when the Pool itself gets unreferenced.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Nothke.Collections
{
    public interface IPoolable
    {
        void OnGet();
        void OnRelease();
    }

    /// <summary> Fixed size collection whose elements are preallocated.
    /// Getting and releasing therefore does not heap alloc every time and call the garbage collector.
    /// </summary>
    public class Pool<T> : ICollection<T>, IEnumerable<T>, ICollection where T : class, IPoolable, new()
    {
        protected T[] array;
        protected bool[] alive;
        int seek;
        public int capacity { get; private set; }
        int aliveCount;
        //Dictionary<T, int> hashmap; // TODO

        public int Count => aliveCount;
        public bool IsReadOnly => false;

        public bool IsSynchronized => false;
        public object SyncRoot => null;

        /// <summary>Creates the pool with capacity and allocates all elements.
        /// </summary>
        public Pool(int capacity)
        {
            this.capacity = capacity;
            array = new T[capacity];
            alive = new bool[capacity];

            for (int i = 0; i < capacity; i++)
            {
                array[i] = new T();
                alive[i] = false;
            }
        }

        public T Get()
        {
            int lastSeek = seek;

            while (alive[seek])
            {
                seek++;

                if (seek >= capacity)
                    seek = 0;

                if (seek == lastSeek)
                    throw new System.Exception("Pool is full");
            }

            return GetAt(seek);
        }

        public bool TryGet(out T item)
        {
            int lastSeek = seek;

            while (alive[seek])
            {
                seek++;

                if (seek >= capacity)
                    seek = 0;

                if (seek == lastSeek)
                {
                    item = default;
                    return false;
                };
            }

            item = GetAt(seek);
            return true;
        }

        T GetAt(int i)
        {
            alive[i] = true;
            array[i].OnGet();
            aliveCount++;
            return array[i];
        }

        // TODO: implement hash?
        public void Release(T item)
        {
            for (int i = 0; i < capacity; i++)
            {
                if (array[i] != item)
                    continue;

                ReleaseAt(i);
                return;
            }
        }

        void ReleaseAt(int i)
        {
            alive[i] = false;
            array[i].OnRelease();
            aliveCount--;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new System.Exception("Pool has fixed memory, it doesn't allow adding new items");
        }

        public void Clear()
        {
            for (int i = 0; i < capacity; i++)
            {
                if (alive[i])
                    ReleaseAt(i);
            }
        }

        // TODO: With hash
        public bool Contains(T item)
        {
            for (int i = 0; i < capacity; i++)
            {
                if (!alive[i])
                    continue;

                if (array[i] == item)
                    return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            if (Contains(item))
            {
                Release(item);
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            Pool<T> pool;
            int seek;

            public T Current
            {
                get
                {
                    return pool.array[seek];
                }
            }

            public Enumerator(Pool<T> pool)
            {
                this.pool = pool;
                seek = -1;
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                do { seek++; }
                while (seek < pool.capacity && !pool.alive[seek]);

                return seek < pool.capacity;
            }

            public void Reset()
            {
                seek = -1;
            }
        }
    }
}