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

        public bool Dead { get => dead; set => dead = value; }
        public Vector2 Position { get => position; set => position = value; }
        public Matrix Rotation { get => rotation; set => rotation = value; }

        public void Update(GameTime gameTime, Player player)
        {
            if(!Dead)
            {
                var move = player.Position - this.position;
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
                Dead = true;
                hitPos = pos - this.position;
                hitPos.Normalize();
            }
        }
    }
}
