using System;
using Twister.Business.Hardware;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
    public class AvailableTests_VM : Base_VM
    {
	    private bool _simulatorIsUsed;

	    public AvailableTests_VM()
        {
            SteeringShaftTestCommand = new RelayCommand(SteeringShaftTest);
            TorsionTestCommand = new RelayCommand(TorsionTest, CanRunFailureTest);
	        FatigueTestCommand = new RelayCommand(FatigueTest);
        }
	   
	    public RelayCommand SteeringShaftTestCommand { get; }
        public RelayCommand TorsionTestCommand { get; }
        public RelayCommand FatigueTestCommand { get; }

		/// <summary>
		/// Indicates if the test simulator will be used.
		/// </summary>
		public bool SimulatorIsUsed
	    {
		    get { return _simulatorIsUsed; }
		    set
		    {
			    if (_simulatorIsUsed != value)
			    {
				    _simulatorIsUsed = value;
					OnPropertyChanged();
			    }
		    }
	    }

	    private void FatigueTest()
	    {
		    if (SimulatorIsUsed)
		    {
				// go to the test creation window using the test simulator.
			    TestBench.Initialize(new AnalogInputDevice(""), new ServoDrive(""));
		    }
		    else
		    {
				// go to the test creation window using the real test bench.
		    }
	    }

		// want to run a torsion test to failure
		private void TorsionTest()
        {
            try
            {
	            if (SimulatorIsUsed)
	            {
		            MainVmMessage = "Simulator not supported for the torsion test.";
		            SimulatorIsUsed = false;
	            }
				else
	            {
		            MainWindow_VM.Instance.TestSession.Initialize(TestType.TorsionTestToFailure);
					MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.UserLoginVm;
	            }
			}
            catch (Exception ex)
            {
                MainVmMessage = ex.Message;
            }
        }

        /// <summary>
        ///     Only give certain users access to running this test.
        /// </summary>
        /// <returns></returns>
        private bool CanRunFailureTest()
        {
	        return IsAuthorizedUser();
        }

        // want to run the normal steering shaft test.
        private void SteeringShaftTest()
        {
            try
            {
	            if (SimulatorIsUsed)
	            {
		            MainVmMessage = "Simulator not supported for the steeering shaft test.";
		            SimulatorIsUsed = false;
	            }
				else
	            {
		            MainWindow_VM.Instance.TestSession.Initialize(TestType.SteeringShaftTest_4000_inlbs);
		            MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.UserLoginVm;
	            }
			}
            catch (Exception ex)
            {
                MainVmMessage = ex.Message;
            }
        }

        private bool IsAuthorizedUser()
        {
            return Environment.UserName == "janderson" ||
				   Environment.UserName == "Jeff" ||
                   Environment.UserName == "tkikkert" ||
                   Environment.UserName == "nwalton" ||
                   Environment.UserName == "vforrester" ||
                   Environment.UserName == "jpbeierle";
        }
    }
}