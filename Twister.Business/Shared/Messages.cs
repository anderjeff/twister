using System;
using System.Text;
using log4net;

namespace Twister.Business.Shared
{
	/// <summary>
	///     A central location for messages for this project.
	/// </summary>
	public class Messages
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Messages));

        private static string UserThatGotMessage()
        {
            return Environment.UserName + " got a message: ";
        }

        /// <summary>
        ///     An error message that indicates that someone has tried to
        ///     start a test before initializing it.
        /// </summary>
        /// <returns></returns>
        internal static string UninitializedTest()
        {
            return "Current test has not been initialized.  Initialization must happen prior to starting.";
        }

        public static string GeneralExceptionMessage(Exception ex, string location)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("* * * EXCEPTION * * *");
            sb.AppendFormat("\t Message: {0}", ex.Message);
            sb.AppendFormat("\n\t Location: {0}", location);
            sb.AppendLine("\n- - - END EXCEPTION - - -");

            // log the exception here instead of all 22 other places.
            var msg = string.Format("General exception thrown for user {0}", Environment.UserName);
            _log.Error(msg, ex);

            return sb.ToString();
        }

        public static string BlankTorqueTestId()
        {
            var message = "Invalid test id number.  The value cannot be blank.";
            _log.Info(UserThatGotMessage() + message);

            return message;
        }

        public static string DuplicateTestId(string testId)
        {
            var message = string.Format("TestId {0} has already been used for a different test.", testId);
            _log.Info(UserThatGotMessage() + message);

            return message;
        }

        public static string DatabaseSaveUnsuccessful()
        {
            return "Unable to save the test, contact Jeff Anderson.";
        }

        public static string BenchOperatorNotFound(string clockNumber)
        {
            var message = string.Format("An employee with clock number {0} could not be located.", clockNumber);
            _log.Info(UserThatGotMessage() + message);

            return message;
        }

        public static string DatabaseSaveSuccessful()
        {
            var message = "The test was saved successfully.";
            return message;
        }

        public static string ScanBarcodeMessage()
        {
            return "Scan or type number from next barcode, the X's indicate the location of the number on the barcode.";
        }

        public static string StartButtonMessage()
        {
            return "Click the Start Button to begin";
        }

        public static string SelectDirectionMessage()
        {
            return "Select test direction.";
        }

        public static string TestCancelledMessage()
        {
            var message = "Test was cancelled by the user.";
            _log.Info(UserThatGotMessage() + message);

            return message;
        }

	    public static string WorkOrderCannotBeNullOrEmpty()
	    {
		    return "Work order cannot be an empty value.";
	    }

	    public static string ClockNumberCannotBeNullOrEmpty()
	    {
		    return "Clock number cannot be an empty value.";
	    }

		public static string WorkOrderDoesNotExist(string workId)
        {
            return string.Format("Work order {0} does not exist, please try a different number.", workId);
        }

        public static string ConfirmCloseApplication()
        {
            return "Click Again To Close Program";
        }

        public static string DisplayCloseApplication()
        {
            return "Close Program";
        }

        public static string TestFailureMessage(float percentDiffCw, float percentDiffCcw, float allowable)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("** TEST FAILED **");
            sb.AppendFormat("Angular deflection percent difference was greater than the allowable {0:P1}\r\n",
                allowable);
            sb.AppendFormat("\tCW : {0:P1}\r\n", percentDiffCw);
            sb.AppendFormat("\tCCW: {0:P1}\r\n", percentDiffCcw);
            sb.Append("Mark the test part as failed, remove and continue testing.");

            var message = sb.ToString();
            _log.Info(UserThatGotMessage() + message);

            return message;
        }

        public static string TestPassedMessage(float percentDiffCw, float percentDiffCcw, float allowable)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("** TEST PASSED!! **");
            sb.AppendFormat("Angular deflections were within the allowable {0:P1}\r\n", allowable);
            sb.AppendFormat("\tCW : {0:P1}\r\n", percentDiffCw);
            sb.AppendFormat("\tCCW: {0:P1}\r\n", percentDiffCcw);

            return sb.ToString();
        }

        public static string MissingCalibrationDataError()
        {
            return "No saved calibration exists.";
        }
    }
}