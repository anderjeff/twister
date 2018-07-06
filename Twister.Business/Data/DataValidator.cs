using System;
using Twister.Business.Database;
using Twister.Business.Shared;
using Twister.Business.Tests;

namespace Twister.Business.Data
{
    public class DataValidator
    {
        /// <summary>
        ///     Determines if the workId provided is a number that has been used
        ///     for a work order.
        /// </summary>
        /// <param name="workId">The work order indentification number.</param>
        /// <param name="message">
        ///     If the work order is not found, this value will be an error message
        ///     indicating that the work order was not found. If the work order was
        ///     found, this value will be null.
        /// </param>
        /// <returns></returns>
        public bool ValidWorkOrder(string workId, out string message)
        {
            try
            {
                var workIdFound = ShopfloorWorkOrderDb.WorkIdExists(workId);
                if (workIdFound)
                    message = null;
                else
                    message = Messages.WorkOrderDoesNotExist(workId);

                return workIdFound;
            }
            catch (Exception ex)
            {
                Messages.GeneralExceptionMessage(ex, "DataValidator.ValidWorkOrder()");
                message = "";
                return false;
            }
        }

        public bool ValidEmployeeNumber(string clockNumber, out string message)
        {
            try
            {
                var employeeFound = false;
                BenchOperator bo = BenchOperatorDb.GetEmployeeById(clockNumber);
                if (bo == null)
                {
                    message = Messages.BenchOperatorNotFound(clockNumber);
                    employeeFound = false;
                }
                else
                {
                    message = null;
                    employeeFound = true;
                }

                return employeeFound;
            }
            catch (Exception ex)
            {
                Messages.GeneralExceptionMessage(ex, "DataValidator.ValidEmployeeNumber()");
                message = "";
                return false;
            }
        }

		/// <summary>
		///     Verifies that the test id has not already been used, is not null,
		///     and is not an empty value.  If the number is verified, this call
		///     will also save the _testToSave, and indicate that in the response
		///     to the caller.
		/// </summary>
		/// <param name="testId">
		///     The testId the user is trying to assign to the test that just completed.
		/// </param>
		/// <returns>
		///     If the testId is null, whitespace, or a duplicate testId, an error
		///     will be returned to the user.  If no error is detected, null will be
		///     returned
		/// </returns>
		public static string VerifyTestId(string testId)
		{
			try
			{
				// verification of number provided

				if (string.IsNullOrWhiteSpace(testId)) return Messages.BlankTorqueTestId();
				if (TorqueTestDb.InvalidTestId(testId)) return Messages.DuplicateTestId(testId);
				return null;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}