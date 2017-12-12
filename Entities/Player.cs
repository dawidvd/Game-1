using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_1.Entities
{
    class Player
    {
        private Vector2 position;
        private Matrix rotation;
        float speed = 0.002f;
        bool shooting = false;
        int weapon = 0;
        int count = 0;

        public Player(Graph graph)
        {
            this.graph = graph;
            Position = new Vector2();
            Rotation = Matrix.Identity;
        }

        private Graph graph;

        public Vector2 Position { get => position; set => position = value; }
        public Matrix Rotation { get => rotation; set => rotation = value; }

        public void Update(GameTime time, Game1 game)
        {
            var keyState = Keyboard.GetState();
            Vector2 moveVector = new Vector2(0,0);
            bool move = false;
            if(keyState.IsKeyDown(Keys.W))
            {
                moveVector.Y -= 1;
                moveVector.X -= 1;
                move = true;
            }

            if(keyState.IsKeyDown(Keys.S))
            {
                moveVector.X += 1;
                moveVector.Y += 1;
                move = true;
            }

            if(keyState.IsKeyDown(Keys.D))
            {
                moveVector.X += 1;
                moveVector.Y -= 1;
                move = true;
            }

            if(keyState.IsKeyDown(Keys.A))
            {
                moveVector.X -= 1;
                moveVector.Y += 1;
                move = true;
            }

            if (move && moveVector.Length() != 0) 
            {
                moveVector.Normalize();
                if (graph.CanGo(position, position + (moveVector * speed * (float)time.ElapsedGameTime.TotalMilliseconds)))
                {
                    var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 16 / 9, 0.1f, 100f);
                    rotation = Matrix.CreateLookAt(new Vector3(position.X, 0, position.Y), new Vector3(position.X - moveVector.X, 0, position.Y + moveVector.Y), Vector3.Up);
                    rotation.M41 = 0;
                    rotation.M43 = 0;
                    position += moveVector * speed * (float)time.ElapsedGameTime.TotalMilliseconds;
                }
            }

            if(keyState.IsKeyDown(Keys.D1))
            {
                weapon = 0;
            }
            else if(keyState.IsKeyDown(Keys.D2))
            {
                weapon = 1;
            }

            if(keyState.IsKeyDown(Keys.Space))
            {
                if (weapon == 0)
                {
                    if (shooting)
                        return;
                    game.AddBullet(this.position, this.rotation);
                    game.AddBullet(this.position, Matrix.CreateRotationY(MathHelper.ToRadians(30)) * this.rotation);
                    game.AddBullet(this.position, Matrix.CreateRotationY(MathHelper.ToRadians(-30)) * this.rotation);
                    shooting = true;
                }
                else if(weapon == 1)
                {
                    if (shooting)
                        return;
                    game.AddBullet(this.position, this.rotation, 2);
                    count++;
                    if(count > 30)
                    {
                        shooting = true;
                    }
                }
            }
            else if (shooting)
            {
                shooting = false;
                count = 0;
            }
        }
    }
}
