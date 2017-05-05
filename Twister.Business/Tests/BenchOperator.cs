using System;
using System.Diagnostics;
using Twister.Business.Database;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    /// <summary>
    ///     The PSS employee that is running the test.
    /// </summary>
    public class BenchOperator
    {
        public BenchOperator(string clockId)
        {
            ClockId = clockId;
        }

        public string LastName { get; internal set; }

        public string FirstName { get; internal set; }

        public string ClockId { get; }

        public override string ToString()
        {
            return string.Format("{0} {1} [{2}]", FirstName, LastName, ClockId);
        }

        /// <summary>
        ///     Loads saved info about the BenchOperator from disk.
        /// </summary>
        public void GetName()
        {
            try
            {
                BenchOperator b = BenchOperatorDb.GetEmployeeById(ClockId);
                FirstName = b.FirstName;
                LastName = b.LastName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "BenchOperator.GetName()"));
            }
        }
    }
}