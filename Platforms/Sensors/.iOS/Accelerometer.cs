// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Input.Sensors;

namespace Microsoft.Devices.Sensors
{
    public sealed class Accelerometer : SensorBase<AccelerometerReading>
    {
        private AccelerometerStrategy _strategy;

        private bool _isDisposed;

        internal AccelerometerStrategy Strategy
        {
            get { return _strategy; }
        }

        public static bool IsSupported
        {
            get { return ConcreteAccelerometer._motionManager.AccelerometerAvailable; }
        }

        public SensorState State
        {
            get { return Strategy.State; }
        }

        protected override bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public override bool IsDataValid
        {
            get { return Strategy.IsDataValid; }
        }

        public override TimeSpan TimeBetweenUpdates
        {
            get { return Strategy.TimeBetweenUpdates; }
            set { Strategy.TimeBetweenUpdates = value; }
        }

        public override AccelerometerReading CurrentValue
        {
            get { return Strategy.CurrentValue; }
        }

        public Accelerometer()
        {
            _strategy = new ConcreteAccelerometer();
            _strategy.CurrentValueChanged += _strategy_CurrentValueChanged;
        }

        private void _strategy_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> eventArgs)
        {
            OnCurrentValueChanged(eventArgs);
        }

        public override void Start()
        {
            Strategy.Start();
        }

        public override void Stop()
        {
            Strategy.Stop();
        }


        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Strategy.Dispose();
                }

                _isDisposed = true;
                //base.Dispose(disposing);
            }
        }
    }
}

