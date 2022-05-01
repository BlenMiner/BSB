using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts;
using XCharts.Runtime;

public class BarChartTest : MonoBehaviour
{
    string ville = "Paris";
    string date = "17/01/2017";
    float[] data_paris = { 23.5f, 16, 43.4f, 22 };
    // Start is called before the first frame update
    void Start()
    {
        var chart = gameObject.GetComponent<BarChart>();
        if (chart == null)
        {
            chart = gameObject.AddComponent<BarChart>();
            chart.Init();
        }
        var title = chart.GetOrAddChartComponent<Title>();
        title.text = ville + " " + date;
        chart.RemoveData();
        chart.AddSerie<Bar>("line");

        chart.AddXAxisData("no2");
        chart.AddXAxisData("o3");
        chart.AddXAxisData("pm10");
        chart.AddXAxisData("weather");
        chart.AddData("line", data_paris[0]);
        chart.AddData("line", data_paris[1]);
        chart.AddData("line", data_paris[2]);
        chart.AddData("line", data_paris[3]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
