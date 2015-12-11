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
    /// An Agario Ball (Player, Virus, Food).
    /// </summary>
    [DebuggerDisplay("{Name}, {Size}")]
    public class Ball
    {
        /// <summary>
        /// The Agario ball Id.
        /// </summary>
        public uint Id { get; internal set; }

        /// <summary>
        /// The name of ball.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The X position of the ball.
        /// </summary>
        public int X { get; internal set; }

        /// <summary>
        /// The Y position of the ball.
        /// </summary>
        public int Y { get; internal set; }

        /// <summary>
        /// The size of the ball.
        /// </summary>
        public short Size { get; internal set; }

        /// <summary>
        /// The red component of the ball's color.
        /// </summary>
        public byte R { get; internal set; }

        /// <summary>
        /// The green component of the ball's color.
        /// </summary>
        public byte G { get; internal set; }

        /// <summary>
        /// The blue component of the ball's color.
        /// </summary>
        public byte B { get; internal set; }

        /// <summary>
        /// Whether you can control the ball.
        /// </summary>
        public bool IsMine { get; internal set; }

        /// <summary>
        /// Whether the ball is a virus.
        /// </summary>
        public bool IsVirus { get; internal set; }

        /// <summary>
        /// Whether the ball is food.
        /// </summary>
        public bool IsFood => !IsVirus && !IsMine && Size <= 13 && string.IsNullOrWhiteSpace(Name);

        /// <summary>
        /// Whether the ball is an enemy.
        /// </summary>
        public bool IsEnemy => !IsVirus && !IsMine && !IsFood;

        /// <summary>
        /// Calculates whether the ball can eat another ball.
        /// </summary>
        /// <param name="target">The target ball.</param>
        /// <returns></returns>
        public bool CanEat(Ball target) => (Size / (double)target.Size) >= 1.3d;

        internal Ball(uint id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{{ {nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(X)}: {X}, {nameof(Y)}: {Y} }}";
        }

        public override bool Equals(object obj)
        {
            return obj is Ball && ((Ball)obj).Id == Id;
        }

        public override int GetHashCode()
        {
            return unchecked((int)Id);
        }

#if DEBUG
        public class MutableBall : Ball
        {
            public MutableBall(uint id, int x, int y, short size) : base(id)
            {
                X = x;
                Y = y;
                Size = size;
            }

            public int ModX { set { X = value; } }

            public int ModY { set { Y = value; } }

            public short ModSize { set { Size = value; } }
        }
    }
#endif

}
