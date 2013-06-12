﻿using Microsoft.Kinect;

namespace SWENG.Sensor
{
    public class KinectSensorController
    {
        private int _targetElevationAngle;
        private bool _isElevationTaskOutstanding;
        private bool _cameraStatus;
        private const int MaxElevationAngle = 27;
        private const int MinElevationAngle = -27;

        private readonly KinectSensor _kinectSensor;
        private readonly ColorCameraSettings _colorCameraSettings;

        public KinectSensorController()
        {
            _kinectSensor = KinectSensor.KinectSensors[0];

            if (!_kinectSensor.IsRunning) _kinectSensor.Start();

            _colorCameraSettings = _kinectSensor.ColorStream.CameraSettings;
            _colorCameraSettings.AutoExposure = true;
        }

        public void KinectSensorTerminate()
        {
            _kinectSensor.Stop();
        }

        public void KinectSensorElevationControl(int angleIncrement)
        {
            if (!CameraStatus(_kinectSensor)) return;

            if ((_kinectSensor.ElevationAngle >= MinElevationAngle) && (_kinectSensor.ElevationAngle <= MaxElevationAngle))
                _kinectSensor.ElevationAngle += angleIncrement;
        }

        public void KinectSensorBrightnessLevel(float brightnessLevel)
        {
            if (!CameraStatus(_kinectSensor)) return;

            if (_colorCameraSettings.Brightness >= -0.9 && _colorCameraSettings.Brightness <= 0.9)
                    _colorCameraSettings.Brightness += brightnessLevel;
        }

        ///<summary>
        /// Checks the status of the camera and returns true if connected and is running
        /// </summary>
        private static bool CameraStatus(KinectSensor kinectSensor)
        {
            return (null != kinectSensor) && (KinectStatus.Connected == kinectSensor.Status) && kinectSensor.IsRunning;
        }

        private void StartElevationTask()
        {
            _isElevationTaskOutstanding = true;
        }
    }

}