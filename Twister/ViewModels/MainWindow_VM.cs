using System.Collections.Generic;
using Twister.Business.Tests;

namespace Twister.ViewModels
{
    public class MainWindow_VM : Base_VM
    {
        private static MainWindow_VM _mainViewModel;

        private readonly List<Base_VM> _availableViewModels;
        private Base_VM _currentViewModel;

        private MainWindow_VM()
        {
            _availableViewModels = new List<Base_VM>();

            _availableViewModels.Add(new AvailableTests_VM());
            _availableViewModels.Add(new UserLogin_VM());
            _availableViewModels.Add(new FullyReversedTorqueTest_VM());
            _availableViewModels.Add(new UnidirectionalTorqueTest_VM());
            _availableViewModels.Add(new Calibration_VM());
			_availableViewModels.Add(new FatigueTestSetupViewModel());
			_availableViewModels.Add(new FatigueTestViewModel());
			_availableViewModels.Add(new FatigueTestSummaryViewModel());

            CurrentViewModel = _availableViewModels[0];

            TestSession = TestSession.Instance;
        }

        /// <summary>
        ///     A single shared instance of the MainWindow_VM object.
        /// </summary>
        /// <returns></returns>
        public static MainWindow_VM Instance
        {
            get
            {
                if (_mainViewModel == null)
                    _mainViewModel = new MainWindow_VM();
                return _mainViewModel;
            }
        }

        /// <summary>
        ///     The view model that hooks into the datatemplate for the current view.
        /// </summary>
        public Base_VM CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public AvailableTests_VM TestSelectionVm => _availableViewModels[0] as AvailableTests_VM;

        public UserLogin_VM UserLoginVm => _availableViewModels[1] as UserLogin_VM;

        public FullyReversedTorqueTest_VM SteeringShaftTestVm => _availableViewModels[2] as FullyReversedTorqueTest_VM;

        public UnidirectionalTorqueTest_VM UnidirectionalToFailureTestVm => _availableViewModels[3] as UnidirectionalTorqueTest_VM;

        public Calibration_VM CalibrationVm => _availableViewModels[4] as Calibration_VM;

	    public FatigueTestSetupViewModel FatigueTestSetupViewModel => _availableViewModels[5] as FatigueTestSetupViewModel;

		public FatigueTestViewModel FatigueTestViewModel => _availableViewModels[6] as FatigueTestViewModel;

		public FatigueTestSummaryViewModel FatigueTestSummaryViewModel => _availableViewModels[7] as FatigueTestSummaryViewModel;

        /// <summary>
        ///     Encapsulates the information needed for a bench operator to run
        ///     some tests on a test bench.
        /// </summary>
        public TestSession TestSession { get; }

        internal void EndTestSession()
        {
            TestBench.Singleton.InformNotReady();
        }
    }
}