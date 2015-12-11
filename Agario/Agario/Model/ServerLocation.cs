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
    /// An agario location ServerLocation.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public sealed class ServerLocation
    {
        /// <summary>
        /// The name/location of the ServerLocation.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The amount of players connected to the ServerLocation.
        /// </summary>
        public readonly int Players;
        
        /// <summary>
        /// No idea.
        /// </summary>
        public readonly int Realms;

        /// <summary>
        /// The amount of agario games running in the ServerLocation.
        /// </summary>
        public readonly int Servers;

        internal string PartyName => Name + ":party";

        internal ServerLocation(string name, int players, int realms, int servers)
        {
            Name = name;
            Players = players;
            Realms = realms;
            Servers = servers;
        }

        public override string ToString() => $"{{ {nameof(Name)}: {Name}, {nameof(Players)}: {Players}, {nameof(Realms)}: {Realms}, {nameof(Servers)}: {Servers} }}";
    }
}
