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

namespace Agario.Bot.Geometry
{
    /// <summary>
    /// Provides mathematical geometry functions for circles and tangents.
    /// All these come from <a href="http://www.csharphelper.com">csharphelper</a>.
    /// </summary>
    internal static class GeometryMath
    {
        public static bool FindTangents(PointF center, float radius,
            PointF external_point, out PointF pt1, out PointF pt2)
        {
            // Find the distance squared from the
            // external point to the circle's center.
            double dx = center.X - external_point.X;
            double dy = center.Y - external_point.Y;
            double D_squared = dx * dx + dy * dy;
            if (D_squared < radius * radius)
            {
                pt1 = new PointF(-1, -1);
                pt2 = new PointF(-1, -1);
                return false;
            }

            // Find the distance from the external point
            // to the tangent points.
            double L = Math.Sqrt(D_squared - radius * radius);

            // Find the points of intersection between
            // the original circle and the circle with
            // center external_point and radius dist.
            FindCircleCircleIntersections(
                center.X, center.Y, radius,
                external_point.X, external_point.Y, (float)L,
                out pt1, out pt2);

            return true;
        }

        public static int FindCircleCircleIntersections(
            float cx0, float cy0, float radius0,
            float cx1, float cy1, float radius1,
            out PointF intersection1, out PointF intersection2)
        {
            // Find the distance between the centers.
            float dx = cx0 - cx1;
            float dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 -
                            radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                // Get the points P3.
                intersection1 = new PointF(
                    (float)(cx2 + h * (cy1 - cy0) / dist),
                    (float)(cy2 - h * (cx1 - cx0) / dist));
                intersection2 = new PointF(
                    (float)(cx2 - h * (cy1 - cy0) / dist),
                    (float)(cy2 + h * (cx1 - cx0) / dist));

                // See if we have 1 or 2 solutions.
                if (dist == radius0 + radius1) return 1;
                return 2;
            }
        }

        public static int FindLineCircleIntersections(
            float cx, float cy, float radius,
            PointF point1, PointF point2,
            out PointF intersection1, out PointF intersection2)
        {
            float dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) +
                (point1.Y - cy) * (point1.Y - cy) -
                radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 =
                    new PointF(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 =
                    new PointF(point1.X + t * dx, point1.Y + t * dy);
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 =
                    new PointF(point1.X + t * dx, point1.Y + t * dy);
                return 2;
            }
        }

        public static int FindCircleCircleTangents(
            PointF c1, float radius1, PointF c2, float radius2,
            out PointF outer1_p1, out PointF outer1_p2,
            out PointF outer2_p1, out PointF outer2_p2,
            out PointF inner1_p1, out PointF inner1_p2,
            out PointF inner2_p1, out PointF inner2_p2)
        {
            // Make sure radius1 <= radius2.
            if (radius1 > radius2)
            {
                // Call this method switching the circles.
                return FindCircleCircleTangents(
                    c2, radius2, c1, radius1,
                    out outer1_p2, out outer1_p1,
                    out outer2_p2, out outer2_p1,
                    out inner1_p2, out inner1_p1,
                    out inner2_p2, out inner2_p1);
            }

            // Initialize the return values in case
            // some tangents are missing.
            outer1_p1 = new PointF(-1, -1);
            outer1_p2 = new PointF(-1, -1);
            outer2_p1 = new PointF(-1, -1);
            outer2_p2 = new PointF(-1, -1);
            inner1_p1 = new PointF(-1, -1);
            inner1_p2 = new PointF(-1, -1);
            inner2_p1 = new PointF(-1, -1);
            inner2_p2 = new PointF(-1, -1);

            // ***************************
            // * Find the outer tangents *
            // ***************************
            {
                float radius2a = radius2 - radius1;
                if (!FindTangents(c2, radius2a, c1,
                    out outer1_p2, out outer2_p2))
                {
                    // There are no tangents.
                    return 0;
                }

                // Get the vector perpendicular to the
                // first tangent with length radius1.
                float v1x = -(outer1_p2.Y - c1.Y);
                float v1y = outer1_p2.X - c1.X;
                float v1_length = (float)Math.Sqrt(v1x * v1x + v1y * v1y);
                v1x *= radius1 / v1_length;
                v1y *= radius1 / v1_length;
                // Offset the tangent vector's points.
                outer1_p1 = new PointF(c1.X + v1x, c1.Y + v1y);
                outer1_p2 = new PointF(
                    outer1_p2.X + v1x,
                    outer1_p2.Y + v1y);

                // Get the vector perpendicular to the
                // second tangent with length radius1.
                float v2x = outer2_p2.Y - c1.Y;
                float v2y = -(outer2_p2.X - c1.X);
                float v2_length = (float)Math.Sqrt(v2x * v2x + v2y * v2y);
                v2x *= radius1 / v2_length;
                v2y *= radius1 / v2_length;
                // Offset the tangent vector's points.
                outer2_p1 = new PointF(c1.X + v2x, c1.Y + v2y);
                outer2_p2 = new PointF(
                    outer2_p2.X + v2x,
                    outer2_p2.Y + v2y);
            }

            // If the circles intersect, then there are no inner tangents.
            float dx = c2.X - c1.X;
            float dy = c2.Y - c1.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist <= radius1 + radius2) return 2;

            // ***************************
            // * Find the inner tangents *
            // ***************************
            {
                float radius1a = radius1 + radius2;
                FindTangents(c1, radius1a, c2,
                    out inner1_p2, out inner2_p2);

                // Get the vector perpendicular to the
                // first tangent with length radius2.
                float v1x = inner1_p2.Y - c2.Y;
                float v1y = -(inner1_p2.X - c2.X);
                float v1_length = (float)Math.Sqrt(v1x * v1x + v1y * v1y);
                v1x *= radius2 / v1_length;
                v1y *= radius2 / v1_length;
                // Offset the tangent vector's points.
                inner1_p1 = new PointF(c2.X + v1x, c2.Y + v1y);
                inner1_p2 = new PointF(
                    inner1_p2.X + v1x,
                    inner1_p2.Y + v1y);

                // Get the vector perpendicular to the
                // second tangent with length radius2.
                float v2x = -(inner2_p2.Y - c2.Y);
                float v2y = inner2_p2.X - c2.X;
                float v2_length = (float)Math.Sqrt(v2x * v2x + v2y * v2y);
                v2x *= radius2 / v2_length;
                v2y *= radius2 / v2_length;
                // Offset the tangent vector's points.
                inner2_p1 = new PointF(c2.X + v2x, c2.Y + v2y);
                inner2_p2 = new PointF(
                    inner2_p2.X + v2x,
                    inner2_p2.Y + v2y);
            }

            return 4;
        }

        public static int FindIntersection(
            PointF p1, PointF p2, PointF p3, PointF p4,
            out bool lines_intersect, out bool segments_intersect,
            out PointF intersection,
            out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);

            return 1;
        }
    }
}