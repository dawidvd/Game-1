﻿using System;
using Game_1.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

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
        Model crate;
        Camera camera = new Camera();
        Floor floor;
        Player player;
        List<Enemy> enemies =new List<Enemy>();
        List<Bullet> bullets = new List<Bullet>();
        Dictionary<ModelMeshPart, Vector3> partColor = new Dictionary<ModelMeshPart, Vector3>();
        RenderTarget2D renderTarget;
        RenderTarget2D game;
        Texture2D gameOverTexture;
        Texture2D shadowMap;
        Texture2D keysTexture;
        Texture2D menuBackground;
        Effect effect;
        Effect postEffect;
        SpriteBatch spriteBatch;
        SpriteFont font;
        int next = 0;
        bool effects = false;
        bool mPressed = false;
        bool nPressed = false;
        bool tilt = true;
        PointSystem pointSystem = new PointSystem();
        Graph graph = new Graph(22, 22, 1);
        Matrix[] crateMatrix = new Matrix[9];
        bool menu = true;
        bool key = false;
        bool inProggress = false;
        bool gameOver = false;
        int level = 1;
        int enemyCount = 0;
        int maxEnemyCount = 4;

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
            player = new Player(graph);
            game = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            enemies.Add(new Enemy(graph) { Position = new Vector2(-10, 10) });
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
            this.crate = this.Content.Load<Model>("Crate-04");
            effect = this.Content.Load<Effect>("shader");
            postEffect = this.Content.Load<Effect>("postEffect");
            font = this.Content.Load<SpriteFont>("font");
            floor.Texture[0] = this.Content.Load<Texture2D>("sand");
            floor.Texture[1] = this.Content.Load<Texture2D>("snow");
            floor.Texture[2] = this.Content.Load<Texture2D>("grass");
            gameOverTexture = this.Content.Load<Texture2D>("gameOver");
            keysTexture = this.Content.Load<Texture2D>("keys");
            menuBackground = this.Content.Load<Texture2D>("menu");
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
            crateMatrix[0] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(-2, 1, 3);
            crateMatrix[1] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(0, 1, 4);
            crateMatrix[2] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(-3, 1, -3);

            crateMatrix[3] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(4, 1, 3);
            crateMatrix[4] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(2, 1, 0);
            crateMatrix[5] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(0, 1, 3);

            crateMatrix[6] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(7, 1, 0);
            crateMatrix[7] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(2, 1, 3);
            crateMatrix[8] = Matrix.CreateScale(2f)* Matrix.CreateTranslation(5, 1, 2);
            graph.AddBlocade(crate, crateMatrix[0]);
            graph.AddBlocade(crate, crateMatrix[1]);
            graph.AddBlocade(crate, crateMatrix[2]);

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
                menu = true;

            // TODO: Add your update logic here
            if (menu)
            {
                var state = Mouse.GetState();
                if(state.LeftButton == ButtonState.Pressed)
                {
                    if(inProggress && state.Position.Y > 70 && state.Position.Y < 140)
                    {
                        menu = false;
                    }
                    else if(state.Position.Y > 140)
                    {
                        if(state.Position.Y < 210)
                        {
                            /// Easy
                            menu = false;
                            pointSystem.Multiplier = 1;
                            Enemy.LifetMultiplier = 1;
                            inProggress = true;
                            Restart();
                        }
                        else if(state.Position.Y < 280)
                        {
                            /// medium
                            menu = false;
                            pointSystem.Multiplier = 2;
                            Enemy.LifetMultiplier = 2;
                            inProggress = true;
                            Restart();
                        }
                        else if(state.Position.Y < 350)
                        {
                            /// hard
                            menu = false;
                            pointSystem.Multiplier = 3;
                            Enemy.LifetMultiplier = 3;
                            inProggress = true;
                            Restart();
                        }
                        else if(state.Position.Y < 420)
                        {
                            Exit();
                        }
                    }
                }
            }
            else if(key)
            {
                if(Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    key = false;
                }
            }
            else
            {
                if(gameOver)
                {
                    return;
                }
                player.Update(gameTime, this);
                foreach (var enemy in enemies)
                {
                    enemy.Update(gameTime, player);
                }

                foreach (var bullet in bullets)
                {
                    bullet.Update(gameTime);
                }

                bullets.RemoveAll(x => x.Time <= 0);
                CheckCollisions();

                var keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.M))
                {
                    if (!mPressed)
                    {
                        if (tilt)
                        {
                            postEffect.CurrentTechnique = postEffect.Techniques[1];
                            tilt = false;
                        }
                        else
                        {
                            postEffect.CurrentTechnique = postEffect.Techniques[0];
                            tilt = true;
                        }

                        mPressed = true;
                    }
                }
                else
                {
                    mPressed = false;
                }

                if (keyState.IsKeyDown(Keys.N))
                {
                    if (!nPressed)
                    {
                        effects = !effects;
                        nPressed = true;
                    }
                }
                else
                {
                    nPressed = false;
                }

                pointSystem.UpdateScore(gameTime);
            }
            base.Update(gameTime);
        }

        private void CheckCollisions()
        {
            bool spawnNew = false;
            foreach (var enemy in enemies)
            {
                if (enemy.Dead)
                {
                    continue;
                }

                float distance = Vector2.Distance(player.Position, enemy.Position);
                if (distance < 0.2)
                {
                    gameOver = true;
                }

                foreach (var bullet in bullets)
                {
                    float distance2 = Vector2.Distance(bullet.Position, enemy.Position);
                    if (distance2 < 0.5f)
                    {
                        enemy.OnColision(bullet.Position);
                        bullets.Remove(bullet);
                        if (enemy.Dead)
                        {
                            spawnNew = true;
                            pointSystem.GainPoint();
                            enemyCount++;
                        }

                        break;
                    }
                }
            }

            if (enemyCount > maxEnemyCount)
            {
                enemyCount = 0;
                NextLevel();
            }

            if (spawnNew)
            {
                Random random = new Random();
                for (int i = 0; i < 1; i++)
                {
                    var enemy = new Enemy(graph);
                    int val = next % 4;
                    switch (val)
                    {
                        case 0:
                            {
                                enemy.Position = new Vector2(-10, -10);
                            }
                            break;
                        case 1:
                            {
                                enemy.Position = new Vector2(-10, 10);
                            }
                            break;
                        case 2:
                            {
                                enemy.Position = new Vector2(10, 10);
                            }
                            break;
                        case 3:
                            {
                                enemy.Position = new Vector2(10, -10);
                            }
                            break;
                    }
                    next++;

                    enemies.Add(enemy);
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
            Vector3 lightPos = new Vector3(10, 8, 10);
            var lightsView = Matrix.CreateLookAt(lightPos, new Vector3(-30, 0, -30), Vector3.Up);
            var lightsProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(120), 1f, 1, 1000f);
            float lightPower = 0.7f;

            if (!menu && !key)
            {
                this.IsMouseVisible = false;

                if (effects)
                {
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
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                }
                else
                {
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Black);
                    effect.CurrentTechnique = effect.Techniques[0];
                    DrawGame(projection, lightPos, lightsView, lightsProjection, lightPower);
                }

                spriteBatch.Begin();
                spriteBatch.DrawString(font, "x" + pointSystem.KillMultiplier, Vector2.Zero, Color.Red);
                spriteBatch.DrawString(font, pointSystem.Points.ToString(), new Vector2(GraphicsDevice.Viewport.Width / 2, 0), Color.White);
                string kills = string.Format("{0,2}\\{1,2}", enemyCount, maxEnemyCount + 1);
                spriteBatch.DrawString(font, kills, new Vector2(GraphicsDevice.Viewport.Width - font.MeasureString(kills).X - 20, 0), Color.White);
                if(gameOver)
                {
                    spriteBatch.Draw(gameOverTexture, new Rectangle(GraphicsDevice.Viewport.Width / 2 - gameOverTexture.Width/2, GraphicsDevice.Viewport.Height /2 - gameOverTexture.Height / 2, gameOverTexture.Width, gameOverTexture.Height), Color.White);
                }
                spriteBatch.End();
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            else if (menu)
            {
                GraphicsDevice.Clear(Color.Black);
                this.IsMouseVisible = true;
                spriteBatch.Begin();
                spriteBatch.Draw(menuBackground, GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.DrawString(font, "Game1", new Vector2(GraphicsDevice.Viewport.Width / 2, 0), Color.White);
                if (inProggress)
                {
                    spriteBatch.DrawString(font, "Resume", new Vector2(GraphicsDevice.Viewport.Width / 2, 70), Color.White);
                }

                spriteBatch.DrawString(font, "Easy", new Vector2(GraphicsDevice.Viewport.Width / 2, 140), Color.White);
                spriteBatch.DrawString(font, "Medium", new Vector2(GraphicsDevice.Viewport.Width / 2, 210), Color.White);
                spriteBatch.DrawString(font, "Hard", new Vector2(GraphicsDevice.Viewport.Width / 2, 280), Color.White);
                spriteBatch.DrawString(font, "Exit", new Vector2(GraphicsDevice.Viewport.Width / 2, 350), Color.White);
                spriteBatch.End();
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                effect.Parameters["View"].SetValue(Matrix.CreateLookAt(new Vector3(0,0,-10), Vector3.Zero, Vector3.Up));
                effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographic(10,10,0,100));
                effect.Parameters["LightPos"].SetValue(new Vector3(0,0, -10));
                effect.Parameters["LightPower"].SetValue(0.9f);

                foreach (var model in snowmanModel.Meshes)
                {
                    var transMatrix = Matrix.CreateTranslation(1, 0, 0);

                    foreach (var part in model.MeshParts)
                    {
                        part.Effect.Parameters["World"].SetValue(transMatrix);
                        part.Effect.Parameters["DeffuseColor"].SetValue(partColor[part]);
                        part.Effect.Parameters["LightWorldViewProjection"].SetValue(player.Rotation * transMatrix * lightsView * lightsProjection);
                    }

                    model.Draw();
                }

                foreach (var model in elf.Meshes)
                {
                    Matrix transMatrix = Matrix.CreateTranslation(3, -0.5f,0);

                    foreach (var part in model.MeshParts)
                    {
                        part.Effect.Parameters["World"].SetValue(Matrix.CreateRotationY(MathHelper.ToRadians(-45))*transMatrix);
                        part.Effect.Parameters["DeffuseColor"].SetValue(partColor[part]);
                        part.Effect.Parameters["LightWorldViewProjection"].SetValue(transMatrix * lightsView * lightsProjection);
                    }

                    model.Draw();
                }
            }
            else if(key)
            {
                GraphicsDevice.Clear(Color.Black);
                this.IsMouseVisible = false;
                var str = "Press space to play";
                spriteBatch.Begin();
                spriteBatch.Draw(this.keysTexture, new Rectangle(0,0,GraphicsDevice.Viewport.Width, (int)(GraphicsDevice.Viewport.Height - font.MeasureString(str).Y * 2)), Color.White);
                spriteBatch.DrawString(font,str, new Vector2(GraphicsDevice.Viewport.Width / 2 - font.MeasureString(str).X / 2, GraphicsDevice.Viewport.Height - font.MeasureString(str).Y), Color.Gray);
                spriteBatch.End();

            }

            base.Draw(gameTime);
        }

        private void DrawGame(Matrix projection, Vector3 lightPos, Matrix lightsView, Matrix lightsProjection, float lightPower)
        {
            
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["LightPos"].SetValue(lightPos);
            effect.Parameters["LightPower"].SetValue(0.9f);

            floor.DrawGround(camera, projection, GraphicsDevice, lightPos, lightsView, lightsProjection);
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

            // test
            // snowmanModel.DrawBoundingBox(projection, camera.ViewMatrix, player.Rotation * Matrix.CreateTranslation(player.Position.X, 0, player.Position.Y), GraphicsDevice, Color.Blue);

            foreach (var enemy in enemies)
            {
                var bb = elf.GetBoundingBox();
                foreach (var model in elf.Meshes)
                {
                    Matrix transMatrix;
                    if (enemy.Dead && effect.CurrentTechnique != effect.Techniques[1])
                    {
                        transMatrix = Matrix.CreateTranslation(enemy.Position.X, 1f, enemy.Position.Y);
                    }
                    else
                    {
                        transMatrix = Matrix.CreateTranslation(enemy.Position.X, 0, enemy.Position.Y);
                    }

                    foreach (var part in model.MeshParts)
                    {
                        part.Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(-(bb.Max.X - bb.Min.X)/2, 0, -(bb.Max.Z - bb.Min.Z)/2)*enemy.Rotation * transMatrix);
                        part.Effect.Parameters["DeffuseColor"].SetValue(partColor[part]);
                        part.Effect.Parameters["LightWorldViewProjection"].SetValue(enemy.Rotation * transMatrix * lightsView * lightsProjection);
                    }

                    model.Draw();
                }
            }

            for(int i = 0; i < 3; i++)
            {
                foreach (var model in crate.Meshes)
                {

                    var effect = model.Effects[0] as BasicEffect;
                    effect.World = crateMatrix[i + (level -1) % 3 * 3];
                    effect.Projection = projection;
                    effect.View = camera.ViewMatrix;
                    model.Draw();
                }
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


        public void AddBullet(Vector2 positon, Matrix rotation, double maxTime = 0.5)
        {
            var bullet = new Bullet();
            bullet.Respawn(positon, rotation, maxTime);
            bullets.Add(bullet);
        }

        private void Restart()
        {
            pointSystem.Points = 0;
            pointSystem.KillMultiplier = 1;
            player.Position = new Vector2();
            bullets.Clear();
            enemies.Clear();
            floor.ChangeColor(1);
            graph = new Graph(22, 22, 1);
            graph.AddBlocade(crate, crateMatrix[0]);
            graph.AddBlocade(crate, crateMatrix[1]);
            graph.AddBlocade(crate, crateMatrix[2]);
            player.graph = graph;
            enemies.Add(new Enemy(graph) { Position = new Vector2(-10, 10) });
            gameOver = false;
            level = 1;
            Enemy.Level = 1;
            enemyCount = 0;
            maxEnemyCount = 4;
            this.key = true;
        }

        private void NextLevel()
        {
            level++;
            Enemy.Level = level;
            enemies.Clear();
            bullets.Clear();
            player.Position = new Vector2();
            floor.ChangeColor(level);
            enemyCount = 0;
            maxEnemyCount *= 2;
            graph = new Graph(22, 22, 1);
            graph.AddBlocade(crate, crateMatrix[0 + (level -1) % 3 * 3]);
            graph.AddBlocade(crate, crateMatrix[1 + (level - 1) % 3 * 3]);
            graph.AddBlocade(crate, crateMatrix[2 + (level -1) % 3 * 3]);
            player.graph = graph;
        }
    }
}
