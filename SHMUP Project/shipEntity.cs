using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace SHMUP_Project
{
    public enum entityType { player, enemy, ally, husk };
    public class shipEntity : DrawableGameComponent
    {
        // protected bool isAlive;
        protected cState myState;                     // state
        protected entityType myType;
        protected int scrapValue;

        protected List<shipEntity> entities;
        protected List<attractor> attractors;
        protected List<bullet> projectiles;

        protected Texture2D myTexture;
        protected Rectangle drawRect;
        protected SpriteBatch spriteBatch;
        protected SpriteFont Font1;

        protected Vector2 Position;
        protected Vector2 Direction;            // Movement Direction Component
        protected Vector2 gravPull;
        protected Vector2 Velocity;
        protected Vector2 facing;               // direction player is facing for shooting and the like
        protected float radianRotation;         // radians for drawing the sprite facing the right direction

        protected int healthPoints;

        protected float baseSpeed;
        protected float speed;
        protected int spriteEdge;       // offset from center of sprite to approximate edges
        protected int cRadius;
        protected float collCircle;
        protected float gunCycle;
        protected float fireRate;
        protected int gunDamage;
        protected bool hasGun;
        protected int scrapval;
        protected int playerLevel;

        protected SoundEffect gunFire;

        protected Game1 game;
        protected Random rand;
        protected float screenWidth;
        protected float screenHeight;
        protected float eTime;
        MouseState mouseState;

        public shipEntity(Game1 theGame, Vector2 startPos, cState state, entityType type, int shipDamage) // default constructor
            : base(theGame)
        {
            Position = startPos;

            Direction = new Vector2();

            Velocity = Vector2.Zero;

            myState = state;
            myType = type;
            hasGun = true;

            gunDamage = shipDamage;

            game = theGame;
        }
        public shipEntity(Game1 theGame, Vector2 startPos, cState state, entityType type, int shipDamage, bool gun) // constructor but with A GUN
            : base(theGame)
        {
            Position = startPos;

            Direction = new Vector2();

            Velocity = Vector2.Zero;

            myState = state;
            myType = type;
            hasGun = gun;

            gunDamage = shipDamage;

            game = theGame;
        }

        public override void Initialize()
        {
            baseSpeed = 5f;
            speed = baseSpeed; // set to your speed
            gunCycle = 0;
            if (myType == entityType.player)
            {
                fireRate = 0.2f;
                playerLevel = 1;
            }
            else
                fireRate = 1f;
            myState = cState.Alive;
            gravPull = Vector2.Zero;

            healthPoints = 20;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            rand = game.getRand();
            
            entities = game.getEntityList();
            attractors = game.getAttractorsList();
            projectiles = game.getProjectileList();

            myTexture = game.getTexturePlayer();
            gunFire = game.getGunFire();

            spriteBatch = game.getSpriteBatch();
            screenWidth = game.getScreenWidth();
            screenHeight = game.getScreenHeight();

            drawRect = game.positionToDrawRectangle(Position, myTexture, 1, 1); // change to adj. sprite size

            cRadius = myTexture.Width / 2;

            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            if (myState == cState.Dead) return;
            if (healthPoints <= 0)
            {
                kill(false);
                return;
            }
            eTime = (float)game.getTimeStep();
            gunCycle += eTime;
            entities = game.getEntityList();

            switch (myType)
            {
                case entityType.player:
                    integrityBuffs();
                    lookAtMouse();
                    getInput();
                    break;

                case entityType.enemy:
                    tacklePlayer();
                    fireGun();
                    break;

                case entityType.ally:

                    break;
                case entityType.husk:

                    break;
            }
            gravity();
            Position += Velocity * eTime;
            game.cameraUpdate();

            collCircle = (float)(cRadius / game.getScale());
            collisionCheck();

        }

        public override void Draw(GameTime gameTime)
        {
            if (myState == cState.Dead) return;
            if (myTexture == null)
            {
                myTexture = game.getTexturePlayer();
            }
            else
            {
                drawRect = game.positionToDrawRectangle(Position, myTexture, 1, 1);
                spriteBatch.Draw(myTexture, drawRect, null, Color.White, radianRotation, new Vector2(myTexture.Width / 2, myTexture.Height / 2), SpriteEffects.None, 1f);
            }
        }

        protected void getInput()
        {
            mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            Vector2 inputVel = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W))                    // accellerators
            {
                inputVel.Y += 1;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                inputVel.Y -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                inputVel.X -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                inputVel.X += 1;
            }
            if (keyboardState.IsKeyDown(Keys.LeftShift))                    // brakes
            {
                Velocity *= 0.95f;
            }
            if (mouseState.LeftButton == ButtonState.Pressed)       // gun
            {
                fireGun();
            }
            if (inputVel.Length() > 0)
            {
                inputVel.Normalize();
                inputVel *= speed * eTime;
                Velocity += inputVel;
            }
        }

        void integrityBuffs()
        {
            if (healthPoints < 30)
            {
                gunDamage = 8;
                speed = baseSpeed;
                playerLevel = 1;
            }
            else if (healthPoints < 60)
            {
                gunDamage = 10;
                speed = baseSpeed + 1;
                playerLevel = 2;
            }
            else if (healthPoints < 100)
            {
                gunDamage = 12;
                speed = baseSpeed + 2;
                playerLevel = 3;
            }
            else if (healthPoints < 150)
            {
                gunDamage = 14;
                speed = baseSpeed + 3;
                playerLevel = 4;
            }
            else if (healthPoints < 210)
            {
                gunDamage = 18;
                speed = baseSpeed + 4;
                playerLevel = 5;
            }
            else if (healthPoints < 300)
            {
                gunDamage = 25;
                speed = baseSpeed + 5;
                playerLevel = 6;
            }
        }
        void fireGun()
        {
            if (hasGun && gunCycle > fireRate)
            {
                bullet newBullet = new bullet(game, this, Position + Velocity * eTime, facing, Velocity, gunDamage);
                game.Components.Add(newBullet);
                projectiles.Add(newBullet);
                gunCycle = 0;
                if(myType == entityType.player)
                {
                    gunFire.Play(game.getGameVolume(), 0.0f, 0.0f);
                }
            }
        }
        void collisionCheck()
        {
            foreach(shipEntity ship in entities)
            {
                if (ship != this && ship.getState() == cState.Alive)
                {
                    if ((ship.getPosition() - Position).Length() < ship.getCollisionRadius() + collCircle)
                    {
                        //ship.damageShip(1);
                        damageShip(1);
                        Vector2 bump = (ship.getPosition() - Position);
                        bump.Normalize();
                        Velocity += bump;
                        // float a = Velocity.Length() / ship.getVelocity().Length();
                        // Velocity = a * Velocity + 1 / a * ship.getVelocity();
                    }
                }
            }
            foreach (attractor bh in attractors)
            {
                if ((bh.getPosition() - Position).Length() < collCircle * 3)
                {
                    if(myType == entityType.player)
                        game.blackHoled();
                    kill(true);
                    Velocity = Vector2.Zero;
                }
            }
        }

        public Vector2 getVelocity()
        {
            return Velocity;
        }
        public void damageShip(int damage)
        {
            healthPoints -= damage;
        }

        public void healShip(int damage)
        {
            healthPoints += damage;
        }
        public cState getState()
        {
            return myState;
        }
        public int getHealth()
        {
            return healthPoints;
        }
        public entityType getShipType()
        {
            return myType;
        }
        public bool getHasGun()
        {
            return hasGun;
        }
        public void setHasGun(bool gun)
        {
            hasGun = gun;
        }

        void lookAtMouse()
        {
            Vector2 mousepos = game.screenToWorld( new Vector2( Mouse.GetState().X, Mouse.GetState().Y) );
            lookAtTarget(mousepos);
        }

        public Vector2 getPosition()
        {
            return Position;
        }
        public float getCollisionRadius()
        {
            return collCircle;
        }

        public void kill(bool blackhole)
        {
            switch (myType)
            {
                case entityType.player:
                    break;

                case entityType.enemy: // drop scrap, add score, and die
                    if (blackhole)
                    {
                        game.incrEnemyDead();
                    }
                    else
                    {
                        scrapval = 5;
                        scrapPickup s = new scrapPickup(game, Position.X, Position.Y, scrapval, Velocity);
                        game.Components.Add(s);
                        game.addScore(10);
                        game.incrEnemyDead();
                    }
                    break;

                case entityType.ally:
                    break;

                case entityType.husk:
                    if (!blackhole)
                    {
                        scrapval = 5;
                        scrapPickup t = new scrapPickup(game, Position.X, Position.Y, scrapval, Velocity);
                        game.Components.Add(t);
                        game.addScore(5);
                    }
                    break;
            }
            myState = cState.Dead;
        }

        void gravity()
        {
            gravPull = game.doGravity(Position);
            Velocity += gravPull;
        }


        // enemy functions
        void tacklePlayer()
        {
            shipEntity p = game.getPlayerEntity();
            if (p.getState() != cState.Dead)
            {
                moveTowards(p.getPosition(), p.getVelocity());
                lookAtTarget(p.getPosition());
            }
        }
        void moveTowards(Vector2 posTar, Vector2 velTar)
        {
            Vector2 correction = velTar - Velocity;
            Vector2 accel = posTar - Position + correction;
            accel.Normalize();
            Velocity += accel * speed * eTime;
        }
        void lookAtTarget(Vector2 targetPos)
        {
            facing = targetPos - Position;
            facing.Normalize();
            radianRotation = (float)Math.Atan2(-(double)facing.Y, (double)facing.X) + MathHelper.PiOver2;
        }

        public int getPlayerLevel()
        {
            return playerLevel;
        }

        public void recieveBulletMomentum(Vector2 bVel)
        {
            Velocity += bVel * 0.1f;
        }

    }
}
