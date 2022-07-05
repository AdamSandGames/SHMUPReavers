using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SHMUP_Project
{
    public class attractor : DrawableGameComponent
    {
        protected Game1 game;
        protected Vector2 Position;
        protected Vector2 Velocity;
        protected Vector2 gravPull;
        protected float Strength;

        SpriteBatch spriteBatch;
        Texture2D scrapTex;
        protected Texture2D attractorTex;
        protected Rectangle drawRect;
        protected float eTime;
        public attractor(Game1 theGame, Vector2 startPos, Vector2 startVel, float str)
            : base(theGame)
        {
            game = theGame;

            Position = startPos;
            Velocity = startVel;
            Strength = str;
        }

        public override void Initialize()
        {
            gravPull = Vector2.Zero;
            spriteBatch = game.getSpriteBatch();
            scrapTex = game.getTextureAttractor();
            base.Initialize();
        }


        protected override void LoadContent()
        {
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            eTime = (float)game.getTimeStep();
            gravity();
            Position += Velocity * eTime;

        }

        public override void Draw(GameTime gameTime)
        {
            drawRect = game.positionToDrawRectangle(Position, scrapTex, 2, 2);
            spriteBatch.Draw(scrapTex, drawRect, null, Color.White, 0, new Vector2(scrapTex.Width / 2, scrapTex.Height / 2), SpriteEffects.None, 1f);
        }

        void gravity()
        {
            gravPull = game.doGravity(Position);
            Velocity += gravPull;
        }

        public Vector2 getPosition()
        {
            return Position;
        }
        public Vector2 getVelocity()
        {
            return Velocity;
        }
        public float getStrength()
        {
            return Strength;
        }
    }
}
