using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MazeGamePillaPilla
{
    static class ScreenManager
    {
        private static Stack<IScreen> screensStack;
        internal static IScreen CurrentScreen { get; private set; }

        private static GraphicsDevice _GraphicsDevice;
        private static ContentManager _Content;


        static ScreenManager()
        {
            screensStack = new Stack<IScreen>();
        }


        internal static void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            _GraphicsDevice = GraphicsDevice;
            _Content = Content;
        }


        internal static void PushScreen(IScreen screen)
        {
            CurrentScreen?.Exit();

            screen.Initialize(_GraphicsDevice, _Content);
            screen.Enter();
            screensStack.Push(screen);
            CurrentScreen = screen;
        }


        internal static void PopScreen()
        {
            CurrentScreen.Exit();

            screensStack.Pop();
            CurrentScreen = screensStack.Peek();
            CurrentScreen.Enter();
        }


        internal static void ReplaceCurrent(IScreen screen)
        {
            screensStack.Pop();
            PushScreen(screen);
        }


        internal static void GoBackToMainMenu()
        {
            for (int i = screensStack.Count-1; i > 0; --i)
            {
                screensStack.Pop().Exit();
            }

            CurrentScreen = screensStack.Peek();
            CurrentScreen.Enter();
        }
    }
}
