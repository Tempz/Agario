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

namespace Agario.Bot.Geometry
{
    /// <summary>
    /// An arc with degrees and radians support.
    /// </summary>
    public sealed class Arc
    {
        /// <summary>
        /// The arc's start angle in degrees.
        /// </summary>
        public float StartAngleDegrees { get; internal set; }

        /// <summary>
        /// The arc's start angle in radians.
        /// </summary>
        public float StartAngleRadians => StartAngleDegrees.ToRadians();

        /// <summary>
        /// The arc's angle in degrees.
        /// </summary>
        public float AngleDegrees { get; internal set; }

        /// <summary>
        /// The arc's angle in radians.
        /// </summary>
        public float AngleRadians => AngleDegrees.ToRadians();

        /// <summary>
        /// The arc's end angle in degrees.
        /// </summary>
        public float EndAngleDegrees => StartAngleDegrees + AngleDegrees;

        /// <summary>
        /// The arc's end angle in radians.
        /// </summary>
        public float EndAngleRadians => EndAngleDegrees.ToRadians();

        /// <summary>
        /// Creates a new arc and inverts it if the angle is negative.
        /// </summary>
        /// <param name="startAngle">The start angle of the arc.</param>
        /// <param name="angle">The angle of the arc.</param>
        /// <param name="degrees">Whether startAngle and angle are degrees or radians.</param>
        public Arc(float startAngle, float angle, bool degrees = true)
        {
            if (degrees)
            {
                StartAngleDegrees = startAngle;
                AngleDegrees = angle;
            }
            else
            {
                StartAngleDegrees = startAngle.ToDegrees();
                AngleDegrees = angle.ToDegrees();
            }

            if (ShouldInvertArc(this))
            {
                InvertArc(this);
            }
        }

        /// <summary>
        /// Convert an arc from a start and end angle, and inverts it if the angle is negative.
        /// </summary>
        /// <param name="startAngle">The start angle of the arc.</param>
        /// <param name="endAngle">The end angle of the arc.</param>
        /// <returns></returns>
        public static Arc FromAngles(float startAngle, float endAngle)
        {
            float tempAngle = Math.Abs(endAngle - startAngle) % 360f;
            float angle = tempAngle > 180f ? 360f - tempAngle : tempAngle;

            Arc arc = new Arc(startAngle, angle);

            if (ShouldInvertArc(arc))
            {
                InvertArc(arc);
            }

            return arc;
        }

        private static bool ShouldInvertArc(Arc arc) => arc.AngleDegrees < 0f;

        private static Arc InvertArc(Arc arc)
        {
            arc.StartAngleDegrees = arc.EndAngleDegrees;
            arc.AngleDegrees = -arc.AngleDegrees;

            if (arc.StartAngleDegrees < 0f)
            {
                arc.StartAngleDegrees = 360f - arc.StartAngleDegrees;
            }

            return arc;
        }
    }
}