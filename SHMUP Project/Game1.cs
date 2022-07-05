using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;//to access Debug

namespace SHMUP_Project
{
    public enum cState { Dead, Alive };
    public enum GameState { Startup, Playing, Gameover}
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;
        protected SpriteBatch starsBatch;
        protected SpriteFont Font1;
        protected SpriteFont Font2;

        protected Texture2D checkerWhite;

        protected Texture2D starfieldTex;
        protected Rectangle starrect;
        protected Texture2D mouseTex;
        protected Texture2D playerTex;
        protected Texture2D scrapTex;
        protected Texture2D bulletTex;
        protected Texture2D enemy1Tex;
        protected Texture2D attractorTex;
        protected Color bgColor;
        protected float screenWidth, screenHeight;
        protected Random rand;
        protected Rectangle screenRect;

        // object lists
        protected List<bullet> projectiles;
        protected List<scrapPickup> scraps;
        protected List<attractor> attractors;

        protected List<shipEntity> entities;
        protected shipEntity playerEntity;
        protected shipEntity cameraFocus;
        protected int enemyCount;
        protected int enemyDead;

        protected GameState gameState;
        protected int score;
        protected float GravitationalConstant;
        // add world coordinate system and panning mechanism that follows the player

        protected double camScale;
        protected double xTrans, yTrans;
        protected double xOffset, yOffset;
        protected double aspectRatio;
        protected double xWorldMin, xWorldMax, yWorldMin, yWorldMax;

        protected Song bgmusic;
        protected SoundEffect gunFire;
        protected string gameOverMessage;
        protected float gameVolume;


        protected MouseState curMouseState;
        protected MouseState lastMouseState;
        protected KeyboardState curKeyboardState;
        protected KeyboardState lastKeyboardState;
        protected long elapsedTime;
        protected static double eTime;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 1200;
            graphics.PreferredBackBufferWidth = 1800;
            IsFixedTimeStep = true;
            
            IsMouseVisible = false;

            camScale = 100;
            screenRect = new Rectangle();

        }

        protected override void Initialize()
        {
            gameState = GameState.Startup;
            GravitationalConstant = 1;
            gameVolume = 0.25f;
            // create new random number generator
            rand = new Random((int)DateTime.Now.Ticks);

            //Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            starsBatch = new SpriteBatch(GraphicsDevice);
            screenWidth = (float)(Window.ClientBounds.Width);
            screenHeight = (float)(Window.ClientBounds.Height);
            aspectRatio = screenHeight / screenWidth;
            xTrans = 0;
            yTrans = 0;
            xOffset = (screenWidth / 2) / camScale;
            yOffset = (screenHeight / 2) / camScale;
            cameraUpdate();

            bgColor = Color.Transparent;  // set to your background color

            // Load text font
            Font1 = Content.Load<SpriteFont>("thisFont");
            Font2 = Content.Load<SpriteFont>("BigFont");

            // Load the image
            mouseTex = Content.Load<Texture2D>("reavermouse");
            attractorTex = Content.Load<Texture2D>("BlackHole128");
            playerTex = Content.Load<Texture2D>("gunnerplaceholder64");
            scrapTex = Content.Load<Texture2D>("scrapplaceholder16");
            bulletTex = Content.Load<Texture2D>("bullet1");
            enemy1Tex = Content.Load<Texture2D>("gunnerplaceholder64");
            checkerWhite = Content.Load<Texture2D>("whiteBlankSquare32");
            starfieldTex = Content.Load<Texture2D>("starMapCustom");
            starrect = starfieldTex.Bounds;

            bgmusic = Content.Load<Song>("RainyDevil");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = gameVolume;
            gunFire = Content.Load<SoundEffect>("Gun+Silencer");

            /*Debug code for illustration purposes.*/
            {
                Debug.WriteLine("Texture Width " + playerTex.Width);
                Debug.WriteLine("Texture Height " + playerTex.Height);
                Debug.WriteLine("Window Width " + Window.ClientBounds.Width);
                Debug.WriteLine("Window Height " + Window.ClientBounds.Height);
                Debug.WriteLine("IsFixedTimeStep " + IsFixedTimeStep);
                Debug.WriteLine("TargetElapsedTime " + TargetElapsedTime);
            }


            projectiles = new List<bullet> { };
            entities = new List<shipEntity> { };
            attractors = new List<attractor> { };

            gameOverMessage = "Default GameOver";

            enemyCount = 0;
            enemyDead = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
 
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            timeStepper(gameTime);
            switch (gameState)
            {
                case GameState.Startup:
                    GetInput(gameTime);
                    break;

                case GameState.Playing:
                    if (playerEntity.getState() == cState.Dead)
                    {
                        gameOver();
                        break;
                    }
                          // updates eTime with elapsed time in seconds
                    //cameraUpdate();             // moves camera to follow player
                    GetInput(gameTime);         // get user input

                    // Collision detection loop
                    // collisionHandler();
                    spawnEnemies();
                    break;

                case GameState.Gameover:
                    GetInput(gameTime);
                    if (curKeyboardState.IsKeyDown(Keys.Enter) && !lastKeyboardState.IsKeyDown(Keys.Enter))
                    {
                        score = 0;
                        clearBoard();
                        startGame();
                    }
                    break;
            }


            base.Update(gameTime);
        }

        protected void GetInput(GameTime gameTime)
        {
            lastMouseState = curMouseState;
            lastKeyboardState = curKeyboardState;

            curMouseState = Mouse.GetState();
            curKeyboardState = Keyboard.GetState();

            if (curKeyboardState.IsKeyDown(Keys.Escape))
            { // id escape key pressed, then exit
                this.Exit();
            }
            if (curKeyboardState.IsKeyDown(Keys.Enter) && gameState == GameState.Startup)
            {
                startGame();
            }
            if (curKeyboardState.IsKeyDown(Keys.OemOpenBrackets) && !lastKeyboardState.IsKeyDown(Keys.OemOpenBrackets))
            {
                camScale *= 0.8;

                xOffset = (screenWidth / 2) / camScale;
                yOffset = (screenHeight / 2) / camScale;
            }
            if (curKeyboardState.IsKeyDown(Keys.OemCloseBrackets) && !lastKeyboardState.IsKeyDown(Keys.OemCloseBrackets))
            {
                camScale *= 1.25;

                xOffset = (screenWidth / 2) / camScale;
                yOffset = (screenHeight / 2) / camScale;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(bgColor);
            
            Rectangle curMouseRect = new Rectangle(curMouseState.X - (int)(mouseTex.Width * 1), // mouseposition offset for a centered mouse cursor
                                                    curMouseState.Y - (int)(mouseTex.Height * 1),
                                                    mouseTex.Width * 2, mouseTex.Height * 2);

            drawStars();

            spriteBatch.Begin();


            // write a class for creating a background image/starmap on startup, work on code for navigating around a defined playspace

            base.Draw(gameTime);  // draw everything else in the game
            
            
            switch (gameState)
            {
                case GameState.Startup:
                    spriteBatch.DrawString(Font2, "Reaver's Revenge", new Vector2(screenWidth * 4 / 9, screenHeight * 4 / 10), Color.Red);
                    spriteBatch.DrawString(Font2, "Press Enter to Start", new Vector2(screenWidth * 4 / 9, screenHeight * 4 / 10 + 60), Color.Red);
                    spriteBatch.DrawString(Font2, "WASD to Accelerate\nLeft Mouse to shoot\nShift to break", new Vector2(screenWidth * 7 / 9, screenHeight * 5 / 10), Color.Red);
                    break;
                case GameState.Playing:
                    spriteBatch.DrawString(Font2, "Integrity: " + playerEntity.getHealth(), new Vector2(screenWidth * 4 / 9, screenHeight * 8 / 10), Color.Red);
                    spriteBatch.DrawString(Font2, "Score: " + score, new Vector2(screenWidth * 4 / 9, screenHeight * 8 / 10 + 30), Color.Red);
                    spriteBatch.DrawString(Font2, "Enemies Debug: " + enemyCount, new Vector2(screenWidth * 4 / 9, screenHeight * 8 / 10 + 60), Color.Red);
                    spriteBatch.DrawString(Font2, "Dead Debug: " + enemyDead, new Vector2(screenWidth * 4 / 9, screenHeight * 8 / 10 + 90), Color.Red);
                    // debugText();
                    break;
                case GameState.Gameover:
                    spriteBatch.DrawString(Font2, "Reaver's Revenge", new Vector2(screenWidth * 4 / 9, screenHeight * 4 / 10), Color.Red);
                    spriteBatch.DrawString(Font2, gameOverMessage, new Vector2(screenWidth * 4 / 9, screenHeight * 4 / 10 + 60), Color.Red);
                    break;
            }
            double fps = 1.0 / ((double)elapsedTime / (double)TimeSpan.TicksPerSecond);  // calculate and display frame rate
            spriteBatch.DrawString(Font1, "fps " + fps.ToString("f1"), new Vector2(10, 10), Color.Red);

            spriteBatch.Draw(mouseTex, curMouseRect, Color.White); // draw mouse
            spriteBatch.End();
        }

        void debugText()
        {
            spriteBatch.DrawString(Font1, "scale " + camScale.ToString("f1"), new Vector2(10, 30), Color.Red);
            spriteBatch.DrawString(Font1, "xMin " + xWorldMin.ToString("f1"), new Vector2(10, 50), Color.Red);
            spriteBatch.DrawString(Font1, "xMax " + xWorldMax.ToString("f1"), new Vector2(10, 70), Color.Red);
            spriteBatch.DrawString(Font1, "yMin " + yWorldMin.ToString("f1"), new Vector2(80, 50), Color.Red);
            spriteBatch.DrawString(Font1, "yMax " + yWorldMax.ToString("f1"), new Vector2(80, 70), Color.Red);
            spriteBatch.DrawString(Font1, "xposition " + playerEntity.getPosition().X.ToString("f1"), new Vector2(10, 90), Color.Red);
            spriteBatch.DrawString(Font1, "yposition " + playerEntity.getPosition().Y.ToString("f1"), new Vector2(10, 110), Color.Red);
            spriteBatch.DrawString(Font1, "xTrans " + xTrans.ToString("f1"), new Vector2(90, 90), Color.Red);
            spriteBatch.DrawString(Font1, "yTrans " + yTrans.ToString("f1"), new Vector2(90, 110), Color.Red);

            spriteBatch.DrawString(Font1, "mouse world " + screenToWorld(new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y)).ToString(), new Vector2(10, 140), Color.Red);
            spriteBatch.DrawString(Font1, "mouse screen " + Mouse.GetState().Position.ToString(), new Vector2(10, 200), Color.Red);

            spriteBatch.DrawString(Font1, "camera world " + cameraFocus.getPosition(), new Vector2(10, 240), Color.Red);
            spriteBatch.DrawString(Font1, "camera screen " + worldToScreen(cameraFocus.getPosition()), new Vector2(10, 270), Color.Red);

            spriteBatch.DrawString(Font1, "player world " + playerEntity.getPosition(), new Vector2(10, 340), Color.Red);
            spriteBatch.DrawString(Font1, "player screen " + worldToScreen(playerEntity.getPosition()), new Vector2(10, 370), Color.Red);

        }

        public void timeStepper(GameTime gameTime)
        {
            elapsedTime = gameTime.ElapsedGameTime.Ticks;
            eTime = (double)elapsedTime / (double)TimeSpan.TicksPerSecond;
            if (curKeyboardState.IsKeyDown(Keys.D1)) // in game stops everything relying on eTime
            {
                eTime = 0;
            }
            if (curKeyboardState.IsKeyDown(Keys.D2)) // in game slows down everything relying on eTime
            {
                eTime *= 0.2;
            }
        }

        void drawStars()
        {
            starsBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null);
            Rectangle testdraw = new Rectangle();
            if (cameraFocus != null)
            {
                testdraw = new Rectangle((int)(cameraFocus.getPosition().X * 10), (int)(-cameraFocus.getPosition().Y * 10), (int)screenWidth, (int)screenWidth);
            }
            else
            {

                testdraw = new Rectangle((int)0, (int)0, (int)screenWidth, (int)screenWidth);
            }
            //Rectangle testdraw = new Rectangle((int)(Mouse.GetState().X), (int)(Mouse.GetState().Y), (int)screenWidth, (int)screenWidth);

            starsBatch.Draw(
                starfieldTex,
                Vector2.Zero, // position
                testdraw, //positionToDrawRectangle(Vector2.Zero, starfieldTex, 1, 1), // source rectangle
                Color.White, // color
                0, // rotation
                Vector2.Zero, // origin
                2, // scale
                SpriteEffects.None,
                0 // layer
            );
            starsBatch.Draw(
                starfieldTex,
                Vector2.Zero, // position
                testdraw, //positionToDrawRectangle(Vector2.Zero, starfieldTex, 1, 1), // source rectangle
                Color.White, // color
                0, // rotation
                Vector2.Zero, // origin
                3, // scale
                SpriteEffects.None,
                0 // layer
            );
            starsBatch.Draw(
                starfieldTex,
                Vector2.Zero, // position
                testdraw, //positionToDrawRectangle(Vector2.Zero, starfieldTex, 1, 1), // source rectangle
                Color.White, // color
                0, // rotation
                Vector2.Zero, // origin
                1, // scale
                SpriteEffects.None,
                0 // layer
            );
            starsBatch.End();
        }
        void startGame()
        {
            gameState = GameState.Playing;
            xTrans = 0; // center of screen at game start is 0,0
            yTrans = 0;
            camScale = 100;
            if(MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(bgmusic);

            addPlayer(Vector2.Zero);

            for (int x = -15; x <= 15; x++)
            {
                for (int y = -15; y <= 15; y++)
                {
                    if(x%2 != 0 && y%2 != 0)
                        addAttractor(new Vector2(x * 100, y * 100), 30);
                }
            }
            //addAttractor(new Vector2(1, 0), 0);

            spawnHuskRandom();
        }
        void clearBoard()
        {
            for(int i = 0; i < Components.Count(); i++)
            { 
                Components.Remove(Components[i]);
            }
        }

        void spawnHuskRandom()
        {
            for(int i = 0; i < 100; i++)
            {
                int x = rand.Next(-500, 500);
                int y = rand.Next(-500, 500);
                Vector2 pos = new Vector2(x, y);
                addHusk(pos);
            }
        }
        void spawnEnemies()
        {
            Vector2 plPos = playerEntity.getPosition();
            if(enemyCount == enemyDead)
            {
                switch (playerEntity.getPlayerLevel())
                {
                    case 1: default:
                        for (int i = 0; i < 1; i++)
                        {
                            addSimpleEnemy(plPos + randomDirection() * rand.Next(12, 50));
                            enemyCount++;
                        }
                        break;
                    case 2:
                        for (int i = 0; i < 3; i++)
                        {
                            addSimpleEnemy(plPos + randomDirection() * rand.Next(12, 60));
                            enemyCount++;
                        }
                        break;
                    case 3:
                        for (int i = 0; i < 4; i++)
                        {
                            addSimpleEnemy(plPos + randomDirection() * rand.Next(12, 70));
                            enemyCount++;
                        }
                        break;
                    case 4:
                        for (int i = 0; i < 6; i++)
                        {
                            addSimpleEnemy(plPos + randomDirection() * rand.Next(12, 80));
                            enemyCount++;
                        }
                        break;
                    case 5:
                        for (int i = 0; i < 8; i++)
                        {
                            addSimpleEnemy(plPos + randomDirection() * rand.Next(12, 100));
                            enemyCount++;
                        }
                        break;
                    case 6:
                        for (int i = 0; i < 12; i++)
                        {
                            addSimpleEnemy(plPos + randomDirection() * rand.Next(12, 150));
                            enemyCount++;
                        }
                        break;
                }
            }
            // this.enemyCount = enemyCount - enemyDead;
        }
        public void incrEnemyDead()
        {
            enemyDead++;
        }

        Vector2 randomDirection()
        {
            Vector2 result = Vector2.Zero;
            while (result == Vector2.Zero)
            {
                result.X = rand.Next(-1, 1);
                result.Y = rand.Next(-1, 1);
            }
            result.Normalize();
            return result;
        }
        void gameOver()
        {
            gameOverMessage = "You were Killed!\nGame Over!";
            gameState = GameState.Gameover;
        }

        public void blackHoled()
        {
            gameOverMessage = "You Got Sucked into a Black Hole!\nGame Over!";
            gameState = GameState.Gameover;
        }
        public void winCondition()
        {
            if(playerEntity.getHealth() >= 500)
            {
                gameOverMessage = "You're Unkillable!\nGood Job! You Win!";
                gameState = GameState.Gameover;
            }
        }
        // *********************************************************************************************************** Camera Code
        public void cameraUpdate()
        {
            // pan camera to follow player
            if (playerEntity != null)
            {
                cameraFocus = playerEntity;
                xTrans = cameraFocus.getPosition().X;
                yTrans = cameraFocus.getPosition().Y;
            }

            // manage world/screen boundries
            xWorldMin = xTrans - screenWidth / camScale;
            xWorldMax = xTrans;
            yWorldMin = yTrans;
            yWorldMax = yTrans + screenHeight / camScale;
            screenRect.X = (int)xWorldMin;
            screenRect.Y = (int)yWorldMax;
            screenRect.Width = (int)(screenWidth / camScale);
            screenRect.Height = (int)(screenHeight / camScale);

        }
        public Rectangle positionToDrawRectangle(Vector2 position, Texture2D tex, float wscale, float hscale)
        {
            Rectangle result = new Rectangle();
            if (cameraFocus != null)
            {
                result.X = (int)((position.X - xTrans + xOffset) * camScale);
                result.Y = (int)screenHeight - (int)((position.Y - yTrans + yOffset) * camScale);
            }
            else
            {
                result.X = (int)((position.X - xTrans + xOffset) * camScale);
                result.Y = (int)((position.Y - yTrans + yOffset) * camScale);
            }
            result.Width = (int)(wscale * tex.Width * camScale / 100);
            result.Height = (int)(hscale * tex.Height * camScale / 100);

            return result;
        }
        public Vector2 worldToScreen(Vector2 position)
        {
            return new Vector2((float)((position.X - xTrans + xOffset) * camScale),
                screenHeight - (float)((position.Y - yTrans + yOffset) * camScale) );
        }
        public Vector2 screenToWorld(Vector2 screencoord)
        {
            return new Vector2((float)((screencoord.X / camScale) + xTrans - xOffset),
                (float)(((screenHeight - screencoord.Y) / camScale) + yTrans - yOffset) );
        }
        // ***********************************************************************************************************
        public Vector2 doGravity(Vector2 myPos)
        {
            Vector2 gravPull = Vector2.Zero;
            Vector2 dir;
            Vector2 stuck = Vector2.Zero;
            float dist;
            foreach(attractor bh in attractors)
            {
                dir = bh.getPosition() - myPos;
                dist = dir.Length();
                if (dist > 0.1)
                {
                    dir.Normalize();
                    gravPull += dir * (GravitationalConstant * bh.getStrength() / (dist * dist));
                }
                else
                    return stuck;
            }

            return gravPull;
        }
        void addPlayer(Vector2 position)
        {
            playerEntity = new shipEntity(this, position, cState.Alive, entityType.player, 8);
            Components.Add(playerEntity);
            entities.Add(playerEntity);
            cameraFocus = playerEntity;
        }
        void addSimpleEnemy(Vector2 position)
        {
            shipEntity e = new shipEntity(this, position, cState.Alive, entityType.enemy, 5, true);
            Components.Add(e);
            entities.Add(e);
        }
        void addHusk(Vector2 position)
        {
            shipEntity h = new shipEntity(this, position, cState.Alive, entityType.husk, 0, false);
            Components.Add(h);
            entities.Add(h);
        }
        void addAttractor(Vector2 position, float strength)
        {
            attractor a = new attractor(this, position, Vector2.Zero, strength);
            Components.Add(a);
            attractors.Add(a);
        }

        // getters and setters
        public KeyboardState getCurrentKeys()
        {
            return curKeyboardState;
        }
        public KeyboardState getLastKeys()
        {
            return lastKeyboardState;
        }
        public MouseState getCurrentMouse()
        {
            return curMouseState;
        }
        public MouseState getLastMouse()
        {
            return lastMouseState;
        }
        public Rectangle getScreenRect()
        {
            return screenRect;
        }
        public SpriteFont getFont()
        {
            return Font1;
        }
        public double getTimeStep()
        {
            return eTime;
        }
        public List<bullet> getProjectileList()
        {
            return projectiles;
        }
        public List<shipEntity> getEntityList()
        {
            return entities;
        }
        public List<attractor> getAttractorsList()
        {
            return attractors;
        }
        public shipEntity getPlayerEntity()
        {
            return playerEntity;
        }
        public float getScreenWidth()
        {
            return screenWidth;
        }
        public float getScreenHeight()
        {
            return screenHeight;
        }
        public Random getRand()
        {
            return rand;
        }
        public SpriteBatch getSpriteBatch()
        {
            return spriteBatch;
        }
        public Texture2D getTexturePlayer()
        {
            return playerTex;
        }
        public Texture2D getTextureScrap()
        {
            return scrapTex;
        }
        public Texture2D getTextureAttractor()
        {
            return attractorTex;
        }
        public Texture2D getTextureBullet()
        {
            return bulletTex;
        }
        public Texture2D getTextureEnemy()
        {
            return enemy1Tex;
        }
        public SoundEffect getGunFire()
        {
            return gunFire;
        }
        public float getGameVolume()
        {
            return gameVolume;
        }
        public void addScore(int addition)
        {
            score += addition;
        }
        public double getScale()
        {
            return camScale;
        }
        public void setScale(double n)
        {
            camScale = n;
        }
        public double getXtrans()
        {
            return xTrans;
        }
        public void setXtrans(double n)
        {
            xTrans = n;
        }
        public double getYtrans()
        {
            return yTrans;
        }
        public void setYtrans(double n)
        {
            yTrans = n;
        }
        public double getAspect()
        {
            return aspectRatio;
        }
        public void setAspect(double n)
        {
            aspectRatio = n;
        }
        public double getXworldMin()
        {
            return xWorldMin;
        }
        public void setXworldMinMax(double min, double max)
        {
            xWorldMin = min;
            xWorldMax = max;
        }
        public double getXworldMax()
        {
            return xWorldMax;
        }
        public double getYworldMin()
        {
            return yWorldMin;
        }
        public void setYworldMinMax(double min, double max)
        {
            yWorldMin = min;
            yWorldMax = max;
        }
        public double getYworldMax()
        {
            return yWorldMax;
        }

        
    }//End class
}
