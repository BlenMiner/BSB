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
        "R??glements de compte entre malfaiteurs",
        "Homicides pour voler et ?? l'occasion de vols",
        "Homicides pour d'autres motifs",
        "Tentatives d'homicides pour voler et ?? l'occasion de vols",
        "Tentatives homicides pour d'autres motifs",
        "Coups et blessures volontaires suivis de mort",
        "Autres coups et blessures volontaires criminels ou correctionnels",
        "Prises d'otages ?? l'occasion de vols",
        "Prises d'otages dans un autre but",
        "Sequestrations",
        "Menaces ou chantages pour extorsion de fonds",
        "Menaces ou chantages dans un autre but",
        "Atteintes ?? la dignit?? et ?? la personnalit??",
        "Violations de domicile",
        "Vols ?? main arm??e contre des ??tablissements financiers",
        "Vols ?? main arm??e contre des ??tablissements industriels ou commerciaux",
        "Vols ?? main arm??e contre des entreprises de transports de fonds",
        "Vols ?? main arm??e contre des particuliers ?? leur domicile",
        "Autres vols ?? main arm??e",
        "Vols avec armes blanches contre des ??tablissements financiers,commerciaux ou industriels",
        "Vols avec armes blanches contre des particuliers ?? leur domicile",
        "Autres vols avec armes blanches",
        "Vols violents sans arme contre des ??tablissements financiers,commerciaux ou industriels",
        "Vols violents sans arme contre des particuliers ?? leur domicile",
        "Vols violents sans arme contre des femmes sur voie publique ou autre lieu public",
        "Vols violents sans arme contre d'autres victimes",
        "Cambriolages de locaux d'habitations principales",
        "Cambriolages de r??sidences secondaires",
        "Cambriolages de locaux industriels, commerciaux ou financiers",
        "Cambriolages d'autres lieux",
        "Vols avec entr??e par ruse en tous lieux",
        "Vols ?? la tire",
        "Vols ?? l'??talage",
        "Vols de v??hicules de transport avec fr??t",
        "Vols d'automobiles",
        "Vols de v??hicules motoris??s ?? 2 roues",
        "Vols ?? la roulotte",
        "Vols d'accessoires sur v??hicules ?? moteur immatricul??s",
        "Vols simples sur chantier",
        "Vols simples sur exploitations agricoles",
        "Autres vols simples contre des ??tablissements publics ou priv??s",
        "Autres vols simples contre des particuliers dans deslocaux priv??s",
        "Autres vols simples contre des particuliers dans des locaux ou lieux publics",
        "Recels",
        "Prox??n??tisme",
        "Viols sur des majeur(e)s",
        "Viols sur des mineur(e)s",
        "Harc??lements sexuels et autres agressions sexuelles contre des majeur(e)s",
        "Harc??lements sexuels et autres agressions sexuelles contre des mineur(e)s",
        "Atteintes sexuelles",
        "Homicides commis contre enfants de moins de 15 ans",
        "Violences, mauvais traitements et abandons d'enfants.",
        "D??lits au sujet de la garde des mineurs",
        "Non versement de pension alimentaire",
        "Trafic et revente sans usage de stup??fiants",
        "Usage-revente de stup??fiants",
        "Usage de stup??fiants",
        "Autres infractions ?? la l??gislation sur les stup??fiants",
        "D??lits de d??bits de boissons et infraction ?? la r??glementation sur l'alcool et le tabac",
        "Fraudes alimentaires et infractions ?? l'hygi??ne",
        "Autres d??lits contre sant?? publique et la r??glementation des professions m??dicales",
        "Incendies volontaires de biens publics",
        "Incendies volontaires de biens priv??s",
        "Attentats ?? l'explosif contre des biens publics",
        "Attentats ?? l'explosif contre des biens priv??s",
        "Autres destructions er d??gradations de biens publics",
        "Autres destructions er d??gradations de biens priv??s",
        "Destructions et d??gradations de v??hicules priv??s",
        "Infractions aux conditions g??n??rales d'entr??e et de s??jour des ??trangers",
        "Aide ?? l'entr??e, ?? la circulation et au s??jour des ??trangers",
        "Autres infractions ?? la police des ??trangers",
        "Outrages ?? d??positaires autorit??",
        "Violences ?? d??positaires autorit??",
        "Port ou d??tention armes prohib??es",
        "Atteintes aux int??r??ts fondamentaux de la Nation",
        "D??lits des courses et des jeux",
        "D??lits interdiction de s??jour et de para??tre",
        "Destructions, cruaut??s et autres d??lits envers les animaux",
        "Atteintes ?? l'environnement",
        "Chasse et p??che",
        "Faux documents d'identit??",
        "Faux documents concernant la circulation des v??hicules",
        "Autres faux documents administratifs",
        "Faux en ??criture publique et authentique",
        "Autres faux en ??criture",
        "Fausse monnaie",
        "Contrefa??ons et fraudes industrielles et commerciales",
        "Contrefa??ons litt??raires et artistique",
        "Falsification et usages de ch??ques vol??s",
        "Falsification et usages de cartes de cr??dit",
        "Escroqueries et abus de confiance",
        "Infractions ?? la l??gislation sur les ch??ques",
        "Travail clandestin",
        "Emploi d'??tranger sans titre de travail",
        "Marchandage - pr??t de main d'oeuvre",
        "Index non utilis??",
        "Index non utilis??",
        "Banqueroutes, abus de biens sociaux et autres d??lits de soci??t??",
        "Index non utilis??",
        "Index non utilis??",
        "Prix illicittes, publicit?? fausse et infractions aux r??gles de la concurrence",
        "Achats et ventes sans factures",
        "Infractions ?? l'exercice d'une profession r??glement??e",
        "Infractions au droit de l'urbanisme et de la construction",
        "Fraudes fiscales",
        "Autres d??lits ??conomiques et financiers",
        "Autres d??lits"
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

    public override bool GetData(int postalCode, string property, float time, out float value)
    {
        value = default;

        if (!int.TryParse(property.Substring(1), out var crimeId)) return false;

        string departmentId = GetDepartmentID(postalCode);
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
