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
namespace Agario.Model
{
    /// <summary>
    /// An Agario party server.
    /// </summary>
    public sealed class PartyServer
    {
        /// <summary>
        /// The server info of the party.
        /// </summary>
        public readonly ServerInfo ServerInfo;

        /// <summary>
        /// The token of the party.
        /// </summary>
        public string Token => ServerInfo.Token;

        internal PartyServer(ServerInfo serverInfo)
        {
            ServerInfo = serverInfo;
        }

        public override string ToString() => $"{{ {nameof(ServerInfo)}: {ServerInfo} }}";
    }
}
