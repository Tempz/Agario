/*
	Copyright (C) 2015 Tempz@users.noreply.github.com

	This file is part of https://github.com/Tempz/Agario

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using Agario.Http;
using Agario.Model;
using WebSocketSharp;

namespace Agario
{
    /// <summary>
    /// A socket connector for Agario.
    /// </summary>
    public sealed class SocketConnector
    {
        public delegate void OnReadyEventHandler(World world);
        public event OnReadyEventHandler OnReady;

        public delegate void OnUpdateEventHandler(World world);
        public event OnUpdateEventHandler OnUpdate;

        public delegate void OnErrorEventHandler(Exception exception);
        public event OnErrorEventHandler OnError;

        public delegate void OnCloseEventHandler(SocketConnector socketConnector, CloseEventArgs closeEventArgs);
        public event OnCloseEventHandler OnClose;

        public delegate void OnDiedEventHandler(World world);
        public event OnDiedEventHandler OnDied;

        /// <summary>
        /// The world of the game.
        /// </summary>
        public World World => _world;

        /// <summary>
        /// The server info of the game.
        /// </summary>
        public readonly ServerInfo ServerInfo;

        /// <summary>
        /// Whether the connector is connected to the Agario server.
        /// </summary>
        public bool IsConnected => _webSocket?.IsAlive ?? false;

        private readonly World _world;
        private readonly WebSocket _webSocket;
        private bool _connectionReady = false;

        internal SocketConnector(WebSocket webSocket, World world, ServerInfo serverInfo)
        {
            ServerInfo = serverInfo;

            _webSocket = webSocket;
            _world = world;

            webSocket.OnOpen += (sender, args) => WebSocketOnOpen(serverInfo);
            webSocket.OnMessage += (sender, args) => WebSocketOnMessage(args);
            webSocket.OnError += (sender, args) => WebSocketOnError(args.Exception);
            webSocket.OnClose += (sender, args) => WebSocketOnClose(args);
        }

        /// <summary>
        /// Initiates the connection.
        /// </summary>
        public void Connect()
        {
            _webSocket?.Connect();
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            _webSocket?.Close();
        }

        internal void WebSocketOnOpen(ServerInfo serverInfo)
        {
            //Step 1
            byte[] buffer = { 254, 4 };

            _webSocket.Send(buffer);

            //Step 2
            buffer = new byte[5];
            buffer[0] = 255;
            byte[] tempU = BitConverter.GetBytes(154669603u);
            if (!BitConverter.IsLittleEndian) Array.Reverse(tempU);
            Array.Copy(tempU, 0, buffer, 1, 4);

            _webSocket.Send(buffer);

            //Step 3
            buffer = new byte[1 + serverInfo.Token.Length];
            buffer[0] = 80;
            for (int i = 0; i < serverInfo.Token.Length; i++)
            {
                buffer[i + 1] = (byte)serverInfo.Token[i];
            }

            _webSocket.Send(buffer);

            //Finish
            _connectionReady = false;
        }

        internal void WebSocketOnMessage(MessageEventArgs messageEventArgs)
        {
            Packet packet = new Packet(messageEventArgs.RawData);
            byte id = packet.ReadByte();

            switch (id)
            {
                case 16:
                    _world.ProcessTick(packet, this);
                    break;
                case 17:
                    _world.ProcessSpectate(packet);
                    break;
                case 20:
                    //Nothing important happens here.
                    break;
                case 32:
                    _world.ProcessSpawn(packet);
                    break;
                case 49:
                    _world.ProcessFfaScores(packet);
                    break;
                case 50:
                    _world.ProcessTeamScores(packet);
                    break;
                case 64:
                    _world.ProcessMapSize(packet);
                    break;
                case 72:
                    OnError?.Invoke(new InvalidOperationException("Unknown packet: " + id));
                    break;
                case 81:
                    //Update experience when logged in.
                    break;
                case 240:
                    //TODO
                    break;
                case 254:
                    //Somebody won the game. Not gonna handle this.
                    break;
                default:
                    OnError?.Invoke(new InvalidOperationException("Unknown packet: " + id));
                    break;
            }

            if (!_connectionReady)
            {
                _connectionReady = true;
                OnReady?.Invoke(_world);
            }
            else
            {
                OnUpdate?.Invoke(_world);
            }
        }

        internal void WebSocketOnError(Exception exception)
        {
            OnError?.Invoke(exception);
        }

        internal void WebSocketOnClose(CloseEventArgs closeEventArgs)
        {
            OnClose?.Invoke(this, closeEventArgs);
        }

        internal void UserDied()
        {
            OnDied?.Invoke(_world);
        }
    }
}
