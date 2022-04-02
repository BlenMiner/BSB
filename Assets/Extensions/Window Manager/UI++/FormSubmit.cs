using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMPro.TMP_InputField))]
public class FormSubmit : MonoBehaviour
{
    TMPro.TMP_InputField me;

    [SerializeField] Button m_button;
    
    private void Awake()
    {
        me = GetComponent<TMPro.TMP_InputField>();
        me.onSubmit.AddListener((_) => {
            m_button.onClick?.Invoke();
        });
    }
}
