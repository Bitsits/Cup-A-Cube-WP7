using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BitSits_Framework;

namespace VideoGame
{
    enum SugarAndBeanState
    {
        rigid, dropping,
    }

    class Level
    {
        public ContentManager Content;
        public static Random random = new Random();
        Vector2 sugarCubePosition = new Vector2(-200), startSugarCubePosition, splatPosition = new Vector2(-300);
        Vector2[] beansPosition = new Vector2[4];
        float[] rotation = new float[4];
        float maxTime = 4.0f, currentTime = 0.0f, velocity = 500.0f;
        Texture2D bean, cube, menuScreen, background, cup, missCube, gameOver;
        KeyboardState previousKb;
        int score = 0, sugarCubeCount = 0, miss = 0, previousValue = -1, value = -1,
            totalMiss = 5, levelUps = 1, previousScore = -1, wrongBeanPosition, direction = -1, beanCollected = 0;
        SpriteFont font;
        public GameState gameState;
        public Rectangle viewport = new Rectangle(0, 0, 800, 600);
        SugarAndBeanState sugarState = SugarAndBeanState.rigid;
        SugarAndBeanState beanState = SugarAndBeanState.rigid;
        bool splat = false;
        Texture2D[] splatTextures = new Texture2D[3];
        int splatValue = 0;
        SoundEffect drop;
        float waitTime = 0;

        public Level(IServiceProvider Services)
        {
            Content = new ContentManager(Services, "Content");
            gameState = new GameState();
            LoadContent();
        }

        public void LoadContent()
        {
            for (int i = 0; i < 4; i++) rotation[i] = (float)random.Next(360) / 180 * (float)Math.PI;
            bean = Content.Load<Texture2D>("bean");
            cube = Content.Load<Texture2D>("cube");
            font = Content.Load<SpriteFont>("Ravie30");
            menuScreen = Content.Load<Texture2D>("gameMenu");
            background = Content.Load<Texture2D>("gameBackground");
            cup = Content.Load<Texture2D>("cup");
            missCube = Content.Load<Texture2D>("missCube");
            gameOver = Content.Load<Texture2D>("gameOver");
            drop=Content.Load<SoundEffect>("dropSound");
            for (int i = 0; i < 3; i++)
            {
                splatTextures[i] = Content.Load<Texture2D>("splat" + i);
            }
        }

        public void Update(GameTime gameTime, InputState inputSate)
        {
            MouseState currentMouse = Mouse.GetState();
            if (gameState.state == State.over) return;
            if (gameState.state == State.menu)
            {
                KeyboardState currentKb=Keyboard.GetState();
                waitTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (inputSate.IsMouseLeftButtonClick() && waitTime > .1f)
                {
                    gameState.state = State.play; waitTime = 0;
                }
                previousKb=currentKb;
            }

            if (gameState.state == State.play)
            {
                if (miss == totalMiss)
                {
                    currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (currentTime > 3.0f)
                    {
                        gameState.state = State.over;
                    }
                }
                if (sugarState == SugarAndBeanState.rigid && beanState == SugarAndBeanState.rigid)
                {
                    KeyboardState currentKb = Keyboard.GetState();
                    beansPosition[(int)Keys.Up - (int)Keys.Left] = new Vector2(240, 180);
                    beansPosition[(int)Keys.Left - (int)Keys.Left] = new Vector2(360, 300);
                    beansPosition[(int)Keys.Right - (int)Keys.Left] = new Vector2(120, 300);
                    beansPosition[(int)Keys.Down - (int)Keys.Left] = new Vector2(240, 420);

                    currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (currentTime >= maxTime && miss<totalMiss)
                    {
                        while (value == previousValue)
                        {
                            value = random.Next(4);
                        }
                        startSugarCubePosition = sugarCubePosition = beansPosition[value];
                        rotation[value] = (float)random.Next(360) / 180 * (float)Math.PI;
                        currentTime = 0.0f;
                        if (previousScore == score) { miss += 1; }
                        previousScore = score;
                        previousValue = value;
                    }
                    if (currentTime <= maxTime && miss < totalMiss && inputSate.IsMouseLeftButtonClick())
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (sugarCubePosition == beansPosition[i] && 
                                Vector2.Distance(beansPosition[i], new Vector2(inputSate.CurrentMouseState.X,
                                    inputSate.CurrentMouseState.Y)) < 75)
                            {
                                score += levelUps; sugarCubeCount += 1; currentTime = maxTime;
                                sugarState = SugarAndBeanState.dropping; break;
                            }
                            if (sugarCubePosition != beansPosition[i] && 
                                Vector2.Distance(beansPosition[i], new Vector2(inputSate.CurrentMouseState.X,
                                    inputSate.CurrentMouseState.Y)) < 75)
                            {
                                wrongBeanPosition = i; currentTime = maxTime; beanCollected += 1;
                                beanState = SugarAndBeanState.dropping; break;
                            }
                        }
                    }
                    if (sugarCubeCount >= 5)
                    {
                        maxTime = 0.9f * maxTime;
                        levelUps += 1;
                        sugarCubeCount = 0;
                        maxTime = MathHelper.Clamp(maxTime, 1.0f, 4.0f);
                    }
                    previousKb = currentKb;
                }

                int cupTop = 670, screenHeight = 800;

                if (beanState == SugarAndBeanState.dropping)
                {
                    if (beansPosition[wrongBeanPosition].Y <= screenHeight)
                        beansPosition[wrongBeanPosition].Y += (float)gameTime.ElapsedGameTime.TotalSeconds * velocity;
                }
                else if(sugarState == SugarAndBeanState.dropping)
                {
                    if (sugarCubePosition.Y <= screenHeight)
                        sugarCubePosition.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * velocity;
                }
                if ((sugarCubePosition.Y >= cupTop || beansPosition[wrongBeanPosition].Y >= cupTop) && splat == false)
                {
                    splat = true;
                    splatValue = random.Next(3);
                    splatPosition = new Vector2(
                        (sugarCubePosition.Y >= cupTop ? sugarCubePosition.X : beansPosition[wrongBeanPosition].X) - 50,
                        cupTop + splatTextures[splatValue].Height);
                    drop.Play();
                }
                
                if (splat == true)
                {
                    splatPosition.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * velocity * direction;
                    if (splatPosition.Y <= (cupTop + 10 - splatTextures[splatValue].Height)) direction = 1;
                    if (splatPosition.Y > (cupTop + 10 + splatTextures[splatValue].Height))
                    {
                        sugarState = SugarAndBeanState.rigid;
                        beanState = SugarAndBeanState.rigid;
                        direction = -1;
                        splat = false;
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            if (gameState.state == State.menu)
            {
                spriteBatch.Draw(menuScreen, Vector2.Zero, Color.White);
            }
            if (gameState.state == State.play)
            {
                Vector2 origin = new Vector2(bean.Width / 2, bean.Height / 2);

                for (int i = 0; i < 4; i++)
                {
                    if (startSugarCubePosition != beansPosition[i])
                    {
                        spriteBatch.Draw(bean, beansPosition[i], null, Color.White, rotation[i], origin, 1,
                       SpriteEffects.None, 0.0f);
                    }
                }
                if (value != -1)
                    spriteBatch.Draw(cube, sugarCubePosition, null, Color.White, rotation[value],
                            new Vector2(cube.Width, cube.Height) / 2, 1, SpriteEffects.None, 1.0f);

                spriteBatch.Draw(splatTextures[splatValue], splatPosition, Color.White);
                spriteBatch.Draw(cup, Vector2.Zero, Color.White);

                spriteBatch.DrawString(font, levelUps.ToString() + "x", new Vector2(300, 725), Color.White);
                spriteBatch.Draw(cube, new Vector2(375, 725), null, Color.White, 0, Vector2.Zero, 0.35f,
                    SpriteEffects.None, 1.0f);

                spriteBatch.Draw(cube, new Vector2(10, 20), null, Color.White, 0.0f,
                    Vector2.Zero, 0.5f, SpriteEffects.None, 1.0f);
                spriteBatch.DrawString(font, " = " + score.ToString(), new Vector2(60, 20), Color.White);

                spriteBatch.Draw(bean, new Vector2(20, 100), null, Color.White, 0.5f,
                    Vector2.Zero, 0.5f, SpriteEffects.None, 1.0f);
                spriteBatch.DrawString(font, " = " + beanCollected.ToString(), new Vector2(60, 110), Color.White);

                for (int i = 0; i < totalMiss; i++)
                    spriteBatch.Draw(missCube, new Vector2(230 + i * 45, 20), Color.Black * 0.5f);
                for (int i = 0; i < miss; i++) spriteBatch.Draw(missCube, new Vector2(230 + i * 45, 20), Color.White);
            }
            if (gameState.state == State.over)
            {
                string text = "tasteless";
                spriteBatch.Draw(gameOver, Vector2.Zero, Color.White);
                if (beanCollected > score) { text = "bitter"; }
                else if (score > 0 && score <= 150) { text = "sweet"; }
                else if (score > 150 && score <= 300) { text = "sweeter"; }
                else if (score > 300) { text = "sweetest"; }
                spriteBatch.DrawString(font, text, new Vector2(160, 300),Color.Black,0.0f,new Vector2(text.Length/2,text.Length/2),
                    1.2f,SpriteEffects.None,1.0f);
                spriteBatch.DrawString(font, beanCollected.ToString(), new Vector2(300, 450), Color.White);
                spriteBatch.DrawString(font, score.ToString(), new Vector2(300, 580), Color.White);                           
            }
            spriteBatch.End();
        }
    }
}