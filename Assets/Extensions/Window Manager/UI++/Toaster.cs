using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toaster : MonoBehaviour
{
    static Toaster me;

    [SerializeField] GameObject m_toastPrefab;


    private void SpawnToast(Color c, string text)
    {
        var go = GameObject.Instantiate(m_toastPrefab, transform);
        var toast = go.GetComponent<Toast>();
        toast.Set(c, text);
    }

    private void Awake()
    {
        me = this;
    }

    public static void Toast(Color c, string text)
    {
        me.SpawnToast(c, text);
    }
}
