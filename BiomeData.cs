using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class BiomeData
    {
        public struct Biome
        {
            public Color BackgroundColor;
            public Color GroundColor;
            public Color WallColor;
            public Color WallTopColor;
            public Color TintColor;
        }

        public static int BiomesCount { get; } = 3;

        public static Biome GetBiome(int id)
        {
            switch (id)
            {
                case 0: return new Biome()
                {
                    BackgroundColor = new Color(77, 174, 183),
                    GroundColor = new Color(182, 186, 159),
                    WallColor = new Color(118, 166, 19),
                    WallTopColor = new Color(153, 186, 0),
                    TintColor = Color.White,
                };

                case 1: return new Biome()
                {
                    BackgroundColor = new Color(55, 45, 73),
                    GroundColor = new Color(65, 53, 86),
                    WallColor = new Color(117, 104, 142),
                    WallTopColor = new Color(128, 121, 142),
                    TintColor = Color.White,
                };

                case 2: return new Biome()
                {
                    BackgroundColor = new Color(226, 226, 225),
                    GroundColor = new Color(196, 196, 194),
                    WallColor = new Color(153, 153, 153),
                    WallTopColor = new Color(45, 45, 58),
                    TintColor = Color.White,
                };

                case 3: return new Biome()
                {
                    BackgroundColor = new Color(76, 68, 45),
                    GroundColor = new Color(173, 183, 134),
                    WallColor = new Color(155, 115, 40),
                    WallTopColor = new Color(134, 52, 26),
                    TintColor = Color.White,
                };
            }

            throw new System.ComponentModel.InvalidEnumArgumentException();
        }
    }
}
