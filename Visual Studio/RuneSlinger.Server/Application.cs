using System.Collections.Generic;
using System.IO;
using ExitGames.Logging.Log4Net;
using Photon.SocketServer;
using log4net;
using log4net.Config;

namespace RuneSlinger.Server
{
    public class Application : ApplicationBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Application));

        private readonly List<RunePeer> _peers;

        public IEnumerable<RunePeer> Peers { get { return _peers; } }

        public Application()
        {
            _peers = new List<RunePeer>();
        }

        public void DestroyPeer(RunePeer peer)
        {
            _peers.Remove(peer);
        }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            var peer = new RunePeer(this, initRequest);
            _peers.Add(peer);
            return peer;
        }

        protected override void Setup()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(BinaryPath, "log4net.config")));
            ExitGames.Logging.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);


            Log.Info("Application Started!");
        }

        protected override void TearDown()
        {
            Log.Info("Application ending...");
        } 
    }
}