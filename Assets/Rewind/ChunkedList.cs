using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Collections.Generic
{
    public class ChunkedList<T>
    {
        List<List<(float, T)>> m_lists;

        private int m_firstChunkId = 0;

        private int m_count = 0;

        public int Count => m_count;

        private float m_modulos;

        /// <summary>
        /// Creates a list of lists, makes it cheap to remove a chunk of data
        /// </summary>
        /// <param name="modulos">Defines what chunk this item will be in</param>
        /// <param name="maxChunks">How many chunks at once can we have?</param>
        public ChunkedList(float modulos, int maxChunks)
        {
            m_modulos = modulos;
            m_lists = new List<List<(float, T)>>(maxChunks);

            for (int i = 0; i < maxChunks; ++i)
            {
                m_lists.Add(new List<(float, T)>(Mathf.FloorToInt(modulos)));
            }
        }

        public void Clear()
        {
            m_firstChunkId = 0;
            m_count = 0;

            for (int i = 0; i < m_lists.Count; ++i)
            {
                m_lists[i].Clear();
            }
        }

        public (int, int) GetChunkIdOffset(int index)
        {
            int chunkIndex = 0;
            int offset = 0;

            // Find appropriate chunk
            for (int i = 0; i < m_lists.Count; ++i)
            {
                int c = m_lists[i].Count;

                if ((index - offset) < c)
                    break;

                offset += c;

                ++chunkIndex;
            }

            return (chunkIndex, offset);
        }

        public T this[int index]
        {
            get
            {
                if (index < 0) index += m_count;

                (int chunkIndex, int offset) = GetChunkIdOffset(index);
                return m_lists[chunkIndex][index - offset].Item2;
            }
        }

        private int BinarySearch(List<(float, T)> m_frames, float time) {
            if (m_frames.Count > 0 && time > m_frames[m_frames.Count - 1].Item1) {
                return m_frames.Count - 1;
            }

            int minNum = 0;
            int maxNum = m_frames.Count - 1;
            
            while (minNum <= maxNum)
            {
                int mid = minNum + ((maxNum - minNum) / 2);

                if (time == m_frames[mid].Item1)
                {
                    return mid;
                }
                else if (time < m_frames[mid].Item1) 
                {
                    maxNum = mid - 1;
                }
                else 
                {
                    minNum = mid + 1;
                }
            }
            
            return minNum - 1;
        }

        public void Add(float key, T value)
        {
            int chunkId = Mathf.FloorToInt(key / m_modulos);
            int localChunkId = chunkId - m_firstChunkId;

            if (localChunkId >= m_lists.Count)
            {
                int rotate = (localChunkId + 1) - m_lists.Count;

                for (int j = 0; j < m_lists.Count; ++j)
                {
                    if (j + rotate >= m_lists.Count)
                    {
                        m_count -= m_lists[j].Count;
                        m_lists[j] = new List<(float, T)>(m_lists[j].Capacity);
                    }
                    else m_lists[j] = m_lists[j + rotate];
                }

                m_firstChunkId += rotate;
            }

            #if UNITY_EDITOR
            if (chunkId < m_firstChunkId)
                throw new Exception($"Chunk index ({chunkId} : key={key}) is behind the deleted past ({m_firstChunkId})");
            #endif

            localChunkId = chunkId - m_firstChunkId;
            
            int insertId = BinarySearch(m_lists[localChunkId], key) + 1;

            m_lists[localChunkId].Insert(insertId, (key, value));

            ++m_count;
        }
    }
}