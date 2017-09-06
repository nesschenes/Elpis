using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Extensions/ToggleExGroup")]
    public class ToggleExGroup : UIBehaviour
    {
        protected readonly List<ToggleEx> m_Toggles = new List<ToggleEx>();

        private ToggleEx mPreviousToggle;

        private ToggleEx mCurrentToggle;

        protected void CertificateToggle(ToggleEx _toggle)
        {
            if (_toggle == null || !m_Toggles.Contains(_toggle))
                throw new ArgumentException(string.Format("ToggleEx : {0} isn't in ToggleExGroup : {1} !", _toggle, this));
        }

        //回傳開關結果
        public bool SwitchToggle(ToggleEx _toggle, bool _isOn)
        {
            //理論上不需要檢查
            CertificateToggle(_toggle);

            if (mCurrentToggle == _toggle)
            {
                return true;
            }
            else
            {
                if (_isOn)
                {
                    mPreviousToggle = mCurrentToggle;
                    mCurrentToggle = _toggle;
                    mPreviousToggle.IsOn = false;
                }

                return _isOn;
            }
        }

        //註冊的第一位優先開啟
        public void RegisterToggle(ToggleEx _toggle)
        {
            if (!m_Toggles.Contains(_toggle))
            {
                m_Toggles.Add(_toggle);

                if (mCurrentToggle == null)
                {
                    mCurrentToggle = _toggle;
                    _toggle.IsOn = true;
                }
                else
                    _toggle.IsOn = false;
            }
        }

        public void UnregisterToggle(ToggleEx _toggle)
        {
            if (m_Toggles.Contains(_toggle))
                m_Toggles.Remove(_toggle);

            if (mCurrentToggle == _toggle)
            {
                if(m_Toggles.Count > 0)
                    mCurrentToggle = m_Toggles[0];
            }
        }
    }
}
