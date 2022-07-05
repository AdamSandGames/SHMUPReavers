using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SHMUP_Project
{
    public class scrapPickup : DrawableGameComponent
    {
        protected cState myState; // alive for available, dead for used.
        protected int scrapValue;
        protected Vector2 Position;
        protected Vector2 Velocity;
        protected Vector2 gravPull;
        protected Texture2D scrapTex;
        protected Rectangle drawRect;
        protected SpriteBatch spriteBatch;
        protected SpriteFont Font1;
        protected int cRadius;
        protected int collCircle;
        protected float radianRotation;
        protected Game1 game;

        protected shipEntity thePlayer;

        public scrapPickup(Game1 theGame, float ix, float iy, int value, Vector2 pvelocity)
            : base(theGame)
        {
            Position = new Vector2(ix, iy);

            Velocity = pvelocity;
            scrapValue = value;

            game = theGame;
        }
        public override void Initialize()
        {
            base.Initialize();
        }


        protected override void LoadContent()
        {
            myState = cState.Alive;

            spriteBatch = game.getSpriteBatch();

            scrapTex = game.getTextureScrap();
            thePlayer = game.getPlayerEntity();

            drawRect = game.positionToDrawRectangle(Position, scrapTex, 1, 1); // change to adj. sprite size

            cRadius = scrapTex.Width / 2;

            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            if (myState == cState.Dead) return;
            collisionCheck();
            // gravity();
            Position += Velocity * (float)game.getTimeStep();

        }

        public override void Draw(GameTime gameTime)
        {
            if (myState == cState.Dead) return;
            else
            {
                drawRect = game.positionToDrawRectangle(Position, scrapTex, 1, 1);
                spriteBatch.Draw(scrapTex, drawRect, null, Color.White, radianRotation, new Vector2(scrapTex.Width / 2, scrapTex.Height / 2), SpriteEffects.None, 1f);
            }

        }
        public void pickup()
        {
            myState = cState.Dead;
        }
        void collisionCheck()
        {
            // add a flag so the bullet doesn't hit whatever creates it while it's being created

            if (thePlayer.getState() != cState.Dead)
            {
                if ((thePlayer.getPosition() - Position).Length() < thePlayer.getCollisionRadius())
                {
                    if (!thePlayer.getHasGun())
                    {
                        thePlayer.setHasGun(true);
                    }
                    thePlayer.healShip(scrapValue);
                    game.addScore(scrapValue);
                    pickup();
                }
                if ((thePlayer.getPosition() - Position).Length() < thePlayer.getCollisionRadius() * 15)
                {
                    moveTowards(thePlayer.getPosition(), thePlayer.getVelocity());
                }
            }
        }
        void moveTowards(Vector2 posTar, Vector2 velTar)
        {
            Vector2 correction = velTar - Velocity;
            Vector2 accel = (posTar - Position) * 5 + correction;
            accel.Normalize();
            Velocity += accel * 50 * (float)game.getTimeStep();
            
        }
        void gravity()
        {
            gravPull = game.doGravity(Position);
            Velocity += gravPull;
        }
    }
}
