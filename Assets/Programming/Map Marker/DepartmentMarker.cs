using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.UI;

public class DepartmentMarker : MonoBehaviour
{
    [SerializeField] Graphic m_shape;

    [SerializeField] Vector2 m_hue;

    [Inject] TimeMachine m_timeMachine;

    [Inject] WeatherDataset m_weather;

    [Inject] CrimeDataset m_crime;

    AbstractMap m_map;

    Vector2d m_pos;

    string m_depId;

    public Vector2d LongLat => m_pos;

    public void Setup(AbstractMap map, Vector2d longLat, string departmentId)
    {
        m_shape.color = Random.ColorHSV(m_hue.x, m_hue.y, 0.5f, 0.5f, 1, 1);

        m_map = map;
        m_pos = longLat;
        m_depId = departmentId;
    }

    private void Start()
    {
        TimeUpdated(m_timeMachine.CurrentPercentage, m_timeMachine.SnapshotPercentage);
    }

    private void OnEnable()
    {
        m_timeMachine.OnTimeMachineUpdate += TimeUpdated;
    }

    private void TimeUpdated(float percentage100, float snapshot)
    {
        if (m_crime.GetTotalCrime(m_depId, percentage100, out var crime))
        {
            float scale = crime.Count * 0.0005f;
            m_shape.rectTransform.sizeDelta = new Vector2(10, 10) * scale;
        }
        else
        {
            m_shape.rectTransform.sizeDelta = new Vector2(10, 10);
        }

        if (m_weather.GetWeather(m_depId, percentage100, out var weather))
        {
            float t = weather.TemperatureMax24h;

            t += 20f;
            t /= 60f;

            var c = Color.Lerp(Color.cyan, Color.red, Mathf.Min(1f, Mathf.Max(0, t)));
            m_shape.color = c;
        }
        else
        {
            var c = Color.white;
            m_shape.color = c;
        }
    }

    private void OnDisable()
    {
        m_timeMachine.OnTimeMachineUpdate -= TimeUpdated;
    }
}
