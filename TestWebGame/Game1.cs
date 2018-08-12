using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TestWebGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        Retyped.dom.HTMLDivElement divdata;
        GraphicsDeviceManager graphics;
        Song song;
        MouseState prevstate;
        bool playing;
        KeyboardState prevkstate;
        SpriteBatch spriteBatch;
        Texture2D texBall;
        SoundEffect _seffect;
        SoundEffectInstance _sinstance;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            divdata = Retyped.dom.document.getElementById("testoutput") as Retyped.dom.HTMLDivElement;

            song = Content.Load<Song>("awake");
            MediaPlayer.Play(song);
            MediaPlayer.Pause(); 
            MediaPlayer.Volume = 0.1f;

            // texBall = Content.Load<Texture2D>("hacker");
            texBall = Texture2D.FromURL(GraphicsDevice, "Content/hacker.png");

            _seffect = SoundEffect.FromURL("awake.ogg");
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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var state = Mouse.GetState();
            var kstate = Keyboard.GetState();
            var jstate = Joystick.GetState(0);
            var gstate = GamePad.GetState(0);

            if (prevstate.RightButton == ButtonState.Released && state.RightButton == ButtonState.Pressed)
            {
                graphics.IsFullScreen = !graphics.IsFullScreen;
                graphics.ApplyChanges();
            }

            if (prevkstate.IsKeyUp(Keys.M) && kstate.IsKeyDown(Keys.M))
            {
                if (playing)
                    MediaPlayer.Pause();
                else
                    MediaPlayer.Resume();

                playing = !playing;
            }

            if (prevkstate.IsKeyUp(Keys.S) && kstate.IsKeyDown(Keys.S))
            {
                // DO NOTE THAT IT TAKES A MOMENT FOR THE SOUND EFFECT TO LOAD
                // THIS WILL CRASH IF CALLED TO EARLY
                if (_sinstance == null)
                {
                    _sinstance = _seffect.CreateInstance();
                    _sinstance.Volume = 0.1f;
                }
                _sinstance.Play();
            }

            if (prevkstate.IsKeyUp(Keys.D) && kstate.IsKeyDown(Keys.D))
                _sinstance.Pause();

            if (prevkstate.IsKeyUp(Keys.F) && kstate.IsKeyDown(Keys.F))
                _sinstance.Resume();

            divdata.innerHTML = "Left: " + state.LeftButton + "<br>Right: " + state.RightButton + "<br>Mouse pos: " + state.Position + "<br>A: " + kstate.IsKeyDown(Keys.A) + "<br>Caps: " + kstate.CapsLock;

            prevstate = state;
            prevkstate = kstate;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            // spriteBatch.Draw(texBall, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
