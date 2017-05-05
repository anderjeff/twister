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
    }
}