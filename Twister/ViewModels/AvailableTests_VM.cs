using System;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
    public class AvailableTests_VM : Base_VM
    {
        public AvailableTests_VM()
        {
            SteeringShaftTestCommand = new RelayCommand(SteeringShaftTest);
            TorsionTestCommand = new RelayCommand(TorsionTest, CanRunFailureTest);
        }

        public RelayCommand SteeringShaftTestCommand { get; }
        public RelayCommand TorsionTestCommand { get; }

        // want to run a torsion test to failure
        private void TorsionTest()
        {
            try
            {
                MainWindow_VM.Instance.TestSession.Initialize(
                    TestType.TorsionTestToFailure);
                MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.UserLoginVm;
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
            if (IsAuthorizedUser()) return true;
            return false;
        }

        // want to run the normal steering shaft test.
        private void SteeringShaftTest()
        {
            try
            {
                MainWindow_VM.Instance.TestSession.Initialize(
                    TestType.SteeringShaftTest_4000_inlbs);
                MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.UserLoginVm;
            }
            catch (Exception ex)
            {
                MainVmMessage = ex.Message;
            }
        }

        private bool IsAuthorizedUser()
        {
            return Environment.UserName == "janderson" ||
                   Environment.UserName == "tkikkert" ||
                   Environment.UserName == "nwalton" ||
                   Environment.UserName == "vforrester" ||
                   Environment.UserName == "jpbeierle";
        }
    }
}