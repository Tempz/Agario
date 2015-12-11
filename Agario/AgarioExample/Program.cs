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
using System.Linq;
using Agario;

namespace AgarioExample
{
    static class Program
    {
        static void Main(string[] args)
        {
            var serverLocations = Game.FetchServerLocations();
            var serverInfo = serverLocations.First().FetchServerInfo();
            var partyServer = serverLocations.Skip(1).First().CreateParty();
            var validParty = Game.VerifyParty(partyServer);

            var connector = serverInfo.CreateSocket();
            var connector2 = partyServer.CreateSocket();
            var connector3 = Game.CreateSocket(partyServer.Token);

            Action<World> spawn = world => world.Spawn("AgarioExample");
            connector.OnReady += world =>
            {
                Console.WriteLine("Ready");
                spawn(world);
            };
            connector.OnDied += world =>
            {
                Console.WriteLine("Died");
                spawn(world);
            };
            connector.OnUpdate += world => world.MoveTo(0, 0);
            connector.OnError += exception => Console.WriteLine("Error: " + exception.Message);
            connector.OnClose += (socketConnector, eventArgs) => Console.WriteLine("Close: " + eventArgs);

            Console.WriteLine("Server locations:");
            Console.WriteLine(string.Join("\n", serverLocations.Select(location => location.ToString())));
            Console.WriteLine();

            Console.WriteLine("Server info:");
            Console.WriteLine(serverInfo);
            Console.WriteLine();

            Console.WriteLine("Party server:");
            Console.WriteLine(partyServer);
            Console.WriteLine();

            Console.WriteLine("Party server from connector 2:");
            Console.WriteLine(connector2.ServerInfo);
            Console.WriteLine();

            Console.WriteLine("Party server from connector 3:");
            Console.WriteLine(connector3.ServerInfo);
            Console.WriteLine();

            Console.WriteLine("Valid Party:");
            Console.WriteLine(validParty);
            Console.WriteLine();

            Console.WriteLine("Connecting to server.");
            connector.Connect();

            DateTime dateTimeCounter = DateTime.UtcNow;
            while (connector.IsConnected)
            {
                DateTime currentDateTime = DateTime.UtcNow;
                if (currentDateTime.Subtract(dateTimeCounter).TotalMilliseconds >= 1000)
                {
                    dateTimeCounter = currentDateTime;

                    Console.WriteLine(connector.World.MainBall?.ToString() ?? "Player not spawned");
                }

                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
