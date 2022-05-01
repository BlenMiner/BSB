using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts;
using XCharts.Runtime;

public class LineChartTest : MonoBehaviour
{

    int[] data_paris = {23, 16, 43, 22 };
    // Start is called before the first frame update
    void Start()
    {
        var chart = gameObject.GetComponent<LineChart>();
        if (chart == null)
        {
            chart = gameObject.AddComponent<LineChart>();
            chart.Init();
        }
        var title = chart.GetOrAddChartComponent<Title>(); 
        title.text = " Gurpal Line ";

        chart.RemoveData();
        chart.AddSerie<Line>("line");
        chart.AddXAxisData("no2");
        chart.AddXAxisData("o3");
        chart.AddXAxisData("pm10");
        chart.AddXAxisData("weather");
        chart.AddData(0, 15.5d);
        chart.AddData(0, 23.5f);
        chart.AddData(0, data_paris[2]);
        chart.AddData(0, data_paris[3]);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
