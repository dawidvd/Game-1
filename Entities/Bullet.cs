using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_1.Entities
{
    class Bullet
    {
        Matrix rotation;
        Vector2 position;
        float speed = 0.01f;
        double time;
        double maxTime = 0.5;


        public Matrix Rotation { get => rotation; private set => rotation = value; }
        public Vector2 Position { get => position; private set => position = value; }
        public double Time { get => time; set => time = value; }

        public void Respawn(Vector2 position, Matrix rotation)
        {
            this.position = position;
            this.rotation = rotation;
            time = maxTime;
        }

        public void Update(GameTime gameTime)
        {
            var  move = rotation.Forward * (float)gameTime.ElapsedGameTime.TotalMilliseconds * speed;
            position += new Vector2(move.X, move.Z);
            Time -= gameTime.ElapsedGameTime.TotalSeconds; 
        }
    }
}
