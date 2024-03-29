﻿using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    struct Edge
    {
        public Vector2 A, B;

        public Edge(float AX, float AY, float BX, float BY)
        {
            A = new Vector2(AX, AY);
            B = new Vector2(BX, BY);
        }

        public Edge(Edge other, float dx, float dy)
        {
            A = new Vector2(other.A.X + dx, other.A.Y + dy);
            B = new Vector2(other.B.X + dx, other.B.Y + dy);
        }
    }
}
