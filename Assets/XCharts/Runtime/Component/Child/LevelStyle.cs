using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    [System.Serializable]
    public class Level : ChildComponent
    {
        [SerializeField] private LabelStyle m_Label = new LabelStyle();
        [SerializeField] private LabelStyle m_UpperLabel = new LabelStyle();
        [SerializeField] private ItemStyle m_ItemStyle = new ItemStyle();

        public LabelStyle label { get { return m_Label; } }
        public LabelStyle upperLabel { get { return m_UpperLabel; } }
        public ItemStyle itemStyle { get { return m_ItemStyle; } }
    }

    [System.Serializable]
    public class LevelStyle : ChildComponent
    {
        [SerializeField] private bool m_Show = false;
        [SerializeField] private List<Level> m_Levels = new List<Level>() { new Level() };

        /// <summary>
        /// 是否启用LevelStyle
        /// </summary>
        public bool show { get { return m_Show; } set { m_Show = value; } }
        /// <summary>
        /// 各层节点对应的配置。当enableLevels为true时生效，levels[0]对应的第一层的配置，levels[1]对应第二层，依次类推。当levels中没有对应层时用默认的设置。
        /// </summary>
        public List<Level> levels { get { return m_Levels; } }
    }
}