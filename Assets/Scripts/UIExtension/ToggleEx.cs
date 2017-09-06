using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Extensions/ToggleEx"), RequireComponent(typeof(RectTransform))]
    public class ToggleEx : Selectable, IPointerClickHandler, ISubmitHandler, ICanvasElement, IEventSystemHandler
    {
        [SerializeField]
        private ToggleExGroup m_ToggleGroup = null;

        [SerializeField]
        private bool m_IsOn;

        public class ToggleExEvent : UnityEvent<ToggleEx, bool> { };

        public ToggleExEvent onValueChanged = new ToggleExEvent();

        public bool IsBlock = false;

        public bool IsOn
        {
            get { return m_IsOn; }
            set { Switch(value); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Initialize();
        }

        protected void Switch(bool _isOn)
        {
            if (m_IsOn == _isOn)
                return;

            m_IsOn = m_ToggleGroup.SwitchToggle(this, _isOn);

            //開關結果與預期不同
            if (m_IsOn != _isOn)
                return;

            EffectOnValueChanged();

            onValueChanged.Invoke(this, m_IsOn);
        }

        protected virtual void EffectOnValueChanged()
        {

        }

        protected virtual void OnToggleClick()
        {
            if (!IsInteractable() || IsBlock)
                return;

            IsOn = !IsOn;
        }

        private void Initialize()
        {
            m_ToggleGroup.RegisterToggle(this);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnToggleClick();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {

        }

        public virtual void GraphicUpdateComplete()
        {

        }

        public virtual void LayoutComplete()
        {

        }

        public virtual void Rebuild(CanvasUpdate executing)
        {

        }
    }
}
