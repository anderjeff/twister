using System;
using System.Diagnostics;
using Twister.Business.Hardware;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    /******************************************************************************
     * This class represents a fully reversed style torque test.  Fully reversed 
     * means that torque is applied in two directions (CW and CCW) with equal 
     * magnitudes, but opposite directions.
     * 
     * The two parameters that control the behavior of this test are:
     *     1. cwTorque - Means clockwise torque and represents the single magnitude
     *                   in both the CW and CCW direction.  The TestParams.xml file
     *                   needs to contain an attribute called cwTorque, along with 
     *                   a value attribute, the value of which will be interpreted 
     *                   as torque in inch-lbs.
     *     2. runSpeed - Sets the MOVE.RUNSPEED value for the AKD Basic drive.  The 
     *                   units depend on the UNIT.VROTARY parameter, but for this 
     *                   test, it is expected that the runSpeed will correlate to 
     *                   motor rpm.  The higher this value, the faster the motor 
     *                   will turn during the test.
     *                   
     * The players
     *     1. TestBench -  Represents an entity that can report its current state, 
     *                     meaning the value of it's sensors.  The state is reported 
     *                     in the form of a single object of type Sample.  
     *     2. TorqueTest - Represents an entity that has the potential to change the 
     *                     state of the TestBench, can discover and save snapshots of 
     *                     the state of the TestBench, and can send updates on the 
     *                     state of the TestBench to the ServoDrive 
     *****************************************************************************/


    /// <summary>
    ///     Represents a torque test that has an equal torque in
    ///     both the clockwise and counterclockwise directions.
    /// </summary>
    public class FullyReversedTorqueTest : TorqueTest
    {
        private static object _objLock = new object();

        /// <summary>
        ///     Prepares the test to be able to be started by setting the
        ///     clockwise torque, counterclockwise torque, run speed and
        ///     manual mode speed.
        /// </summary>
        /// <param name="testTemplate">A template that has all the necessary parameters.</param>
        public override void LoadTestParameters(TestTemplate testTemplate)
        {
            try
            {
                MaxTorque = testTemplate.ClockwiseTorque;
                MinTorque = testTemplate.CounterclockwiseTorque;

                // set clockwise torque
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.CwTorqueLimit, MaxTorque);

                /* set counterclockwise torque to be the same value, this is, after all,
                 * a fully reversed torque test, so by definition the values need to be 
                 * equal and opposite (thus the -1 multiplier). 
                 */
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.CcwTorqueLimit, MinTorque);

                // set run speed
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.Runspeed, testTemplate.RunSpeed);

                // set manual speed
                TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.Manualspeed, testTemplate.MoveSpeed);

                InformInitializationComplete();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "FullyReversedTorqueTest.LoadTestParameters"));
            }
        }
    }
}