using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SHMUP_Project
{
    public class bullet : DrawableGameComponent
    {
        protected List<shipEntity> entities;
        protected List<bullet> projectiles;

        protected Vector2 Position;
        protected Vector2 facing;
        protected Vector2 Velocity;
        protected Vector2 ParentMotion;
        protected Vector2 gravPull;
        protected Game1 game;
        protected shipEntity parent;

        // maybe use an enum for bullet types?
        protected Texture2D bulletTex;
        protected SpriteBatch spriteBatch;
        protected Rectangle drawRect;
        protected float radianRotation;
        protected float lengthscale;
        protected float collCircle;

        protected float speed;
        protected int damage;
        protected bool barrelfree;

        protected float eTime;
        protected float lifeTime;
        protected float expireTime;
        protected bool active;


        public bullet(Game1 theGame, shipEntity _parent, Vector2 _startPos, Vector2 _parentFacing, Vector2 _parentVelocity, int dam)
            : base(theGame)
        {
            parent = _parent;

            Position = _startPos;

            facing = _parentFacing;

            speed = 10f;
            Velocity = _parentVelocity + _parentFacing * speed;
            barrelfree = false;

            damage = dam;

            game = theGame;
        }
        public override void Initialize()
        {
            entities = game.getEntityList();
            projectiles = game.getProjectileList();
            gravPull = Vector2.Zero;
            active = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            expireTime = 300;
            spriteBatch = game.getSpriteBatch();
            bulletTex = game.getTextureBullet();

            lengthscale = 1;
            float a = Velocity.Length();
            drawRect = game.positionToDrawRectangle(Position, bulletTex, 2, 2);
            //drawRect.Width = (int)(bulletTex.Width * 2 / lengthscale);
            //drawRect.Height = (int)(bulletTex.Height * 2 * lengthscale); // change to adj. sprite size
            radianRotation = (float)Math.Atan2(-(double)facing.Y, (double)facing.X) + MathHelper.PiOver2;

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (lifeTime > expireTime || !active) return;
            eTime = (float)game.getTimeStep();
            lifeTime += eTime;
            gravity();
            Position += ( Velocity ) * eTime;
            collisionCheck();
        }

        public override void Draw(GameTime gameTime)
        {
            if (lifeTime > expireTime || !active ) return;
            drawRect = game.positionToDrawRectangle(Position, bulletTex, 1, 4);
            //drawRect.Width = (int)(bulletTex.Width * 2 / lengthscale);
            //drawRect.Height = (int)(bulletTex.Height * 2 * lengthscale);
            spriteBatch.Draw(bulletTex, drawRect, null, Color.White, radianRotation, new Vector2(bulletTex.Width / 2, bulletTex.Height / 2), SpriteEffects.None, 1f);
            
            // String vstring = "X: " + Velocity.X.ToString("0.00") + "Y: " + Velocity.Y.ToString("0.00");
            // spriteBatch.DrawString(game.getFont(), vstring, Position + new Vector2(20, 0), Color.White);

        }
        void gravity()
        {
            gravPull = game.doGravity(Position);
            Velocity += gravPull;
        }
        void collisionCheck()
        {
            if (barrelfree && lifeTime < expireTime)
            {
                foreach (shipEntity ship in entities)
                {
                    if (ship.getState() != cState.Dead && active)
                    {
                        if ((ship.getPosition() - Position).Length() < ship.getCollisionRadius())
                        {
                            ship.damageShip(damage);
                            ship.recieveBulletMomentum(Velocity);
                            lifeTime = expireTime;
                        }
                    }
                }
                foreach (bullet bul in projectiles)
                {
                    if(bul.getActive() && bul != this)
                    {
                        if(drawRect.Intersects(bul.getRect()))
                        {
                            lifeTime = expireTime;
                            setActive(false);
                            bul.setActive(false);
                        }
                    }
                }
            }
            if ((parent.getPosition() - Position).Length() > parent.getCollisionRadius())
            {
                barrelfree = true;
            }
        }
        public bool getActive()
        {
            return active;
        }
        public void setActive(bool act)
        {
            active = act;
        }
        public Vector2 getPosition()
        {
            return Position;
        }
        public Rectangle getRect()
        {
            return drawRect;
        }
    }
}
