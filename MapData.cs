﻿using System;

namespace MazeGamePillaPilla
{
    class MapData
    {
        internal static int MapsCount { get; } = 2;

        internal static int[,] GetMap(int id)
        {
            switch (id)
            {
                case 0:
                    return new int[,] {
                        { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                        { 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  1,  1,  1,  1,  1,  1, 14, 13, 14, 14, 13,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0, 25,  1, 18,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  1,  1,  1, 17, 17, 10, 11,  0, 24,  1, 19,  0,  1,  1,  1,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  1,  0,  0,  0,  0,  0,  0,  0, 23,  1, 20,  0,  0,  1,  1,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  1,  0,  0,  0,  0,  0,  0,  0, 22,  1, 21,  0,  0,  4,  1,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  1,  1,  0,  0, 16, 17,  1,  0,  0,  1, 10, 11,  0,  0,  1,  2,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  1,  0,  0, 25,  1, 13, 12,  0,  0, 15, 14,  1, 18,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  1,  0,  0, 24, 20,  0,  0,  0,  0,  0,  0, 23, 19,  0,  1,  1,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  5,  1, 21,  0,  0,  0,  0,  0,  0, 22,  1,  0,  1,  3,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  1,  1,  1,  0,  0,  0,  1,  0,  6,  0,  0,  1,  0,  1,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  1,  0,  0,  0,  0,  0,  0, 15,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0, 16, 17,  1, 10, 11,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  1,  1, 18, 25,  1, 13, 12,  0, 15, 14,  1,  1,  0,  0,  1,  1,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    };

                case 1:
                    return new int[,] {
                        { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                        { 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  5,  0,  2,  0,  9,  0,  6,  0,  0,  1,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  4,  0,  3,  0,  8,  0,  7,  0,  0, 25,  0, 18,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 24,  0, 19,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0, 16,  0, 17,  0, 10,  0, 11,  0,  0, 23,  0, 20,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0, 15,  0, 14,  0, 13,  0, 12,  0,  0, 22,  0, 21,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  0 },
                        { 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0 },
                        { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    };
            }

            throw new Exception();
        }
    }
}
