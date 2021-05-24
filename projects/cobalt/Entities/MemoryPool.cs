using System;
using System.Collections.Generic;

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
    public class MemoryPool<Type> : IMemoryPool
    {
        public static readonly uint PageSize = 1024;

        private struct Page
        {
            public Entity[] Payload;
        }

        private readonly List<Entity> _packed = new List<Entity>();
        private readonly List<Page> _sparse = new List<Page>();
        private readonly List<Type> _payload = new List<Type>();

        public uint Capacity => (uint)_packed.Capacity;
        public uint Extent => (uint)_sparse.Count * PageSize;
        public uint Count => (uint)_packed.Count;
        public bool Empty => _packed.Count == 0;

        public void Reserve(uint Capacity)
        {
            _packed.Capacity = (int)Capacity;
            _payload.Capacity = (int)Capacity;
        }

        public void ShrinkToFit()
        {
            if (_packed.Count == 0)
            {
                _sparse.Clear();
            }
            else
            {
                _sparse.TrimExcess();
                _packed.TrimExcess();
                _payload.TrimExcess();
            }
        }

        public void Assign(Entity ent, ref Type value)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            Page pg = _Assure(page);
            pg.Payload[offset] = ent;
            _payload.Add(value);
            _packed.Add(ent);
        }

        public bool Contains(Entity ent)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            return page < _sparse.Count && _sparse[(int)page].Payload != null && _sparse[(int)page].Payload[offset] != Entity.Invalid;
        }

        public Entity EntityAt(uint index)
        {
            return index > _packed.Count ? Entity.Invalid : _packed[(int)index];
        }

        public Type Get(Entity ent)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            uint index = _sparse[(int)page].Payload[offset].Identifier;
            return _payload[(int)index];
        }

        public Type TryGet(Entity ent)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            Entity sparse = _sparse[(int)page].Payload[offset];
            if (sparse.IsInvalid)
            {
                return default(Type);
            }
            return _payload[(int)sparse.Identifier];
        }

        public void Replace(Entity ent, Type value)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            uint index = _sparse[(int)page].Payload[offset].Identifier;
            _payload[(int)index] = value;
        }

        public void Remove(Entity ent)
        {
            uint page = _Page(ent);
            uint offset = _PageOffset(ent);
            ref Entity r = ref _sparse[(int)page].Payload[offset];

            uint payloadIndex = r.Identifier;
            Type last = _payload[_payload.Count - 1];
            _payload[(int)payloadIndex] = last;
            _payload.RemoveAt(_payload.Count - 1);
            
            Entity back = _packed[_packed.Count - 1];
            uint backPage = _Page(back);
            uint backOffset = _PageOffset(back);
            _packed[_packed.Count - 1] = ent;
            _sparse[(int)backPage].Payload[backOffset] = r;
            r = Entity.Invalid;
            _packed.RemoveAt(_packed.Count - 1);
        }

        public List<Type> GetPayloadEnumerable()
        {
            return _payload;
        }
        public List<Entity> GetEntityEnumerable()
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

        private Page _Assure(uint pageId)
        {
            if (pageId >= _sparse.Count)
            {
                _sparse.Capacity = (int)pageId + 1;
            }

            while (pageId >= _sparse.Count)
            {
                Page page = new Page
                {
                    Payload = new Entity[PageSize]
                };
                Array.Fill(page.Payload, Entity.Invalid);

                _sparse.Add(page);
            }

            if (_sparse[(int)pageId].Payload == null)
            {
                var x = _sparse[(int)pageId];
                x.Payload = new Entity[PageSize];

                Array.Fill(_sparse[(int)pageId].Payload, Entity.Invalid);
            }

            return _sparse[(int)pageId];
        }
    }
}
