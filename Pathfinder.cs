using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    static class Pathfinder
    {
        static public void DrawPath(SpriteBatch spriteBatch, Texture2D pixel, List<Vector2> path)
        {
            foreach(Vector2 node in path)
            {
                int x = (int)node.X;
                int y = (int)node.Y;
                int w = 2;
                int h = 2;
                spriteBatch.Draw(pixel, new Rectangle(x-w/2, y-h/2, w, h), Color.Red);
            }
        }

        static public List<Vector2> CalculatePath(PathfindingNode[] map, PathfindingNodeHorizontal[,] horizontalMap, PathfindingNodeVertical[,] verticalMap, Vector2 originPos, Vector2 goalPos, Cell[,] maze)
        {
            ClearNodesState(map);

            List<PathfindingNode> open = new List<PathfindingNode>();
            List<PathfindingNode> explored = new List<PathfindingNode>();

            int GoalI = (int)(goalPos.Y / Tile.Size);
            int GoalJ = (int)(goalPos.X / Tile.Size);

            open.AddRange(GetNodesForPosition(horizontalMap, verticalMap, originPos));
            foreach (PathfindingNode node in open)
            {
                node.Cost = 0;
            }
            
            while (open.Count > 0)
            {
                PathfindingNode currentNode = ChooseNode(open, GoalI, GoalJ);

                if(currentNode.IsGoal(GoalI, GoalJ))
                {
                    return BuildPath(currentNode, goalPos, maze);
                }

                explored.Add(currentNode);
                open.Remove(currentNode);

                foreach(PathfindingNode node in currentNode.OpenNodes)
                {
                    if(!explored.Contains(node) && !open.Contains(node))
                    {
                        open.Add(node);
                        if(node.Cost > currentNode.Cost + 1)
                        {
                            node.Cost = currentNode.Cost + 1;
                            node.Previous = currentNode;
                        }
                    }
                }
            }

            return null;
        }

        static private List<PathfindingNode> GetNodesForPosition(PathfindingNodeHorizontal[,] horizontalMap, PathfindingNodeVertical[,] verticalMap, Vector2 pos)
        {
            List<PathfindingNode> nodes = new List<PathfindingNode>();

            int i = (int)(pos.Y / Tile.Size);
            int j = (int)(pos.X / Tile.Size);
            nodes.AddIfNotNull(horizontalMap[i, j]);
            nodes.AddIfNotNull(horizontalMap[i+1, j]);
            nodes.AddIfNotNull(verticalMap[i, j]);
            nodes.AddIfNotNull(verticalMap[i, j+1]);

            return nodes;
        }

        static private PathfindingNode ChooseNode(List<PathfindingNode> open, int GoalI, int GoalJ)
        {
            int ManhattanDistance(PathfindingNode node)
            {
                return Math.Abs(node.I - GoalI) + Math.Abs(node.J - GoalJ);
            }

            int minCost = 9999999;
            PathfindingNode bestNode = open[0];
            foreach(PathfindingNode node in open)
            {
                int cost = node.Cost + ManhattanDistance(node);
                if(cost < minCost)
                {
                    minCost = cost;
                    bestNode = node;
                }
            }

            return bestNode;
        }

        static private List<Vector2> BuildPath(PathfindingNode lastNode, Vector2 goalPosition, Cell[,] maze)
        {
            List<Vector2> path = new List<Vector2>();
            PathfindingNode currentNode = lastNode;

            while(currentNode != null)
            {
                path.Add(currentNode.NavigationPosition);
                currentNode = currentNode.Previous;
            }
            path.Insert(0, goalPosition);

            SmoothPath(ref path, maze);

            return path;
        }

        static private void ClearNodesState(PathfindingNode[] map)
        {
            foreach(PathfindingNode node in map)
            {
                node.Cost = 9999;
                node.Previous = null;
            }
        }

        static private void SmoothPath(ref List<Vector2> path, Cell[,] maze)
        {
            for (int i = 0; i < path.Count; i++)
            {
                while (i+2 < path.Count)
                {
                    VisibilityRay ray = new VisibilityRay(path[i], path[i+2]);
                    if (!CollisionUtils.RayMapIntersectionTest(ray, maze))
                    {
                        path.RemoveAt(i + 1);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
