using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    public class TogglePanel : PanelEx
    {
        [SerializeField]
        private ToggleEx[] m_Toggles = null;
        [SerializeField]
        private ToggleExGroup m_ToggleGroup = null;

        protected override void Awake()
        {
            for (int i = 0; i < m_Toggles.Length; i++)
            {
                m_Toggles[i].onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        protected virtual void OnToggleValueChanged(ToggleEx _toggle, bool _isOn)
        {

        }
    }
}
