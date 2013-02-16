using System;
using System.Collections.Generic;
using System.IO;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using Photon.SocketServer;
using RuneSlinger.Server.Entities;
using RuneSlinger.Server.ValueObjects;
using log4net;
using log4net.Config;
using Log4NetLoggerFactory = ExitGames.Logging.Log4Net.Log4NetLoggerFactory;

namespace RuneSlinger.Server
{
    public class Application : ApplicationBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Application));

        private readonly List<RunePeer> _peers;
        private ISessionFactory _sessionFactory;

        public IEnumerable<RunePeer> Peers { get { return _peers; } }

        public Application()
        {
            _peers = new List<RunePeer>();
        }

        public ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
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

            SetupNHibernate();

            Log.Info("Application Started!");
        } 

        private void SetupNHibernate()
        {
            var config = new Configuration();
            config.Configure();

            var mapper = new ModelMapper();
            mapper.AddMapping<UserMap>();
            config.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            _sessionFactory = config.BuildSessionFactory();
        }

        protected override void TearDown()
        {
            Log.Info("Application ending...");
        } 
    }
}