
namespace Photon.Realtime
{
    using System;
    using Photon.Client;


    /// <summary>Container for port definitions.</summary>
    public class ProtocolPorts
    {
        public ServerPorts Udp = new ServerPorts() { NameServer = 27000, MasterServer = 27001, GameServer = 27002 };
        public ServerPorts Tcp = new ServerPorts() { NameServer = 4533 };
        public ServerPorts Ws = new ServerPorts() { NameServer = 80 };
        public ServerPorts Wss = new ServerPorts() { NameServer = 443 };

        public ushort Get(ConnectionProtocol protocol, ServerConnection serverType)
        {
            switch (protocol)
            {
                case ConnectionProtocol.Udp:
                    return this.Udp.Get(serverType);
                case ConnectionProtocol.Tcp:
                    return this.Tcp.Get(serverType);
                case ConnectionProtocol.WebSocket:
                    return this.Ws.Get(serverType);
                case ConnectionProtocol.WebSocketSecure:
                    return this.Wss.Get(serverType);
            }

            throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null);
        }

        public void SetUdpDefault()
        {
            this.Udp = new ServerPorts() { NameServer = 27000, MasterServer = 27001, GameServer = 27002 };
        }
        public void SetUdpDefaultOld()
        {
            this.Udp = new ServerPorts() { NameServer = 5058 };
        }
        public void SetTcpDefault()
        {
            this.Tcp = new ServerPorts() { NameServer = 4533 };
        }
        public void SetWsDefault()
        {
            this.Ws = new ServerPorts() { NameServer = 80 };
        }
        public void SetWsDefaultOld()
        {
            this.Ws = new ServerPorts() { NameServer = 9093, MasterServer = 9090, GameServer = 9091};
        }
        public void SetWssDefault()
        {
            this.Wss = new ServerPorts() { NameServer = 433 };
        }
        public void SetWssDefaultOld()
        {
            this.Wss = new ServerPorts() { NameServer = 19093 };
        }

        public void SetOldDefaults()
        {
            this.SetUdpDefaultOld();
            this.SetWsDefaultOld();
            this.SetWssDefaultOld();
        }
    }

    public struct ServerPorts
    {
        /// <summary>Typical ports: UDP: 5058 or 27000, TCP: 4533, WSS: 19093 or 443.</summary>
        public ushort NameServer;

        /// <summary>Typical ports: UDP: 5056 or 27002, TCP: 4530, WSS: 19090 or 443.</summary>
        public ushort MasterServer;

        /// <summary>Typical ports: UDP: 5055 or 27001, TCP: 4531, WSS: 19091 or 443.</summary>
        public ushort GameServer;

        public ushort Get(ServerConnection serverType)
        {
            switch (serverType)
            {
                case ServerConnection.NameServer:
                    return this.NameServer;
                case ServerConnection.MasterServer:
                    return this.MasterServer;
                case ServerConnection.GameServer:
                    return this.GameServer;
            }
            throw new ArgumentOutOfRangeException(nameof(serverType), serverType, null);
        }
    }
}