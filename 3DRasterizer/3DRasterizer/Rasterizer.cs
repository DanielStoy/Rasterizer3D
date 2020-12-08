using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace _3DRasterizer
{
    class Rasterizer
    {
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Unicode)]
        static extern bool WriteConsoleOutput(Handle hConsoleOutput, CharInfo[] lpBuffer, Coord dwBufferSize, Coord dwBufferCoord, ref SmallRect lpWriteRegion);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Handle GetStdHandle(int nStdHandle);

        private Handle consoleHandle;
        private int width = 0;
        private int height = 0;
        CharInfo[] buf;
        SmallRect consoleRect;
        Coord bufferSize;
        Coord pos;
        bool active = false;

        public Rasterizer()
        {
            consoleHandle = GetStdHandle(-11);
            pos = new Coord(0, 0);
        }

        //TODO: remove testing functionality
        public void BuildConsole(int w, int h)
        {
            width = w;
            height = h;
            Console.Clear();

            buf = new CharInfo[width * height];

            Array.Clear(buf, 0, buf.Length);

            //buf[1].Attributes = 0x000F;
            //buf[1].Char.UnicodeChar = 0x2588;

            bufferSize = new Coord((short)width, (short)height);

            consoleRect = new SmallRect() { Left = 0, Top = 0, Right = (short)(width - 1), Bottom = (short)(height - 1) };


        }

        public void ThreadStart()
        {
            active = true;
            Thread t = new Thread(drawConsoleEasy);
            t.Start();
            t.Join();
        }

        //TODO: Either use tasking or threading to make this more seemless
        private void drawConsoleEasy()
        {
            while (active)
            {
                WriteConsoleOutput(consoleHandle, buf, bufferSize, new Coord(0, 0), ref consoleRect);
                Thread.Sleep(1);
            }
        }


        public void Draw(int x, int y, short c = (short)Pixel.Pixel_Solid, short attr = 0x000F)
        {
            if ((x < width && x > 0) && (y < height && y > 0))
            {
                buf[y * width + x].Attributes = attr;
                buf[y * width + x].Char.UnicodeChar = c;
            }
        }

        public void DrawLine(Vec2d point1, Vec2d point2, short c = (short)Pixel.Pixel_Solid, short attr = 0x000F)
        {
            int xDis, yDis, clusterPoints, yMove = ReturnSide(point1.y, point2.y), xMove = ReturnSide(point1.x, point2.x), px, py;
            xDis = Math.Abs(point2.x - point1.x);
            yDis = Math.Abs(point2.y - point1.y);

            px = point1.x;
            py = point1.y;

            Draw(px, py, c, attr);

            if(yDis > xDis)
            {
                if (xDis != 0)
                {
                    clusterPoints =  2 * xDis - yDis;

                    for (int i = 0; i < yDis; i++)
                    {
                        py += yMove;

                        if(clusterPoints <= 0)
                        {
                            clusterPoints = clusterPoints + 2 * xDis;
                        }
                        else
                        {
                            px += xMove;
                            clusterPoints = clusterPoints + 2 * (xDis - yDis);
                        }

                        Draw(px, py, c, attr);
                    }
                }
                else
                {
                    for(int i = 0; i < yDis; i++)
                    {
                        py += yMove;
                        Draw(px, py, c, attr);
                    }
                }
            }
            else {
                if (yDis != 0)
                {
                    clusterPoints = 2 * yDis - xDis;
                    for (int i = 0; i < xDis; i++)
                    {
                        px += xMove;

                        if (clusterPoints < 0)
                        {
                            clusterPoints = clusterPoints + 2 * yDis;
                        }
                        else
                        {
                            py += yMove;
                            clusterPoints = clusterPoints + 2 * (yDis - xDis);
                        }

                        Draw(px, py, c, attr);
                    }
                }
                else
                {
                    for (int i = 0; i < xDis; i++)
                    {
                        px += xMove;
                        Draw(px, py, c, attr);
                    }
                }
            }
        }

        public void DrawStraightLine(int x, int ex, int y, short c = (short)Pixel.Pixel_Solid, short attr = 0x000F)
        {
            for(int i = x; i<=ex; i++)
            {
                Draw(i, y, c, attr);
            }
        }

        public void Fill(Vec2d point1, Vec2d point2, short c = (short)Pixel.Pixel_Solid, short attr = 0x000F)
        {
            for (int i = point1.x; i < point2.x; i++)
                for (int y = point1.y; y < point2.y; y++)
                    Draw(i, y, c, attr);
        }

        //Draws wireframe
        public void DrawTriangle(Vec2d point1, Vec2d point2, Vec2d point3, short c = (short)Pixel.Pixel_Solid, short attr = 0x000F)
        {
            DrawLine(point1, point2, c, attr);
            DrawLine(point2, point3, c, attr);
            DrawLine(point3, point1, c, attr);
        }

        //Bresenham Method - cuts non flat-top triangles into two flat tops and deals with them accordingly
        //Original from https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/olcPixelGameEngine.h
        // and https://www.avrfreaks.net/sites/default/files/triangles.c rewritten to C#
        public void FillTriangle(Vec2d point1, Vec2d point2, Vec2d point3, short c = (short)Pixel.Pixel_Solid, short attr = 0x000F)
        {

            int t1x, t2x, y, minx, maxx, t1xp, t2xp;
            bool changed1 = false;
            bool changed2 = false;
            int signx1, signx2, dx1, dy1, dx2, dy2;
            int e1, e2;
            // Sort vertices
            if (point1.y > point2.y) { Swap(ref point1.y, ref point2.y); Swap(ref point1.x, ref point2.x); }
            if (point1.y > point3.y) { Swap(ref point1.y, ref point3.y); Swap(ref point1.x, ref point3.x); }
            if (point2.y > point3.y) { Swap(ref point2.y, ref point3.y); Swap(ref point2.x, ref point3.x); }

            t1x = t2x = point1.x; y = point1.y;   // Starting points
            dx1 = (point2.x - point1.x);
            if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (point2.y - point1.y);

            dx2 = (int)(point3.x - point1.x);
            if (dx2 < 0) { dx2 = -dx2; signx2 = -1; }
            else signx2 = 1;
            dy2 = (int)(point3.y - point1.y);

            if (dy1 > dx1) { Swap(ref dx1, ref dy1); changed1 = true; }
            if (dy2 > dx2) { Swap(ref dy2, ref dx2); changed2 = true; }

            e2 = (int)(dx2 >> 1);
            // Flat top, just process the second half
            if (point1.y == point2.y) goto next;
            e1 = (int)(dx1 >> 1);

            for (int i = 0; i < dx1;)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    i++;
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1) t1xp = signx1;
                        else goto next1;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                }
            // Move line
            next1:
                // process second line until y value is about to change
                while (true)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        if (changed2) t2xp = signx2;
                        else goto next2;
                    }
                    if (changed2) break;
                    else t2x += signx2;
                }
            next2:
                if (minx > t1x) minx = t1x;
                if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x;
                if (maxx < t2x) maxx = t2x;
                DrawStraightLine(minx, maxx, y, c, attr);    // Draw line from min to max points found on the y
                                            // Now increase y
                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y == point2.y) break;

            }
        next:
            // Second half
            dx1 = (int)(point3.x - point2.x); if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (int)(point3.y - point2.y);
            t1x = point2.x;

            if (dy1 > dx1)
            {   // swap values
                Swap(ref dy1, ref dx1);
                changed1 = true;
            }
            else changed1 = false;

            e1 = (int)(dx1 >> 1);

            for (int i = 0; i <= dx1; i++)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1) { t1xp = signx1; break; }//t1x += signx1;
                        else goto next3;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                    if (i < dx1) i++;
                }
            next3:
                // process second line until y value is about to change
                while (t2x != point3.x)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        if (changed2) t2xp = signx2;
                        else goto next4;
                    }
                    if (changed2) break;
                    else t2x += signx2;
                }
            next4:

                if (minx > t1x) minx = t1x;
                if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x;
                if (maxx < t2x) maxx = t2x;
                DrawStraightLine(minx, maxx, y, c, attr);
                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y > point3.y) return;
            }
        }

        //Helper functions
        private int ReturnSide(int p1, int p2)
        {
            int returnVal = 1;
            if((p2 - p1) < 0)
            {
                returnVal = -1;
            }
            return returnVal;
        }

        private void Swap(ref int x, ref int y)
        {
            int temp = x;
            x = y;
            y = temp;
        }

    }
}
