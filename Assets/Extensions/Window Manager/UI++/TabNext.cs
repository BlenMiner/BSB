using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_InputField))]
public class TabNext : MonoBehaviour
{
    TMPro.TMP_InputField me;

    [SerializeField] TMPro.TMP_InputField m_prev;

    [SerializeField] TMPro.TMP_InputField m_next;

    private void Awake()
    {
        me = GetComponent<TMPro.TMP_InputField>();
    }
    
    void Update()
    {
        if (!me.isFocused) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                 m_prev.Select();
            else m_next.Select();
        }
    }
}
