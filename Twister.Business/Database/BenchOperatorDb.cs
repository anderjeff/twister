using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Twister.Business.Shared;
using Twister.Business.Tests;

namespace Twister.Business.Database
{
    internal class BenchOperatorDb : TableBaseDb
    {
        internal static BenchOperator GetEmployeeById(string employeeId)
        {
            SqlConnection connection = null;

            try
            {
                var sp = "up_GetBenchOperatorByClockId";
                connection = TwisterConnection();
                SqlCommand command = CreateCommand(connection, sp);
                command.Parameters.Add(new SqlParameter("@ClockId", employeeId));

                BenchOperator employee = null;

                using (SqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        employee = new BenchOperator(employeeId)
                        {
                            FirstName = dr["FirstName"] as string,
                            LastName = dr["LastName"] as string
                        };

                        // only one employee possible
                        break;
                    }
                }

                command.Dispose();
                return employee;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "BenchOperatorDb.GetEmployeeById"));
                return null;
            }
            finally
            {
                CloseConnection(connection);
            }
        }
    }
}