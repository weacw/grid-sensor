using System.Collections.Generic;
using UnityEngine;

namespace MBaske.Sensors
{
    /// <summary>
    /// Result generated by <see cref="ColliderDetector"/>.
    /// </summary>
    public class DetectionResult
    {
        /// <summary>
        /// Result item associated with a detected collider.
        /// </summary>
        public class Item
        {
            public Vector3 Position; // world
            public Collider Collider;
            // x: normalized lon
            // y: normalized lat
            // z: normalized distance
            // w: distance ratio
            public HashSet<Vector4> Coords;
            // normalized lon/lat rect
            public Rect Rect;
            
            public Item()
            {
                Coords = new HashSet<Vector4>();
                Clear();
            }

            public void Clear()
            {
                Coords.Clear();
                Rect.min = Vector2.one;
                Rect.max = Vector2.zero;
            }

            public void Add(Vector4 coord)
            {
                Coords.Add(coord);
                Rect.min = Vector2.Min(Rect.min, coord);
                Rect.max = Vector2.Max(Rect.max, coord);
            }
        }

        private readonly Dictionary<string, List<Item>> m_Dict;
        private readonly Stack<Item> m_Pool;

        public DetectionResult(IEnumerable<string> tags)
        {
            m_Pool = new Stack<Item>();
            m_Dict = new Dictionary<string, List<Item>>();

            foreach (var tag in tags)
            {
                m_Dict.Add(tag, new List<Item>());
            }
        }

        public void Clear()
        {
            foreach (var list in m_Dict.Values)
            {
                foreach (var item in list)
                {
                    item.Clear();
                    m_Pool.Push(item);
                }

                list.Clear();
            }
        }

        public Item NewItem()
        {
            if (m_Pool.Count > 0)
            {
                return m_Pool.Pop();
            }

            return new Item();
        }

        public void AddItem(Item item)
        {
            if (m_Dict.TryGetValue(item.Collider.tag, out List<Item> list))
            {
                list.Add(item);
            }
            else
            {
                throw new KeyNotFoundException(item.Collider.tag);
            }
        }

        public List<Item> GetItems(string tag)
        {
            if (m_Dict.TryGetValue(tag, out List<Item> list))
            {
                return list;
            }

            throw new KeyNotFoundException(tag);
        }
    }
}