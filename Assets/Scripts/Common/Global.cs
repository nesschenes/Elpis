using System;
using Elpis.Network;

namespace Elpis
{
    public sealed class Global : Singleton<Global>
    {
        public event Action<InitialStep, object[]> OnInitialComplete = delegate { };
        public InitialStep InitStep = InitialStep.None;

        private readonly SocketHandler mSocketHandler;

        public SocketHandler Socket { get { return mSocketHandler; } }

        public Global()
        {
            mSocketHandler = new SocketHandler();
        }

        public void SetInitStep(InitialStep _step, params object[] _args)
        {
            InitStep = _step;

            OnInitialComplete(_step, _args);
        }
    }
}
