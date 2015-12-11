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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agario.Http;
using Agario.Model;
using WebSocketSharp;

namespace Agario
{
    /// <summary>
    /// An Agario world.
    /// </summary>
    public sealed class World
    {
        /// <summary>
        /// The X position of the player's latest primary ball.
        /// </summary>
        public float X => MainBall?.X ?? _x;
        private float _x = 0f;

        /// <summary>
        /// The Y position of the player's latest primary ball.
        /// </summary>
        public float Y => MainBall?.Y ?? _y;
        private float _y = 0f;

        /// <summary>
        /// The size of the player's latest primary ball.
        /// </summary>
        public int Size => _myBalls.Sum(pair => pair.Value.Size);

        /// <summary>
        /// The zoom of the current game. Used for spectator mode or when your mass increases.
        /// </summary>
        public float Zoom { get; private set; }

        /// <summary>
        /// The X position of the left bound of the world.
        /// </summary>
        public double MinX { get; private set; }

        /// <summary>
        /// The Y position of the top bound of the world.
        /// </summary>
        public double MinY { get; private set; }

        /// <summary>
        /// The X position of the right bound of the world.
        /// </summary>
        public double MaxX { get; private set; }

        /// <summary>
        /// The Y position of the bottom bound of the world.
        /// </summary>
        public double MaxY { get; private set; }

        /// <summary>
        /// The names of the players in the leaderboard.
        /// </summary>
        public string[] FfaLeaderboard { get; private set; }

        /// <summary>
        /// The scores of the teams.
        /// </summary>
        public float[] TeamScores { get; private set; }

        /// <summary>
        /// The player's primary ball, or null if there are no balls.
        /// </summary>
        public Ball MainBall => _mainBall ?? (_mainBall = _myBalls.FirstOrDefault().Value);
        private Ball _mainBall;

        /// <summary>
        /// The balls of the current player.
        /// </summary>
        public Dictionary<uint, Ball> MyBalls
        {
            get
            {
                Dictionary<uint, Ball> dictionary;
                lock (_balls)
                {
                    dictionary = new Dictionary<uint, Ball>(_myBalls);
                }
                return dictionary;
            }
        }

        private readonly Dictionary<uint, Ball> _myBalls = new Dictionary<uint, Ball>();

        /// <summary>
        /// All the visible balls (including yours).
        /// </summary>
        public Dictionary<uint, Ball> Balls
        {
            get
            {
                Dictionary<uint, Ball> dictionary;
                lock(_balls)
                {
                    dictionary = new Dictionary<uint, Ball>(_balls);
                }
                return dictionary;
            }
        }

        private readonly Dictionary<uint, Ball> _balls = new Dictionary<uint, Ball>();

        private readonly WebSocket _webSocket;

        public World(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        /// <summary>
        /// Changes the mode to spectator mode.
        /// </summary>
        public void Spectate() => _webSocket.Send(new byte[] { 1 });

        /// <summary>
        /// Splits your balls.
        /// </summary>
        public void Split() => _webSocket.Send(new byte[] { 17 });

        /// <summary>
        /// Eject mass.
        /// </summary>
        public void Eject() => _webSocket.Send(new byte[] { 21 });

        /// <summary>
        /// Set the movement target to a position.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        public void MoveTo(int x, int y)
        {
            byte[] buffer = new byte[13];
            buffer[0] = 16;
            Array.Copy(BitConverter.GetBytes(x), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(y), 0, buffer, 5, 4);
            //9-12 = 0

            _webSocket.Send(buffer);
        }

        /// <summary>
        /// Spawn the player.
        /// </summary>
        /// <param name="name">The name of the player.</param>
        public void Spawn(string name)
        {
            var buffer = new byte[1 + 2 * name.Length];
            //buffer[0] = 0; //In original, but useless here
            byte[] nameBuffer = Encoding.Unicode.GetBytes(name);
            Array.Copy(nameBuffer, 0, buffer, 1, nameBuffer.Length);

            _webSocket.Send(buffer);
        }

        internal void ProcessSpectate(Packet packet)
        {
            _x = packet.ReadFloat();
            _y = packet.ReadFloat();
            Zoom = packet.ReadFloat();
        }

        internal void ProcessTick(Packet packet, SocketConnector socketConnector)
        {
            lock(_balls)
            {
                lock (_myBalls)
                {
                    //Reading eat events
                    ushort eatersCount = packet.ReadUShort();
                    for (uint i = 0; i < eatersCount; i++)
                    {
                        //uint eaterId = packet.ReadUInt();
                        packet.Index += 4;

                        uint eatenId = packet.ReadUInt();

                        if (_balls.ContainsKey(eatenId))
                            _balls.Remove(eatenId);

                        if (_myBalls.ContainsKey(eatenId))
                        {
                            _myBalls.Remove(eatenId);

                            if (_mainBall?.Id == eatenId)
                                _mainBall = null;

                            _balls.Clear();

                            if (_myBalls.Count == 0)
                            {
                                socketConnector.UserDied();
                            }
                        }
                    }

                    //Reading actions of balls
                    uint ballId;
                    while ((ballId = packet.ReadUInt()) != 0u)
                    {
                        int x = packet.ReadInt();
                        int y = packet.ReadInt();
                        short size = packet.ReadShort();

                        byte r = packet.ReadByte();
                        byte g = packet.ReadByte();
                        byte b = packet.ReadByte();

                        byte opt = packet.ReadByte();
                        bool virus = (opt & 1) != 0;

                        if ((opt & 2) != 0)
                        {
                            packet.Index += (int)packet.ReadUInt();
                        }

                        if ((opt & 4) != 0)
                        {
                            string unknown = "";
                            byte unknownChar;
                            while ((unknownChar = packet.ReadByte()) != 0)
                            {
                                unknown += (char)unknownChar;
                            }
                        }

                        if (_myBalls.ContainsKey(ballId)) { }

                        string name = "";
                        ushort @char;
                        while ((@char = packet.ReadUShort()) != 0u)
                        {
                            name += (char)@char;
                        }

                        if (!_balls.ContainsKey(ballId))
                            _balls.Add(ballId, new Ball(ballId));

                        Ball ball = _balls[ballId];
                        ball.X = x;
                        ball.Y = y;
                        ball.Size = size;
                        ball.R = r;
                        ball.G = g;
                        ball.B = b;
                        ball.IsVirus = virus;

                        if (ball.Name == null || name.Length > 0)
                            ball.Name = name;
                    }

                    //Disappear events
                    uint ballsOnScreen = packet.ReadUInt();
                    for (uint i = 0; i < ballsOnScreen; i++)
                    {
                        uint removedBallId = packet.ReadUInt();

                        if (_balls.ContainsKey(removedBallId))
                            _balls.Remove(removedBallId);
                        if (_myBalls.ContainsKey(removedBallId))
                            _myBalls.Remove(removedBallId);
                    }
                }
            }
        }

        internal void ProcessSpawn(Packet packet)
        {
            lock(_balls)
            {
                lock (_myBalls)
                {
                    uint ballId = packet.ReadUInt();

                    if (!_balls.ContainsKey(ballId))
                        _balls.Add(ballId, new Ball(ballId));

                    Ball ball = _balls[ballId];
                    ball.IsMine = true;

                    if (!_myBalls.ContainsKey(ballId))
                        _myBalls.Add(ballId, ball);
                }
            }
        }

        internal void ProcessFfaScores(Packet packet)
        {
            uint scoreCount = packet.ReadUInt();
            string[] scores = new string[scoreCount];

            for (uint i = 0; i < scoreCount; i++)
            {
                //uint id = packet.ReadUInt();
                packet.Index += 4;

                string name = "";
                ushort @char;
                while ((@char = packet.ReadUShort()) != 0u)
                {
                    name += (char)@char;
                }

                scores[i] = name;
            }

            FfaLeaderboard = scores;
        }

        internal void ProcessTeamScores(Packet packet)
        {
            uint teamsCount = packet.ReadUInt();
            float[] teamScores = new float[teamsCount];

            for (uint i = 0; i < teamsCount; i++)
            {
                teamScores[i] = packet.ReadFloat();
            }

            TeamScores = teamScores;
        }

        internal void ProcessMapSize(Packet packet)
        {
            MinX = packet.ReadDouble();
            MinY = packet.ReadDouble();
            MaxX = packet.ReadDouble();
            MaxY = packet.ReadDouble();
        }
    }
}
