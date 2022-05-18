using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

public class LineChartScript : MonoBehaviour
{
    public void UpdateGraph(DateTime start, DateTime end, float[][] data, string[] label)
    {
        var chart = gameObject.GetComponent<LineChart>();
        if (chart == null)
        {
            chart = gameObject.AddComponent<LineChart>();
            chart.Init();
        }
        var title = chart.GetOrAddChartComponent<Title>();
        title.text = "Données jusqu'au " + end.ToString().Substring(0, 5);

        var xAxis = chart.GetOrAddChartComponent<XAxis>();
        xAxis.boundaryGap = true;
        xAxis.type = Axis.AxisType.Category;

        var yAxis = chart.GetOrAddChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;

        chart.RemoveData();
        chart.AddSerie<Line>("weather");
        chart.AddSerie<Line>("pm10");

        int i = 0;
        DateTime futureDate = new DateTime(2022, 6, 29, 19, 30, 52);
        for (DateTime date = start; end.CompareTo(date) > 0; date = date.AddDays(1.0))
        {
            chart.AddXAxisData(date.ToString().Substring(0, 5));
            chart.AddData("weather", data[0][i]);
            chart.AddData("pm10", data[1][i]);
            i++;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        DateTime date = DateTime.Now;
        DateTime futureDate = new DateTime(2022, 5, 22, 19, 30, 52);
        float[][] data = new float[][]
        {
            new float[] {23.5f, 24.0f, 28.3f, 22.0f, 20.0f },
            new float[] {5f, 7f, 18f, 7f, 4f}
        };
        UpdateGraph(date, futureDate, data, null);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
