using System;
using System.Collections;
using System.Collections.Generic;

namespace Cobalt.Entities
{
    public class Vector<Type>
    {
        public class Enumerator
        {
            private Type[] _payload;
            private ulong _index;
            private ulong _size;

            public Enumerator(ref Type[] payload, ulong size)
            {
                _payload = payload;
                _index = ulong.MaxValue;
                _size = size;
            }

            public ref Type Current => ref _payload[_index];

            public void Dispose()
            {
                // Empty
            }

            public bool MoveNext()
            {
                _index += 1;
                return _index < _size;
            }

            public void Reset()
            {
                _index = ulong.MaxValue;
            }
        }

        private Type[] _payload;

        public uint Count { get; private set; } = 0;
        public uint Capacity { get; private set; } = 0;

        public Vector()
        {
            _payload = Array.Empty<Type>();
        }

        public Vector(uint initialCapacity)
        {
            Reserve(initialCapacity);
        }

        public ref Type At(ulong index)
        {
            return ref _payload[index];
        }

        public ref Type this[ulong index]
        {
            get
            {
                return ref At(index);
            }
        }

        public ref Type Front()
        {
            return ref At(0);
        }

        public ref Type Back()
        {
            return ref At(Count - 1);
        }

        public void Reserve(uint newCapacity)
        {
            if (newCapacity > Capacity)
            {
                Array.Resize(ref _payload, (int)newCapacity);
                Capacity = newCapacity;
            }
        }

        public void Resize(uint newCount)
        {
            Array.Resize(ref _payload, (int) newCount);
            Count = newCount;
            Capacity = newCount;
        }

        public void ShrinkToFit()
        {
            if (Count != Capacity)
            {
                Array.Resize(ref _payload, (int) Capacity);
                Capacity = Count;
            }
        }

        public void Clear()
        {
            Array.Clear(_payload, 0, (int) Count);
            Count = 0;
        }

        public void Add(Type item)
        {
            if (Count == Capacity)
            {
                Reserve(Count + 1);
            }
            _payload[Count] = item;
            ++Count;
        }

        public void AddAll(IEnumerable<Type> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Insert(uint index, Type item)
        {
            if (Count == Capacity)
            {
                Reserve(Count + 1);
            }

            uint toCopy = Count - index;
            Array.Copy(_payload, index, _payload, index + 1, toCopy);
            _payload[index] = item;
            ++Count;
        }

        public void Erase(uint index)
        {
            uint toCopy = Count - index - 1;
            Array.Copy(_payload, index + 1, _payload, index, toCopy);
            _payload[index] = default;
            --Count;
        }

        public bool Empty()
        {
            return Count == 0;
        }

        public void PopBack()
        {
            _payload[Count - 1] = default;
            --Count;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(ref _payload, Count);
        }
    }
}
