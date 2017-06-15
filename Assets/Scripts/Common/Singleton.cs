using UnityEngine;

namespace Elpis
{
    public class Singleton<T> where T : new()
    {
        private static T mInstance = default(T);
        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new T();
                }
                return mInstance;
            }
        }
    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T mInstance;
        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = (T)FindObjectOfType(typeof(T));

                    if (mInstance == null)
                    {
                        string typeName = typeof(T).ToString();
                        string stackTrace = StackTraceUtility.ExtractStackTrace();
                        string error = string.Format("找不到 {0} 實體: {1}", typeName, stackTrace);

                        Debug.LogError(error);
                    }
                }

                return mInstance;
            }
        }

        public static bool IsExist { get { return (mInstance != null); } }

        protected virtual void Awake()
        {
            mInstance = (T)((System.Object)this);
        }

        protected virtual void OnDestroy()
        {
            mInstance = null;
        }
    }
}