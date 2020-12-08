using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace _3DRasterizer
{
    class TestArea
    {
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Unicode)]
        static extern bool WriteConsoleOutput(Handle hConsoleOutput, CharInfo[] lpBuffer, Coord dwBufferSize, Coord dwBufferCoord, ref SmallRect lpWriteRegion);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Handle GetStdHandle(int nStdHandle);
        static void Main(string[] args)
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
            int height = Console.WindowHeight;
            int width = Console.WindowWidth;
            Rasterizer Ras = new Rasterizer();
            Ras.BuildConsole(width, height);
            Vec2d point1 = new Vec2d(1, 1);
            Vec2d point2 = new Vec2d(70, 1);
            Vec2d point4 = new Vec2d(10, 20);
            //Ras.DrawTriangle(point1, point2, point4);
            Ras.FillTriangle(point1, point2, point4);
            Ras.ThreadStart();
        }
    }
}
