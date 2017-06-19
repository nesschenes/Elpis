using System;
using System.Collections.Generic;
using UnityEngine;

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
            Debug.Log("I got Test");
        }

        private void cmd_connection(string _value)
        {
            Debug.Log("I got Connection");
        }

        public Dictionary<string, Action<string>> GetCmds()
        {
            mCmds.Clear();

            mCmds.Add(Cmd.Test.ToString().ToLower(), cmd_test);
            mCmds.Add(Cmd.Connection.ToString().ToLower(), cmd_connection);

            return mCmds;
        }
    }
}
