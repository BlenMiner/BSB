using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Collections.Generic
{
    public class UnlimitedList<T>
    {
        List<(float, T)> m_list;

        private int m_count = 0;

        public int Count => m_count;

        /// <summary>
        /// Creates a list of lists, makes it cheap to remove a chunk of data
        /// </summary>
        /// <param name="modulos">Defines what chunk this item will be in</param>
        /// <param name="maxChunks">How many chunks at once can we have?</param>
        public UnlimitedList(int capacity = 10)
        {
            m_list = new List<(float, T)>(capacity);
        }

        public void Clear()
        {
            m_count = 0;
            m_list.Clear();
        }

        public T this[int index]
        {
            get
            {
                if (index < 0) index += m_count;
                return m_list[index].Item2;
            }
        }

        public void RemoveAt(int index)
        {
            m_list.RemoveAt(index);
            --m_count;
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
            int insertId = BinarySearch(m_list, key) + 1;

            m_list.Insert(insertId, (key, value));

            ++m_count;
        }
    }
}