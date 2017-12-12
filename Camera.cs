using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_1
{
    class Camera
    {
        public Vector3 FocusPoint = Vector3.Zero;

        //This tells us where the camera should be, RELATIVE to the point we are watching.
        //I set this a little up and a little back
        Vector3 CameraOffset = new Vector3(0f, 8, 8);

        public Matrix ViewMatrix
        {
            get
            {
                //The Offset is just up and back, we need to rotate it 45*
                var rotatedOffste = Vector3.Transform(CameraOffset, Matrix.CreateRotationY(MathHelper.PiOver2 * 0.5f));
                
                //Now we can create out viewmatrix. No need to use a transformed "up" unless it's not going to be upside down or something.
                return Matrix.CreateLookAt(rotatedOffste + FocusPoint, FocusPoint, Vector3.Up);
            }
        }
    }
}
