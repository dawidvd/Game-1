﻿using System;
using Game_1.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Game_1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        Model snowmanModel;
        Model elf;
        Camera camera = new Camera();
        Floor floor;
        Player player;
        Enemy enemy;
        List<Bullet> bullets = new List<Bullet>();
        Dictionary<ModelMeshPart, Vector3> partColor = new Dictionary<ModelMeshPart, Vector3>();
        RenderTarget2D renderTarget;
        RenderTarget2D game;
        Texture2D shadowMap;
        Effect effect;
        Effect postEffect;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            graphics.PreferredBackBufferHeight= GraphicsDevice.DisplayMode.Height;
            graphics.PreferredBackBufferWidth= GraphicsDevice.DisplayMode.Width;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            // TODO: Add your initialization logic here
            floor = new Floor(this.graphics.GraphicsDevice);
            player = new Player();
            enemy = new Enemy();
            game = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            // TODO: use this.Content to load your game content here
            this.snowmanModel = this.Content.Load<Model>("snowMan");
            this.elf = this.Content.Load<Model>("elf");
            effect = this.Content.Load<Effect>("shader");
            postEffect = this.Content.Load<Effect>("postEffect");
            foreach (var mesh in this.snowmanModel.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var basic = part.Effect as BasicEffect;
                    partColor.Add(part, basic.DiffuseColor);

                    part.Effect = effect;
                }
            }

            foreach (var mesh in elf.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var basic = part.Effect as BasicEffect;
                    partColor.Add(part, basic.DiffuseColor);
                    part.Effect = effect;
                }
            }

            var pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            player.Update(gameTime, this);
            enemy.Update(gameTime);
            foreach (var bullet in bullets)
            {
               bullet.Update(gameTime);
            }

            bullets.RemoveAll(x => x.Time <= 0);
            CheckCollisions();
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.M))
            {
                postEffect.CurrentTechnique = postEffect.Techniques[0];
            }
            else
            {
                postEffect.CurrentTechnique = postEffect.Techniques[1];

            }
            base.Update(gameTime);
        }

        private void CheckCollisions()
        {
            if (enemy.Dead)
            {
                return;
            }

            float distance = Vector2.Distance(player.Position, enemy.Position); 
            if(distance < 1)
            {
                enemy.OnColision(player.Position);
            }
            foreach (var bullet in bullets)
            {
                float distance2 = Vector2.Distance(bullet.Position, enemy.Position);
                if(distance2 < 0.5f)
                {
                    enemy.OnColision(bullet.Position);
                    bullets.Remove(bullet);
                    break;
                }
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 16 / 9, 0.1f, 100f);
            // TODO: Add your drawing code here
            Vector3 lightPos = new Vector3(10, 7, 10);
            var lightsView = Matrix.CreateLookAt(lightPos, new Vector3(-30, 0, -30), Vector3.Up);
            var lightsProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(120), 1f, 1, 1000f);
            float lightPower = 0.7f;

            /// Shadow map
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Black);
            effect.CurrentTechnique = effect.Techniques[1];
            DrawGame(projection, lightPos, lightsView, lightsProjection, lightPower);

            GraphicsDevice.SetRenderTarget(game);
            shadowMap = renderTarget;
            /// Game
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            effect.CurrentTechnique = effect.Techniques[2];
            effect.Parameters["ShadowMap"].SetValue(shadowMap);
            DrawGame(projection, lightPos, lightsView, lightsProjection, lightPower);
            GraphicsDevice.SetRenderTarget(null);
            Texture2D texture2D = game;
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone, postEffect);
            spriteBatch.Draw(game, GraphicsDevice.PresentationParameters.Bounds, Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }

        private void DrawGame(Matrix projection, Vector3 lightPos, Matrix lightsView, Matrix lightsProjection, float lightPower)
        {
            floor.DrawGround(camera, projection, GraphicsDevice, effect, lightPos, lightsView, lightsProjection);
            foreach (var model in snowmanModel.Meshes)
            {
                var transMatrix = Matrix.CreateTranslation(player.Position.X, 0, player.Position.Y);

                foreach (var part in model.MeshParts)
                {
                    part.Effect.Parameters["World"].SetValue(player.Rotation * transMatrix);
                    part.Effect.Parameters["DeffuseColor"].SetValue(partColor[part]);
                    part.Effect.Parameters["LightWorldViewProjection"].SetValue(player.Rotation * transMatrix * lightsView * lightsProjection);
                }
                model.Draw();
            }

            foreach (var model in elf.Meshes)
            {
                Matrix transMatrix;
                if(enemy.Dead && effect.CurrentTechnique == effect.Techniques[2])
                {
                    transMatrix = Matrix.CreateTranslation(enemy.Position.X, 1f, enemy.Position.Y);
                }
                else
                {
                    transMatrix = Matrix.CreateTranslation(enemy.Position.X, 0, enemy.Position.Y);
                }

                foreach (var part in model.MeshParts)
                {
                    part.Effect.Parameters["World"].SetValue(enemy.Rotation * transMatrix);
                    part.Effect.Parameters["DeffuseColor"].SetValue(partColor[part]);
                    part.Effect.Parameters["LightWorldViewProjection"].SetValue(enemy.Rotation * transMatrix * lightsView * lightsProjection);
                }

                model.Draw();
            }

            foreach (var bullet in bullets)
            {
                foreach (var model in snowmanModel.Meshes)
                {
                    var transMatrix = Matrix.CreateTranslation(bullet.Position.X, 0, bullet.Position.Y);
                    var world = Matrix.CreateScale(0.25f) * bullet.Rotation * Matrix.CreateTranslation(0, 0.5f, 0) * transMatrix;

                    foreach (var part in model.MeshParts)
                    {
                        part.Effect.Parameters["World"].SetValue(world);
                        part.Effect.Parameters["DeffuseColor"].SetValue(partColor[part]);
                        part.Effect.Parameters["LightWorldViewProjection"].SetValue(world * lightsView * lightsProjection);
                    }
                    model.Draw();
                }
            }
        }

        public void AddBullet(Vector2 positon, Matrix rotation)
        {
            var bullet = new Bullet();
            bullet.Respawn(positon, rotation);
            bullets.Add(bullet);
        }
    }
}
