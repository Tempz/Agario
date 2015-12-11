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
using System.Drawing;

namespace Agario.Bot
{
    /// <summary>
    /// Provides trigonometry implementations.
    /// </summary>
    internal static class Trigonometry
    {
        private const float RadianDegreeConversionNumber = 180f / (float)Math.PI;

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The degrees to convert.</param>
        /// <returns></returns>
        public static float ToRadians(this float degrees) => degrees / RadianDegreeConversionNumber;

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static float ToDegrees(this float radians) => radians * RadianDegreeConversionNumber;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="angle"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static PointF RotateCoordinate(float x, float y, float angle, float distance)
        {
            return new PointF(
                (float)(Math.Cos(angle) * distance + x),
                (float)(Math.Sin(angle) * distance + y)
            );
        }
    }
}
