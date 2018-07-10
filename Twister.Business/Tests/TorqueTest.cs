using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Twister.Business.Data;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    public abstract class TorqueTest
    {
        /// <summary>
        ///     The data that the test is generating.
        /// </summary>
        internal ObservableCollection<Sample> Data;

        public TorqueTest()
        {
            Data = new ObservableCollection<Sample>();
        }

        public int MinTorque { get; set; }
        public int MaxTorque { get; set; }

        /// <summary>
        ///     A unique identifier for the test, linking it to a type of Torque
        ///     Test defined elsewhere.
        /// </summary>
        public int TestTemplateId { get; set; }

        /// <summary>
        ///     The name of the person running the test (clock number)
        /// </summary>
        public BenchOperator Operator { get; set; }

        /// <summary>
        ///     A unique identifier for this test.
        /// </summary>
        public string TestId { get; internal set; }

        /// <summary>
        ///     The work order id, used to tie the test back to the
        ///     job the test was created for
        /// </summary>
        public string WorkOrder { get; set; }

        /// <summary>
        ///     The time of day the test finished.
        /// </summary>
        public TimeSpan FinishTime { get; set; }

        /// <summary>
        ///     The time of day the test started.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        ///     The date this test was ran.
        /// </summary>
        public DateTime? StartDate { get; internal set; }

        /// <summary>
        ///     Used by UI, do not remove.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                TimeSpan difference = FinishTime.Subtract(StartTime);
                return difference;
            }
        }

        public int Samples => Data.Count;

        /// <summary>
        ///     Gets a deep copy of the test data.
        /// </summary>
        public List<Sample> CopyOfData
        {
            get
            {
                var copy = new List<Sample>();
                if (Data == null)
                {
                    return null;
                }
                foreach (Sample s in Data)
                    copy.Add(s.Clone() as Sample);
                return copy;
            }
        }

        /// <summary>
        ///     Indicates if the test is in process, true indicates
        ///     the test is in process.
        /// </summary>
        internal bool InProcess { get; set; }

        /// <summary>
        ///     Indicates if the test has been initialized.
        /// </summary>
        internal bool Initialized { get; set; }

        /// <summary>
        ///     Indicates if the test data for this test should be saved.
        /// </summary>
        public bool ShouldBeSaved { get; set; }

        /// <summary>
        ///     Indicates if the bench operator prematurely shut down the
        ///     test, as in the case of an E-Stop.  Also could indicate
        ///     that the safety rules were violated.
        /// </summary>
        public bool WasShutDownEarly { get; set; }


        /// <summary>
        ///     Sets the test variables from a TestTemplate.  Each test can decide
        ///     how to use the TestTemplate.
        /// </summary>
        /// <param name="testTemplate"></param>
        public abstract void LoadTestParameters(TestTemplate testTemplate);

        protected void InformInitializationComplete()
        {
            // at this point, the test has been initialized and it is free to start.
	        Initialized = true;
	        TestBench.Singleton.VerifyAlive();
        }

        /// <summary>
        ///     Begins the TorqueTest.
        /// </summary>
        internal void Start()
        {
			// it's important to initialize the test first.
	        if (!Initialized) throw new Exception(Messages.UninitializedTest());
	        InProcess = true;
		}
    }
}