using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using RuneSlinger.Base;
using RuneSlinger.Base.Abstract;
using RuneSlinger.Base.Commands;
using RuneSlinger.Server.CommandHandlers;
using log4net;

namespace RuneSlinger.Server
{
    public class RunePeer : PeerBase
    {
        private static ILog Log = LogManager.GetLogger(typeof (RunePeer));

        private readonly Application _application;
        private readonly JsonSerializer _jsonSerializer;

        public RunePeer(Application application, InitRequest initRequest)
            : base(initRequest.Protocol, initRequest.PhotonPeer)
        {
            _application = application;
            _jsonSerializer = new JsonSerializer();

            Log.InfoFormat("Peer created at {0}:{1}", initRequest.RemoteIP, initRequest.RemotePort);

        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (operationRequest.OperationCode != (byte) RuneOperationCode.DispatchCommand)
            {
                SendOperationResponse(new OperationResponse((byte) RuneOperationResponse.Invalid), sendParameters);
                Log.WarnFormat("Peer sent unknown operation code: {0}", operationRequest.OperationCode);
                return;
            }

            using (var session = _application.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        var commandContext = new CommandContext();
                        var commandType = (string)operationRequest.Parameters[(byte)RuneOperationCodeParameter.CommandType];
                        var commandBytes = (byte[])operationRequest.Parameters[(byte)RuneOperationCodeParameter.CommandBytes];
                        
                        ICommand command;
                        using (var ms = new MemoryStream(commandBytes))
                            command = (ICommand) _jsonSerializer.Deserialize(new BsonReader(ms), Type.GetType(commandType));

                        var loginCommand = command as LoginCommand;
                        var registerCommand = command as RegisterCommand;

                        if (loginCommand != null)
                            (new LoginHandler(session)).Handle(commandContext, loginCommand);
                        
                        else if (registerCommand != null)
                            (new RegisterHandler(session)).Handle(commandContext, registerCommand);
                        
                        else
                        {
                            SendOperationResponse(new OperationResponse((byte) RuneOperationResponse.Invalid), sendParameters);
                            Log.WarnFormat("Peer sent unknown command: {0}", commandType);
                            trans.Rollback();
                            return;
                        }

                        var parameters = new Dictionary<byte, object>();

                        if (commandContext.Response != null)
                            parameters[(byte) RuneOperationResponseParameter.CommandResponse] = SerializeBSON(commandContext.Response);

                        parameters[(byte)RuneOperationResponseParameter.OperationErrors] = SerializeBSON(commandContext.OperationErrors);
                        parameters[(byte)RuneOperationResponseParameter.PropertyErrors] = SerializeBSON(commandContext.PropertyErrors);

                        SendOperationResponse(new OperationResponse((byte)RuneOperationResponse.CommandDispatched, parameters), sendParameters);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        SendOperationResponse(new OperationResponse((byte)RuneOperationResponse.FatalError), sendParameters);

                        trans.Rollback();
                        Log.ErrorFormat("Error processing operation{0}: {1}", operationRequest.OperationCode, ex);
                    }
                }
            }
        }

        private byte[] SerializeBSON(object obj)
        {
            using (var ms = new MemoryStream())
            {
                _jsonSerializer.Serialize(new BsonWriter(ms), obj);
                return ms.ToArray();
            }
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            Log.InfoFormat("Peer disconnected: {0}, {1}",reasonCode, reasonDetail);
            _application.DestroyPeer(this); 
        }

        
    } 
}