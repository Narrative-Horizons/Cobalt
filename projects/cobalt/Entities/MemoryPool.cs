using System;

namespace Cobalt.Entities
{
    public interface IMemoryPool
    {
        bool Contains(Entity ent);
        void Remove(Entity ent);
    }

    /// <summary>
    /// Sparse set backed memory pool.
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public class MemoryPool<Type> : IMemoryPool where Type : unmanaged
    {
        public static readonly uint PageSize = 1024;

        private struct Page
        {
            public Entity[] Payload;
        }

        private readonly Vector<Entity> _packed = new Vector<Entity>();
        private readonly Vector<Page> _sparse = new Vector<Page>();
        private readonly Vector<Type> _payload = new Vector<Type>();

        public uint Capacity => _packed.Capacity;
        public uint Extent => _sparse.Count * PageSize;
        public uint Count => _packed.Count;
        public bool Empty => _packed.Empty();

        public void Reserve(uint Capacity)
        {
            _packed.Reserve(Capacity);
            _payload.Reserve(Capacity);
        }

        public void ShrinkToFit()
        {
            if (_packed.Empty())
            {
                _sparse.Clear();
            }
            else
            {
                _sparse.ShrinkToFit();
                _packed.ShrinkToFit();
                _payload.ShrinkToFit();
            }
        }

        public void Assign(Entity ent, ref Type value)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            ref Page pg = ref _Assure(page);
            pg.Payload[offset] = ent;
            _payload.Add(value);
            _packed.Add(ent);
        }

        public bool Contains(Entity ent)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            return page < _sparse.Count && _sparse[page].Payload != null && _sparse[page].Payload[offset] != Entity.Invalid;
        }

        public Entity EntityAt(uint index)
        {
            return index > _packed.Count ? Entity.Invalid : _packed[index];
        }

        public ref Type Get(Entity ent)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            uint index = _sparse[page].Payload[offset].Identifier;
            return ref _payload[index];
        }

        public void Replace(Entity ent, ref Type value)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            uint index = _sparse[page].Payload[offset].Identifier;
            _payload[index] = value;
        }

        public void Remove(Entity ent)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            ref Entity r = ref _sparse[page].Payload[offset];

            uint payloadIndex = r.Identifier;
            ref Type last = ref _payload.Back();
            _payload[payloadIndex] = last;
            _payload.PopBack();
            
            ref Entity back = ref _packed.Back();
            uint backPage = _Page(back);
            uint backOffset = _PageOffset(back);
            _packed.Back() = ent;
            _sparse[backPage].Payload[backOffset] = r;
            r = Entity.Invalid;
            _packed.PopBack();
        }

        public Vector<Type> GetPayloadEnumerable()
        {
            return _payload;
        }
        public Vector<Entity> GetEntityEnumerable()
        {
            return _packed;
        }

        private uint _Page(Entity ent)
        {
            return ent.Identifier / PageSize;
        }

        private uint _PageOffset(Entity ent)
        {
            return ent.Identifier & (PageSize - 1);
        }

        private ref Page _Assure(uint pageId)
        {
            if (pageId >= _sparse.Count)
            {
                _sparse.Resize(pageId + 1);
            }

            if (_sparse[pageId].Payload == null)
            {
                _sparse[pageId].Payload = new Entity[PageSize];
                Array.Fill(_sparse[pageId].Payload, Entity.Invalid);
            }

            return ref _sparse[pageId];
        }
    }
}
