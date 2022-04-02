using System.Collections.Generic;

namespace RewindSystem
{
    public class Rewind<T> where T : struct
    {
        [System.Serializable]
        public struct RewindFrame
        {
            public T frameData;

            public float frameTime;
        }

        private System.Func<T, T, float, T> m_lerpFunc;

        private UnlimitedList<RewindFrame> m_frames;

        public Rewind(System.Func<T, T, float, T> lerpFunction)
        {
            m_lerpFunc = lerpFunction;
            m_frames = new UnlimitedList<RewindFrame>();
        }

        public Rewind()
        {
            m_lerpFunc = null;
            m_frames = new UnlimitedList<RewindFrame>();
        }

        public T GetFrame(float time) { 
            if (m_frames.Count == 0) return default;

            int index = BinarySearch(time);

            if (index < 0) return m_frames[0].frameData;
            else if (index >= m_frames.Count - 1) return m_frames[m_frames.Count - 1].frameData;
            else
            {
                var a = m_frames[index];
                var b = m_frames[index + 1];

                float lerp = (time - a.frameTime) / (b.frameTime - a.frameTime);
                return Lerp(a.frameData, b.frameData, lerp);
            }
        }

        public RewindFrame GetFrame(int index) { 
            if (m_frames.Count == 0) return default;

            if (index < 0) return m_frames[0];
            else if (index >= m_frames.Count - 1) return m_frames[m_frames.Count - 1];
            else
            {
                return m_frames[index];
            }
        }

        public void RemoveAt(int index)
        {
            m_frames.RemoveAt(index);
        }

        public int FrameCount => m_frames.Count;

        public void RegisterFrame(T data, float time) {
            if (time == -1f && m_frames.Count > 0 && time == m_frames[0].frameTime)
                m_frames.RemoveAt(0);

            m_frames.Add(time, new RewindFrame{
                frameData = data,
                frameTime = time
            });
        }

        public int BinarySearch(float time) {
            if (m_frames.Count > 0 && time > m_frames[m_frames.Count - 1].frameTime) {
                return m_frames.Count - 1;
            }

            int minNum = 0;
            int maxNum = m_frames.Count - 1;
            
            while (minNum <= maxNum)
            {
                int mid = minNum + ((maxNum - minNum) / 2);

                if (time == m_frames[mid].frameTime)
                {
                    return mid;
                }
                else if (time < m_frames[mid].frameTime) 
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

        public void Clear()
        {
            m_frames.Clear();
        }

        public T Lerp(T a, T b, float t) {
            if (m_lerpFunc == null) return a;
            return m_lerpFunc(a, b, t); 
        }
    }
   
}