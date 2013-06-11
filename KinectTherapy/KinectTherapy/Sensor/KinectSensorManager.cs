using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using KinectTherapy;
using Microsoft.Kinect;


namespace SWENG.Sensor
{
    public class KinectSensorManager : Freezable
    {
        public static readonly DependencyProperty KinectSensorProperty;
        public static readonly DependencyProperty KinectSensorStatusProperty;
        public static readonly DependencyProperty UniqueKinectIdProperty;
        public static readonly DependencyProperty KinectSensorEnabledProperty;
        public static readonly DependencyProperty SupportsCameraSettingsProperty;
        public static readonly DependencyProperty CameraSettingsProperty;
        public static readonly DependencyProperty KinectSensorAppConflictProperty;
        public static readonly DependencyProperty ElevationAngleProperty;
        public static readonly DependencyProperty ForceInfraredEmitterOffProperty;
        private static readonly DependencyPropertyKey UniqueKinectIdPropertyKey;
        private static readonly DependencyPropertyKey KinectSensorAppConflictPropertyKey;

        private int _targetElevationAngle = int.MinValue;
        private bool _isElevationTaskOutstanding;

        static KinectSensorManager()
        {
            UniqueKinectIdPropertyKey =
                DependencyProperty.RegisterReadOnly(
                    "UniqueKinectId",
                    typeof(string),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(null));


            KinectSensorProperty =
                DependencyProperty.Register(
                    "KinectSensor",
                    typeof(KinectSensor),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(null, KinectSensorOrStatusChanged));

            KinectSensorStatusProperty =
                DependencyProperty.Register(
                    "KinectSensorStatus",
                    typeof(KinectStatus),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(KinectStatus.Undefined, KinectSensorOrStatusChanged));

            UniqueKinectIdProperty = UniqueKinectIdPropertyKey.DependencyProperty;


            SupportsCameraSettingsProperty =
                DependencyProperty.Register(
                    "SupportsCameraSettings",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(false));

            CameraSettingsProperty =
                DependencyProperty.Register(
                    "CameraSettings",
                    typeof(ColorCameraSettings), 
                    typeof(KinectSensorManager),
                    new PropertyMetadata(null));

            KinectSensorAppConflictProperty = KinectSensorAppConflictPropertyKey.DependencyProperty;

            ElevationAngleProperty =
                DependencyProperty.Register(
                    "ElevationAngle",
                    typeof(int),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(0, (sender, args) => ((KinectSensorManager)sender).EnsureElevationAngle(), CoerceElevationAngle));

        }

        public event EventHandler<KinectSensorManagerEventArgs<KinectSensor>> KinectSensorChanged;

        public event EventHandler<KinectSensorManagerEventArgs<KinectStatus>> KinectStatusChanged;

        public event EventHandler<KinectSensorManagerEventArgs<bool>> KinectRunningStateChanged;

        protected override Freezable CreateInstanceCore()
        {
            return new KinectSensorManager();
        }


        public KinectSensor KinectSensor
        {
            get { return (KinectSensor)GetValue(KinectSensorProperty); }
            set { SetValue(KinectSensorProperty, value); }
        }

        public string UniqueKinectId
        {
            get { return (string)GetValue(UniqueKinectIdProperty); }
            private set { SetValue(UniqueKinectIdPropertyKey, value); }
        }

        public int ElevationAngle
        {
            get { return (int)GetValue(ElevationAngleProperty); }
            set { SetValue(ElevationAngleProperty, value); }
        }

        public bool ForceInfraredEmitterOff
        {
            get { return (bool)GetValue(ForceInfraredEmitterOffProperty); }
            set { SetValue(ForceInfraredEmitterOffProperty, value); }
        }

        public KinectStatus KinectSensorStatus
        {
            get { return (KinectStatus)GetValue(KinectSensorStatusProperty); }
            set { SetValue(KinectSensorStatusProperty, value); }
        }

        public bool SupportsCameraSettings
        {
            get { return (bool)GetValue(SupportsCameraSettingsProperty); }
            set { SetValue(SupportsCameraSettingsProperty, value); }
        }

        /// <summary>
        /// This method will ensure that the local state and the state of the KinectSensor itself
        /// is uninitialized.
        /// The parameter to this method may be null, and it may already be unintialized.
        /// </summary>
        /// <param name="sensor">The sensor to uninit</param>
        private static void EnsureSensorUninit(KinectSensor sensor)
        {
            if (sensor == null)
            {
                return;
            }

            UninitializeKinectServices(sensor);
        }

        public ColorCameraSettings CameraSettings
        {
            get { return (ColorCameraSettings)GetValue(CameraSettingsProperty); }
            set { SetValue(CameraSettingsProperty, value); }
        }

        /// <summary>
        /// Called as part of Freezable.Freeze() to determine whether this class can be frozen.
        /// We can freeze, but only if the KinectSensor is null.
        /// </summary>
        /// <param name="isChecking">True if this is a query for Freezability, false if this is an actual Freeze call.</param>
        /// <returns>True if a Freeze is legal or has occurred, false otherwise.</returns>
        protected override bool FreezeCore(bool isChecking)
        {
            return (null == KinectSensor) && base.FreezeCore(isChecking);
        }

        private void InitializeKinectServices()
        {
            EnsureElevationAngle();

        }

        private static void UninitializeKinectServices(KinectSensor sensor)
        {
            if (null == sensor)
            {
                return;
            }

            // Stop streaming
            sensor.Stop();

            if (null != sensor.AudioSource)
            {
                sensor.AudioSource.Stop();
            }

            if (null != sensor.SkeletonStream)
            {
                sensor.SkeletonStream.Disable();
            }

            if (null != sensor.DepthStream)
            {
                sensor.DepthStream.Disable();
            }

            if (null != sensor.ColorStream)
            {
                sensor.ColorStream.Disable();
            }
        }

        private void EnsureElevationAngle()
        {
            var sensor = KinectSensor;

            // We cannot set the angle on a sensor if it is not running.
            // We will therefore call EnsureElevationAngle when the requested angle has changed or if the
            // sensor transitions to the Running state.
            if ((null == sensor) || (KinectStatus.Connected != sensor.Status) || !sensor.IsRunning)
            {
                return;
            }

            _targetElevationAngle = ElevationAngle;

            // If there already a background task, it will notice the new targetElevationAngle
            if (!this._isElevationTaskOutstanding)
            {
                // Otherwise, we need to start a new task
                this.StartElevationTask();
            }
        }

        private void StartElevationTask()
        {
            var sensor = KinectSensor;
            int lastSetElevationAngle = int.MinValue;

            if (null == sensor) return;

            _isElevationTaskOutstanding = true;

            Task.Factory.StartNew(
                () =>
                    {
                        var angleToSet = _targetElevationAngle;

                        // Keep going until we "match", assuming that the sensor is running
                        while ((lastSetElevationAngle != angleToSet) && sensor.IsRunning)
                        {
                            // We must wait at least 1 second, and call no more frequently than 15 times every 20 seconds
                            // So, we wait at least 1350ms afterwards before we set backgroundUpdateInProgress to false.
                            sensor.ElevationAngle = angleToSet;
                            lastSetElevationAngle = angleToSet;
                            Thread.Sleep(1350);

                            angleToSet = _targetElevationAngle;
                        }
                    }).ContinueWith(
                        results =>
                            {
                                // This can happen if the Kinect transitions from Running to not running
                                // after the check above but before setting the ElevationAngle.
                                if (results.IsFaulted)
                                {
                                    var exception = results.Exception;

                                    Debug.WriteLine(
                                        "Set Elevation Task failed with exception " +
                                        exception);
                                }

                                // We caught up and handled all outstanding angle requests.
                                // However, more may come in after we've stopped checking, so
                                // we post this work item back to the main thread to determine
                                // whether we need to start the task up again.
                                Dispatcher.BeginInvoke((Action) (() =>
                                    {
                                        if (_targetElevationAngle !=
                                            lastSetElevationAngle)
                                        {
                                            StartElevationTask();
                                        }
                                        else
                                        {
                                            // If there's nothing to do, we can set this to false.
                                            _isElevationTaskOutstanding = false;
                                        }
                                    }));
                            });
        }

        /// <summary>
        /// Callback that occurs when either the KinectSensor or its status has changed
        /// </summary>
        /// <param name="sender">The source KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        private static void KinectSensorOrStatusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var sensorWrapper = sender as KinectSensorManager;

            if (null == sensorWrapper)
            {
                return;
            }

            var oldSensor = sensorWrapper.KinectSensor;
            var sensor = sensorWrapper.KinectSensor;
            var oldStatus = KinectStatus.Undefined;
            var status = null == sensor ? KinectStatus.Undefined : sensor.Status;

            var sensorChanged = KinectSensorProperty == args.Property;
            var statusChanged = KinectSensorStatusProperty == args.Property;

            if (sensorChanged)
            {
                oldSensor = (KinectSensor)args.OldValue;

                // The elevation task is per-sensor
                sensorWrapper._isElevationTaskOutstanding = false;

                // This can throw if the sensor is going away or gone.
                try
                {
                    sensorWrapper.UniqueKinectId = (null == sensor) ? null : sensor.UniqueKinectId;
                }
                catch (InvalidOperationException)
                {
                }
            }

            if (statusChanged)
            {
                oldStatus = (KinectStatus)args.OldValue;
            }

            // Ensure that the sensor is uninitialized if the sensor has changed or if the status is not Connected
            if (sensorChanged || (statusChanged && (KinectStatus.Connected != status)))
            {
                EnsureSensorUninit(oldSensor);
            }

            bool wasRunning = (null != sensor) && sensor.IsRunning;

            sensorWrapper.InitializeKinectServices();

            bool isRunning = (null != sensor) && sensor.IsRunning;

            sensorWrapper.KinectSensorStatus = status;

            if (sensorChanged && (null != sensorWrapper.KinectSensorChanged))
            {
                sensorWrapper.KinectSensorChanged(sensorWrapper, new KinectSensorManagerEventArgs<KinectSensor>(sensorWrapper, oldSensor, sensor));
            }

            if ((status != oldStatus) && (null != sensorWrapper.KinectStatusChanged))
            {
                sensorWrapper.KinectStatusChanged(sensorWrapper, new KinectSensorManagerEventArgs<KinectStatus>(sensorWrapper, oldStatus, status));
            }

            if ((wasRunning != isRunning) && (null != sensorWrapper.KinectRunningStateChanged))
            {
                sensorWrapper.KinectRunningStateChanged(sensorWrapper, new KinectSensorManagerEventArgs<bool>(sensorWrapper, wasRunning, isRunning));
            }

            try
            {
                if (sensor != null && sensor.ColorStream.CameraSettings != null)
                {
                    sensorWrapper.SupportsCameraSettings = true;
                    sensorWrapper.CameraSettings = sensor.ColorStream.CameraSettings;
                }
                else
                {
                    sensorWrapper.SupportsCameraSettings = false;
                    sensorWrapper.CameraSettings = null;
                }
            }
            catch (InvalidOperationException)
            {
                sensorWrapper.SupportsCameraSettings = false;
                sensorWrapper.CameraSettings = null;
            }
        }

        /// <summary>
        /// Coerce the requested elevation angle to a valid angle
        /// </summary>
        /// <param name="sender">The source KinectSensorManager</param>
        /// <param name="baseValue">The baseValue to coerce</param>
        /// <returns>A valid elevation angle.</returns>
        private static object CoerceElevationAngle(DependencyObject sender, object baseValue)
        {
            var sensorWrapper = sender as KinectSensorManager;

            if ((null == sensorWrapper) || !(baseValue is int))
            {
                return 0;
            }

            // Best guess default values for min/max angles
            int minVal = -27;
            int maxVal = 27;

            if (null != sensorWrapper.KinectSensor)
            {
                minVal = sensorWrapper.KinectSensor.MinElevationAngle;
                maxVal = sensorWrapper.KinectSensor.MaxElevationAngle;
            }

            if ((int)baseValue < minVal)
            {
                return minVal;
            }

            if ((int)baseValue > maxVal)
            {
                return maxVal;
            }

            return baseValue;
        }

    }
}
