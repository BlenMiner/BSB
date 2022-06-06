using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

public class LineChartScript : MonoBehaviour
{
    [SerializeField] LineChart m_chart;

    public void UpdateTitle(string value)
    {
        var title = m_chart.GetOrAddChartComponent<Title>();
        title.text = value;
    }

    public LineChart UpdateGraph(DateTime start, DateTime end, DateTime a, DateTime b, float[][] data, string[] label)
    {
        var chart = m_chart;

        var xAxis = chart.GetOrAddChartComponent<XAxis>();
        xAxis.boundaryGap = true;
        xAxis.type = Axis.AxisType.Time;

        var yAxis = chart.GetOrAddChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;

        chart.RemoveData();

        var duration = end - start;

        for (int i = 0; i < label.Length; ++i)
        {
            var line = chart.AddSerie<Line>(label[i]);
            line.symbol.show = false;

            int dataLen = data[i].Length;

            for (int j = 0; j < dataLen; j++)
            {
                double percentage = j / (dataLen - 1.0);

                DateTime date = start.AddTicks((long)(duration.Ticks * percentage));

                chart.AddData(label[i], date, data[i][j]);
                // chart.AddXAxisData(date.ToShortDateString(), i);
            }
        }

        // chart.InitAxisRuntimeData(xAxis);

        return chart;
    }
}
