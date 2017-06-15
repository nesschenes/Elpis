using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elpis
{
    public class LoginLogic : MonoSingleton<MonoBehaviour>
    {
        [SerializeField]
        private string m_Url = null;

        void Start()
        {
            //Global.Instance.Socket.Connect();
        }
    }
}
