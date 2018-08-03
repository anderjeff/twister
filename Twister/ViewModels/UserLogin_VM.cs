using System;
using System.Windows.Threading;
using log4net;
using Twister.Business.Data;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
    public class UserLogin_VM : Base_VM
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(UserLogin_VM));
        private string _clockNumber;
        private string _errorMessage;

        private string _workOrder;


        public UserLogin_VM()
        {
            EnterInfoConfirmationCommand = new RelayCommand(ConfirmTestInfo);
            SelectTestCommand = new RelayCommand(SelectTest);
        }

        /// <summary>
        ///     The work order number for the item being tested.
        /// </summary>
        public string WorkOrder
        {
            get { return _workOrder; }
            set
            {
                if (_workOrder != value)
                {
                    _workOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The clock number of the employee running the test.
        /// </summary>
        public string ClockNumber
        {
            get { return _clockNumber; }
            set
            {
                if (_clockNumber != value)
                {
                    _clockNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     A message for the user in the event of invalid data.
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();

                    if (ErrorMessage != null)
                        ResetMessage();
                }
            }
        }

        /// <summary>
        ///     Command sent by the user to begin the test.
        /// </summary>
        public RelayCommand EnterInfoConfirmationCommand { get; }

        /// <summary>
        ///     Command sent by the user to begin the test.
        /// </summary>
        public RelayCommand SelectTestCommand { get; }

        /// <summary>
        ///     alerts the caller taht the validation of work order and bench operator
        ///     have succeeded.
        /// </summary>
        public EventHandler LoginValidationPassed { get; private set; }

        private void ResetMessage()
        {
            // create dispatcher timer to fire once to set ErrorMessage back to null
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = new TimeSpan(0, 0, 2);
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            ErrorMessage = null;
        }

        private void ConfirmTestInfo()
        {
            try
            {
                if (PassedValidation())
                {
                    // finish up initializing the TestSession
                    BenchOperator currentOperator = new BenchOperator(ClockNumber);
                    currentOperator.GetName();
                    MainWindow_VM.Instance.TestSession.BenchOperator = currentOperator;
                    MainWindow_VM.Instance.TestSession.WorkId = WorkOrder;

                    if (MainWindow_VM.Instance.TestSession.TestTemplate.Id == (int) TestType.TorsionTestToFailure)
                    {
                        MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.UnidirectionalToFailureTestVm;
	                    var vm = (UnidirectionalTorqueTest_VM) MainWindow_VM.Instance.CurrentViewModel;

                        // starts up the test monitoring.
                        vm.PrepareToTest(MainWindow_VM.Instance.TestSession);
                    }
                    else if (MainWindow_VM.Instance.TestSession.TestTemplate.Id == (int) TestType.SteeringShaftTest_4000_inlbs)
                    {
                        // if this is a brand new part number with no calibration information, this shaft will need to be calibrated.
                        WorkOrderInfo woInfo = new WorkOrderInfo(WorkOrder);
                        woInfo.Load();

                        if (HasBeenCalibrated(woInfo.PartNumber, woInfo.Revision))
                        {
                            MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.SteeringShaftTestVm;
	                        var vm = (FullyReversedTorqueTest_VM) MainWindow_VM.Instance.CurrentViewModel;

                            // starts up the test monitoring.
                            vm.PrepareToTest(MainWindow_VM.Instance.TestSession);
                        }
                        else
                        {
							MainWindow_VM.Instance.CalibrationVm.PartNumber = woInfo.PartNumber;
                            MainWindow_VM.Instance.CalibrationVm.Revision = woInfo.Revision;

                            MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.CalibrationVm;
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid test type, must select a valid test type to continue.");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                ErrorMessage = ex.Message;
            }
        }

        public bool HasBeenCalibrated(string partNumber, string revision)
        {
            Calibration calibration = new Calibration(partNumber, revision);
            calibration.Load();

            return calibration.DateCalibrated != null;
        }


        private void SelectTest()
        {
            MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.TestSelectionVm;
        }

        private bool PassedValidation()
        {
            DataValidator validator = new DataValidator();

            var message = "";
            if (!validator.ValidWorkOrder(WorkOrder, out message))
            {
                ErrorMessage = message;
                return false;
            }
            if (!validator.ValidEmployeeNumber(ClockNumber, out message))
            {
                ErrorMessage = message;
                return false;
            }
            return true;
        }

        private void OnLoginValidationPassed()
        {
            EventHandler handler = LoginValidationPassed;
            if (handler != null)
                LoginValidationPassed(this, EventArgs.Empty);
        }
    }
}