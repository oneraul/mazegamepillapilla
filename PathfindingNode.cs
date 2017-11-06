using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MazeGamePillaPilla
{
    abstract class PathfindingNode
    {
        public int I { get; private set; }
        public int J { get; private set; }
        public PathfindingNode[] OpenNodes;
        public bool ToRemove = false;
        public int Cost = 99999;
        public PathfindingNode Previous;
        public Vector2 NavigationPosition { get; protected set; }

        public PathfindingNode(int i, int j)
        {
            this.I = i;
            this.J = j;
        }

        public abstract bool IsGoal(int goalI, int goalJ);

        public abstract void Draw(SpriteBatch spritebatch, Texture2D pixel);

        public void DrawOpen(SpriteBatch spritebatch, Texture2D pixel)
        {
            foreach (PathfindingNode node in OpenNodes)
            {
                node.Draw(spritebatch, pixel);
            }
        }

        public void DrawNavigationPosition(SpriteBatch spritebatch, Texture2D pixel)
        {
            Rectangle rectangle = new Rectangle((int)NavigationPosition.X-1, (int)NavigationPosition.Y-1, 2, 2);
            spritebatch.Draw(pixel, rectangle, Color.Blue);
        }
    }


    class PathfindingNodeHorizontal : PathfindingNode
    {
        private int Left;
        private int Right;

        public PathfindingNodeHorizontal(int i, int j, Tile topTile, Tile bottomTile) : base(i, j)
        {
            int topLeft = 0, topRight = Tile.Size;
            if (topTile.Id != 0)
            {
                Edge topCellBottonEdge = topTile.Edges[2];
                if (topCellBottonEdge.A.Y == Tile.Size && topCellBottonEdge.B.Y == Tile.Size)
                {
                    int right = (int)topCellBottonEdge.A.X;
                    int left = (int)topCellBottonEdge.B.X;

                    if (right - left == Tile.Size)
                    {
                        ToRemove = true;
                        return;
                    }

                    if (topLeft != left || topRight != right)
                    {
                        if (topLeft != left) topRight = left;
                        else if (topRight != right) topLeft = right;
                    }
                }
            }

            int bottomLeft = 0, bottomRight = Tile.Size;
            if (bottomTile.Id != 0)
            {
                Edge bottomCellTopEdge = bottomTile.Edges[0];
                if (bottomCellTopEdge.A.Y == 0 && bottomCellTopEdge.B.Y == 0)
                {
                    int right = (int)bottomCellTopEdge.B.X;
                    int left = (int)bottomCellTopEdge.A.X;

                    if (right - left == Tile.Size)
                    {
                        ToRemove = true;
                        return;
                    }

                    if (bottomLeft != left || bottomRight != right)
                    {
                        if (bottomLeft != left) bottomRight = left;
                        else if (bottomRight != right) bottomLeft = right;
                    }
                }
            }

            this.Left = System.Math.Max(topLeft, bottomLeft);
            this.Right = System.Math.Min(topRight, bottomRight);

            NavigationPosition = new Vector2(J * Tile.Size + Left + (Right - Left)/2, I * Tile.Size);
        }

        public override void Draw(SpriteBatch spritebatch, Texture2D pixel)
        {
            int x = J * Tile.Size + Left;
            int y = I * Tile.Size;
            int w = Right - Left;

            Rectangle rectangle = new Rectangle(x, y, w, 1);
            spritebatch.Draw(pixel, rectangle, Color.Blue);
        }

        public override bool IsGoal(int goalI, int goalJ)
        {
            return J == goalJ && (I == goalI || I == goalI + 1);
        }
    }


    class PathfindingNodeVertical : PathfindingNode
    {
        private int Bottom;
        private int Top;

        public PathfindingNodeVertical(int i, int j, Tile leftTile, Tile rightTile) : base(i, j)
        {
            int leftTop = 0, leftBottom = Tile.Size;
            if (leftTile.Id != 0)
            {
                Edge leftCellRightEdge = leftTile.Edges[1];
                if (leftCellRightEdge.A.X == Tile.Size && leftCellRightEdge.B.X == Tile.Size)
                {
                    int top = (int)leftCellRightEdge.A.Y;
                    int bottom = (int)leftCellRightEdge.B.Y;

                    if (bottom - top == Tile.Size)
                    {
                        ToRemove = true;
                        return;
                    }

                    if (leftTop != top || leftBottom != bottom)
                    {
                        if (leftTop != top) leftBottom = top;
                        else if (leftBottom != bottom) leftTop = bottom;
                    }
                }
            }

            int rightTop = 0, rightBottom = Tile.Size;
            if (rightTile.Id != 0)
            {
                Edge rightCellLeftEdge = rightTile.Edges[3];
                if (rightCellLeftEdge.A.X == 0 && rightCellLeftEdge.B.X == 0)
                {
                    int top = (int)rightCellLeftEdge.B.Y;
                    int bottom = (int)rightCellLeftEdge.A.Y;

                    if (bottom - top == Tile.Size)
                    {
                        ToRemove = true;
                        return;
                    }

                    if (rightTop != top || rightBottom != bottom)
                    {
                        if (rightTop != top) rightBottom = top;
                        else if (rightBottom != bottom) rightTop = bottom;
                    }
                }
            }

            this.Top = System.Math.Max(leftTop, rightTop);
            this.Bottom = System.Math.Min(leftBottom, rightBottom);

            NavigationPosition = new Vector2(J * Tile.Size, I * Tile.Size + Top + (Bottom - Top) / 2);
        }

        public override void Draw(SpriteBatch spritebatch, Texture2D pixel)
        {
            int x = J * Tile.Size;
            int y = I * Tile.Size + Top;
            int h = Bottom - Top;

            Rectangle rectangle = new Rectangle(x, y, 1, h);
            spritebatch.Draw(pixel, rectangle, Color.Blue);
        }

        public override bool IsGoal(int goalI, int goalJ)
        {
            return I == goalI && (J == goalJ || J == goalJ + 1);
        }
    }
}
