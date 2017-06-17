using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityThreading;
using Elpis.Message;

namespace Elpis.Network
{
    public sealed class SocketHandler
    {
        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Reconnecting,
            Connected,
        }

        private UnityThreading.ActionThread mPingPongThread;

        private readonly WebSocket mWebSocket;
        private readonly PingPong mPingPong;

        private Dictionary<string, Action<string>> mCmds;

        private uint mServerTime;
        private float mClientBaseTime;

        private string mServerPath = "http://ynserver.herokuapp.com/";

        public event Action SocketConnected = delegate { };
        public event Action<string> SocketDisconnected;
        public event Action<string> SocketOnErrorOccured = delegate { };

        public long PingTime { get { return mPingPong.PingTime; } }
        public ConnectionState State;
        public bool CanReconnet = true;

        public string ServerPath { get { return mServerPath; } }

        // 是不精確的時間(沒考慮network latency), 但拿來當ui顯示時間應該可以(最多誤差10 + latency秒)
        public uint ServerTime { get { return (mServerTime + (uint)Mathf.CeilToInt(Time.realtimeSinceStartup - mClientBaseTime)); } }

        private List<string> mIgnoreCmdLog = new List<string>() { "grenade_pos", "bomb_pos", "prop_pos" };

        internal SocketHandler()
        {
            mServerTime = 0;
            mClientBaseTime = 0.0f;

            mWebSocket = new WebSocket();
            mWebSocket.Opened += WebSocket_OnConnected;
            mWebSocket.Closed += WebSocket_OnDisconnected;
            mWebSocket.ErrorOccurred += WebSocket_OnErrorOccured;
            mWebSocket.TextReceived += WebSocket_OnTextReceived;
            mWebSocket.DataReceived += WebSocket_OnDataReceived;

            // 初始化 PingPong Thread
            mPingPongThread = UnityThreadHelper.CreateThread(() => { return; });

            mPingPong = new PingPong();

            State = ConnectionState.Disconnected;

            mCmds = new Dictionary<string, Action<string>>(ClientData.Instance.GetCmds());
        }

        private void WebSocket_OnConnected(WebSocket ws)
        {
            Global.Instance.SetInitStep(InitialStep.NetworkComplete);

            State = ConnectionState.Connected;

            // 已連線，開始 Ping-Pong
            mPingPongThread.Dispatch(mPingPong.StartPingPong);

            UnityThreadHelper.Dispatcher.Dispatch(SocketConnected);
        }

        private void WebSocket_OnDisconnected(WebSocket ws, int code, string reason)
        {
            Debug.LogFormat("非主動斷線： host: {0}, code: {1}, reason: {2}", ws.Uri, code, reason);

            if (code != 9527 && CanReconnet)
            {
                mPingPong.StopPingPong();

                // 試試看可不可以連到別台
                TryConnectServer();
            }
        }

        private void WebSocket_OnErrorOccured(WebSocket ws, Exception e)
        {
            Debug.LogFormat("連線錯誤： host: {0}, msg: {1}", ws.Uri, e);

            mPingPongThread.Dispatch(mPingPong.StopPingPong);

            UnityThreadHelper.Dispatcher.Dispatch(TryConnectServer);

            SocketOnErrorOccured(e.Message);
        }

        private void WebSocket_OnTextReceived(WebSocket ws, string text)
        {
            string key, value;

            key = GetTextCmdDetail(text, out value);

            //if (!key.StartsWith("!pong"))
            Debug.LogFormat("接收到{0}", text);

            if (!string.IsNullOrEmpty(key))
            {
                Task task = Task.Create(HandleTextCommand);
                task.UserData = new string[] { key, value };

                UnityThreadHelper.Dispatcher.Dispatch(task);
            }
        }

        private void WebSocket_OnDataReceived(WebSocket ws, byte[] data, int length)
        {
            // Debug.LogFormat(Debug.Filter.EditorOnly, "接收到{0}byte{1} raw data", data.Length, (data.Length > 1 ? "s" : string.Empty));

            Task task = Task.Create(HandleBinaryCommand);
            task.UserData = new ArraySegment<byte>(data, 0, length);

            UnityThreadHelper.Dispatcher.Dispatch(task);
        }

        private void HandleTextCommand(Task task)
        {
            string[] cmd = task.UserData as string[];
            HandleTextCommand(cmd[0], cmd[1]);
        }

        private void HandleBinaryCommand(Task task)
        {
            ArraySegment<byte> aa = (ArraySegment<byte>)task.UserData;

            HandleBinaryCommand(aa.Array, aa.Count);
        }

        private void HandleTextCommand(string key, string value)
        {
            switch (key)
            {
                case "!pong":
                    {
                        mPingPong.OnServerResponse();

                        // 更新server時間
                        string[] values = value.Split(' ');

                        if (values.Length > 0)
                        {
                            value = values[0];

                            if (value.Length > 3)
                            {
                                // ms -> second
                                value = value.Substring(0, value.Length - 3);

                                if (uint.TryParse(value, out mServerTime))
                                    mClientBaseTime = Time.realtimeSinceStartup;
                            }
                        }

                        break;
                    }
                default:
                    {
                        Action<string> cmd;

                        if (mCmds.TryGetValue(key, out cmd))
                        {
                            if (cmd != null)
                            {
                                try
                                {
                                    cmd(value);
                                }
                                catch (Exception _ex)
                                {
                                    Debug.LogErrorFormat("cmd {0} exception {1} \n {2}", key, _ex.Message, _ex.StackTrace);
                                }
                            }
                            else
                                Debug.LogErrorFormat("cmd {0} is null", key);
                        }
                        else
                        {
                            Debug.LogWarningFormat("尚未處理的cmd: {0}", key);
                        }

                        break;
                    }
            }
        }

        // 暫時用不到
        private void HandleBinaryCommand(byte[] data, int length)
        {
            switch (data[0])
            {
                case 0x90:
                    {
                        Action<string> target;
                        MemoryStream ms = null;

                        ms = new MemoryStream(data, 1, length - 1, false, true);

                        string error = string.Empty;
                        string dataStr = string.Empty;
                        string cmd = string.Empty;

                        if (TryUnpackAsString(ms, out cmd, out dataStr, out error) == 1)
                        {
                            if (!string.IsNullOrEmpty(cmd))
                            {
                                if (!mIgnoreCmdLog.Contains(cmd))
                                {
                                    Debug.LogFormat("接收到bcmd:{0}", cmd);
                                    Debug.LogFormat("接收到bcmd:{0}, 資料:{1}", cmd, dataStr);
                                }

                                if (mCmds.TryGetValue(cmd, out target))
                                {
                                    if (target != null)
                                    {
                                        target(dataStr);
                                    }
                                }
                                else
                                {
                                    Debug.LogWarningFormat("尚未處理的cmd: {0}", cmd);
                                }
                            }
                        }
                        else
                        {
                            Debug.LogFormat(string.Format("bcmd error code:{0}", error));
                        }

                        break;
                    }
            }
        }

        private void TryConnectServer()
        {
            mWebSocket.Close(9527, "新連線");

            Uri uri = new Uri(ServerPath);

            Debug.LogFormat("準備連線至 {0}:{1}", uri.Host, uri.Port);
            mWebSocket.Connect(uri.Host, uri.Port, ServerPath.Contains("https"), "null");
        }

        public void Connect(string _serverPath)
        {
            if (State > ConnectionState.Disconnected)
                return;

            mServerPath = _serverPath;

            State = ConnectionState.Connecting;

            TryConnectServer();
        }

        public void Disconnect(int code, string reason)
        {
            if (State == ConnectionState.Connected)
            {
                Debug.LogFormat("主動斷線 Code：{0}, Reason：{1}", code, reason);
                mWebSocket.Close(code, reason);
            }
        }

        internal void DisconnectImmediately()
        {
            CanReconnet = false;

            if (State == ConnectionState.Connected)
            {
                Debug.Log("主動立即斷線");
                mWebSocket.CloseImmediately();
                mPingPong.StopPingPong();
            }
        }

        public void SendText(string text)
        {
            mWebSocket.Send(text);

            // if (text.StartsWith("!ping"))
            //     return;

            Debug.Log("送出" + text);
        }

        // 暫時用不到
        public void SendBinaryCmd(Dictionary<object, object> _args = null, bool _showLog = true)
        {
            object cmd;
            if (!_args.TryGetValue("cmd", out cmd))
            {
                Debug.LogFormat("送出bcmd 失敗! 找不到 Command !!!");
                return;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                writer.Write((0x0000000090U).GetBytesLE()[0]);

                if (_args != null && _args.Count > 0)
                    MessagePack.Pack(ms, _args);

                ms.Flush();
                ms.Close();

                byte[] output = ms.ToArray();

                SendBinaryRaw(output);

                if (_showLog)
                    Debug.LogFormat("送出bcmd:{0}", cmd.ToString());
            }
        }

        // 暫時用不到
        public void SendBinaryRaw(byte[] raw)
        {
            mWebSocket.Send(raw);
        }

        // 暫時用不到
        public void SendBinaryRaw(byte[] raw, int start, int length)
        {
            mWebSocket.Send(raw, start, length);
        }

        private static string GetTextCmdDetail(string input, out string value)
        {
            int split = input.IndexOf(' ');

            if (split < 0)
            {
                value = string.Empty;
                return input;
            }

            value = (++split < input.Length ? input.Substring(split, input.Length - split) : string.Empty);
            return input.Substring(0, split - 1);
        }

        public static int TryUnpackAsString(MemoryStream ms, out string cmd, out string dataStr, out string error)
        {
            MessagePackObject obj;
            int ret = 1;

            dataStr = string.Empty;
            error = string.Empty;
            cmd = string.Empty;

            try
            {
                if (MessagePack.Unpack(ms, out obj))
                {
                    var dict = obj.AsDictionary();

                    // <= 0都是錯誤
                    if (dict != null)
                    {
                        if (dict.TryGetStringValue("cmd", out cmd))
                            dataStr = obj.ToString();
                        else if (dict.TryGetStringValue("err", out error))
                            ret = 0;
                    }
                }
            }
            catch
            {
                ret = 0;
            }

            return ret;
        }

        private sealed class PingPong
        {
            private const int PING_INTERVAL = 2;
            private const int PING_TIMEOUT = 1;
            private const int PING_LIMIT = 9;

            private readonly System.Threading.Timer mPingPongTimer;

            private long mPingTime;

            private readonly Queue<System.Diagnostics.Stopwatch> mPingQueue;

            public PingPong()
            {
                mPingPongTimer = new System.Threading.Timer(new System.Threading.TimerCallback(PingPongTick));

                mPingQueue = new Queue<System.Diagnostics.Stopwatch>();
                mPingTime = 0;
            }

            /// <summary>
            /// 開始 PingPong
            /// </summary>
            public void StartPingPong()
            {
                mPingTime = 0;
                mPingQueue.Clear();
                mPingPongTimer.Change(0, PING_INTERVAL * 1000);
            }

            /// <summary>
            /// 停止 PingPong
            /// </summary>
            public void StopPingPong()
            {
                mPingPongTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                mPingTime = System.Threading.Timeout.Infinite;
            }

            /// <summary>
            /// PingPong Callback
            /// </summary>
            private void PingPongTick(object stateInfo)
            {
                lock (mPingQueue)
                {
                    mPingQueue.Enqueue(System.Diagnostics.Stopwatch.StartNew());
                    Global.Instance.Socket.SendText("!ping");
                }
            }

            /// <summary>
            /// 取得目前ping time
            /// </summary>
            public long PingTime
            {
                get
                {
                    if (mPingQueue.Count > 0)
                    {
                        System.Diagnostics.Stopwatch sw = null;

                        try { sw = mPingQueue.Peek(); }
                        catch (InvalidOperationException) { /* queue is empty, ignore */ }

                        if (sw != null && sw.ElapsedMilliseconds > (PING_TIMEOUT * 1000))
                            return System.Threading.Timeout.Infinite;
                    }

                    return mPingTime;
                }
            }

            /// <summary>
            /// 收到 Pong
            /// </summary>
            internal void OnServerResponse()
            {
                if (mPingQueue.Count > 0)
                {
                    lock (mPingQueue)
                    {
                        System.Diagnostics.Stopwatch sw = mPingQueue.Dequeue();
                        sw.Stop();
                        mPingTime = sw.ElapsedMilliseconds; // 取得總耗時 (單位：ms)

                        // Timeout 後清掉 PingQueue
                        if (mPingTime > PING_TIMEOUT * 1000)
                            mPingQueue.Clear();

                        Global.Instance.Socket.SendText("pong" + mPingTime);
                    }
                }
            }
        }
    }
}
