using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField] WindowBehaviour[] m_windowDatabase;

    [SerializeField] int m_startLayerIndex = 10;

    public static WindowManager LastInstance;

    List<WindowBehaviour> m_windows = new List<WindowBehaviour>();

    private void Awake()
    {
        LastInstance = this;
    }

    public T Push<T>() where T : WindowBehaviour
    {
        foreach(var w in m_windowDatabase)
        {
            if (w is T)
            {
                T v = Instantiate(w.gameObject, transform).GetComponent<T>();
                v.PreAwake(this);
                v.Canvas.sortingOrder = m_startLayerIndex + m_windows.Count + 1;
                m_windows.Add(v);
                return v;
            }
        }
        
        return null;
    }

    public void Pop(WindowBehaviour target)
    {
        if (m_windows.Contains(target))
        {
            int id = m_windows.IndexOf(target);
            m_windows.RemoveAt(id);
            UpdateSorting(id);
        }
    }

    void UpdateSorting(int startId)
    {
        for (int i = startId; i < m_windows.Count; ++i)
            m_windows[i].Canvas.sortingOrder = m_startLayerIndex + (i++) + 1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && m_windows.Count > 0)
        {
            var top = m_windows[m_windows.Count - 1];
            top.PopWindow();
        }
    }
}
