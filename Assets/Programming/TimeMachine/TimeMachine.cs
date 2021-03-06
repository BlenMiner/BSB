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

    [SerializeField] RectTransform m_knobB;

    [SerializeField] TMP_Text m_startTxt;

    [SerializeField] TMP_Text m_currTxt;

    [SerializeField] TMP_Text m_endTxt;

    [SerializeField] TMP_Dropdown m_filter;

    public event Action<float, float> OnTimeMachineUpdate;

    public float CurrentPercentage {get; private set;}

    public float SnapshotPercentage {get; private set; }

    private int m_startDate;

    private int m_endDate;

    private int m_lengthDate;

    public int StartDate => m_startDate;

    public int LengthDate => m_lengthDate;

    private Vector2Int m_subSpan;

    private void Awake()
    {
        m_currTxt.SetText(string.Empty);

        SetSnapshot(0f);
        SetTime(0f);
    }

    private void OnGUI()
    {
        float increment = 100f / (m_subSpan.y - m_subSpan.x);
        var e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftArrow)
            SetTime(CurrentPercentage - increment);
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.RightArrow)
            SetTime(CurrentPercentage + increment);
    }

    float GetPercentage(PointerEventData data) => 
        (data.position.x - 100f) / (m_parent.rect.width * 0.01f);

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            SetSnapshot(GetPercentage(eventData));
        else SetTime(GetPercentage(eventData));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            SetSnapshot(GetPercentage(eventData));
        else SetTime(GetPercentage(eventData));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            SetSnapshot(GetPercentage(eventData));
        else SetTime(GetPercentage(eventData));
    }

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
        int select = 0;

        for (int i = 2022; i >= startYear; --i)
        {
            if (i < 2016) ++select;
            m_filter.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }

        m_filter.onValueChanged.AddListener(OnFilterChanged);
        m_filter.value = select;

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

    public DateTime GetTime(float percentage)
    {
        int len = m_subSpan.y - m_subSpan.x;

        int date = m_subSpan.x + (int)(percentage * len / 100f);
        var dateTime = WeatherDataset.START_DATE.AddDays(date);

        return dateTime;
    }

    public float GlobalToLocal(float percentage)
    {
        int len = m_subSpan.y - m_subSpan.x;
        int date = m_subSpan.x + (int)(percentage * len / 100f);
        return (date - m_startDate) / (float)(m_lengthDate * 0.01f);
    }

    public void SetSnapshot(float percentage)
    {
        percentage = Mathf.Max(0f, Mathf.Min(percentage, 100f));
        if (Mathf.Abs(percentage - CurrentPercentage) < 2f) percentage = CurrentPercentage;

        SnapshotPercentage = percentage;

        float width = m_parent.rect.width * 0.01f;
        m_knobB.anchoredPosition = new Vector2(percentage * width, 0);

        SetTime(CurrentPercentage);
    }
    
    public void SetTime(float percentage)
    {
        // Map percentage based on filter

        percentage = Mathf.Max(0f, Mathf.Min(percentage, 100f));
        CurrentPercentage = percentage;

        int len = m_subSpan.y - m_subSpan.x;

        int date = m_subSpan.x + (int)(percentage * len / 100f);
        int dateB = m_subSpan.x + (int)(SnapshotPercentage * len / 100f);
        var dateTime = WeatherDataset.START_DATE.AddDays(date);

        m_currTxt.SetText($"{dateTime.Day:00}/{dateTime.Month:00}/{dateTime.Year:0000}");

        float width = m_parent.rect.width * 0.01f;

        m_knob.anchoredPosition = new Vector2(percentage * width, 0);
        m_bar.sizeDelta = new Vector2(percentage * width, m_bar.sizeDelta.y);

        float actualPercentage = (date - m_startDate) / (float)(m_lengthDate * 0.01f);
        float actualSPercentage = (dateB - m_startDate) / (float)(m_lengthDate * 0.01f);

        OnTimeMachineUpdate?.Invoke(actualPercentage, actualSPercentage);
    }
}
