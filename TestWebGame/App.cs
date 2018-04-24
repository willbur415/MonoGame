using Bridge;
using Bridge.Html5;
using System;
using System.IO;

namespace TestWebGame
{
    public class App
    {
		private static Game1 game;
		
        public static void Main()
		{
			var canvas = new HTMLCanvasElement();
			canvas.Width = 800;
			canvas.Height = 480;
			canvas.Id = "monogamecanvas";

            var button = new HTMLButtonElement();
            button.InnerHTML = "Run Game";

            button.OnClick = (ev) =>
            {
				Console.WriteLine("Bridge.NET sanity test :)");
				game = new Game1();
				game.Run();
			};

			Document.Body.AppendChild(canvas);
            Document.Body.AppendChild(button);
        }
    }
}
