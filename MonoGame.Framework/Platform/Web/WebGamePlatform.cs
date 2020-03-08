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

        public WebGamePlatform(Game game)
            : base(game)
        {
            Window = new WebGameWindow(this);

            _view = (WebGameWindow)Window;
        }

        public virtual void Callback()
        {
            this.Game.Tick();
        }
        
        public override void Exit()
        {
        }

        public override void RunLoop()
        {
            throw new InvalidOperationException("You can not run a synchronous loop on the web platform.");
        }

        public override void StartRunLoop()
        {

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

