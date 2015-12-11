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
using System.Drawing;
using System.Linq;
using Agario.Model;

namespace Agario.Bot.Geometry
{
    /// <summary>
    /// Provides mathematical arc functions.
    /// </summary>
    public static class ArcMath
    {
        /// <summary>
        /// The intersection area of the second Arc compared to the first Arc.
        /// </summary>
        public enum ArcIntersection
        {
            Inside,
            Full,
            Left,
            Right,
            None
        }

        /// <summary>
        /// Calculate the arcs where no enemies collide relative to the player. Results vaery depending on the projection (screenWidth, screenHeight, viewAngleX, viewAngleY)
        /// </summary>
        /// <param name="playerBall">The player's primary ball.</param>
        /// <param name="enemyBalls">All the enemy/dangerous balls.</param>
        /// <param name="world">The world.</param>
        /// <param name="enemySizeScale">The scale for enemy size. Bigger size means smaller safe arcs.</param>
        /// <param name="screenWidth">The width of the canvas.</param>
        /// <param name="screenHeight">The height of the canvas.</param>
        /// <param name="viewAngleX">The game's horizontal view angle.</param>
        /// <param name="viewAngleY">The game's vertical view angle.</param>
        /// <returns></returns>
        public static IEnumerable<Geometry.Arc> FindSafeArcs(Ball playerBall, IEnumerable<Ball> enemyBalls, World world, Func<Ball, float> enemySizeScale, int screenWidth, int screenHeight, float viewAngleX = 1000f, float viewAngleY = 600f)
        {
            var tangents = new List<Tuple<PointF, PointF>>();
            return FindSafeArcs(playerBall, enemyBalls, world, enemySizeScale, screenWidth, screenHeight, out tangents, viewAngleX, viewAngleY);
        }

        /// <summary>
        /// Calculate the arcs where no enemies collide relative to the player. Results vaery depending on the projection (screenWidth, screenHeight, viewAngleX, viewAngleY)
        /// </summary>
        /// <param name="playerBall">The player's primary ball.</param>
        /// <param name="enemyBalls">All the enemy/dangerous balls.</param>
        /// <param name="world">The world.</param>
        /// <param name="enemySizeScale">The scale for enemy size. Bigger size means smaller safe arcs.</param>
        /// <param name="screenWidth">The width of the canvas.</param>
        /// <param name="screenHeight">The height of the canvas.</param>
        /// <param name="tangents">The tangents that return from the calculations.</param>
        /// <param name="viewAngleX">The game's horizontal view angle.</param>
        /// <param name="viewAngleY">The game's vertical view angle.</param>
        /// <returns></returns>
        public static IEnumerable<Geometry.Arc> FindSafeArcs(Ball playerBall, IEnumerable<Ball> enemyBalls, World world, Func<Ball, float> enemySizeScale, int screenWidth, int screenHeight, out List<Tuple<PointF, PointF>> tangents, float viewAngleX = 1000f, float viewAngleY = 600f)
        {
            var playerWorldPosition = playerBall.ProjectToScreen(world, screenWidth, screenHeight, viewAngleX, viewAngleY);

            List<Geometry.Arc> safeArcs = new List<Geometry.Arc>() { new Geometry.Arc(0f, 360f) };
            tangents = new List<Tuple<PointF, PointF>>();

            foreach (Ball enemyBall in enemyBalls)
            {
                var enemyWorldPosition = enemyBall.ProjectToScreen(world, screenWidth, screenHeight, viewAngleX, viewAngleY);
                var enemyWorldRadius = (enemyBall.ProjectDiameter(world, screenWidth, screenHeight).X / 2f) * enemySizeScale(enemyBall);

                PointF tangent1, tangent2;
                Geometry.GeometryMath.FindTangents(enemyWorldPosition, enemyWorldRadius, playerWorldPosition, out tangent1, out tangent2);

                if (new[] { tangent1.X, tangent1.Y, tangent2.X, tangent2.Y }.Any(f => f != -1f))
                {
                    float tangentsStartAngle = (((float)Math.Atan2(tangent2.Y - playerWorldPosition.Y, tangent2.X - playerWorldPosition.X)).ToDegrees() + 360f) % 360f;
                    float tangentsEndAngle = (((float)Math.Atan2(tangent1.Y - playerWorldPosition.Y, tangent1.X - playerWorldPosition.X)).ToDegrees() + 360f) % 360f;

                    Geometry.Arc arc = Geometry.Arc.FromAngles(tangentsStartAngle, tangentsEndAngle);

                    safeArcs = FindSafeArcsInternal(safeArcs, arc);
                    tangents.Add(new Tuple<PointF, PointF>(tangent1, tangent2));
                }
                else
                {
                    float angle = (((float)Math.Atan2(enemyWorldPosition.Y - playerWorldPosition.Y, enemyWorldPosition.X - playerWorldPosition.X)).ToDegrees() + 360f) % 360f;
                    safeArcs = FindSafeArcsInternal(safeArcs, new Arc(angle - 90f, 180f));
                }
            }

            return safeArcs;
        }

        /// <summary>
        /// Calculate the intersection area of the second Arc compared to the first Arc.
        /// </summary>
        /// <param name="arc">The primary arc.</param>
        /// <param name="intersector">The secondary arc.</param>
        /// <returns></returns>
        public static ArcIntersection CalculateArcIntersection(Arc arc, Arc intersector)
        {
            if ((intersector.EndAngleDegrees <= arc.StartAngleDegrees) || (arc.EndAngleDegrees <= intersector.StartAngleDegrees)) return ArcIntersection.None;
            if (intersector.StartAngleDegrees >= arc.StartAngleDegrees && intersector.EndAngleDegrees <= arc.EndAngleDegrees) return ArcIntersection.Inside;
            if (intersector.StartAngleDegrees < arc.StartAngleDegrees && intersector.EndAngleDegrees > arc.EndAngleDegrees) return ArcIntersection.Full;
            if (intersector.StartAngleDegrees < arc.StartAngleDegrees && intersector.EndAngleDegrees <= arc.EndAngleDegrees) return ArcIntersection.Left;
            if (intersector.StartAngleDegrees >= arc.StartAngleDegrees && intersector.EndAngleDegrees > arc.EndAngleDegrees) return ArcIntersection.Right;
            return ArcIntersection.None;
        }

        private static List<Geometry.Arc> FindSafeArcsInternal(IEnumerable<Geometry.Arc> arcs, Geometry.Arc negater)
        {
            //Split the negater into two seperate arcs if the angle over over 360 degrees for easier math.
            if (ShouldSplitArc(negater))
            {
                Geometry.Arc[] newArcs = SplitArc(negater);

                return FindSafeArcsInternal(FindSafeArcsInternal(arcs, newArcs[0]), newArcs[1]);
            }

            //Split arcs that go over 360 degrees into two seperate arcs for easier math.
            List<Geometry.Arc> splittedArcs = new List<Geometry.Arc>();
            foreach (Geometry.Arc arc in arcs)
            {
                if (ShouldSplitArc(arc))
                {
                    splittedArcs.AddRange(SplitArc(arc));
                }
                else
                {
                    splittedArcs.Add(arc);
                }
            }

            return MergeArcs(NegateArcsInternal(splittedArcs, negater));
        }

        private static List<Geometry.Arc> NegateArcsInternal(List<Geometry.Arc> arcs, Geometry.Arc negater)
        {
            List<Geometry.Arc> negatedArcs = arcs.ToList();

            foreach (Geometry.Arc arc in arcs)
            {
                ArcIntersection intersection = CalculateArcIntersection(arc, negater);

                if (arc.AngleDegrees < 0.1f)
                {
                    negatedArcs.Remove(arc);
                }
                else if (intersection == ArcIntersection.Inside)
                {
                    negatedArcs.Remove(arc);
                    negatedArcs.Add(new Geometry.Arc(arc.StartAngleDegrees, negater.StartAngleDegrees - arc.StartAngleDegrees));
                    negatedArcs.Add(new Geometry.Arc(negater.EndAngleDegrees, arc.EndAngleDegrees - negater.EndAngleDegrees));
                }
                else if (intersection == ArcIntersection.Full)
                {
                    negatedArcs.Remove(arc);
                }
                else if (intersection == ArcIntersection.Left)
                {
                    negatedArcs.Remove(arc);
                    negatedArcs.Add(new Geometry.Arc(negater.EndAngleDegrees, arc.EndAngleDegrees - negater.EndAngleDegrees));
                }
                else if (intersection == ArcIntersection.Right) 
                {
                    negatedArcs.Remove(arc);
                    negatedArcs.Add(new Geometry.Arc(arc.StartAngleDegrees, negater.StartAngleDegrees - arc.StartAngleDegrees));
                }
            }

            return negatedArcs;
        }

        private static List<Geometry.Arc> MergeArcs(List<Geometry.Arc> arcs)
        {
            if (arcs.Count < 2)
                return arcs;

            foreach (Geometry.Arc arc1 in arcs)
            {
                foreach (Geometry.Arc arc2 in arcs)
                {
                    if (arc1 == arc2)
                        continue;

                    float result;

                    if (((result = arc2.StartAngleDegrees - arc1.EndAngleDegrees) < 0.1f && result >= 0f) ||
                        (arc1.EndAngleDegrees > 359.9f && arc2.StartAngleDegrees < 0.1f))
                    {
                        arcs.Remove(arc1);
                        arcs.Remove(arc2);
                        arcs.Add(new Geometry.Arc(arc1.StartAngleDegrees, arc1.AngleDegrees + arc2.AngleDegrees));

                        return MergeArcs(arcs);
                    }
                }
            }

            return arcs;
        }

        private static bool ShouldSplitArc(Geometry.Arc arc) => arc.EndAngleDegrees > 360f;

        private static Geometry.Arc[] SplitArc(Geometry.Arc arc)
        {
            Geometry.Arc endArc = new Geometry.Arc(arc.StartAngleDegrees, 360f - arc.StartAngleDegrees);
            Geometry.Arc beginArc = new Geometry.Arc(0f, arc.EndAngleDegrees - 360f);

            return new Geometry.Arc[2]
            {
                endArc,
                beginArc,
            };
        }

    }
}