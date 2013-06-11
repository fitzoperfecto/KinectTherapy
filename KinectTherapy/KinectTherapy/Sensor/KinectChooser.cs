//------------------------------------------------------------------------------
// <copyright file="KinectChooser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.XnaBasics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Kinect;
    using Xna.Framework;
    using Xna.Framework.Graphics;


    /// <summary>
    /// This class will pick a kinect sensor if available.
    /// </summary>
    public class KinectChooser : DrawableGameComponent
    {
        /// <summary>
        /// The status to string mapping.
        /// </summary>
        private readonly Dictionary<KinectStatus, string> statusMap = new Dictionary<KinectStatus, string>();

        /// <summary>
        /// The requested color image format.
        /// </summary>
        private readonly ColorImageFormat colorImageFormat;

        /// <summary>
        /// The requested depth image format.
        /// </summary>
        private readonly DepthImageFormat depthImageFormat;

        /// <summary>
        /// The chooser background texture.
        /// </summary>
        private Texture2D chooserBackground;
        
        /// <summary>
        /// The SpriteBatch used for rendering.
        /// </summary>
        private SpriteBatch spriteBatch;
        
        /// <summary>
        /// The font for rendering the state text.
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// Initializes a new instance of the KinectChooser class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="colorFormat">The desired color image format.</param>
        /// <param name="depthFormat">The desired depth image format.</param>
        public KinectChooser(Game game, ColorImageFormat colorFormat, DepthImageFormat depthFormat)
            : base(game)
        {
            colorImageFormat = colorFormat;
            depthImageFormat = depthFormat;

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            DiscoverSensor();

            statusMap.Add(KinectStatus.Connected, string.Empty);
            statusMap.Add(KinectStatus.DeviceNotGenuine, "Device Not Genuine");
            statusMap.Add(KinectStatus.DeviceNotSupported, "Device Not Supported");
            statusMap.Add(KinectStatus.Disconnected, "Required");
            statusMap.Add(KinectStatus.Error, "Error");
            statusMap.Add(KinectStatus.Initializing, "Initializing...");
            statusMap.Add(KinectStatus.InsufficientBandwidth, "Insufficient Bandwidth");
            statusMap.Add(KinectStatus.NotPowered, "Not Powered");
            statusMap.Add(KinectStatus.NotReady, "Not Ready");
        }

        /// <summary>
        /// Gets the selected KinectSensor.
        /// </summary>
        public KinectSensor Sensor { get; private set; }

        /// <summary>
        /// Gets the last known status of the KinectSensor.
        /// </summary>
        public KinectStatus LastStatus { get; private set; }

        /// <summary>
        /// This method initializes necessary objects.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        /// <summary>
        /// This method renders the current state of the KinectChooser.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            // If the spritebatch is null, call initialize
            if (spriteBatch == null)
            {
                Initialize();
            }

            // If the background is not loaded, load it now
            if (chooserBackground == null)
            {
                LoadContent();
            }

            // If we don't have a sensor, or the sensor we have is not connected
            // then we will display the information text
            if (Sensor == null || LastStatus != KinectStatus.Connected)
            {
                spriteBatch.Begin();

                // Render the background
                spriteBatch.Draw(
                    chooserBackground,
                    new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2),
                    null,
                    Color.White,
                    0,
                    new Vector2(chooserBackground.Width / 2, chooserBackground.Height / 2),
                    1,
                    SpriteEffects.None,
                    0);

                // Determine the text
                string txt = "Required";
                if (Sensor != null)
                {
                    txt = statusMap[LastStatus];
                }

                // Render the text
                Vector2 size = font.MeasureString(txt);
                spriteBatch.DrawString(
                    font,
                    txt,
                    new Vector2((Game.GraphicsDevice.Viewport.Width - size.X) / 2, (Game.GraphicsDevice.Viewport.Height / 2) + size.Y),
                    Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// This method loads the textures and fonts.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            chooserBackground = Game.Content.Load<Texture2D>("ChooserBackground");
            font = Game.Content.Load<SpriteFont>("Segoe16");
        }

        /// <summary>
        /// This method ensures that the KinectSensor is stopped before exiting.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            // Always stop the sensor when closing down
            if (Sensor != null)
            {
                Sensor.Stop();
            }
        }

        /// <summary>
        /// This method will use basic logic to try to grab a sensor.
        /// Once a sensor is found, it will start the sensor with the
        /// requested options.
        /// </summary>
        private void DiscoverSensor()
        {
            // Grab any available sensor
            Sensor = KinectSensor.KinectSensors.FirstOrDefault();

            if (Sensor != null)
            {
                LastStatus = Sensor.Status;

                // If this sensor is connected, then enable it
                if (LastStatus == KinectStatus.Connected)
                {
                    try
                    {
                        Sensor.SkeletonStream.Enable();
                        Sensor.ColorStream.Enable(colorImageFormat);
                        Sensor.DepthStream.Enable(depthImageFormat);

                        try
                        {
                            Sensor.Start();
                        }
                        catch (IOException)
                        {
                            // sensor is in use by another application
                            // will treat as disconnected for display purposes
                            Sensor = null;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // KinectSensor might enter an invalid state while
                        // enabling/disabling streams or stream features.
                        // E.g.: sensor might be abruptly unplugged.
                        Sensor = null;
                    }
                }
            }
            else
            {
                LastStatus = KinectStatus.Disconnected;
            }
        }

        /// <summary>
        /// This wires up the status changed event to monitor for 
        /// Kinect state changes.  It automatically stops the sensor
        /// if the device is no longer available.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // If the status is not connected, try to stop it
            if (e.Status != KinectStatus.Connected)
            {
                e.Sensor.Stop();
            }

            LastStatus = e.Status;
            DiscoverSensor();
        }
    }
}
