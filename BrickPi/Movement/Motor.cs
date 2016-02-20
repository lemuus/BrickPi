﻿//////////////////////////////////////////////////////////
// This code has been originally created by Laurent Ellerbach
// It intend to make the excellent BrickPi from Dexter Industries working
// on a RaspberryPi 2 runing Windows 10 IoT Core in Universal
// Windows Platform.
// Credits:
// - Dexter Industries Code
// - MonoBrick for great inspiration regarding sensors implementation in C#
//
// This code is under https://opensource.org/licenses/ms-pl
//
//////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Threading;

namespace BrickPi.Movement
{

    /// <summary>
    /// Polarity of the motor
    /// </summary>
    public enum Polarity
    {
#pragma warning disable
        Backward = -1, Forward = 1, OppositeDirection = 0
#pragma warning restore
    };

    /// <summary>
    /// This class contains a motor object and all needed functions and properties to pilot it
    /// </summary>
    public sealed class Motor : INotifyPropertyChanged
    {
        // represent the Brick
        private Brick brick = null;

        /// <summary>
        /// Create a motor
        /// </summary>
        /// <param name="port">Motor port</param>
        public Motor(BrickPortMotor port):this(port, 1000)
        { }

        public Motor(BrickPortMotor port, int timeout)
        {
            brick = new Brick();
            Port = port;
            //brick.Start();
            periodRefresh = timeout;
            timer = new Timer(UpdateSensor, this, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(timeout));
        }

        /// <summary>
        /// Set the speed of the motor
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void SetSpeed(int speed)
        {
            if (speed > 255)
                speed = 255;
            if (speed < -255)
                speed = -255;
            brick.BrickPi.Motor[(int)Port].Speed = speed;

            //raise the event to notify the UI
            OnPropertyChanged(nameof(Speed));
        }

        /// <summary>
        /// Set Tachometer encoder offset
        /// Use this to reset or setup a specific position
        /// </summary>
        /// <param name="position">New offset, 0 to reset</param>
        public void SetTachoCount(Int32 position)
        {
            brick.BrickPi.Motor[(int)Port].EncoderOffset = position;
        }

        /// <summary>
        /// Stop the Motor
        /// </summary>
        public void Stop()
        {
            brick.BrickPi.Motor[(int)Port].Enable = 0;
        }

        /// <summary>
        /// Start the motor
        /// </summary>
        public void Start()
        {
            brick.BrickPi.Motor[(int)Port].Enable = 1;
        }

        /// <summary>
        /// Start with the specified speed
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void Start(int speed)
        {
            SetSpeed(speed);
            Start();
        }

        /// <summary>
        /// Change the polatity of the motor
        /// </summary>
        /// <param name="polarity">Polarity of the motor, backward, forward or opposite</param>
        public void SetPolarity(Polarity polarity)
        {
            switch (polarity)
            {
                case Polarity.Backward:
                    if (brick.BrickPi.Motor[(int)Port].Speed > 0)
                        brick.BrickPi.Motor[(int)Port].Speed = -brick.BrickPi.Motor[(int)Port].Speed;
                    break;
                case Polarity.Forward:
                    if (brick.BrickPi.Motor[(int)Port].Speed < 0)
                        brick.BrickPi.Motor[(int)Port].Speed = -brick.BrickPi.Motor[(int)Port].Speed;
                    break;
                case Polarity.OppositeDirection:
                    brick.BrickPi.Motor[(int)Port].Speed = -brick.BrickPi.Motor[(int)Port].Speed;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets the tacho count
        /// </summary>
        /// <returns>The tacho count in 0.5 of degrees</returns>
        public Int32 GetTachoCount()
        {
            return brick.BrickPi.Motor[(int)Port].Encoder;
        }

        /// <summary>
        /// Get the speed
        /// </summary>
        /// <returns>speed is between -255 and +255</returns>
        public int GetSpeed()
        {
            return brick.BrickPi.Motor[(int)Port].Speed;
        }

        /// <summary>
        /// Set or read the speed of the motor
        /// speed is between -255 and +255
        /// </summary>
        public int Speed
        { get { return GetSpeed(); } set { SetSpeed(value); } }

        public BrickPortMotor Port { get; internal set; }

        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// To notify a property has changed. The minimum time can be set up
        /// with timeout property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private int periodRefresh;
        /// <summary>
        /// Period to refresh the notification of property changed in milliseconds
        /// </summary>
        public int PeriodRefresh
        {
            get { return periodRefresh; }
            set
            {
                periodRefresh = value;
                timer.Change(TimeSpan.FromMilliseconds(periodRefresh), TimeSpan.FromMilliseconds(periodRefresh));
            }
        }

        /// <summary>
        /// Update the sensor and this will raised an event on the interface
        /// </summary>
        public void UpdateSensor(object state)
        {
            TachoCount = GetTachoCount();
        }

        private int tacho;
        /// <summary>
        /// Tacho count as a property, events are rasied when value is changing
        /// </summary>
        public int TachoCount
        {
            get { return GetTachoCount(); }
            internal set {
                if (tacho != value)
                {
                    tacho = value;
                    OnPropertyChanged(nameof(TachoCount));
                }
            }
        }

        private Timer timer = null;
        private void StopTimerInternal()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

    }
}
