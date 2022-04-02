using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TMPAutoCompletion : MonoBehaviour
{
    [SerializeField] RectTransform m_autocompletionParent;

    [SerializeField] GameObject m_template;

    [Inject] DatasetAutocompletion m_autoCompletionProvider;

    List<GameObject> m_instances;

    TMPro.TMP_InputField m_input;

    private bool m_active = false;

    private int m_selection = 0;

    private DatasetProp[] m_options;

    private int m_wordIndex;

    private int? m_caretPos;

    private void Awake()
    {
        m_instances = new List<GameObject>();
        m_template.SetActive(false);
    
        m_autocompletionParent.gameObject.SetActive(false);
        m_input = GetComponentInChildren<TMPro.TMP_InputField>();

        m_input.onDeselect.AddListener(HideAutocompletion);
        m_input.onValueChanged.AddListener(UpdateAutocompletion);
        m_input.onSubmit.AddListener(OnSubmit);
    }

    private void UpdateMatchesList(int wordIndex, DatasetProp[] options)
    {
        if (m_wordIndex != wordIndex)
        {
            m_selection = 0;
            m_wordIndex = wordIndex;
        }

        m_options = options;
        m_active = options.Length != 0;
        m_autocompletionParent.gameObject.SetActive(m_active);

        if (!m_active) return;
        
        while (m_instances.Count < options.Length)
        {
            var go = Instantiate(m_template, m_autocompletionParent, false);
            go.SetActive(true);
            m_instances.Add(go);
        }
        
        while (m_instances.Count > options.Length)
        {
            Destroy(m_instances[m_instances.Count - 1]);
            m_instances.RemoveAt(m_instances.Count - 1);
        }

        for (int i = 0; i < options.Length; ++i)
        {
            var txt = m_instances[i].GetComponentInChildren<TMP_Text>();
            var graphic = m_instances[i].GetComponentInChildren<Graphic>();

            graphic.enabled = i == m_selection;
            txt.SetText($"{options[i].Value} <color=grey>{options[i].Description}</color>");
        }
    }

    private void UpdateMatches(TMP_WordInfo parent, TMPro.TMP_WordInfo word)
    {
        if (word.textComponent != null)
        {
            var w = word.GetWord();

            if (w.Length > 1)
            {
                char first = w[0];
                if (!char.IsLetter(first))
                    return;
            }
        }

        if (word.firstCharacterIndex > 0)
        {
            bool followedByDot = 
                word.textComponent.text[word.firstCharacterIndex - 1] == '.' ||
                word.textComponent.text[word.firstCharacterIndex] == '.';

            if (followedByDot && parent.textComponent != null)
            {
                UpdateMatchesList(
                    word.firstCharacterIndex,
                    m_autoCompletionProvider.GetAutocompletion(parent.GetWord(), word)
                );
                return;
            }
        }

        UpdateMatchesList(
            word.textComponent == null ? 0 : word.firstCharacterIndex,
            m_autoCompletionProvider.GetAutocompletion(word)
        );
    }

    private string ApplyAutoCompletion()
    {
        if (m_options != null && m_selection < m_options.Length)
            return m_options[m_selection].Value;
        
        return null;
    }

    public void OnSubmit(string value)
    {
        value = m_input.text;
        var textInfo = m_input.textComponent.GetTextInfo(value);
        
        m_input.ActivateInputField();

        int selection = m_input.caretPosition;

        var words = textInfo.wordInfo;
        string replacement = ApplyAutoCompletion();
        if (replacement == null) return;
        m_caretPos = null;

        foreach(var word in words)
        {
            int insertPos = word.firstCharacterIndex;

            if (selection >= word.firstCharacterIndex && selection <= word.lastCharacterIndex + 1)
            {
                try
                {
                    if (value.Length > word.firstCharacterIndex && word.characterCount > 0)
                        value = value.Remove(word.firstCharacterIndex, word.characterCount);
                }
                catch
                {
                    insertPos = value.Length;
                }
                value = value.Insert(insertPos, replacement);

                m_caretPos = insertPos + replacement.Length;
                break;
            }
        }

        if (m_caretPos == null)
        {
            value = value.Insert(selection, replacement);
            m_caretPos = selection + replacement.Length;
        }
        
        m_input.text = value;
    }

    int lastCaretPos = 0;

    private void Update()
    {
        if (m_options != null)
        {
            int oldSelection = m_selection;
            int newSelection = m_selection;

            if (Input.GetKeyDown(KeyCode.UpArrow) && m_options.Length > 0)
            {
                if (--newSelection < 0)
                    newSelection = m_options.Length - 1;

                m_caretPos = lastCaretPos;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && m_options.Length > 0)
            {
                newSelection = (newSelection + 1) % m_options.Length;
                m_caretPos = lastCaretPos;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideAutocompletion(null);
            }

            if (oldSelection != newSelection)
            {
                var oldgraphic = m_instances[oldSelection].GetComponentInChildren<Graphic>();
                var graphic = m_instances[newSelection].GetComponentInChildren<Graphic>();
                oldgraphic.enabled = false;
                graphic.enabled = true;

                m_selection = newSelection;
            }
        }
    }

    private void LateUpdate()
    {
        if (m_caretPos != null)
        {
            m_input.caretPosition = m_caretPos.Value;
            m_caretPos = null;
        }

        if (lastCaretPos != m_input.caretPosition)
        {
            lastCaretPos = m_input.caretPosition;

            if (EventSystem.current.currentSelectedGameObject == m_input.gameObject)
                UpdateAutocompletion(null);
        }
    }

    private TMP_WordInfo GetWordAtIndex(int index, out TMP_WordInfo parent)
    {
        var value = m_input.text;
        var wordBuilder = new StringBuilder();

        TMP_WordInfo prev = default;
        TMP_WordInfo word = default;

        int wordStart = 0;
        
        void ProcessWord(int start, int end, string wrd)
        {
            var tmpWord = new TMP_WordInfo{
                characterCount = wrd.Length,
                firstCharacterIndex = start,
                lastCharacterIndex = end,
                textComponent = m_input.textComponent
            };
    
            if (start <= index && index <= end + 1)
                word = tmpWord;
            else if (end < index)
                prev = tmpWord;
        }

        for (int i = 0; i < value.Length; ++i)
        {
            char c = value[i];

            if (wordBuilder.Length == 0)
            {
                if (char.IsLetter(c))
                {
                    wordBuilder.Append(c);
                    wordStart = i;
                }
            }
            else if (char.IsLetterOrDigit(c))
            {
                wordBuilder.Append(c);
            }
            else
            {
                ProcessWord(wordStart, i - 1, wordBuilder.ToString());
                wordBuilder.Clear();
            }
        }

        if (wordBuilder.Length > 0)
            ProcessWord(wordStart, value.Length - 1, wordBuilder.ToString());

        parent = prev;
        return word;
    }

    private void UpdateAutocompletion(string value)
    {
        try
        {
            var word = GetWordAtIndex(m_input.caretPosition, out var parent);
            bool updated = false;

            if (word.textComponent != null)
            {
                UpdateMatches(parent, word);
                updated = true;
            }
            if (!updated) HideAutocompletion(null);
        }
        catch
        {
            HideAutocompletion(null);
        }
    }

    private void HideAutocompletion(string value)
    {
        m_selection = 0;
        m_wordIndex = -1;
        m_active = false;
        m_autocompletionParent.gameObject.SetActive(m_active);
    }
}
