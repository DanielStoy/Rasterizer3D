using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Text;

namespace _3DRasterizer
{
    public struct Vec2d
    {
        public int x, y;
        public Vec2d(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

    }

    public struct Vec3d
    {
        public float x, y, z;
        public Vec3d(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct Triangle2d
    {
        public List<Vec2d> points;
        public Triangle2d(Vec2d p1, Vec2d p2, Vec2d p3)
        {
            points = new List<Vec2d>(3);
            points[0] = p1;
            points[1] = p2;
            points[2] = p3;
        }
    }

    public struct Triangle3d
    {
        public List<Vec3d> points;

        public Triangle3d(Vec3d p1, Vec3d p2, Vec3d p3)
        {
            points = new List<Vec3d>(3);
            points[0] = p1;
            points[1] = p2;
            points[2] = p3;
        }
    }
}
