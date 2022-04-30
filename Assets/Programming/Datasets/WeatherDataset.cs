using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using RewindSystem;

public struct WeatherState
{
    public int Date; // 1

    public DateTime ActualDate => new DateTime(2012, 1, 1).AddDays(Date);

    public float TemperatureMin24h;

    public float TemperatureMax24h;

    public float HumidityMax24h;

    public float HumidityMin24h;

    public float TotalNebulosity; // 14

    public float SnowLayerHeight; // 35

    public float RainPast24h; // 42

    public string depId; // 78
}

public class WeatherDataset : Dataset
{
    public static readonly DateTime START_DATE = new DateTime(2010, 1, 1);

    [SerializeField, Provides] WeatherDataset me;

    [SerializeField] TextAsset m_dataset;

    [SerializeField, Header("Used for importing")] DepartmentDataset m_departments;

    [Inject] TimeMachine m_timecontroller;

    Dictionary<string, Rewind<WeatherState>> m_timemachine = new Dictionary<string, Rewind<WeatherState>>();
    
    WeatherState minWeather, maxWeather;

    float m_timemachineLength = 0;
    
    private void Awake()
    {
        minWeather = new WeatherState{
            HumidityMax24h = int.MaxValue,
            HumidityMin24h = int.MaxValue,
            TemperatureMax24h = int.MaxValue,
            TemperatureMin24h = int.MaxValue,
            TotalNebulosity = int.MaxValue,
            SnowLayerHeight = int.MaxValue,
            RainPast24h = int.MaxValue,
        };

        maxWeather = new WeatherState{
            HumidityMax24h = int.MinValue,
            HumidityMin24h = int.MinValue,
            TemperatureMax24h = int.MinValue,
            TemperatureMin24h = int.MinValue,
            TotalNebulosity = int.MinValue,
            SnowLayerHeight = int.MinValue,
            RainPast24h = int.MinValue,
        };

        BinaryReader br = new BinaryReader(new MemoryStream(m_dataset.bytes));

        int startDate = int.MaxValue;
        int endDate = 0;
    
        int entries = br.ReadInt32();

        List<WeatherState> allStates = new List<WeatherState>();

        for (int i = 0; i < entries; ++i)
        {
            int subentries = br.ReadInt32();

            for (int j = 0; j < subentries; ++j)
            {
                int date = br.ReadInt32();

                startDate = Mathf.Min(startDate, date);
                endDate = Mathf.Max(endDate, date);

                WeatherState weather = new WeatherState() {
                    Date = date,
                    depId = br.ReadString().TrimStart('0'),
                    HumidityMax24h = br.ReadSingle(),
                    HumidityMin24h = br.ReadSingle(),
                    RainPast24h = br.ReadSingle(),
                    TemperatureMax24h = br.ReadSingle(),
                    TemperatureMin24h = br.ReadSingle(),
                    SnowLayerHeight = br.ReadSingle(),
                    TotalNebulosity = br.ReadSingle(),
                };

                minWeather = new WeatherState
                {
                    HumidityMax24h = Mathf.Min(weather.HumidityMax24h, minWeather.HumidityMax24h),
                    HumidityMin24h = Mathf.Min(weather.HumidityMin24h, minWeather.HumidityMin24h),
                    TemperatureMax24h = Mathf.Min(weather.TemperatureMax24h, minWeather.TemperatureMax24h),
                    TemperatureMin24h = Mathf.Min(weather.TemperatureMin24h, minWeather.TemperatureMin24h),
                    TotalNebulosity = Mathf.Min(weather.TotalNebulosity, minWeather.TotalNebulosity),
                    SnowLayerHeight = Mathf.Min(weather.SnowLayerHeight, minWeather.SnowLayerHeight),
                    RainPast24h = Mathf.Min(weather.RainPast24h, minWeather.RainPast24h),
                };

                maxWeather = new WeatherState
                {
                    HumidityMax24h = Mathf.Max(weather.HumidityMax24h, maxWeather.HumidityMax24h),
                    HumidityMin24h = Mathf.Max(weather.HumidityMin24h, maxWeather.HumidityMin24h),
                    TemperatureMax24h = Mathf.Max(weather.TemperatureMax24h, maxWeather.TemperatureMax24h),
                    TemperatureMin24h = Mathf.Max(weather.TemperatureMin24h, maxWeather.TemperatureMin24h),
                    TotalNebulosity = Mathf.Max(weather.TotalNebulosity, maxWeather.TotalNebulosity),
                    SnowLayerHeight = Mathf.Max(weather.SnowLayerHeight, maxWeather.SnowLayerHeight),
                    RainPast24h = Mathf.Max(weather.RainPast24h, maxWeather.RainPast24h),
                };

                allStates.Add(weather);

                if (!m_timemachine.ContainsKey(weather.depId))
                    m_timemachine.Add(weather.depId, new Rewind<WeatherState>());
            }
        }

        m_timemachineLength = endDate - startDate;

        foreach(var weather in allStates)
        {
            float time = (weather.Date - startDate) / (m_timemachineLength * 0.01f);

            var machine = m_timemachine[weather.depId];

            machine.RegisterFrame(weather, time);
        }

        m_timecontroller.UpdateRange(startDate, endDate);
    
        allStates.Clear();

        br.Close();
    }

    [ContextMenu("Bake")]
    private void Bake()
    {
        m_departments.Load();

        string resultPath = Application.dataPath + "/Programming/Datasets/WeatherDataset-2.bytes";
        //string inputPath = Application.dataPath + "/../donnees-synop-essentielles-omm.bytes";
        string inputPath = Application.dataPath + "/../meteo.bytes";
        
        StringBuilder stringBuilder = new StringBuilder();
        
        BinaryReader br = new BinaryReader(new FileStream(inputPath, FileMode.Open));
        BinaryWriter bw = new BinaryWriter(new FileStream(resultPath, FileMode.Create));

        Dictionary<string, Dictionary<int, WeatherState>> m_data = new Dictionary<string, Dictionary<int, WeatherState>>();
        long filelen = br.BaseStream.Length;

        string ReadLine()
        {
            stringBuilder.Clear();

            while(br.BaseStream.Position < filelen)
            {
                var c = br.ReadChar();
                if (c == '\n') break;

                stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        var header = ReadLine();
        var headers = header.Split(';');
        int headerIdx = 0;

        foreach(var h in headers) Debug.Log($"{headerIdx++} : {h}");

        DateTime highestDate = default;

        while (true)
        {
            string line = ReadLine();

            if (string.IsNullOrWhiteSpace(line)) break;
            
            var rows = line.Split(';');
            
            if (rows.Length != headers.Length) break;

            WeatherState weather = new WeatherState();

            string CurateNumber(string n)
            {
                if (string.IsNullOrWhiteSpace(n)) return "0";
    
                var v = n.Split(' ');
                if (v.Length > 0) return v[0];
                else return string.Empty;
            }

            double lat = double.Parse(rows[70]);
            double lon = double.Parse(rows[71]);


            weather.depId = m_departments.GetClosestDepartment(new Mapbox.Utils.Vector2d(lat, lon));

            var date = DateTime.Parse(rows[1]);

            if (date > highestDate) highestDate = date;

            var diff = date - START_DATE;

            weather.Date = diff.Days;

            if (weather.Date < 0) continue;

            weather.RainPast24h = float.Parse(CurateNumber(rows[42]));
            weather.SnowLayerHeight = float.Parse(CurateNumber(rows[35]));
            weather.TotalNebulosity = float.Parse(CurateNumber(rows[14]));

            float Humidity = float.Parse(CurateNumber(rows[9]));
            float Temperature = float.Parse(CurateNumber(rows[64]));

            weather.TemperatureMax24h = Temperature;
            weather.TemperatureMin24h = Temperature;

            weather.HumidityMax24h = Humidity;
            weather.HumidityMin24h = Humidity;


            if (!m_data.ContainsKey(weather.depId)) m_data[weather.depId] = new Dictionary<int, WeatherState>();

            var localData = m_data[weather.depId];

            if (!localData.ContainsKey(weather.Date)) localData.Add(weather.Date, weather);

            var curr = localData[weather.Date];

            curr.TemperatureMin24h = Mathf.Min(Temperature, curr.TemperatureMin24h);
            curr.TemperatureMax24h = Mathf.Max(Temperature, curr.TemperatureMax24h);

            curr.HumidityMin24h = Mathf.Min(Humidity, curr.HumidityMin24h);
            curr.HumidityMax24h = Mathf.Max(Humidity, curr.HumidityMax24h);

            curr.SnowLayerHeight = Mathf.Max(curr.SnowLayerHeight, weather.SnowLayerHeight);
        }

        bw.Write(m_data.Count);
        foreach(var local in m_data)
        {
            bw.Write(local.Value.Count);
            foreach(var d in local.Value)
            {
                bw.Write(d.Value.Date);
                bw.Write(d.Value.depId);
                bw.Write(d.Value.HumidityMax24h);
                bw.Write(d.Value.HumidityMin24h);
                bw.Write(d.Value.RainPast24h);
                bw.Write(d.Value.TemperatureMax24h);
                bw.Write(d.Value.TemperatureMin24h);
                bw.Write(d.Value.SnowLayerHeight);
                bw.Write(d.Value.TotalNebulosity);
            }
        }

        bw.Close();
        br.Close();
    }

    public bool GetWeather(string departmentId, float timep100, out WeatherState value)
    {
        value = default;
        if (!m_timemachine.ContainsKey(departmentId)) return false;
        value = m_timemachine[departmentId].GetFrame(timep100);
        return true;
    }

    readonly DatasetProp[] m_properties = new DatasetProp[] {
        new DatasetProp {Value = "TemperatureMin" },
        new DatasetProp {Value = "TemperatureMax" },
        new DatasetProp {Value = "Rain" },
        new DatasetProp {Value = "HumidityMin" },
        new DatasetProp {Value = "HumidityMax" },
        new DatasetProp {Value = "Nebulosity" },
        new DatasetProp {Value = "SnowHeight" },
    };

    public override DatasetProp[] GetDataProperties()
    {
        return m_properties;
    }

    public override bool GetData(int postalCode, string property, float time, out float value)
    {
        value = 0f;

        if (!GetWeather(GetDepartmentID(postalCode), time, out var weather)) return false;

        switch (property)
        {
            case "TemperatureMin": value = weather.TemperatureMin24h; return true;
            case "TemperatureMax": value = weather.TemperatureMax24h; return true;
            case "HumidityMin": value = weather.HumidityMin24h; return true;
            case "HumidityMax": value = weather.HumidityMax24h; return true;
            case "Rain": value = weather.RainPast24h; return true;
            case "Nebulosity": value = weather.TotalNebulosity; return true;
            case "SnowHeight": value = weather.SnowLayerHeight; return true;
        }

        return false;
    }

    public override float GetMinPossibleValue(string property)
    {
        var value = 0f;
        var weather = minWeather;

        switch (property)
        {
            case "TemperatureMin": value = weather.TemperatureMin24h; break;
            case "TemperatureMax": value = weather.TemperatureMax24h; break;
            case "HumidityMin": value = weather.HumidityMin24h; break;
            case "HumidityMax": value = weather.HumidityMax24h; break;
            case "Rain": value = weather.RainPast24h; break;
            case "Nebulosity": value = weather.TotalNebulosity; break;
            case "SnowHeight": value = weather.SnowLayerHeight; break;
        }

        return value;
    }

    public override float GetMaxPossibleValue(string property)
    {
        var value = 0f;
        var weather = maxWeather;

        switch (property)
        {
            case "TemperatureMin": value = weather.TemperatureMin24h; break;
            case "TemperatureMax": value = weather.TemperatureMax24h; break;
            case "HumidityMin": value = weather.HumidityMin24h; break;
            case "HumidityMax": value = weather.HumidityMax24h; break;
            case "Rain": value = weather.RainPast24h; break;
            case "Nebulosity": value = weather.TotalNebulosity; break;
            case "SnowHeight": value = weather.SnowLayerHeight; break;
        }
        
        return value;
    }
}
