using Microsoft.Xna.Framework;
using System;

namespace Game_1
{
    class PointSystem
    {
        public float Multiplier { get; set; } = 1;
        public int KillMultiplier { get; set; } = 1;
        public int Points { get; set; }
        private double time = 0;
        private float basePoints = 100;

        public void GainPoint()
        {
            Points += (int)Math.Ceiling(basePoints * Multiplier * KillMultiplier);
            KillMultiplier = MathHelper.Clamp(KillMultiplier + 1, 1, 5);
            time = 0;
        }

        public void UpdateScore(GameTime time)
        {
            this.time += time.ElapsedGameTime.TotalMilliseconds;
            if(this.time > 2000)
            {
                KillMultiplier = 1;
            }
        }
    }
}
