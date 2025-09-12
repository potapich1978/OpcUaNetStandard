using System;
using Opc.Ua;
using Opc.Ua.Client;
using OpcSessionFactory.Abstract;

namespace OpcSessionFactory
{
    /// <remarks>
    /// Represents a managed OPC UA session that supports 
    /// keep-alive monitoring, automatic reconnection, 
    /// and proper resource disposal.
    /// </remarks>
    internal sealed class OpcUaSession : IOpcUaSession, IDisposable
    {
        /// <inheritdoc />
        /// <remarks>
        /// The active OPC UA session instance used for communication with the server.
        /// </remarks>
        public ISession Session { get; private set; }

        private readonly object _sync = new object();
        private SessionReconnectHandler _reconnectHandler;
        private KeepAliveEventHandler _keepAliveHandler;

        /// <remarks>
        /// Creates a new session wrapper and attaches a keep-alive handler 
        /// to monitor session health.
        /// </remarks>
        public OpcUaSession(ISession session)
        {
            Attach(session);
        }

        /// <remarks>
        /// Attaches a keep-alive handler to the given session 
        /// while detaching any previously registered handlers.
        /// </remarks>
        private void Attach(ISession session)
        {
            if (Session != null && _keepAliveHandler != null)
                try { Session.KeepAlive -= _keepAliveHandler; } catch { /* no-op */ }

            Session = session;

            if (_keepAliveHandler == null)
                _keepAliveHandler = OnKeepAlive;

            Session.KeepAlive += _keepAliveHandler;
        }

        /// <remarks>
        /// Handles keep-alive events from the OPC UA session. 
        /// Initiates a reconnection process if the session is deemed unhealthy.
        /// </remarks>
        private void OnKeepAlive(ISession session, KeepAliveEventArgs e)
        {
            if (e == null) return;

            if (_reconnectHandler != null)  return;

            if (ServiceResult.IsBad(e.Status))
            {
                lock (_sync)
                {
                    if (_reconnectHandler == null)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] KeepAlive issue. BeginReconnect...");

                        _reconnectHandler = new SessionReconnectHandler();
                        _reconnectHandler.BeginReconnect(
                            session,
                            10_000, 
                            OnReconnectComplete
                        );
                    }
                }
            }
        }

        /// <remarks>
        /// Finalizes the reconnection process by replacing the current session 
        /// with the reconnected instance and releasing the reconnect handler.
        /// </remarks>
        private void OnReconnectComplete(object sender, EventArgs args)
        {
            try
            {
                lock (_sync)
                {
                    if (!ReferenceEquals(sender, _reconnectHandler)) return; 

                    var session = _reconnectHandler.Session;
                    _reconnectHandler.Dispose();
                    _reconnectHandler = null;

                    if (session != null)
                    {
                        Session = session; 
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Reconnected. " +
                                          $"SessionId = {Session.SessionId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnect error: {ex.Message}");
            }
        }

        /// <remarks>
        /// Releases all resources associated with the session, 
        /// including event handlers, subscriptions, and the reconnect handler.
        /// </remarks>
        public void Dispose()
        {
            if (Session != null && _keepAliveHandler != null)
                try { 
                    Session.KeepAlive -= _keepAliveHandler;
                    foreach (var subscription in Session.Subscriptions)
                        subscription?.Dispose();
                } catch { /* no-op */ }

            try { _reconnectHandler?.Dispose(); } catch { /* no-op */ }
            _reconnectHandler = null;

            

            try
            {
                if (Session != null)
                {
                    try { Session.Close(); }
                    catch (Exception exClose)
                    { Console.WriteLine($"Error closing session: {exClose.Message}"); }

                    try { Session.Dispose(); }
                    catch (Exception exDisp)
                    { Console.WriteLine($"Error disposing session: {exDisp.Message}"); }
                }
            }
            finally
            {
                Session = null;
            }
        }
    }
}
