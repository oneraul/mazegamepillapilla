using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class BiomeDrawScreen : IScreen
    {
        public struct Biome
        {
            public enum BlendingMode
            {
                Normal, Darken, Reflect
            }

            public Color BackgroundColor;
            public Color FloorColor;
            public Color WallColor;
            public Color WallTopColor;

            public Texture2D BackgroundOverlayTexture;
            public Texture2D FloorOverlayTexture;
            public Texture2D WallOverlayTexture;
            public Texture2D WallTopOverlayTexture;
            
            public BlendingMode BackgroundBlendingMode;
            public BlendingMode FloorBlendingMode;
            public BlendingMode WallBlendingMode;
            public BlendingMode WallTopBlendingMode;
        }


        Biome biome;
        Effect biomeEffect;
        Effect biomeBackgroundEffect;
        RenderTarget2D renderTarget;
        GameWorld world;
        Rectangle floorRectangle;
        Rectangle renderTargetRectangle;
        Matrix cameraMatrix;
        Texture2D pixel;

        public void Draw(SpriteBatch spritebatch)
        {
            List<IDrawable> SortDrawables()
            {
                List<IDrawable> drawables = new List<IDrawable>();

                List<IDrawable> itemsToInsert = new List<IDrawable>();
                itemsToInsert.AddRange(world.Pjs.Values);
                itemsToInsert.AddRange(world.Drops.Values);
                itemsToInsert.Sort((a, b) => b.GetSortY().CompareTo(a.GetSortY()));

                int mazeW = world.maze.GetLength(1);
                int mazeH = world.maze.GetLength(0);
                for (int y = 0; y < mazeH; y++)
                {
                    for (int i = itemsToInsert.Count - 1; i >= 0; i--)
                    {
                        IDrawable item = itemsToInsert[i];
                        Rectangle itemAabb = ((IIntersectable)item).GetAABB();

                        int leftX = itemAabb.Left / Tile.Size;
                        int rightX = itemAabb.Right / Tile.Size;

                        if (leftX < 0 || rightX >= world.maze.GetLength(1))
                        {
                            itemsToInsert.RemoveAt(i);
                        }
                        else
                        {
                            Cell leftCell = world.maze[y, leftX];
                            Cell rightCell = world.maze[y, rightX];

                            if (item.GetSortY() < leftCell.GetSortY() && item.GetSortY() < rightCell.GetSortY())
                            {
                                drawables.Add(item);
                                itemsToInsert.RemoveAt(i);
                            }
                        }
                    }

                    for (int x = 0; x < mazeW; x++)
                    {
                        drawables.Add(world.maze[y, x]);
                    }
                }

                return drawables;
            }

            // Draw the game onto a texture
            spritebatch.GraphicsDevice.SetRenderTarget(renderTarget);
            {
                // Draw Background
                spritebatch.GraphicsDevice.Clear(Color.TransparentBlack);

                biomeBackgroundEffect.Parameters["u_blendMode"].SetValue((int)biome.BackgroundBlendingMode);
                spritebatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap, null, null, biomeBackgroundEffect);
                spritebatch.Draw(biome.BackgroundOverlayTexture ?? pixel, renderTargetRectangle, renderTargetRectangle, biome.BackgroundColor);
                spritebatch.End();

                // Draw floor
                biomeBackgroundEffect.Parameters["u_blendMode"].SetValue((int)biome.FloorBlendingMode);
                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, biomeBackgroundEffect, cameraMatrix);
                spritebatch.Draw(biome.FloorOverlayTexture ?? pixel, floorRectangle, floorRectangle, biome.FloorColor);
                spritebatch.End();

                spritebatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, cameraMatrix);
                foreach (IDrawable drawable in SortDrawables())
                {
                    drawable.Draw(spritebatch, cameraMatrix);
                }

                spritebatch.End();
            }

            // Post-process the texture and draw it to the back buffer
            spritebatch.GraphicsDevice.SetRenderTarget(null);
            spritebatch.GraphicsDevice.Clear(Color.Crimson);
            {
                spritebatch.Begin(SpriteSortMode.Immediate, null, null, null, null, biomeEffect);
                spritebatch.Draw(renderTarget, renderTargetRectangle, Color.White);
                spritebatch.End();
            }
        }

        public void Enter() {}
        public void Update(float dt) {}
        public void Exit() {}

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            BiomeData.Biome biome = BiomeData.GetBiome(3);
            int mapId = 0;

            pixel = Content.Load<Texture2D>("pixel");
            Tile.InitTextures(GraphicsDevice, biome);
            world = new GameWorld();
            world.maze = Cell.ParseData(MapData.GetMap(mapId));

            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            renderTargetRectangle = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            cameraMatrix = Matrix.CreateTranslation(GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - Tile.Size * 11, Tile.Size, 0);
            floorRectangle = new Rectangle(0, 0, world.maze.GetLength(1) * Tile.Size, world.maze.GetLength(0) * Tile.Size);


            this.biome = new Biome
            {
                BackgroundColor = biome.BackgroundColor,
                FloorColor = biome.GroundColor,
                WallColor = biome.WallColor,
                WallTopColor = biome.WallTopColor,

                BackgroundOverlayTexture = null,
                FloorOverlayTexture = Content.Load<Texture2D>("gravelOverlay"),
                WallOverlayTexture = Content.Load<Texture2D>("gravelOverlay"),
                WallTopOverlayTexture = Content.Load<Texture2D>("iceOverlay"),

                BackgroundBlendingMode = Biome.BlendingMode.Normal,
                FloorBlendingMode = Biome.BlendingMode.Reflect,
                WallBlendingMode = Biome.BlendingMode.Reflect,
                WallTopBlendingMode = Biome.BlendingMode.Reflect,
            };

            biomeBackgroundEffect = Content.Load<Effect>("floor_shader");
            biomeEffect = Content.Load<Effect>("walls_shader");
            biomeEffect.Parameters["u_wallBlendingMode"]?.SetValue((int)this.biome.WallBlendingMode);
            biomeEffect.Parameters["u_wallColor"]?.SetValue(this.biome.WallColor.ToVector3());
            biomeEffect.Parameters["u_wallOverlayTexture"]?.SetValue(this.biome.WallOverlayTexture);
            biomeEffect.Parameters["u_wallOverlayTextureSize"]?.SetValue(new Vector2(this.biome.WallOverlayTexture.Width, this.biome.WallOverlayTexture.Height));
            biomeEffect.Parameters["u_wallTopBlendingMode"]?.SetValue((int)this.biome.WallTopBlendingMode);
            biomeEffect.Parameters["u_wallTopColor"]?.SetValue(this.biome.WallTopColor.ToVector3());
            biomeEffect.Parameters["u_wallTopOverlayTexture"]?.SetValue(this.biome.WallTopOverlayTexture);
            biomeEffect.Parameters["u_wallTopOverlayTextureSize"]?.SetValue(new Vector2(this.biome.WallTopOverlayTexture.Width, this.biome.WallTopOverlayTexture.Height));
            biomeEffect.Parameters["u_screenSize"]?.SetValue(new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight));
        }
    }
}
