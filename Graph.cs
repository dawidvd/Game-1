using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_1
{
    class Graph
    {
        float[,] graph;
        int width;
        int height;
        float step;
        Dictionary<Tuple<int, int>, List<Vector2>> precalculated = new Dictionary<Tuple<int, int>, List<Vector2>>();


        public Graph(int width, int height, float step)
        {
            this.width = width;
            this.height = height;
            this.step = step;

            width = (int)Math.Ceiling(width / step);
            height = (int)Math.Ceiling(height / step);

            graph = new float[width * height, width * height];
            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    graph[i, j] = float.PositiveInfinity;
                }
            }

            for (int i = 0; i < width * height; i++)
            {
                int x = i % width;
                int y = i / width;
                graph[i, i] = 0;
                if (x < width - 1)
                {
                    graph[i, i + 1] = 1;
                }

                if (y < height - 1)
                {
                    graph[i, i + width] = 1;
                    if (x > 0)
                    {
                        graph[i, i + width - 1] = 1.4f;
                    }

                    if (x < width - 1)
                    {
                        graph[i, i + 1] = 1.4f;
                    }
                }
            }
        }

        public void AddBlocade(Model model, Matrix world)
        {
            var bb = model.GetBoundingBox(Matrix.Identity);
            int xmin = (int)Math.Round(bb.Min.X / 2 + world.M41 + width / 2 - 1);
            int xmax = (int)Math.Round(bb.Max.X / 2 + world.M41 + width / 2);
            int ymin = (int)Math.Round(bb.Min.Y / 2 + world.M43 + height / 2 - 1);
            int ymax = (int)Math.Round(bb.Max.Y/2 + world.M43 + height / 2);

            xmin = MathHelper.Clamp(xmin, 0, width - 1);
            ymin = MathHelper.Clamp(ymin, 0, height - 1);
            xmax = MathHelper.Clamp(xmax, 0, width - 1);
            ymax = MathHelper.Clamp(ymax, 0, height - 1);

            for (int i = xmin; i <= xmax; i++)
            {
                for (int j = ymin; j <= ymax; j++)
                {
                    int index = i + j * width;
                    for (int k = 0; k < width * height; k++)
                    {
                        var u = Math.Min(index, k);
                        var v = Math.Max(index, k);
                        graph[u, v] = float.PositiveInfinity;
                    }
                }
            }
        }

        public List<Vector2> ShortestPath(Vector2 start, Vector2 finish)
        {
            int str = (int)Math.Round(start.X) + width/2 + (int)(Math.Round(start.Y) + height /2) * width;
            int fin = (int)Math.Round(finish.X) + width/2 + (int)(Math.Round(finish.Y) + height/2) * width;
            var key = new Tuple<int, int>(str, fin);

            if(precalculated.ContainsKey(key))
            {
                return precalculated[key];
            }

            var distance = new Dictionary<int, float>();
            var prev = new Dictionary<int, int>();

            var list = Enumerable.Range(0, width * height).ToList();
            foreach (var node in list)
            {
                distance[node] = float.PositiveInfinity;
                prev[node] = -1;
            }

            distance[str] = 0;

            while(list.Any())
            {
                list.Sort((x, y) => distance[x].CompareTo(distance[y]));
                var current = list.First();
                list.Remove(current);

                if(current == fin)
                {
                    break;
                }

                for (int i = 0; i < width*height; i++)
                {
                    int u = Math.Min(current, i);
                    int v = Math.Max(current, i);

                    var dist = distance[current] + graph[u, v];
                    if(dist < distance[i])
                    {
                        distance[i] = dist;
                        prev[i] = current;
                    }
                }
            }

            var output = GetPath(str, fin, prev);
            output.Reverse();
            precalculated[key] = output;

            return output;
        }

        public bool CanGo(Vector2 start, Vector2 finish)
        {
            int str = (int)Math.Round(start.X) + width/2 + (int)(Math.Round(start.Y + height /2)) * width;
            int fin = (int)Math.Round(finish.X) + width/2 + (int)(Math.Round(finish.Y + height/2)) * width;
            if (str > graph.GetLength(0) - 1 || str < 0 || fin > graph.GetLength(1) - 1 || fin < 0)
            {
                return false;
            }

            var u = Math.Min(str, fin);
            var v = Math.Max(str, fin);
            return !float.IsPositiveInfinity(graph[u, v]);
        }

        private List<Vector2> GetPath(int str, int fin, Dictionary<int, int> prev)
        {
            if(fin == -1)
            {
                return new List<Vector2>();
            }

            int node = fin;
            var output = new List<Vector2>();

            while (prev[node] != str && prev[node] != -1)
            {
                output.Add(new Vector2((node % width) - width/2, (node / width) - height/2));
                node = prev[node];
            }

            output.Add(new Vector2((str % width) - width/2, (node / width) - height/2));
            return output;
        }

        public void DrawGraph(GraphicsDevice graphicsDevice, Matrix projection, Matrix view)
        {
            var vertexes = new VertexPositionColor[width * height * 6];
            int index = 0;
            for (int i = 0; i < width*height; i++)
            {
                float x = i % width - width / 2;
                float y = i / width - height / 2;
                var color = graph[i, i] == float.PositiveInfinity ? Color.Red : Color.Blue;
                vertexes[index++] = new VertexPositionColor(new Vector3(x, 0, y), color);
                vertexes[index++] = new VertexPositionColor(new Vector3(x + 1, 0, y), color);
                vertexes[index++] = new VertexPositionColor(new Vector3(x + 1, 0, y + 1), color);

                vertexes[index++] = new VertexPositionColor(new Vector3(x, 0, y), color);
                vertexes[index++] = new VertexPositionColor(new Vector3(x + 1, 0, y + 1), color);
                vertexes[index++] = new VertexPositionColor(new Vector3(x, 0, y + 1), color);
            }

            BasicEffect effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                World = Matrix.Identity,
                Projection = projection,
                View = view
            };

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexes, 0, vertexes.Length/3);
            }

        }
    }
}
