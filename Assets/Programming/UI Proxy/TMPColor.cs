using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMPColor : MonoBehaviour
{
    [SerializeField] Color m_numbers;

    [SerializeField] Color m_text;

    [Inject] DatasetAutocompletion m_autoCompletionProvider;

    TMP_InputField m_input;

    void Awake()
    {
        m_input = GetComponentInChildren<TMP_InputField>();
        m_input.textComponent.OnPreRenderText += OnValueUpdated;

        OnValueUpdated(m_input.textComponent.textInfo);
    }

    private void OnValueUpdated(TMP_TextInfo textInfo)
    {
        foreach(var word in textInfo.wordInfo)
        {
            if (word.characterCount > 0)
            {
                string wordstr = word.GetWord();
                bool isNumber = long.TryParse(wordstr, out _) || double.TryParse(wordstr, out _);

                PaintWord(textInfo, word, isNumber ? m_numbers : m_text);
            }
        }

        m_input.textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void PaintWord(TMP_TextInfo info, TMP_WordInfo word, Color color)
    {
        for (int i = 0; i < word.characterCount; ++i)
        {
            int charIndex = word.firstCharacterIndex + i;
            int meshIndex = info.characterInfo[charIndex].materialReferenceIndex;
            int vertexIndex = info.characterInfo[charIndex].vertexIndex;
        
            Color32[] vertexColors = info.meshInfo[meshIndex].colors32;
            vertexColors[vertexIndex + 0] = color;
            vertexColors[vertexIndex + 1] = color;
            vertexColors[vertexIndex + 2] = color;
            vertexColors[vertexIndex + 3] = color;
        }
    }
}
