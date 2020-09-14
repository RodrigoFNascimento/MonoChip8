﻿using Chip8.Chip8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chip8
{
    public class Game1 : Game
    {
        private const int PixelSize = 20;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _pixelTexture;

        private CPU _cpu { get; set; }
        private Chip8.Keyboard _keyboard { get; set; }
        private Speaker _speaker { get; set; }

        public Game1()
        {
            Window.AllowUserResizing = true;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.ApplyChanges();

            _keyboard = new Chip8.Keyboard();
            _speaker = new Speaker();

            _cpu = new CPU(_keyboard, _speaker);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// This method is called after the constructor, but before the main game loop(Update/Draw).
        /// This is where you can query any required services and load any non-graphic related content.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// This method is used to load your game content.
        /// It is called only once per game, after Initialize method, but before the main game loop methods.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            _cpu.LoadSpritesIntoMemory();
            _cpu.LoadROM(@"C:\Users\rodri\source\repos\Chip8\Chip8\roms\Brix [Andreas Gustafsson, 1990].ch8");

            _pixelTexture = Content.Load<Texture2D>("pixel");
        }

        /// <summary>
        /// This method is called multiple times per second,
        /// and is used to update your game state (checking for collisions, gathering input, playing audio, etc.).
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _cpu.Cycle();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            for (int i = 0; i < CPU.DisplayWidth; i++)
            {
                for (int j = 0; j < CPU.DisplayHeight; j++)
                {
                    if (_cpu.Display[i, j])
                        _spriteBatch.Draw(
                            _pixelTexture,
                            new Rectangle(
                                i * PixelSize,
                                j * PixelSize,
                                PixelSize,
                                PixelSize),
                            Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
