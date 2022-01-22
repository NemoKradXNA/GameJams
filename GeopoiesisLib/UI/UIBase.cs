using Geopoiesis.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Geopoiesis.UI
{
    public abstract class UIBase : DrawableGameComponent, IUIBase
    {
        protected IAudioManager audioManager { get { return Game.Services.GetService<IAudioManager>(); } }
        protected IInputStateHandler inputManager { get { return Game.Services.GetService<IInputStateHandler>(); } }

        public Point Position { get; set; }
        public Point Size { get; set; }

        protected SpriteBatch _spriteBatch { get; set; }

        private Rectangle _rectangle;
        public Rectangle Rectangle
        {
            get
            {
                if(_rectangle == null || (_rectangle.X != Position.X || _rectangle.Y != Position.Y || _rectangle.Width != Size.X || _rectangle.Height != Size.Y))
                    _rectangle = new Rectangle(Position.X, Position.Y, Size.X, Size.Y);

                return _rectangle;
            }
        }
        public Color Tint { get; set; }

        public UIBase(Game game, Point position, Point size) : base(game)
        {
            Position = position;
            Size = size;
            Tint = Color.White;
        }

        public override void Initialize()
        {
            base.Initialize();
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

    }

    

    public delegate void UIMouseEvent(IUIBase sender, IMouseStateManager mouseState);
}
