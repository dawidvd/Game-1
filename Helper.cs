using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game_1
{
    static class Helper
    {
        public static BoundingBox GetBoundingBox(this Model model)
        {
            return model.GetBoundingBox(Matrix.Identity);
        }

        public static BoundingBox GetBoundingBox(this Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            // Create and return bounding box
            return new BoundingBox(min, max);
        }

        public static void DrawBoundingBox(this Model model, Matrix projection, Matrix view, Matrix world, GraphicsDevice graphicsDevice, Color color)
        {
            var bb = model.GetBoundingBox();
            var bblines = new VertexPositionColor[]{
                new VertexPositionColor(new Vector3(bb.Min.X, bb.Min.Y, bb.Min.Z), color),
                new VertexPositionColor(new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z),color),

                new VertexPositionColor(new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z),color),
                new VertexPositionColor(new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z),color),

                new VertexPositionColor(new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z),color),
                new VertexPositionColor(new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z),color),

                new VertexPositionColor(new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z),color),
                new VertexPositionColor(new Vector3(bb.Max.X, bb.Max.Y, bb.Max.Z),color),

                new VertexPositionColor(new Vector3(bb.Max.X, bb.Max.Y, bb.Max.Z),color),
                new VertexPositionColor(new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z),color),

                new VertexPositionColor(new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z),color),
                new VertexPositionColor(new Vector3(bb.Max.X, bb.Max.Y, bb.Max.Z),color),

                new VertexPositionColor(new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z),color),
                new VertexPositionColor(new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z),color),

                new VertexPositionColor(new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z),color),
                new VertexPositionColor(new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z),color),
            };

            var ef = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = projection,
                View = view,
                World = world
            };

            foreach (var pass in ef.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, bblines, 0, bblines.Length / 2);
            }
        }
    }
}
