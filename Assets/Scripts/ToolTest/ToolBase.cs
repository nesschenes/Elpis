using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Elpis.Test
{
    public class ToolBase : MonoBehaviour
    {
        [SerializeField]
        private Canvas m_Canvas = null;

        public Action<ToolBase, bool> Event_OnVisibleChange = delegate { };

        protected virtual void Awake()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        public virtual void Show()
        {
            m_Canvas.enabled = true;

            Event_OnVisibleChange(this, true);
        }

        public virtual void Hide()
        {
            m_Canvas.enabled = false;

            Event_OnVisibleChange(this, false);
        }
    }
}
