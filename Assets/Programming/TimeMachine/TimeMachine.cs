using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TimeMachine : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Provides] TimeMachine provider;

    [SerializeField] RectTransform m_parent;

    [SerializeField] RectTransform m_bar;

    [SerializeField] RectTransform m_knob;

    [SerializeField] TMP_Text m_startTxt;

    [SerializeField] TMP_Text m_currTxt;

    [SerializeField] TMP_Text m_endTxt;

    [SerializeField] TMP_Dropdown m_filter;

    public event Action<float> OnTimeMachineUpdate;

    public float CurrentPercentage {get; private set;}

    private int m_startDate;

    private int m_endDate;

    private int m_lengthDate;

    public int StartDate => m_startDate;

    public int LengthDate => m_lengthDate;

    private Vector2Int m_subSpan;

    private void Awake()
    {
        m_currTxt.SetText(string.Empty);

        SetTime(0f);
    }

    float GetPercentage(PointerEventData data) => 
        (data.position.x - 100f) / (m_parent.rect.width * 0.01f);

    public void OnDrag(PointerEventData eventData) => SetTime(GetPercentage(eventData));

    public void OnPointerDown(PointerEventData eventData) => SetTime(GetPercentage(eventData));

    public void OnPointerUp(PointerEventData eventData) => SetTime(GetPercentage(eventData));

    public void UpdateRange(int start, int end)
    {
        m_startDate = start;
        m_endDate = end;

        m_lengthDate = end - start;

        m_startTxt.SetText(WeatherDataset.START_DATE.AddDays(start).ToLongDateString());
        m_endTxt.SetText(WeatherDataset.START_DATE.AddDays(end).ToLongDateString());

        m_filter.options.Clear();
        m_filter.options.Add(new TMP_Dropdown.OptionData("All"));

        int startYear = WeatherDataset.START_DATE.Year;

        for (int i = 2022; i >= startYear; --i)
            m_filter.options.Add(new TMP_Dropdown.OptionData(i.ToString()));

        m_filter.onValueChanged.AddListener(OnFilterChanged);
        OnFilterChanged(m_filter.value);
    }

    private void OnFilterChanged(int value)
    {
        int index = value - 1;
        int year = 2022 - index;

        if (index < 0)
        {
            m_subSpan = new Vector2Int(m_startDate, m_endDate);
        }
        else
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            int yearStart = (startDate - WeatherDataset.START_DATE).Days;
            int yearEnd = (endDate - WeatherDataset.START_DATE).Days;

            m_subSpan = new Vector2Int(
                Mathf.Max(m_startDate, yearStart),
                Mathf.Min(m_endDate, yearEnd)
            );
        }

        m_startTxt.SetText(WeatherDataset.START_DATE.AddDays(m_subSpan.x).ToLongDateString());
        m_endTxt.SetText(WeatherDataset.START_DATE.AddDays(m_subSpan.y).ToLongDateString());

        SetTime(0f);
    }
    
    public void SetTime(float percentage)
    {
        // Map percentage based on filter

        percentage = Mathf.Max(0f, Mathf.Min(percentage, 100f));
        CurrentPercentage = percentage;

        int len = m_subSpan.y - m_subSpan.x;

        int date = m_subSpan.x + (int)(percentage * len / 100f);
        var dateTime = WeatherDataset.START_DATE.AddDays(date);

        m_currTxt.SetText($"{dateTime.Day:00}/{dateTime.Month:00}/{dateTime.Year:0000}");

        float width = m_parent.rect.width * 0.01f;

        m_knob.anchoredPosition = new Vector2(percentage * width, 0);
        m_bar.sizeDelta = new Vector2(percentage * width, m_bar.sizeDelta.y);

        float actualPercentage = (date - m_startDate) / (float)(m_lengthDate * 0.01f);

        OnTimeMachineUpdate?.Invoke(actualPercentage);
    }
}
