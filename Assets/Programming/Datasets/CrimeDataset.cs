#if UNIT_EDITOR
using NanoXLSX;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using RewindSystem;

[System.Serializable]
public struct CrimeState
{
    public string depId;

    public int TypeID;

    public int Count;
}

public class CrimeDataset : Dataset
{
    [SerializeField, Provides] CrimeDataset m_provider;

    [SerializeField] TextAsset m_dataset;

    [Inject] TimeMachine m_timeMachine;

    Dictionary<string, Dictionary<int, Dictionary<int, CrimeState>>> m_data = 
        new Dictionary<string, Dictionary<int, Dictionary<int, CrimeState>>>();

    Dictionary<string, Rewind<CrimeState>> m_totalCrime = new Dictionary<string, Rewind<CrimeState>>();

    Dictionary<string, Dictionary<int, Rewind<CrimeState>>> m_specificCrime
         = new Dictionary<string, Dictionary<int, Rewind<CrimeState>>>();

    List<int> m_minPerType = new List<int>();

    List<int> m_maxPerType = new List<int>();

    private void Awake()
    {
        BinaryReader br = new BinaryReader(new MemoryStream(m_dataset.bytes));

        int count = br.ReadInt32();

        for (int i = 0; i < count; ++i)
        {
            string depId = br.ReadString().TrimStart('0');
            var time = new Rewind<CrimeState>();

            m_totalCrime.Add(depId, time);

            if (!m_data.ContainsKey(depId)) m_data.Add(depId, new Dictionary<int, Dictionary<int, CrimeState>>());
    
            var depData = m_data[depId];

            int jcount = br.ReadInt32();

            for (int j = 0; j < jcount; ++j)
            {
                int date = br.ReadInt32();
                int kcount = br.ReadInt32();

                if (!depData.ContainsKey(date)) depData.Add(date, new Dictionary<int, CrimeState>());
                if (!depData.ContainsKey(date)) depData.Add(date, new Dictionary<int, CrimeState>());

                var dateData = depData[date];

                int total = 0;

                for (int k = 0; k < kcount; ++k)
                {
                    var crime = new CrimeState {
                        depId = br.ReadString(),
                        TypeID = br.ReadInt32(),
                        Count = br.ReadInt32()
                    };

                    while (m_minPerType.Count <= crime.TypeID) m_minPerType.Add(int.MaxValue);
                    while (m_maxPerType.Count <= crime.TypeID) m_maxPerType.Add(int.MinValue);

                    m_minPerType[crime.TypeID] = Mathf.Min(crime.Count, m_minPerType[crime.TypeID]);
                    m_maxPerType[crime.TypeID] = Mathf.Max(crime.Count, m_maxPerType[crime.TypeID]);

                    total += crime.Count;

                    dateData.Add(crime.TypeID, crime);
                }

                float percentage = (date - m_timeMachine.StartDate) / (m_timeMachine.LengthDate * 0.01f);

                while (m_minPerType.Count < 1) m_minPerType.Add(int.MaxValue);
                while (m_maxPerType.Count < 1) m_maxPerType.Add(int.MinValue);

                m_minPerType[0] = Mathf.Min(total, m_minPerType[0]);
                m_maxPerType[0] = Mathf.Max(total, m_maxPerType[0]);

                time.RegisterFrame(new CrimeState {
                    Count = total,
                    depId = depId,
                    TypeID = -1
                }, percentage);
            }
        }

        br.Close();

        foreach(var depId in m_data)
        {
            string departmentId = depId.Key;

            if (!m_specificCrime.ContainsKey(departmentId)) 
                m_specificCrime.Add(departmentId, new Dictionary<int, Rewind<CrimeState>>());
            
            var dates = m_specificCrime[departmentId];

            foreach(var crimeDate in depId.Value)
            {
                int date = crimeDate.Key;
                float percentage = (date - m_timeMachine.StartDate) / (m_timeMachine.LengthDate * 0.01f);
                foreach(var crimeValue in crimeDate.Value)
                {
                    var crime = crimeValue.Value;
                    
                    if (!dates.ContainsKey(crime.TypeID)) dates.Add(crime.TypeID, new Rewind<CrimeState>());
                
                    var timeLine = dates[crime.TypeID];

                    timeLine.RegisterFrame(crime, percentage);
                }
            }
        }
    }

    public bool GetTotalCrime(string depId, float time, out CrimeState result)
    {
        result = default;
        if (m_totalCrime.TryGetValue(depId, out var v))
        {
            result = v.GetFrame(time);
            return true;
        }
        else return false;
    }

    public bool GetCrime(string depId,  int crimeId, float time, out CrimeState result)
    {
        result = default;
        if (m_specificCrime.TryGetValue(depId, out var v))
        {
            if (v.TryGetValue(crimeId, out var timeline))
            {
                result = timeline.GetFrame(time);
                return true;
            }
            else 
            {
                Debug.Log($"Failed to get crime id {crimeId}");
                return false;
            }
        }
        else 
        {
            Debug.Log($"Failed to get dep {depId}");
            return false;
        }
    }

    [ContextMenu("Bake")]
    void Bake()
    {
#if UNIT_EDITOR
        string resultPath = Application.dataPath + "/Programming/Datasets/CrimeDataset.bytes";
        string inputPath = Application.dataPath + "/../tableaux-4001-ts.xlsx";

        Workbook wb = Workbook.Load("tableaux-4001-ts.xlsx");

        Dictionary<string, Dictionary<int, Dictionary<int, CrimeState>>> data = new
        Dictionary<string, Dictionary<int, Dictionary<int, CrimeState>>>();

        for(int i = 2; i < wb.Worksheets.Count; ++i)
        {
            var sheet = wb.Worksheets[i];
            string depId = sheet.SheetName;

            if (!data.ContainsKey(depId)) data.Add(depId, new Dictionary<int, Dictionary<int, CrimeState>>());

            var departmentData = data[depId];

            for (int c = 2; c < 315; ++c)
            {
                if (!sheet.HasCell(c, 0)) break;

                string dateStr = sheet.GetCell(c, 0).Value.ToString();
                dateStr = dateStr.Trim('_');
                var dataSplt = dateStr.Split('_');

                int year = int.Parse(dataSplt[0]);
                int month = int.Parse(dataSplt[1]);

                DateTime date = new DateTime(year, month, 1);
                int dateNumber = (date - WeatherDataset.START_DATE).Days;

                if (!departmentData.ContainsKey(dateNumber)) departmentData.Add(dateNumber, new Dictionary<int, CrimeState>());

                var dateCrimes = departmentData[dateNumber]; 

                for (int r = 1; r < 108; ++r)
                {
                    int crimeId = int.Parse(sheet.GetCell(0, r).Value.ToString());
                    int crimeCount = int.Parse(sheet.GetCell(c, r).Value.ToString());

                    CrimeState crime = new CrimeState{
                        Count = crimeCount,
                        TypeID = crimeId,
                        depId = depId
                    };

                    if (!dateCrimes.ContainsKey(crimeId)) dateCrimes.Add(crimeId, crime);
                    else
                    {
                        var existingCrime = dateCrimes[crimeId];
                        existingCrime.Count += crime.Count;
                        dateCrimes[crimeId] = existingCrime;
                    }
                }
            }
            sheet.GetCell(0, 0);
        }
    
        BinaryWriter bw = new BinaryWriter(new FileStream(resultPath, FileMode.Create));

        bw.Write(data.Count);

        foreach(var v in data)
        {
            bw.Write(v.Key); // Department
            bw.Write(v.Value.Count);

            foreach(var d in v.Value)
            {
                bw.Write(d.Key); // Date
                bw.Write(d.Value.Count);

                foreach(var c in d.Value)
                {
                    var crime = c.Value;
                    bw.Write(crime.depId); // Crime data
                    bw.Write(crime.TypeID); // Crime data
                    bw.Write(crime.Count); // Crime data
                }
            }
        }

        bw.Close();
#endif
    }

    readonly string[] m_crimeIdName = new string[] {
        "Total",
        "Règlements de compte entre malfaiteurs",
        "Homicides pour voler et à l'occasion de vols",
        "Homicides pour d'autres motifs",
        "Tentatives d'homicides pour voler et à l'occasion de vols",
        "Tentatives homicides pour d'autres motifs",
        "Coups et blessures volontaires suivis de mort",
        "Autres coups et blessures volontaires criminels ou correctionnels",
        "Prises d'otages à l'occasion de vols",
        "Prises d'otages dans un autre but",
        "Sequestrations",
        "Menaces ou chantages pour extorsion de fonds",
        "Menaces ou chantages dans un autre but",
        "Atteintes à la dignité et à la personnalité",
        "Violations de domicile",
        "Vols à main armée contre des établissements financiers",
        "Vols à main armée contre des établissements industriels ou commerciaux",
        "Vols à main armée contre des entreprises de transports de fonds",
        "Vols à main armée contre des particuliers à leur domicile",
        "Autres vols à main armée",
        "Vols avec armes blanches contre des établissements financiers,commerciaux ou industriels",
        "Vols avec armes blanches contre des particuliers à leur domicile",
        "Autres vols avec armes blanches",
        "Vols violents sans arme contre des établissements financiers,commerciaux ou industriels",
        "Vols violents sans arme contre des particuliers à leur domicile",
        "Vols violents sans arme contre des femmes sur voie publique ou autre lieu public",
        "Vols violents sans arme contre d'autres victimes",
        "Cambriolages de locaux d'habitations principales",
        "Cambriolages de résidences secondaires",
        "Cambriolages de locaux industriels, commerciaux ou financiers",
        "Cambriolages d'autres lieux",
        "Vols avec entrée par ruse en tous lieux",
        "Vols à la tire",
        "Vols à l'étalage",
        "Vols de véhicules de transport avec frêt",
        "Vols d'automobiles",
        "Vols de véhicules motorisés à 2 roues",
        "Vols à la roulotte",
        "Vols d'accessoires sur véhicules à moteur immatriculés",
        "Vols simples sur chantier",
        "Vols simples sur exploitations agricoles",
        "Autres vols simples contre des établissements publics ou privés",
        "Autres vols simples contre des particuliers dans deslocaux privés",
        "Autres vols simples contre des particuliers dans des locaux ou lieux publics",
        "Recels",
        "Proxénétisme",
        "Viols sur des majeur(e)s",
        "Viols sur des mineur(e)s",
        "Harcèlements sexuels et autres agressions sexuelles contre des majeur(e)s",
        "Harcèlements sexuels et autres agressions sexuelles contre des mineur(e)s",
        "Atteintes sexuelles",
        "Homicides commis contre enfants de moins de 15 ans",
        "Violences, mauvais traitements et abandons d'enfants.",
        "Délits au sujet de la garde des mineurs",
        "Non versement de pension alimentaire",
        "Trafic et revente sans usage de stupéfiants",
        "Usage-revente de stupéfiants",
        "Usage de stupéfiants",
        "Autres infractions à la législation sur les stupéfiants",
        "Délits de débits de boissons et infraction à la règlementation sur l'alcool et le tabac",
        "Fraudes alimentaires et infractions à l'hygiène",
        "Autres délits contre santé publique et la réglementation des professions médicales",
        "Incendies volontaires de biens publics",
        "Incendies volontaires de biens privés",
        "Attentats à l'explosif contre des biens publics",
        "Attentats à l'explosif contre des biens privés",
        "Autres destructions er dégradations de biens publics",
        "Autres destructions er dégradations de biens privés",
        "Destructions et dégradations de véhicules privés",
        "Infractions aux conditions générales d'entrée et de séjour des étrangers",
        "Aide à l'entrée, à la circulation et au séjour des étrangers",
        "Autres infractions à la police des étrangers",
        "Outrages à dépositaires autorité",
        "Violences à dépositaires autorité",
        "Port ou détention armes prohibées",
        "Atteintes aux intérêts fondamentaux de la Nation",
        "Délits des courses et des jeux",
        "Délits interdiction de séjour et de paraître",
        "Destructions, cruautés et autres délits envers les animaux",
        "Atteintes à l'environnement",
        "Chasse et pêche",
        "Faux documents d'identité",
        "Faux documents concernant la circulation des véhicules",
        "Autres faux documents administratifs",
        "Faux en écriture publique et authentique",
        "Autres faux en écriture",
        "Fausse monnaie",
        "Contrefaçons et fraudes industrielles et commerciales",
        "Contrefaçons littéraires et artistique",
        "Falsification et usages de chèques volés",
        "Falsification et usages de cartes de crédit",
        "Escroqueries et abus de confiance",
        "Infractions à la législation sur les chèques",
        "Travail clandestin",
        "Emploi d'étranger sans titre de travail",
        "Marchandage - prêt de main d'oeuvre",
        "Index non utilisé",
        "Index non utilisé",
        "Banqueroutes, abus de biens sociaux et autres délits de société",
        "Index non utilisé",
        "Index non utilisé",
        "Prix illicittes, publicité fausse et infractions aux règles de la concurrence",
        "Achats et ventes sans factures",
        "Infractions à l'exercice d'une profession règlementée",
        "Infractions au droit de l'urbanisme et de la construction",
        "Fraudes fiscales",
        "Autres délits économiques et financiers",
        "Autres délits"
    };

    DatasetProp[] m_props;

    public override DatasetProp[] GetDataProperties()
    {
        if (m_props == null)
        {
            m_props = new DatasetProp[m_crimeIdName.Length];
            int i = 0;

            foreach(var p in m_crimeIdName)
            {
                m_props[i] = new DatasetProp{
                    Value = "C" + i.ToString(),
                    Description = p
                };

                ++i;
            }
        }

        return m_props;
    }

    public override bool GetData(string departmentId, string property, float time, out float value)
    {
        value = default;

        if (!int.TryParse(property.Substring(1), out var crimeId)) return false;

        bool valid;
        CrimeState crime;
        
        if (crimeId > 0)
        {
            valid = GetCrime(departmentId, crimeId, time, out crime);
        }
        else valid = GetTotalCrime(departmentId, time, out crime);

        if (valid)
        {
            value = crime.Count;
            return true;
        }

        return valid;
    }

    public override float GetMinPossibleValue(string property)
    {
        if (!int.TryParse(property.Substring(1), out var crimeId)) 
            return 0f;

        if (crimeId >= m_minPerType.Count)
            return 0f;

        return m_minPerType[crimeId];
    }

    public override float GetMaxPossibleValue(string property)
    {
        if (!int.TryParse(property.Substring(1), out var crimeId) || crimeId >= m_maxPerType.Count) return 0f;

        return m_maxPerType[crimeId];
    }
}
