using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Twister.Business.Shared;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
    public class FullyReversedTorqueTest_VM : TestBase_VM, IDisposable
    {
        // constructor
        public FullyReversedTorqueTest_VM()
        {
            // let user know what to do
            DisplayTempMessage(Messages.StartButtonMessage(), GOOD_MESSAGE_BRUSH, 0);
        }

        /// <summary>
        ///     All of the tests that have been completed.
        /// </summary>
        public ObservableCollection<TorqueTest> DisplayedCompletedTests
        {
            get
            {
                try
                {
                    return Session.CompletedTests;
                }
                catch (NullReferenceException)
                {
                    if (Session == null)
                    {
                        Debug.WriteLine("Session is null");
                        return null;
                    }
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public void Dispose()
        {
            // end threads
            _updateUiTimer.Stop();
            _updateUiTimer = null;
            _monitoringThread.Abort();
            _runningThread.Abort();

            // unsubscribbe
            TestBench.Singleton.TestStarted -= TestBench_TestStarted;
            TestBench.Singleton.TestCompleted -= TestBench_TestCompleted;

            // let the AKD know that you are done for now
            TestBench.Singleton.InformNotReady();
        }

        protected override void CleanupCurrentTest()
        {
            if (TestIdReceived)
            {
                if (TorqueAngleChart != null && TorqueAngleChart.Series["series1"].Points.Count > 0)
                {
                    TorqueTest completedTest = TestBench.Singleton.CompletedTests.LastOrDefault();
                    if (completedTest != null)
                    {
                        // add the new test to the grid.
                        Session.CompletedTests.Add(completedTest);

                        // show the start button and exit program buttons again.
                        StartTestCommand.RaiseCanExecuteChanged();
                        ExitProgamCommand.RaiseCanExecuteChanged();
                    }
                }

                //clear out the data points (except the dummy point)
                ClearPointsFromChart();

                // change it back.
                TestIdReceived = false;
            }
        }

        protected override void StartTest()
        {
            {
                if (CanStartTest())
                {
                    DisplayTempMessage("Test in process...", GOOD_MESSAGE_BRUSH, 0);

                    FullyReversedTorqueTest test = Session.TestTemplate.TestInstance() as FullyReversedTorqueTest;

                    test.Operator = Session.BenchOperator;
                    test.WorkOrder = Session.WorkId;

                    TestBench.Singleton.LoadTest(test);
                    TestBench.Singleton.BeginCurrentTest();

                    ChartFactory.CreateFullyReversedChart(TorqueAngleChart,
                        (test as FullyReversedTorqueTest).MinTorque,
                        (test as FullyReversedTorqueTest).MaxTorque);

                    ClearTestData();

                    // reevalutate all commands can execute.
                    StartTestCommand.RaiseCanExecuteChanged();
                    ExitProgamCommand.RaiseCanExecuteChanged();
                    StopTestCommand.RaiseCanExecuteChanged();
                }
            }
        }

        protected override void ScaleChartAxis(float currentTorque, float currentAngle)
        {
            // scale the chart to show the ranges desired
        }

        internal override void CreateDefaultChart()
        {
            ChartFactory.CreateFullyReversedChart(TorqueAngleChart, -4000, 4000);
        }
    }
}