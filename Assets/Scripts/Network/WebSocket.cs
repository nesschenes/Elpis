using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Elpis.Network
{
    public sealed class WebSocket
    {
        public enum State : int
        {
            Connecting = 0,
            Connected = 1,
            Closing = 2,
            Closed = 3,
        }

        private Uri mUri;

        private WebSocketSharp.WebSocket mInstance = null;
        public event Action<WebSocket> Opened;
        public event Action<WebSocket, string> TextReceived;
        public event Action<WebSocket, byte[], int> DataReceived;
        public event Action<WebSocket, int, string> Closed;
        public event Action<WebSocket, Exception> ErrorOccurred;

        public State NetworkState { get { return (mInstance == null ? State.Closed : (State)mInstance.ReadyState); } }

        public string Uri { get { return (mUri == null ? string.Empty : mUri.ToString()); } }

        public WebSocket()
        {
            mUri = null;
        }

        public void Connect(string _url, string _origin, params string[] _protocols)
        {
            if (NetworkState != State.Closed)
                return;

            mUri = new Uri(_url);

            mInstance = new WebSocketSharp.WebSocket(mUri.ToString(), _protocols);
            mInstance.Origin = _origin;
            mInstance.SslConfiguration.ServerCertificateValidationCallback = VerifyServerCertification;
            mInstance.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Ssl3;

            mInstance.OnOpen += OnOpen;
            mInstance.OnClose += OnClose;
            mInstance.OnError += OnError;
            mInstance.OnMessage += OnMessage;

            mInstance.ConnectAsync();
        }

        public void Connect(string _host, int _port, bool _security, string _origin, params string[] _protocols)
        {
            if (NetworkState != State.Closed)
                return;

            Connect(string.Format("{0}://{1}{2}", (_security ? "wss" : "ws"),
                                                  _host,
                                                  (_port == 80 ? string.Empty : string.Format(":{0}", _port))), _origin, _protocols);
        }

        public void Close()
        {
            if (NetworkState != State.Connected)
                return;

            mInstance.CloseAsync();
        }

        public void Close(int _code, string _msg)
        {
            if (NetworkState != State.Connected)
                return;

            mInstance.CloseAsync((ushort)_code, _msg);
        }

        public void CloseImmediately()
        {
            if (NetworkState != State.Connected)
                return;

            mInstance.CloseAsync(9527, "CloseImeediately");
        }

        public void Send(string _text)
        {
            if (NetworkState != State.Connected)
                return;

            mInstance.SendAsync(_text, null);
        }

        public void Send(byte[] _raw)
        {
            Send(_raw, 0, _raw.Length);
        }

        public void Send(byte[] _raw, int _offset, int _length)
        {
            if (NetworkState != State.Connected)
                return;

            mInstance.SendAsync(_raw, _offset, _length, null);
        }

        private void OnOpen(object _sender, EventArgs _args)
        {
            if (Opened != null)
                Opened(this);
        }

        private void OnClose(object _sender, WebSocketSharp.CloseEventArgs _args)
        {
            if (Closed != null)
                Closed(this, _args.Code, _args.Reason);
        }

        private void OnError(object _sender, WebSocketSharp.ErrorEventArgs _args)
        {
            if (ErrorOccurred != null)
                ErrorOccurred(this, (_args.Exception ?? new Exception(_args.Message)));
        }

        private void OnMessage(object _sender, WebSocketSharp.MessageEventArgs _args)
        {
            if (_args.IsBinary)
            {
                if (DataReceived != null)
                    DataReceived(this, _args.RawData, _args.RawData.Length);
            }
            else if (_args.IsText)
            {
                if (TextReceived != null)
                    TextReceived(this, _args.Data);
            }
        }

        private static bool VerifyServerCertification(object _sender, X509Certificate _cert, X509Chain _chain, SslPolicyErrors _errors)
        {
            return true;
        }
    }
}
