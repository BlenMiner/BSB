using System.Collections;
using System.Collections.Generic;
using ThisOtherThing.UI.Shapes;
using UnityEngine;

public class Toast : MonoBehaviour
{
    [SerializeField] Rectangle m_graphic;

    [SerializeField] TMPro.TMP_Text m_text;

    float m_timeAlive = 0f;
    
    public void Set(Color c, string text)
    {
        m_graphic.ShapeProperties.OutlineColor = c;
        m_text.SetText(text);
    }

    private void Update()
    {
        m_timeAlive += Time.deltaTime;

        if (m_timeAlive > 10f)
        {
            DestroyMe();
        }
    }

    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}
