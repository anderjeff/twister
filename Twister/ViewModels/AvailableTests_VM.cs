using System;
using System.Diagnostics;
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
				InitializeSimulatedTestBench(TestType.FatigueTest);
		    }
		    else
		    {
				InitializeRealTestBench();
			}
		    MainWindow_VM.Instance.TestSession.Initialize(TestType.FatigueTest);
		    MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.FatigueTestVm;
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
					InitializeRealTestBench();
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
					InitializeRealTestBench();
		            MainWindow_VM.Instance.TestSession.Initialize(TestType.SteeringShaftTest_4000_inlbs);
		            MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.UserLoginVm;
	            }
			}
            catch (Exception ex)
            {
                MainVmMessage = ex.Message;
            }
        }

	    private void InitializeRealTestBench()
	    {
		    var torqueCell = new AnalogInputDevice(new ModbusServer());
		    var servoDrive = new ServoDrive(new ModbusServer());
		    TestBench.Initialize(torqueCell, servoDrive);
	    }

	    private void InitializeSimulatedTestBench(TestType testType)
	    {
			// get the behavior for the simulators, based on test type.
			// todo implement some type of class that takes TestType and helps torque cell and servo drive to figure out the proper behavior
			var engine = new SimulatorEngine(new Stopwatch());
			var torqueCell = new SimulatedTorqueCell(engine);
		    var servoDrive = new SimulatedServoDrive(engine);
			TestBench.Initialize(torqueCell, servoDrive);
	    }

		private bool IsAuthorizedUser()
        {
            return Environment.UserName == "janderson" ||
				   Environment.UserName == "Jeff" ||
                   Environment.UserName == "tkikkert" ||
                   Environment.UserName == "nwalton" ||
                   Environment.UserName == "jpbeierle";
        }
    }
}