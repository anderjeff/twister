using System;

namespace Twister.Business.Tests
{
    public class TorqueTestEventArgs : EventArgs
    {
        public TorqueTestEventArgs(TorqueTest completedTest)
        {
            Test = completedTest;
        }

        /// <summary>
        ///     The torque test with an event of some interest.
        /// </summary>
        public TorqueTest Test { get; }
    }
}