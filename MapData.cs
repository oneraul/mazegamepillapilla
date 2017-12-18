using System.IO;
using System.Collections.Generic;

namespace MazeGamePillaPilla
{
    class MapData
    {
        public static int MapsCount { get; private set; }
        private static Dictionary<string, int> CorrectTileIds;

        static MapData()
        {
            string directoryPath = $"C:/Users/oneraul/Documents/Visual Studio 2017/Projects/mazegamepillapilla/Content/maps";
            int i = 0;
            while(true)
            {
                string path = $"{directoryPath}/{i}.csv";
                if(!File.Exists(path))
                {
                    break;
                }

                i++;
            }

            MapsCount = i;

            CorrectTileIds = new Dictionary<string, int>
            {
                { "-1",  0 },
                {  "0",  0 },
                {  "1",  1 },
                {  "2",  5 },
                {  "3",  2 },
                {  "4", 25 },
                {  "5", 18 },
                {  "6",  9 },
                {  "7",  6 },
                {  "8",  4 },
                {  "9",  3 },
                { "10", 24 },
                { "11", 19 },
                { "12",  8 },
                { "13",  7 },
                { "14",  0 }, // invalid
                { "15",  0 }, // invalid
                { "16", 23 },
                { "17", 20 },
                { "18", 16 },
                { "19", 17 },
                { "20", 10 },
                { "21", 11 },
                { "22", 22 },
                { "23", 21 },
                { "24", 15 },
                { "25", 14 },
                { "26", 13 },
                { "27", 12 },
                { "28",  0 }, // invalid
                { "29",  0 }, // invalid
            };
        }

        
        public static int[,] GetMap(int id)
        {
            string path = $"C:/Users/oneraul/Documents/Visual Studio 2017/Projects/mazegamepillapilla/Content/maps/{id}.csv";

            if (File.Exists(path))
            {
                using (StreamReader file = new StreamReader(File.OpenRead(path)))
                {
                    List<int[]> tmp = new List<int[]>();

                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] tilesString = line.Split(',');
                        int[] tiles = new int[tilesString.Length - 1]; // length-1 because the last comma in every line
                        for (int i = 0; i < tiles.Length; i++)
                        {
                            tiles[i] = CorrectTileIds[tilesString[i]];
                        }
                        tmp.Add(tiles);
                    }
                    tmp.RemoveAt(tmp.Count-1); // remove empty line at the end of the file

                    int[,] map = new int[tmp.Count, tmp[0].Length];

                    for (int y = 0; y < tmp.Count; y++)
                    {
                        for (int x = 0; x < tmp[0].Length; x++)
                        {
                            map[y, x] = tmp[y][x];
                        }
                    }

                    return map;
                }
            }

            throw new System.ComponentModel.InvalidEnumArgumentException();
        }
    }
}
