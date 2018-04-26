using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
// System.Collections.Generic;//67tt6u65u65hh
namespace VRepClient
{
    public class PathNode
    {
        public Point Position { get; set; }//Координаты точки на карте
        public float PathLengthFromStart { get; set; }//Длина пути от старта до точки (G)
        public PathNode CameFrom { get; set; }//Точка из которой пришли в эту точку
        public float HeuristicEstimatePathLength { get; set; }//Примерное расстояние от точки до цели (H)
        public float EstimateFullPathLength
        {
            get
            {
                return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
            }
        }
    }
    public class SearchInGraph
    {
        public List<Point> FindPath(float[,] field, Point start, Point goal) //убрано static
        {//шаг 1
            var closedSet = new Collection<PathNode>();
            var openSet = new Collection<PathNode>();
            //шаг 2
            PathNode startNode = new PathNode()
            {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLenght(start, goal)
            };
            openSet.Add(startNode);
            while (openSet.Count > 0 & openSet.Count<200)
            {
                //Шаг 3 
                var currentNode = openSet.OrderBy(node => node.EstimateFullPathLength).First();
                //Шаг 4
                if (currentNode.Position == goal) 
                    return GetPathForNode(currentNode);
                //Шаг 5
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                //Шаг 6
                foreach (var neighbourNode in GetNeighbours(currentNode, goal, field))
                {
                    //Шаг 7
                    if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0)
                        continue;
                    var openNode = openSet.FirstOrDefault(node => node.Position == neighbourNode.Position);
                    //Шаг 8
                    if (openNode == null)
                        openSet.Add(neighbourNode);
                    else
                        if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                        {
                            //Шаг 9
                            openNode.CameFrom = currentNode;
                            openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                        }
                }
            }
            //Шаг 10
            return null;
        }

        private static float GetDistanceBetweenNeighbours(float weight)//Функция расстояния от Х до Y  (int weight)
        {
            return weight;//сюда надо добавить проходимость клетки, на данный момент расстояние всегда равно 1
        }

        private static int GetHeuristicPathLenght(Point from, Point to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);// функция примерной оценки расстояния до цели
        }

        private static Collection<PathNode> GetNeighbours(PathNode pathNode, Point goal, float[,] field)
        {
            var result = new Collection<PathNode>();
            //Соседними точками являются соседние по стороне клетки
            Point[] neighbourPoints = new Point[8];
            neighbourPoints[0] = new Point(pathNode.Position.X + 1, pathNode.Position.Y);
            neighbourPoints[1] = new Point(pathNode.Position.X - 1, pathNode.Position.Y);
            neighbourPoints[2] = new Point(pathNode.Position.X, pathNode.Position.Y + 1);
            neighbourPoints[3] = new Point(pathNode.Position.X, pathNode.Position.Y - 1);
            neighbourPoints[4] = new Point(pathNode.Position.X + 1, pathNode.Position.Y + 1);
            neighbourPoints[5] = new Point(pathNode.Position.X - 1, pathNode.Position.Y - 1);
            neighbourPoints[6] = new Point(pathNode.Position.X - 1, pathNode.Position.Y + 1);
            neighbourPoints[7] = new Point(pathNode.Position.X + 1, pathNode.Position.Y - 1);
            foreach (var point in neighbourPoints)
            {//проверяем не вышли ли за границы карты
                if (point.X < 0 || point.X >= field.GetLength(0))
                    continue;
                if (point.Y < 0 || point.Y >= field.GetLength(1))
                    continue;
                //проверяем что по клетке можно ходить
                //проверяем пять ближайших клеток
                bool key = false;
                int freeNode = 0;
                for (int i = -3; i < 4; i++)
                {
                    for (int k = -3; k < 4; k++)
                    {
                        if (point.X + i > 0 && point.X + i < field.GetLength(0))
                        {
                            if (point.Y + k > 0 && point.Y + k < field.GetLength(1))
                            {
                                if (field[point.X + i, point.Y + k] == 1)
                                {
                                    freeNode++;
                                }
                            }
                        }
                    }
                }
                // if ((field[point.X, point.Y] != 0) && (field[point.X, point.Y] < 1)&&freeNode>49)
                // continue;
                float weight;
                if (pathNode.Position.X != point.X && pathNode.Position.Y != point.Y)//диагональные смещения стоят 1,4 а прямые 1
                    weight = 1.4f;
                else
                    weight = 1;
                if ((field[point.X, point.Y] < 2) && freeNode == 49)
                {
                    //зополняем данные для точки маршрута
                    var neighbourNode = new PathNode()
                    {
                        Position = point,
                        CameFrom = pathNode,
                        PathLengthFromStart = pathNode.PathLengthFromStart + GetDistanceBetweenNeighbours(weight),
                        HeuristicEstimatePathLength = GetHeuristicPathLenght(point, goal)
                    };
                    result.Add(neighbourNode);
                }
            }
            return result;
        }
        private static List<Point> GetPathForNode(PathNode pathNode)
        {
            var result = new List<Point>();
            var currentNode = pathNode;
            while (currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }

    }

}
