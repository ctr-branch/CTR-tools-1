﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ctrviewer
{
    class InstancedModel
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float Scale;
        public string ModelName;

        public InstancedModel()
        {
        }

        public InstancedModel(string name, Vector3 pos, Vector3 rot, float scale)
        {
            Position = pos;
            Rotation = rot;
            ModelName = name;
            Scale = scale;
        }

        public void Update(GameTime gameTime)
        {
            Rotation += new Vector3((float)(-3.14f / 4 * gameTime.ElapsedGameTime.TotalSeconds), 0f, 0f);
        }

        public void Draw(GraphicsDeviceManager graphics, BasicEffect effect, AlphaTestEffect alpha, FirstPersonCamera camera)
        {
            effect.World = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * Matrix.CreateTranslation(Position);
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;

            if (Game1.instmodels.ContainsKey(ModelName))
            {
                Game1.instmodels[ModelName].Draw(graphics, effect, alpha);
            }
            else if (Game1.instTris.ContainsKey(ModelName))
            {
                Game1.instTris[ModelName].Draw(graphics, effect, alpha);
            }
            else
            {
                Console.WriteLine(ModelName + " not loaded");
            }
        }
    }
}
