﻿namespace Plus.HabboHotel.Rooms.PathFinding
{
    using System;

    public struct ThreeDCoord : IEquatable<ThreeDCoord>
    {
        public int X;
        public int Y;
        public int Z;

        public ThreeDCoord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(ThreeDCoord comparedCoord)
        {
            return X == comparedCoord.X && Y == comparedCoord.Y && Z == comparedCoord.Z;
        }

        public static bool operator ==(ThreeDCoord a, ThreeDCoord b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(ThreeDCoord a, ThreeDCoord b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return X ^ Y ^ Z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return base.GetHashCode().Equals(obj.GetHashCode());
        }
    }
}