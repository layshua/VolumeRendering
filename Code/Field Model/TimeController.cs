//**************************************************************//
//**************************************************************//
//*******           Copyright (C) 2010-2018              *******//
//*******           Team: Wuhan University               *******//
//*******           Developer:Xuequan Zhang              *******//
//*******           Email:xuequan_zhang@126.com         *******//
//*******           Address: 129 Luoyu Road, Wuhan,      *******//
//*******                    Hubei, 430079, China        *******//
//**************************************************************//
//**************************************************************//

using System;
using System.Collections.Generic;
using System.Text;

namespace FieldModel
{
    /// <summary>
    /// Volume annimation controller.
    /// </summary>
    public sealed class TimeController
    {
        private System.Timers.Timer _timer = null;
        private int _timeInterval = 3000; // one second for default setting.
        private int _ticks = 0;

        public int TimeCount = 1;
        /// <summary>
        /// The instance of the class.
        /// </summary>
        public static TimeController Instance = null;

        public TimeController()
        {
            _timer = new System.Timers.Timer(10);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }

        /// <summary>
        /// Time interval between two key frames.
        /// </summary>
        public int KeyFrameInterval
        {
            get { return _timeInterval; }
            set { _timeInterval = value; }
        }

        public int TimeNumber
        {
            get
            {
                int timeNum = (_ticks / _timeInterval) % TimeCount;
                return timeNum;
            }
        }

        public void SetTimeNumber(int num)
        {
            _ticks = _timeInterval * num;
        }

        public void PreTimeNumber()
        {
            _ticks -= _timeInterval;
            if (_ticks < 0)
                _ticks = 0;
        }

        public void NextTimeNumber()
        {
            _ticks += _timeInterval;
            if (_ticks >= int.MaxValue)
                _ticks = 0;
        }

        /// <summary>
        /// A float value between two time number.
        /// </summary>
        public float Timefloat
        {
            get
            {
                float timef = (float)(_ticks % _timeInterval) / (float)_timeInterval;
                return timef;
            }
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_ticks >= int.MaxValue)
                _ticks = 0;
            _ticks += 10;

        }

        /// <summary>
        /// Reset time.
        /// </summary>
        public void ResetTime()
        {
            _ticks = 0;
        }

        /// <summary>
        /// Stop time.
        /// </summary>
        public void StopTime()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Start time.
        /// </summary>
        public void StartTime()
        {
            _timer.Start();
 
        }

    }
}