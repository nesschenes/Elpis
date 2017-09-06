using UnityEngine;
using UnityEngine.UI;
using Elpis.FileBrowser;

namespace Elpis.Test
{
    /// <summary>
    /// 示範 FileBrowser 的使用方法
    /// </summary>
    public class FileBrowserSample : ToolBase
    {
        [SerializeField]
        private Text m_Path = null;
        [SerializeField]
        private Button m_OpenFile = null;
        [SerializeField]
        private Button m_OpenFileMultiple = null;
        [SerializeField]
        private Button m_OpenFileExtension = null;
        [SerializeField]
        private Button m_OpenFileDirectory = null;
        [SerializeField]
        private Button m_OpenFileFilter = null;
        [SerializeField]
        private Button m_OpenFolder = null;
        [SerializeField]
        private Button m_OpenFolderDirectory = null;
        [SerializeField]
        private Button m_SaveFile = null;
        [SerializeField]
        private Button m_SaveFileDefaultName = null;
        [SerializeField]
        private Button m_SaveFileDefaultNameExt = null;
        [SerializeField]
        private Button m_SaveFileDirectory = null;
        [SerializeField]
        private Button m_SaveFileFilter = null;

        protected override void Awake()
        {
            m_OpenFile.onClick.AddListener(OpenFile);
            m_OpenFileMultiple.onClick.AddListener(OpenFileMultiple);
            m_OpenFileExtension.onClick.AddListener(OpenFileExtension);
            m_OpenFileDirectory.onClick.AddListener(OpenFileDirectory);
            m_OpenFileFilter.onClick.AddListener(OpenFileFilter);
            m_OpenFolder.onClick.AddListener(OpenFolder);
            m_OpenFolderDirectory.onClick.AddListener(OpenFolderDirectory);
            m_SaveFile.onClick.AddListener(SaveFile);
            m_SaveFileDefaultName.onClick.AddListener(SaveFileDefaultName);
            m_SaveFileDefaultNameExt.onClick.AddListener(SaveFileNameExt);
            m_SaveFileDirectory.onClick.AddListener(SaveFileDirectory);
            m_SaveFileFilter.onClick.AddListener(SaveFileFilter);
        }

        protected override void OnDestroy()
        {
            m_OpenFile.onClick.RemoveAllListeners();
            m_OpenFileMultiple.onClick.RemoveAllListeners();
            m_OpenFileExtension.onClick.RemoveAllListeners();
            m_OpenFileDirectory.onClick.RemoveAllListeners();
            m_OpenFileFilter.onClick.RemoveAllListeners();
            m_OpenFolder.onClick.RemoveAllListeners();
            m_OpenFolderDirectory.onClick.RemoveAllListeners();
            m_SaveFile.onClick.RemoveAllListeners();
            m_SaveFileDefaultName.onClick.RemoveAllListeners();
            m_SaveFileDefaultNameExt.onClick.RemoveAllListeners();
            m_SaveFileDirectory.onClick.RemoveAllListeners();
            m_SaveFileFilter.onClick.RemoveAllListeners();
        }

        void OpenFile()
        {
            WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false));
        }

        void OpenFileMultiple()
        {
            WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", true));
        }

        void OpenFileExtension()
        {
            WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "txt", true));
        }

        void OpenFileDirectory()
        {
            WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", Application.dataPath, "", true));
        }

        void OpenFileFilter()
        {
            var extensions = new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
                new ExtensionFilter("Sound Files", "mp3", "wav" ),
                new ExtensionFilter("All Files", "*" ),
            };

            WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true));
        }

        void OpenFolder()
        {
            WriteResult(StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", true));
        }

        void OpenFolderDirectory()
        {
            WriteResult(StandaloneFileBrowser.OpenFolderPanel("Select Folder", Application.dataPath, true));
        }

        void SaveFile()
        {
            WriteResult(StandaloneFileBrowser.SaveFilePanel("Save File", "", "", ""));
        }

        void SaveFileDefaultName()
        {
            WriteResult(StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", ""));
        }

        void SaveFileNameExt()
        {
            WriteResult(StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", "dat"));
        }

        void SaveFileDirectory()
        {
            WriteResult(StandaloneFileBrowser.SaveFilePanel("Save File", Application.dataPath, "", ""));
        }

        void SaveFileFilter()
        {
            var extensionList = new[] {
                new ExtensionFilter("Binary", "bin"),
                new ExtensionFilter("Text", "txt"),
            };

            WriteResult(StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", extensionList));
        }

        public void WriteResult(string _path)
        {
            m_Path.text = _path;
        }

        public void WriteResult(string[] _paths)
        {
            if (_paths.Length == 0)
                return;

            string path = "";

            foreach (var p in _paths)
                path += p + "\n";

            m_Path.text = path;
        }
    }

}
