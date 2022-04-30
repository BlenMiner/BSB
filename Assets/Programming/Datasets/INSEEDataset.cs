using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mapbox.Utils;
using UnityEngine;

public struct INSEE
{
    public int ID;

    public int PostalCode;

    public string DepId;

    public Vector2d LonLat;
}

public class INSEEDataset : MonoBehaviour
{
    public static INSEEDataset me;

    [SerializeField, Provides] INSEEDataset m_provider;

    [SerializeField] TextAsset m_dataset;

    Dictionary<int, INSEE> m_INSEELookup = new Dictionary<int, INSEE>();

    Dictionary<int, INSEE> m_postalLookup = new Dictionary<int, INSEE>();

    Dictionary<string, INSEE> m_depIdLookup = new Dictionary<string, INSEE>();

    private void Awake()
    {
        me = this;
        using BinaryReader br = new BinaryReader(new MemoryStream(m_dataset.bytes));
        
        int count = br.ReadInt32();

        for (int i = 0; i < count; ++i)
        {
            INSEE v = new INSEE
            {
                ID = br.ReadInt32(),
                PostalCode = br.ReadInt32(),
                DepId = br.ReadString(),
                LonLat = new Vector2d(br.ReadDouble(), br.ReadDouble())
            };

            m_INSEELookup.Add(v.ID, v);

            if (!m_postalLookup.ContainsKey(v.PostalCode))
                m_postalLookup.Add(v.PostalCode, v);
            
            if (!m_depIdLookup.ContainsKey(v.DepId))
                m_depIdLookup.Add(v.DepId, v);
        }
    }

    public void MapDepCoords(System.Action<int, Vector2d> cb)
    {
        foreach(var v in m_INSEELookup)
            cb(v.Key, v.Value.LonLat);
    }

    public bool GetINSEE(int INSEE, out INSEE val)
    {
        return m_INSEELookup.TryGetValue(INSEE, out val);
    }

    public bool GetINSEE_PostalCode(int postalCode, out INSEE val)
    {
        return m_postalLookup.TryGetValue(postalCode, out val);
    }

    public bool GetINSEE(string depid, out INSEE val)
    {
        return m_depIdLookup.TryGetValue(depid, out val);
    }


    [ContextMenu("Bake")]
    private void Bake()
    {
        string resultPath = Application.dataPath + "/Programming/Datasets/INSEEDataset.bytes";
        string inputPath = Application.dataPath + "/../insee-postal-dep.csv";
        
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

        List<INSEE> m_inseeStuff = new List<INSEE>();

        while (true)
        {
            string line = ReadLine();

            if (string.IsNullOrWhiteSpace(line)) break;
            
            var rows = line.Split(';');
            
            if (rows.Length != headers.Length) break;

            string CurateNumber(string n)
            {
                if (string.IsNullOrWhiteSpace(n)) return "0";
    
                var v = n.Split(' ');
                if (v.Length > 0) return v[0];
                else return string.Empty;
            }

            string[] coors = rows[9].Split(',');

            try
            {
                INSEE insee = new INSEE
                {
                    ID = int.Parse(rows[0]),
                    PostalCode = int.Parse(rows[1]),
                    DepId = rows[15],
                    LonLat = new Vector2d(
                        double.Parse(coors[0]),
                        double.Parse(coors[1])
                    ),
                };
                m_inseeStuff.Add(insee);
            }
            catch
            {
                Debug.LogError($"{rows[0]}; {rows[1]}; {rows[15]}; {rows[9]}");
            }
        }

        bw.Write(m_inseeStuff.Count);
        foreach(var local in m_inseeStuff)
        {
            bw.Write(local.ID);
            bw.Write(local.PostalCode);
            bw.Write(local.DepId);
            bw.Write(local.LonLat.x);
            bw.Write(local.LonLat.y);
        }

        bw.Close();
        br.Close();
    }
}
