using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BitSits_Framework;

#if WINDOWS_PHONE
using System.Windows;
#endif

namespace VideoGame
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Level level;
        KeyboardState currentKb, previousKb;
        InputState inputState = new InputState();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            level = new Level(Services);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            if (MediaPlayer.State == MediaState.Playing)
            {
#if WINDOWS_PHONE
                MessageBoxResult Choice;

                Choice = MessageBox.Show("Media is currently playing, do you want to stop it?",
                    "Stop Player", MessageBoxButton.OKCancel);

                if (Choice != MessageBoxResult.OK) return;
#endif
            }

            MediaPlayer.Play(Content.Load<Song>("Basse Dance"));
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            currentKb = Keyboard.GetState();
            inputState.Update();
            if (level.gameState.state==State.over)
            {
                if (inputState.IsMouseLeftButtonClick())
                {
                    level.gameState.state = State.menu;
                    level = new Level(Services);
                }
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) 
                this.Exit();
            base.Update(gameTime);
            level.Update(gameTime, inputState);
            previousKb = currentKb;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            level.Draw(gameTime, spriteBatch);
            base.Draw(gameTime);
        }
    }
}
