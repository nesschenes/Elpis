using UnityEngine;
using UnityEngine.UI;

namespace Elpis.Login
{
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField]
        private Dropdown m_ServerMenu = null;
        [SerializeField]
        private InputField m_UserName = null;
        [SerializeField]
        private Button m_LoginBtn = null;

        private string mServerPath = string.Empty;

        private string mUserName = string.Empty;

        void Awake()
        {
            m_UserName.onEndEdit.AddListener(OnUserNameEdited);
            m_LoginBtn.onClick.AddListener(OnLogin);
        }

        void OnDestroy()
        {
            m_LoginBtn.onClick.RemoveListener(OnLogin);
        }

        void OnUserNameEdited(string _content)
        {
            mUserName = _content;
        }

        void OnLogin()
        {
            if (string.IsNullOrEmpty(mUserName))
                return;

            mServerPath = "http://ynserver.herokuapp.com/";

            Global.Instance.Socket.Connect(mServerPath);
        }
    }
}
