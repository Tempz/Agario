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
using System.Diagnostics;

namespace Agario.Model
{
    /// <summary>
    /// An Agario server.
    /// </summary>
    [DebuggerDisplay("{Ip}, {Token}")]
    public sealed class ServerInfo
    {
        /// <summary>
        /// The IP address of the server.
        /// </summary>
        public readonly string Ip;

        /// <summary>
        /// The handshake token of the server.
        /// </summary>
        public readonly string Token;

        /// <summary>
        /// The location of the server.
        /// </summary>
        public readonly ServerLocation ServerLocation;

        internal ServerInfo(string ip, string token, ServerLocation serverLocation)
        {
            Ip = ip;
            Token = token;
            ServerLocation = serverLocation;
        }

        public override string ToString() => $"{{ {nameof(Ip)}: {Ip}, {nameof(Token)}: {Token}, {nameof(ServerLocation)}: {ServerLocation?.ToString() ?? "Cannot resolve " + nameof(ServerLocation) + " from a party " + nameof(Token)} }}";
    }
}
