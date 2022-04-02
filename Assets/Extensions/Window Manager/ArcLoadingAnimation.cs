using System.Collections;
using System.Collections.Generic;
using ThisOtherThing.UI.Shapes;
using UnityEngine;

public class ArcLoadingAnimation : MonoBehaviour
{
    Arc m_arc;
    
    private void Awake()
    {
        m_arc = GetComponent<Arc>();
    }

    void Update()
    {
        m_arc.EllipseProperties.BaseAngle = Time.time % 2f;
        m_arc.ArcProperties.Length = Mathf.Sin(Time.time * 0.5f);

        m_arc.SetAllDirty();
    }
}
