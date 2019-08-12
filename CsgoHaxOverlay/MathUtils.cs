using System;
using System.Media;
using System.Numerics;
using CsgoHaxOverlay.Entities;

namespace CsgoHaxOverlay
{
    public static class MathUtils
    {
        #region VARIABLES
        private const float Deg2Rad = (float)(Math.PI / 180f);
        private const float Rad2Deg = (float)(180f / Math.PI);
        #endregion
        #region METHODS
        public static Vector2[] WorldToScreen(Matrix viewMatrix, Vector2 screenSize, params Vector3[] points)
        {
            var worlds = new Vector2[points.Length];
            for (var i = 0; i < worlds.Length; i++)
                worlds[i] = WorldToScreen(viewMatrix, screenSize, points[i]);
            return worlds;
        }
        public static Vector2 WorldToScreen(Matrix viewMatrix, Vector2 screenSize, Vector3 point3D)
        {
            var returnVector = Vector2.Zero;
            var w = viewMatrix[3, 0] * point3D.X + viewMatrix[3, 1] * point3D.Y + viewMatrix[3, 2] * point3D.Z + viewMatrix[3, 3];
            if (w < 0.01f) return returnVector;
            var inverseX = 1f / w;
            returnVector.X =
                (screenSize.X / 2f) +
                (0.5f * (
                    (viewMatrix[0, 0] * point3D.X + viewMatrix[0, 1] * point3D.Y + viewMatrix[0, 2] * point3D.Z + viewMatrix[0, 3])
                    * inverseX)
                 * screenSize.X + 0.5f);
            returnVector.Y =
                (screenSize.Y / 2f) -
                (0.5f * (
                    (viewMatrix[1, 0] * point3D.X + viewMatrix[1, 1] * point3D.Y + viewMatrix[1, 2] * point3D.Z + viewMatrix[1, 3])
                    * inverseX)
                 * screenSize.Y + 0.5f);
            return returnVector;
        }
        public static Vector3[] OffsetVectors(Vector3 offset, params Vector3[] points)
        {
            for (int i = 0; i < points.Length; i++)
                points[i] += offset;
            return points;
        }
        public static Vector3[] CopyVectors(Vector3[] source)
        {
            Vector3[] ret = new Vector3[source.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = new Vector3(source[i].X, source[i].Y, source[i].Z);
            return ret;
        }
        public static float Distance(Vector3 l, Vector3 e)
        {
            var dist = (float)Math.Sqrt(Math.Pow(l.X - e.X, 2) + Math.Pow(l.Y - e.Y, 2) + Math.Pow(l.Z - e.Z, 2));
            return dist;
        }
        public static float Distance(Vector2 l, Vector2 e)
        {
            var dist = (float)Math.Sqrt(Math.Pow(l.X - e.X, 2) + Math.Pow(l.Y - e.Y, 2));
            return dist;
        }

        public static Vector2 RotatePoint2(Vector2 pointToRotate, Vector2 centerPoint, float angleInDegrees)
        {
            var angleInRadians = (float)(angleInDegrees * (Math.PI / 180f));
            var cosTheta = (float)Math.Cos(angleInRadians);
            var sinTheta = (float)Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        public static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, float angle,
            bool angleInRadians = false)
        {
            angle += 180;
            if (!angleInRadians)
                angle = (float)(angle * (Math.PI / 180f));

            var cosTheta = (float)Math.Cos(angle);
            var sinTheta = (float)Math.Sin(angle);
            var returnVec = new Vector2(
                cosTheta * (pointToRotate.X - centerPoint.X) + sinTheta * (pointToRotate.Y - centerPoint.Y),
                sinTheta * (pointToRotate.X - centerPoint.X) - cosTheta * (pointToRotate.Y - centerPoint.Y)
                );
            returnVec += centerPoint;
            return returnVec;
        }
        public static Vector3 ClampAngle(Vector3 qaAng)
        {
            if (qaAng.X > 89.0f && qaAng.X <= 180.0f)
                qaAng.X = 89.0f;

            while (qaAng.X > 180.0f)
                qaAng.X = qaAng.X - 360.0f;

            if (qaAng.X < -89.0f)
                qaAng.X = -89.0f;

            while (qaAng.Y > 180.0f)
                qaAng.Y = qaAng.Y - 360.0f;

            while (qaAng.Y < -180.0f)
                qaAng.Y = qaAng.Y + 360.0f;

            return qaAng;
        }

        public static Vector3 CalcAngle2(LocalPlayer me,Vector3 headPos, Vector3 angles)
        {
            double[] delta =
            {
                (me.Position.X - headPos.X), (me.Position.Y - headPos.Y),
                (me.Position.Z - (headPos.Z - 61))
            };
            var hyp = Math.Sqrt(delta[0] * delta[0] + delta[1] * delta[1]);
            me.PunchAngle = MemUtils.ReadVector2((IntPtr) (me.Entity + Netvars.m_aimPunchAngle));
            angles.X = (float)(Math.Atan(delta[2] / hyp) * 57.295779513082f - me.PunchAngle.X * 2.0f);
            angles.Y = (float)(Math.Atan(delta[1] / delta[0]) * 57.295779513082f - me.PunchAngle.Y * 2.0f);
            angles.Z = 0.0f;

            if (delta[0] >= 0.0f)
            {
                angles.Y += 180.0f;
            }
            return angles;
        }
        public static Vector3 CalcAngle(Vector3 src, Vector3 dst)
        {
            var ret = new Vector3();
            var vDelta = src - dst;
            var fHyp = (float)Math.Sqrt((vDelta.X * vDelta.X) + (vDelta.Y * vDelta.Y));

            ret.X = RadiansToDegrees((float)Math.Atan(vDelta.Z / fHyp));
            ret.Y = RadiansToDegrees((float)Math.Atan(vDelta.Y / vDelta.X));

            if (vDelta.X >= 0.0f)
                ret.Y += 180.0f;
            return ret;
        }

        public static double GetRealDistance(LocalPlayer me, Vector3 target, Player s)
        {
            var fYawDifference = CalculateYawDifference(me.ViewAngles.Y, target.Y);

            fYawDifference = Math.Abs(fYawDifference);

            double dist = Math.Abs(Distance(me.Position, s.Position));

            dist = Math.Abs(Math.Sin(DegreesToRadians(fYawDifference)) * dist);

            return dist;
        }
        
        private static float CalculateYawDifference(float myY, float enemyY)
        {
            return Math.Abs(myY - enemyY);
        }
        public static float DegreesToRadians(float deg) { return deg * Deg2Rad; }
        public static float RadiansToDegrees(float deg) { return deg * Rad2Deg; }
        public static bool PointInCircle(Vector2 point, Vector2 circleCenter, float radius)
        {
            return (point - circleCenter).Length() < radius;
        }
        #endregion
    }
}
