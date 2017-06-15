using System;
using System.Collections.Generic;

namespace Elpis
{
    public class ClientData
    {
        private class Singleton
        {
            static Singleton() { }
            internal static readonly ClientData Instance = new ClientData();
        }

        public static ClientData Instance
        {
            get { return Singleton.Instance; }
        }

        private readonly Dictionary<string, Action<string>> mCmds;

        private ClientData()
        {
            mCmds = new Dictionary<string, Action<string>>();
        }

        private void cmd_test(string _value)
        {

        }

        public Dictionary<string, Action<string>> GetCmds()
        {
            mCmds.Clear();

            mCmds.Add(Cmd.Test.ToString(), cmd_test);

            return mCmds;
        }
    }
}
