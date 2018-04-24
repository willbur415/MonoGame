// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    class WebGamePlatform : GamePlatform
    {
        private WebGameWindow _view;
        private int _threadId;

        public WebGamePlatform(Game game)
            : base(game)
        {
            Window = _view = new WebGameWindow();
        }

        public virtual void Callback()
        {
            this.Game.Tick();
        }
        
        public override void Exit()
        {
            Bridge.Html5.Window.ClearInterval(_threadId);
        }

        public override void RunLoop()
        {
            throw new Exception("How did this happen?");
        }

        public override void StartRunLoop()
        {
            _threadId = Bridge.Html5.Window.SetInterval(() =>
            {
                // Process Events
                Game.Tick();
            }, 20);
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
            ResetWindowBounds();
        }

        public override void ExitFullScreen()
        {
            ResetWindowBounds();
        }

        internal void ResetWindowBounds()
        {
            
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get
            {
                return GameRunBehavior.Asynchronous;
            }
        }
    }
}

