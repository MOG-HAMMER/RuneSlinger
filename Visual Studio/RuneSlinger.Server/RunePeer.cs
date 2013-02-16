using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Linq;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using RuneSlinger.Base;
using RuneSlinger.Server.Entities;
using RuneSlinger.Server.ValueObjects;
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
            using (var session = _application.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        var opCode = (RuneOperationCode) operationRequest.OperationCode;

                        if (opCode == RuneOperationCode.Register)
                        {
                            var username = (string) operationRequest.Parameters[(byte) RuneOperationCodeParameter.Username];
                            var password = (string) operationRequest.Parameters[(byte) RuneOperationCodeParameter.Password];
                            var email = (string) operationRequest.Parameters[(byte) RuneOperationCodeParameter.Email];
                            Register(session, username, password, email);
                        }
                        else if (opCode == RuneOperationCode.Login)
                        {
                            var password = (string) operationRequest.Parameters[(byte) RuneOperationCodeParameter.Password];
                            var email = (string) operationRequest.Parameters[(byte) RuneOperationCodeParameter.Email];
                            Login(session, password, email);
                        }
                        else if (opCode == RuneOperationCode.SendMessage)
                        {
                            var message = (string) operationRequest.Parameters[(byte) RuneOperationCodeParameter.Message];
                            SendMessage(session, message);
                        }
                        else
                        {
                            SendOperationResponse(new OperationResponse((byte)RuneOperationResponse.Invalid), sendParameters);
                        }

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

        private void Register(ISession session, string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                SendError("All fields are required.");
                return;
            }

            if (username.Length > 128)
            {
                SendError("Username must be less than 128 characters long.");
                return;
            }

            if (email.Length > 200)
            {
                SendError("Email must be less than 200 characters long.");
                return;
            }

            if (session.Query<User>().Any(t => t.Username == username || t.Email == email))
            {
                SendError("Username and email must be unique!");
                return;
            }

            var user = new User
            {
                Username = username,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                Password = HashedPassword.FromPlaintext(password)
            };

            session.Save(user);
            SendSuccess();
        }

        private void Login(ISession session, string password, string email)
        {
            var user = session.Query<User>().SingleOrDefault(s => s.Email == email);
            if (user == null || !user.Password.EqualsPlaintext(password))
            {
                SendError("Email or password was incorrect.");
                return;
            }

            SendSuccess();
        }

        private void SendMessage(ISession session, string message)
        {
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            Log.InfoFormat("Peer disconnected: {0}, {1}",reasonCode, reasonDetail);
            _application.DestroyPeer(this); 
        }

        private void SendSuccess()
        {
            SendOperationResponse(new OperationResponse((byte) RuneOperationResponse.Success),
                                  new SendParameters {Unreliable = false});

        }

        private void SendError(string message)
        {
            SendOperationResponse(new OperationResponse((byte)RuneOperationResponse.Error, new Dictionary<byte, object>
            {
                {(byte) RuneOperationResponseParameter.ErrorMessage, message}
            }), new SendParameters
            {
                Unreliable = false
            });

        }
    } 
}