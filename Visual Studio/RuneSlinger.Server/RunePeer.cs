using System.Linq;
using System.Collections.Generic;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using log4net;

namespace RuneSlinger.Server
{
    public class RunePeer : PeerBase
    {
        private static ILog Log = LogManager.GetLogger(typeof (RunePeer));

        private readonly Application _application;

        public RunePeer(Application application, InitRequest initRequest)
            : base(initRequest.Protocol, initRequest.PhotonPeer)
        {
            _application = application;
            Log.InfoFormat("Peer created at {0}:{1}", initRequest.RemoteIP, initRequest.RemotePort);

        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (operationRequest.OperationCode != 0)
            {
                Log.WarnFormat("Peer sent unknown opcode: {0}", operationRequest.OperationCode);
                return;
            }
            var message = (string)operationRequest.Parameters[0];
            Log.DebugFormat("Got message from client:{0}",message);

            var eventData = new EventData(0, new Dictionary<byte, object> {{0, message}});
            var parameters = new SendParameters{ Unreliable = false };
            foreach (var peer in _application.Peers.Where(t => t != this))
            {
                peer.SendEvent(
                    eventData,
                    parameters);
            }
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            Log.InfoFormat("Peer disconnected: {0}, {1}",reasonCode, reasonDetail);
            _application.DestroyPeer(this);
        }
    } 
}