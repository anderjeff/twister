using System;
using System.Diagnostics;
using Twister.Business.Hardware;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    public enum TestDirection
    {
        Unknown,
        Clockwise,
        Counterclockwise
    }

    public class UnidirectionalTorqueTest : TorqueTest
    {
        private float _allowablePctChange;

        private TestDirection _direction;

        public UnidirectionalTorqueTest()
        {
            // change as needed.
            AllowablePctChange = 1500;
        }

        /// <summary>
        ///     The rotational direction of this test, as viewed normal to the torque cell face mounting
        ///     to the test specimen, as opposed to the support bracket attached to the carridge.
        /// </summary>
        public TestDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                if (_direction == TestDirection.Clockwise)
                    TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.TorqueDirection, 1);
                else if (_direction == TestDirection.Counterclockwise)
                    TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.TorqueDirection, -1);
                else TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.TorqueDirection, 0);
            }
        }

        /// <summary>
        ///     The allowable percentage difference between samples, used to determine when to stop the test.  The number
        ///     must be greater than zero.
        /// </summary>
        public float AllowablePctChange
        {
            get { return _allowablePctChange; }
            set
            {
                if (value > 0)
                {
                    _allowablePctChange = value;
                    TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.DiffLimit, _allowablePctChange);
                }
                else
                {
                    throw new Exception("Percent change must be a value greater than zero.");
                }
            }
        }

        public override void LoadTestParameters(TestTemplate testTemplate)
        {
            try
            {
                // In the case of the test to failure, these values are the same as the limiting value 
                // for the measurement instrument (torque cell).  As of the date this code was written,
                // that value was 20,000 in-lbs.
                MaxTorque = testTemplate.ClockwiseTorque;
                MinTorque = testTemplate.CounterclockwiseTorque;
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.CwTorqueLimit, MaxTorque);
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.CcwTorqueLimit, MinTorque);

                // set run speed
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.Runspeed, testTemplate.RunSpeed);

                // set manual speed
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.Manualspeed, testTemplate.MoveSpeed);

                InformInitializationComplete();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    Messages.GeneralExceptionMessage(ex, "UnidirectionalTorqueTestToFailure.LoadTestParameters"));
            }
        }
    }
}