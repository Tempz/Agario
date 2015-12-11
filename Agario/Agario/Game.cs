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
using System.Text.RegularExpressions;
using Agario.Model;
using Newtonsoft.Json;
using WebSocketSharp;
using HttpClient = Agario.Http.HttpClient;

namespace Agario
{
    /// <summary>
    /// Provides server locations/info and prepares connections.
    /// </summary>
    public static class Game
    {
        private const string AgarioHost = "http://agar.io";
        private const string AgarioMobileHost = "http://m.agar.io";
        private static readonly Uri ServerListUri = new Uri(AgarioMobileHost + "/info");
        private static readonly Uri FindServerUri = new Uri(AgarioMobileHost + "/findServer");
        private static readonly Uri GetTokenUri = new Uri(AgarioMobileHost + "/getToken");

        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Fetch the different Agario server locations.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ServerLocation> FetchServerLocations()
        {
            dynamic data = JsonConvert.DeserializeObject(HttpClient.SendGet(ServerListUri).ResponseData);
            IEnumerable<dynamic> regions = (IEnumerable<dynamic>)data["regions"];

            return regions.Select(region => new ServerLocation(region.Name, (int)region.Value["numPlayers"], (int)region.Value["numRealms"], (int)region.Value["numServers"]));
        }

        /// <summary>
        /// Fetch the info of a server location.
        /// </summary>
        /// <param name="serverLocation">The server location.</param>
        /// <returns></returns>
        public static ServerInfo FetchServerInfo(this ServerLocation serverLocation) => FetchServerInfoInternal(serverLocation, serverLocation.Name);

        /// <summary>
        /// Creates a new party.
        /// </summary>
        /// <param name="serverLocation">The location of the server.</param>
        /// <returns></returns>
        public static PartyServer CreateParty(this ServerLocation serverLocation) => new PartyServer(FetchServerInfoInternal(serverLocation, serverLocation.PartyName));

        /// <summary>
        /// Verifier a party.
        /// </summary>
        /// <param name="partyServer">The party server.</param>
        /// <returns></returns>
        public static bool VerifyParty(PartyServer partyServer) => VerifyParty(partyServer.Token);

        /// <summary>
        /// Verifies a party.
        /// </summary>
        /// <param name="partyToken">The party token.</param>
        /// <returns></returns>
        public static bool VerifyParty(string partyToken)
        {
            string id = Regex.Match(partyToken, "(?:.*?/)*(?:[#]*)([A-Z0-9]{1,6})").Groups[1].Value;

            try
            {
                HttpClient.SendPost(GetTokenUri, id, "application/x-www-form-urlencoded");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prepares an Agario connection from a party server.
        /// </summary>
        /// <param name="partyServer">The party server.</param>
        /// <returns></returns>
        public static SocketConnector CreateSocket(this PartyServer partyServer) => CreateSocket(partyServer.Token, partyServer.ServerInfo.ServerLocation);

        /// <summary>
        /// Prepares an Agario connection from a party token.
        /// </summary>
        /// <param name="partyToken">The party token.</param>
        /// <returns></returns>
        public static SocketConnector CreateSocket(string partyToken) => CreateSocket(partyToken, null);

        private static SocketConnector CreateSocket(string partyToken, ServerLocation serverLocation)
        {
            string id = Regex.Match(partyToken, "(?:.*?/)*(?:[#]*)([A-Z0-9]{1,6})").Groups[1].Value;
            string ip = HttpClient.SendPost(GetTokenUri, id, "application/x-www-form-urlencoded").ResponseData.Split('\n')[0];

            return CreateSocket(new ServerInfo(ip, partyToken, serverLocation));
        }

        /// <summary>
        /// Prepares an Agario connection from server info.
        /// </summary>
        /// <param name="serverInfo">The server info.</param>
        /// <returns></returns>
        public static SocketConnector CreateSocket(this ServerInfo serverInfo)
        {
            WebSocket webSocket = new WebSocket("ws://" + serverInfo.Ip)
            {
                Origin = AgarioHost
            };

            return new SocketConnector(webSocket, new World(webSocket), serverInfo);
        }

        private static ServerInfo FetchServerInfoInternal(this ServerLocation serverLocation, string name)
        {
            dynamic data = JsonConvert.DeserializeObject(HttpClient.SendPost(FindServerUri, name, "application/x-www-form-urlencoded").ResponseData);

            return new ServerInfo(data["ip"].ToString(), data["token"].ToString(), serverLocation);
        }
    }
}
