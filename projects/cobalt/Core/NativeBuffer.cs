using System;

namespace Cobalt.Core
{
    public class NativeBuffer<T> where T : unmanaged
    {
        private readonly IntPtr _ptr;
        private uint _index;

        public NativeBuffer(IntPtr ptr)
        {
            _ptr = ptr;
        }

        private T GetRaw(uint index)
        {
            unsafe
            {
                T* objectPtr = (T*)_ptr.ToPointer();
                return objectPtr[index];
            }
        }

        private void SetRaw(T value, uint index)
        {
            unsafe
            {
                T* objectPtr = (T*)_ptr.ToPointer();
                objectPtr[index] = value;
            }
        }

        public T Get()
        {
            return Get(_index);
        }

        public T Get(uint index)
        {
            _index = index;
            return GetRaw(_index++);
        }

        public NativeBuffer<T> Set(T value)
        {
            return Set(value, _index);
        }

        public NativeBuffer<T> Set(T value, uint index)
        {
            _index = index;
            SetRaw(value, _index++);

            return this;
        }
    }
}
