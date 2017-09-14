using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Elpis.Test
{
    public class ToolWindow : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_ToolBtnRoot = null;
        [SerializeField]
        private Button m_ToolBtnTemplate = null;
        [SerializeField]
        private ToolBase[] m_Tools = null;
        [SerializeField]
        private Button m_Exit = null;

        private ToolBase mCurrentTool;

        void Awake()
        {
            CreateTool("FileBrowser", "檔案瀏覽器", OnFileBrowser);
            CreateTool("DOTween", "DOTween", OnDOTween);

            for (int i = 0; i < m_Tools.Length; i++)
                m_Tools[i].Event_OnVisibleChange += OnToolVisibleChange;

            m_Exit.onClick.AddListener(OnExit);
        }

        void OnDestroy()
        {
            m_Exit.onClick.RemoveAllListeners();
        }

        void CreateTool(string _btnName, string _btnTitle, UnityAction _action)
        {
            var btn = Instantiate(m_ToolBtnTemplate, m_ToolBtnRoot, false);
            btn.name = _btnName;
            btn.onClick.AddListener(_action);
            var btnText = btn.GetComponentInChildren<Text>();
            btnText.text = _btnTitle;
            btn.gameObject.SetActive(true);
        }

        void OnToolVisibleChange(ToolBase _tool, bool _isVisible)
        {
            if (_isVisible)
            {
                if (mCurrentTool != null)
                    mCurrentTool.Hide();

                mCurrentTool = _tool;
            }
            else if (mCurrentTool == _tool)
            {
                mCurrentTool = null;
            }
        }

        void OnExit()
        {
            if (mCurrentTool != null)
            {
                mCurrentTool.Hide();
                mCurrentTool = null;
            }
        }

        #region 工具方法

        void OnFileBrowser()
        {
            m_Tools[0].Show();
        }

        void OnDOTween()
        {
            m_Tools[1].Show();
        }

        #endregion
    }
}
