using System.IO;

namespace MazeGamePillaPilla
{
    class MapData
    {
        public static int MapsCount { get; private set; }

        static MapData()
        {
            string directoryPath = $"C:/Users/oneraul/Documents/Visual Studio 2017/Projects/mazegamepillapilla/Content/maps/MapConverter/correct/";
            int i = 0;
            while(true)
            {
                string path = $"{directoryPath}{i}.corrected.csv";
                if(!File.Exists(path))
                {
                    break;
                }

                i++;
            }

            MapsCount = i;
        }
        
        public static int[,] GetMap(int id)
        {
            string path = $"C:/Users/oneraul/Documents/Visual Studio 2017/Projects/mazegamepillapilla/Content/maps/MapConverter/correct/{id}.corrected.csv";

            if (File.Exists(path))
            {
                using (StreamReader file = new StreamReader(File.OpenRead(path)))
                {
                    System.Collections.Generic.List<int[]> tmp = new System.Collections.Generic.List<int[]>();

                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] tilesString = line.Split(',');
                        int[] tiles = new int[tilesString.Length-1]; // length-1 because the last comma in every line
                        for (int i = 0; i < tiles.Length; i++)
                        {
                            tiles[i] = int.Parse(tilesString[i]);
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
