

using SWENG.Sensor;

namespace KinectTherapy
{
    using System;

    /// <summary>
    /// A template class for EventArgs for KinectSensorManager On*Changed events.
    /// </summary>
    /// <typeparam name="T">The type of the property that has changed.</typeparam>
    public sealed class KinectSensorManagerEventArgs<T> : EventArgs
    {
        public KinectSensorManagerEventArgs(KinectSensorManager sensorManager, T oldValue, T newValue)
        {
            KinectSensorManager = sensorManager;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public KinectSensorManager KinectSensorManager { get; private set; }

        public T OldValue { get; private set; }

        public T NewValue { get; private set; }
    }
}
