﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game_1.Entities
{
    class Floor
    {
        int width = 50, height = 50;
        VertexPositionNormalTexture[] positionColor = new VertexPositionNormalTexture[6];
        Vector3 color;
        Color[] groundColors;
        public Texture2D[] Texture { get; set; } = new Texture2D[3];
        int index = 0;

        public Floor(GraphicsDevice graphicsDevice)
        {
            positionColor[0] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), Vector3.Up, new Vector2(0,0));
            positionColor[1] = new VertexPositionNormalTexture(new Vector3(width, 0, 0), Vector3.Up, new Vector2(1,0));
            positionColor[2] = new VertexPositionNormalTexture(new Vector3(0, 0, height), Vector3.Up, new Vector2(0,1));
            positionColor[3] = new VertexPositionNormalTexture(new Vector3(width, 0, 0), Vector3.Up, new Vector2(1,0));
            positionColor[4] = new VertexPositionNormalTexture(new Vector3(width, 0, height), Vector3.Up, new Vector2(1,1));
            positionColor[5] = new VertexPositionNormalTexture(new Vector3(0, 0, height), Vector3.Up, new Vector2(0,1));
            groundColors = new Color[]
            {
                Color.White
            };
            ChangeColor(1);
        }

        public void ChangeColor(int lvl)
        {
            ChangeColor(groundColors[lvl % groundColors.Length]);
            index = lvl - 1;
        }

        public void ChangeColor(Color newColor)
        {
            color = new Vector3(newColor.R / 256f, newColor.G / 256f, newColor.B / 256f);
        }

        public void DrawGround(Camera camera, Matrix projection, GraphicsDevice graphicsDevice, Vector3 lightPos, Matrix lightsView, Matrix lightsProjection)
        {
            var effect = new BasicEffect(graphicsDevice);
            effect.Projection = projection;
            effect.View = camera.ViewMatrix;
            effect.World = Matrix.CreateTranslation(- width/2, 0, -height/2);
            effect.Texture = Texture[index % Texture.Length];
            effect.TextureEnabled = true;
            effect.EnableDefaultLighting();
            effect.DirectionalLight0.Enabled = true;
            effect.DiffuseColor = color;
            /*Matrix world = Matrix.CreateTranslation(-width / 2, 0, -height / 2);
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["DeffuseColor"].SetValue(color);
            effect.Parameters["LightPos"].SetValue(lightPos);
            effect.Parameters["LightPower"].SetValue(0.9f);
            effect.Parameters["LightWorldViewProjection"].SetValue(world * lightsView * lightsProjection);
            */

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, positionColor, 0, 2);
            }
        }
    }
}
