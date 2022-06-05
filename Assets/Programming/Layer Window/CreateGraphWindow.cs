using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CreateGraphWindow : WindowBehaviour
{
    [SerializeField] TMP_InputField m_nameField;
    
    Action<string> m_onCreated;

    public void Setup(Action<string> created)
    {
        m_onCreated = created;
    }

    public void OnCreate()
    {
        m_onCreated?.Invoke(m_nameField.text);
        ForcePopWindow();
    }
}
