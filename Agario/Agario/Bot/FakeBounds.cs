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
using Agario.Model;

namespace Agario.Bot
{
    /// <summary>
    /// Provides world bounds as enemies.
    /// </summary>
    public static class FakeBounds
    {
        private static readonly Ball LeftBound;
        private static readonly Ball RightBount;
        private static readonly Ball TopBound;
        private static readonly Ball BottomBound;

        private static readonly IEnumerable<Ball> Bounds;

        static FakeBounds()
        {
            LeftBound = new Ball(0);
            RightBount = new Ball(1);
            TopBound = new Ball(2);
            BottomBound = new Ball(3);

            Bounds = new List<Ball>()
            {
                LeftBound,
                RightBount,
                TopBound,
                BottomBound,
            };
        }

        /// <summary>
        /// Provides fake world bounds as enemies.
        /// </summary>
        /// <param name="world">The world for which the bounds are calculated.</param>
        /// <returns></returns>
        public static IEnumerable<Ball> GetBounds(World world)
        {
            LeftBound.X = (int)world.MinX;
            LeftBound.Y = (int)world.Y;
            LeftBound.Size = (short)Math.Min(world.Size, short.MaxValue);

            RightBount.X = (int)world.MaxX;
            RightBount.Y = (int)world.Y;
            RightBount.Size = LeftBound.Size;

            TopBound.X = (int)world.X;
            TopBound.Y = (int)world.MinY;
            TopBound.Size = LeftBound.Size;

            BottomBound.X = (int)world.X;
            BottomBound.Y = (int)world.MaxY;
            BottomBound.Size = LeftBound.Size;

            return Bounds;
        }
    }
}
