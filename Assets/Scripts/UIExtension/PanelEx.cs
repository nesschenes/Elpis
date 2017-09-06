using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Extensions/PanelEx")]
    public class PanelEx : UIBehaviour
    {
        [SerializeField]
        protected Canvas m_Canvas;

        public bool IsShow { get { return m_Canvas.enabled; } }

        public virtual void Show(params object[] _args)
        {

        }

        public virtual void Hide()
        {

        }
    }
}
