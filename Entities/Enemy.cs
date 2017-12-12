using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_1.Entities
{
    class Enemy
    {
        bool dead = false;
        Matrix rotation = Matrix.Identity;
        Vector2 position = new Vector2(2, 2);
        float angel = 0;
        float fallSpeed = 2;
        Vector2 hitPos;
        int falls = 1;
        float moveSpeed = 0.001f;
        Graph graph;
        List<Vector2> path;
        Vector2 playerPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 nextPoint;
        int life = 1;
        int hits = 0;

        public Enemy(Graph graph)
        {
            this.graph = graph;
        }

        public bool Dead
        {
            get
            {
                return dead;
            }

            set
            {
                dead = value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        public Matrix Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
            }
        }
        static public float LifetMultiplier { get; set; } = 1;
        static public int Level { get; set; } = 1;


        public void Update(GameTime gameTime, Player player)
        {
            if(!Dead)
            {
                if ((playerPosition - player.Position).LengthSquared() > 1f)
                {
                    path = graph.ShortestPath(this.Position, player.Position);
                    if (path.Count >= 2)
                    {
                        path.RemoveAt(path.Count - 1);
                        path.RemoveAt(0);
                    }

                    path.Add(player.Position);
                    nextPoint = path[0];
                    playerPosition = player.Position;
                }
                else
                {
                    if((position - nextPoint).Length() < 0.01f)
                    {
                        path.Remove(nextPoint);
                        if(path.Any())
                        {
                            nextPoint = path[0];
                        }
                        else
                        {
                            nextPoint = player.Position;
                        }
                    }
                }

                var move = nextPoint - this.position;
                move.Normalize();
                position += move * moveSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                var angle = -(float)Math.Atan2(move.Y, move.X) - MathHelper.PiOver2 ;
                Rotation = Matrix.CreateRotationY(angle);
            }

            if (Dead && angel != 90)
            {
                angel += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000 * 90 * fallSpeed;
                angel = MathHelper.Clamp(angel, 0, 90);
                if(angel == 90 && falls > 0)
                {
                    angel = 90 - falls * 5;
                    falls--;
                    fallSpeed /= 4;
                }

                Rotation = Matrix.CreateFromAxisAngle(new Vector3(-hitPos.Y, 0, hitPos.X), MathHelper.ToRadians(angel));
            }
        }


        public void OnColision(Vector2 pos)
        {
            if (!Dead)
            {
                hits++;
                if (hits > LifetMultiplier * (life + Level))
                {
                    Dead = true;
                    hitPos = pos - this.position;
                    hitPos.Normalize();
                }
            }
        }
    }
}
