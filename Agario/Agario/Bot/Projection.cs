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
using Agario.Model;

namespace Agario.Bot
{
    /// <summary>
    /// Provides agario to screen implementations.
    /// </summary>
    public static class Projection
    {
        /// <summary>
        /// Projects a ball from agario to screen coodinates.
        /// </summary>
        /// <param name="ball">The ball to project.</param>
        /// <param name="world">The world to project from.</param>
        /// <param name="screenWidth">The width of the canvas.</param>
        /// <param name="screenHeight">The height of the canvas.</param>
        /// <param name="viewAngleX">The horizontal view angle of your agario game.</param>
        /// <param name="viewAngleY">The vertical view angle of your agario game.</param>
        /// <returns></returns>
        public static PointF ProjectToScreen(this Ball ball, World world, int screenWidth, int screenHeight, float viewAngleX = 1000f, float viewAngleY = 600f)
        {
            return ProjectToScreen(ball.X, ball.Y, world.X, world.Y, screenWidth, screenHeight, viewAngleX, viewAngleY);
        }

        /// <summary>
        /// Projects a ball from agario to screen coodinates.
        /// </summary>
        /// <param name="ballX">The X position the ball.</param>
        /// <param name="ballY">The Y position of the ball.</param>
        /// <param name="worldX">The X position of your world.</param>
        /// <param name="worldY">The Y positon of your world.</param>
        /// <param name="screenWidth">The width of the canvas.</param>
        /// <param name="screenHeight">The height of the canvas.</param>
        /// <param name="viewAngleX">The horizontal view angle of your agario game.</param>
        /// <param name="viewAngleY">The vertical view angle of your agario game.</param>
        /// <returns></returns>
        public static PointF ProjectToScreen(float ballX, float ballY, float worldX, float worldY, int screenWidth, int screenHeight, float viewAngleX = 1000f, float viewAngleY = 600f)
        {
            return new PointF(
                (float)Math.Floor(((ballX - worldX) / viewAngleX / 2d + 0.5d) * screenWidth),
                (float)Math.Floor(((ballY - worldY) / viewAngleY / 2d + 0.5d) * screenHeight)
            );
        }

        /// <summary>
        /// Project a ball for it's diameter.
        /// </summary>
        /// <param name="ball">The ball to project.</param>
        /// <param name="world">The world to project from.</param>
        /// <param name="screenWidth">The width of the canvas.</param>
        /// <param name="screenHeight">The height of the canvas.</param>
        /// <returns></returns>
        public static PointF ProjectDiameter(this Ball ball, World world, int screenWidth, int screenHeight)
        {
            return ProjectDiameter(ball.Size, world, screenWidth, screenHeight);
        }

        /// <summary>
        /// Project a size to it's diameter.
        /// </summary>
        /// <param name="size">The size of anything relative to the agario world.</param>
        /// <param name="world">The world to project from.</param>
        /// <param name="screenWidth">The width of the canvas.</param>
        /// <param name="screenHeight">The height of the canvas.</param>
        /// <returns></returns>
        public static PointF ProjectDiameter(short size, World world, int screenWidth, int screenHeight)
        {
            return new PointF(
                Math.Max((screenWidth / 25f) * (size / 32f), 10f),
                Math.Max((screenHeight / 25f) * (size / 32f), 10f)
            );
        }
    }
}
