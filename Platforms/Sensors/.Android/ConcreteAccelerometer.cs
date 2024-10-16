﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Devices.Sensors;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Xna.Platform.Input.Sensors
{
    internal class ConcreteAccelerometer : AccelerometerStrategy
    {
        internal static SensorManager _sensorManager;
        internal static Sensor _sensorAccelerometer;
        static int _instanceCount;

        SensorListener _sensorListener;

        private SensorReadingEventArgs<AccelerometerReading> _eventArgs = new SensorReadingEventArgs<AccelerometerReading>(default(AccelerometerReading));


        public override SensorState State
        {
            get
            {
                if (_sensorManager == null)
                {
                    ConcreteAccelerometer.Initialize();
                    base.State = (_sensorAccelerometer != null)
                               ? SensorState.Initializing 
                               : SensorState.NotSupported;
                }

                return base.State;
            }
            set { base.State = value; }
        }

        public override bool IsDataValid
        {
            get { return base.IsDataValid; }
            set { base.IsDataValid = value; }
        }

        public override TimeSpan TimeBetweenUpdates
        {
            get { return base.TimeBetweenUpdates; }
            set
            {
                if (base.TimeBetweenUpdates != value)
                {
                    base.TimeBetweenUpdates = value;
                    // TODO: implement TimeBetweenUpdates for Android
                }
            }
        }

        public override AccelerometerReading CurrentValue
        {
            get { return base.CurrentValue; }
            set { base.CurrentValue = value; }
        }


        public ConcreteAccelerometer()
        {
            _instanceCount++;

            base.State = (_sensorAccelerometer != null)
                       ? SensorState.Initializing 
                       : SensorState.NotSupported;

            _sensorListener = new SensorListener();
            _sensorListener.AccuracyChanged += _sensorListener_AccuracyChanged;
            _sensorListener.SensorChanged += _sensorListener_SensorChanged;
        }

        static internal void Initialize()
        {
            _sensorManager = (SensorManager)AndroidGameWindow.Activity.GetSystemService(Context.SensorService);
            _sensorAccelerometer = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
        }

        public override void Start()
        {
            if (_sensorManager == null)
                ConcreteAccelerometer.Initialize();

            if (this.State != SensorState.Ready)
            {
                if (_sensorManager != null && _sensorAccelerometer != null)
                {
                    _sensorManager.RegisterListener(_sensorListener, _sensorAccelerometer, SensorDelay.Game);
                    // So the system can pause and resume the sensor when the activity is paused
                    AndroidGameWindow.Activity.Paused += _activity_Paused;
                    AndroidGameWindow.Activity.Resumed += _activity_Resumed;
                }
                else
                {
                    throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);
                }
                base.State = SensorState.Ready;
            }
            else
            {
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);
            }
        }

        public override void Stop()
        {
            if (this.State == SensorState.Ready)
            {
                if (_sensorManager != null && _sensorAccelerometer != null)
                {
                    AndroidGameWindow.Activity.Paused -= _activity_Paused;
                    AndroidGameWindow.Activity.Resumed -= _activity_Resumed;
                    _sensorManager.UnregisterListener(_sensorListener, _sensorAccelerometer);
                }
            }
            base.State = SensorState.Disabled;
        }

        void _activity_Paused(object sender, EventArgs eventArgs)
        {
            _sensorManager.UnregisterListener(_sensorListener, _sensorAccelerometer);
        }

        void _activity_Resumed(object sender, EventArgs eventArgs)
        {
            _sensorManager.RegisterListener(_sensorListener, _sensorAccelerometer, SensorDelay.Game);
        }

        private void _sensorListener_AccuracyChanged(object sender, EventArgs eventArgs)
        {
            //do nothing
        }

        private void _sensorListener_SensorChanged(object sender, SensorListener.SensorChangedEventArgs eventArgs)
        {
            try
            {
                SensorEvent e = eventArgs.Event;
                if (e != null && e.Sensor.Type == SensorType.Accelerometer)
                {
                    IList<float> values = e.Values;
                    try
                    {
                        AccelerometerReading reading = new AccelerometerReading();
                        base.IsDataValid = (values != null && values.Count == 3);
                        if (base.IsDataValid)
                        {
                            const float gravity = SensorManager.GravityEarth;
                            reading.Acceleration = new Vector3(values[0], values[1], values[2]) / gravity;
                            reading.Timestamp = DateTime.UtcNow;
                        }
                        base.CurrentValue = reading;

                        _eventArgs.SensorReading = base.CurrentValue;
                        base.OnCurrentValueChanged(_eventArgs);
                    }
                    finally
                    {
                        IDisposable d = values as IDisposable;
                        if (d != null)
                            d.Dispose();
                    }
                }
            }
            catch (NullReferenceException)
            {
                //Occassionally an NullReferenceException is thrown when accessing e.Values??
                // mono    : Unhandled Exception: System.NullReferenceException: Object reference not set to an instance of an object
                // mono    :   at Android.Runtime.JNIEnv.GetObjectField (IntPtr jobject, IntPtr jfieldID) [0x00000] in <filename unknown>:0 
                // mono    :   at Android.Hardware.SensorEvent.get_Values () [0x00000] in <filename unknown>:0
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (this.State == SensorState.Ready)
                Stop();

            _instanceCount--;
            if (_instanceCount == 0)
            {
                _sensorAccelerometer = null;
                _sensorManager = null;
            }

            base.Dispose(disposing);
        }

    }
}