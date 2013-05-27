using System;

namespace KinectTherapy
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (KinectTherapyGame game = new KinectTherapyGame())
            {
                game.Run();
            }
        }
    }
#endif
}

