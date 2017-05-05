using System;
using System.Collections.ObjectModel;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    /// <summary>
    ///     Represents a bench operator running one or more identical tests for
    ///     the same work order.  Used to encapsulate the data needed for historical
    ///     purposes.
    /// </summary>
    public class TestSession : NotifyPropertyChangedBase
    {
        private static TestSession _testSession;

        private TestSession()
        {
        }

        /// <summary>
        ///     A single shared instance of a TestSession.
        /// </summary>
        /// <returns>A single shared instance of a TestSession.</returns>
        public static TestSession Instance
        {
            get
            {
                if (_testSession == null) _testSession = new TestSession();
                return _testSession;
            }
        }

        /// <summary>
        ///     The work order number this test is linked back to.
        /// </summary>
        public string WorkId { get; set; }

        /// <summary>
        ///     The PSS employee running the test.
        /// </summary>
        public BenchOperator BenchOperator { get; set; }

        /// <summary>
        ///     The test the bench operator is going to attempt to run this session.
        /// </summary>
        public TestTemplate TestTemplate { get; private set; }

        /// <summary>
        ///     All of the tests completed this session.
        /// </summary>
        public ObservableCollection<TorqueTest> CompletedTests { get; set; }


        /// <summary>
        ///     Creates an instance of TestTemplate, hooks up a test to run based off that.
        /// </summary>
        public void Initialize(TestType desiredTest)
        {
            try
            {
                TestTemplate = new TestTemplate(desiredTest);
                TestTemplate.Load();

                CompletedTests = new ObservableCollection<TorqueTest>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}