using System;
using System.Diagnostics;
using System.Windows.Threading;
using Twister.Business.Tests;
using Twister.Utilities;
using Twister.Business.Shared;

namespace Twister.ViewModels
{
    public class Calibration_VM : Base_VM
    {
        private bool _calibrationCanBegin = true;

        private bool _canGoBack = true;

        private string _message;

        public Calibration_VM()
        {
            PreviousScreenCommand = new RelayCommand(GoBackToPreviousScreen, CanGoBack);
            RunCalibrationCommand = new RelayCommand(RunCalibration, CalibrationCanBegin);

            // so binding doesn't break
            TorqueTestVm = new FullyReversedTorqueTest_VM();
        }

        public Calibration CurrentCalibration { get; } = new Calibration();

        public string PartNumber
        {
            get { return CurrentCalibration.PartNumber; }
            set
            {
                if (CurrentCalibration.PartNumber != value)
                {
                    CurrentCalibration.PartNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Revision
        {
            get { return CurrentCalibration.Revision; }
            set
            {
                if (CurrentCalibration.Revision != value)
                {
                    CurrentCalibration.Revision = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal? CwValue
        {
            get { return CurrentCalibration.NominalCwDeflection; }
            set
            {
                // don't check for != value here, this is only set once
                CurrentCalibration.NominalCwDeflection = value;
                OnPropertyChanged();
            }
        }

        public decimal? CcwValue
        {
            get { return CurrentCalibration.NominalCcwDeflection; }
            set
            {
                // don't check for != value here, this is only set once
                CurrentCalibration.NominalCcwDeflection = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand PreviousScreenCommand { get; }
        public RelayCommand RunCalibrationCommand { get; }
        public FullyReversedTorqueTest_VM TorqueTestVm { get; }

        private bool CanGoBack()
        {
            return _canGoBack;
        }

        private bool CalibrationCanBegin()
        {
            return _calibrationCanBegin;
        }


        private void GoBackToPreviousScreen()
        {
            // return to the user login page
            MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.UserLoginVm;
        }

        private void RunCalibration()
        {
            _calibrationCanBegin = false;
            RunCalibrationCommand.RaiseCanExecuteChanged();
            _canGoBack = false;
            PreviousScreenCommand.RaiseCanExecuteChanged();
            Message = "Calibration in process...";

            // save calibration to server.
            TorqueTestVm.PrepareToTest(MainWindow_VM.Instance.TestSession);
            TorqueTestVm.StartTestCommand.Execute(new FullyReversedTorqueTest());

            TestBench.Singleton.TestCompleted += Singleton_TestCompleted;
        }

        private void Singleton_TestCompleted(object sender, TorqueTestEventArgs e)
        {
            try
            {
                FullyReversedTorqueTest completedTest = e.Test as FullyReversedTorqueTest;

                // normalize the data to the min and max torque, so it's easy to compare to actual 
                // test results.
                CurrentCalibration.CalculateCalibrationValues(completedTest.CopyOfData,
                    completedTest.MaxTorque, completedTest.MinTorque);

                // update UI
                CwValue = CurrentCalibration.NominalCwDeflection;
                CcwValue = CurrentCalibration.NominalCcwDeflection;

                CurrentCalibration.CalibratedByClockId = MainWindow_VM.Instance.TestSession.BenchOperator.ClockId;
                int recordsAffected = CurrentCalibration.Save();

                // so the calibration is enabled again.
                _calibrationCanBegin = true;
                RunCalibrationCommand.RaiseCanExecuteChanged();
                Message = "Calibration completed. Going to test panel shortly.";

                DispatcherTimer t = new DispatcherTimer();
                t.Interval = new TimeSpan(0, 0, 0, 5);
                t.Tick += T_Tick;
                t.Start();
            }
            catch (Exception ex)
            {
                ExceptionHandler.WriteToEventLog(ex);
            }
        }

        // wait a few seconds after the test is complete to run this.
        private void T_Tick(object sender, EventArgs e)
        {
            try
            {
                //clean up the TorqueTestVm.
                TorqueTestVm.Dispose();

                // start up the test UI.
                MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.SteeringShaftTestVm;
                FullyReversedTorqueTest_VM vm = MainWindow_VM.Instance.CurrentViewModel as FullyReversedTorqueTest_VM;

                // set these so the test screen shows up normal again.
                vm.IsTestIdNeeded = false;
                vm.DisplayTempMessage(Business.Shared.Messages.StartButtonMessage(), vm.GOOD_MESSAGE_BRUSH, 0);

                // starts up the test monitoring.
                vm.PrepareToTest(MainWindow_VM.Instance.TestSession);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}