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
        for (int j = 0; j < label.Length; j++)
        {
            chart.AddSerie<Line>(label[j]);
        }

        


        DateTime date = start;
        while (end.CompareTo(date) > 0)
        {
            chart.AddXAxisData(date.ToString().Substring(0, 5));
            date = date.AddDays(1);
        }

        int i = 0;
        while (i < data.Length)
        {
            for (int j = 0; j < data[i].Length; j++)
                chart.AddData(label[i], data[i][j]);
            i += 1;
        } 
    }
    // Start is called before the first frame update
    void Start()
    {
        DateTime date = DateTime.Now;
        DateTime futureDate = new DateTime(2022, 5, 25, 23, 59, 59);
        String[] label = { "weather", "pm10", "o3" };
        float[][] data = new float[][]
        {
            new float[] {23.5f, 24.0f, 28.3f, 22.0f, 20.0f },
            new float[] {5f, 7f, 18f, 7f, 4f},
            new float[] {11f, 12f, 13f, 14f, 15f}
        };
        UpdateGraph(date, futureDate, data, label);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
