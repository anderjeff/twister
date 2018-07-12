using System;
using System.Data.SqlClient;

namespace Twister.Business.Database
{
    internal class ShopfloorEmployeeDb : TableBaseDb
    {
        internal static bool EmployeeExists(string employeeId)
        {
            SqlConnection connection = null;

            try
            {
                var sp = "up_ValidEmployee";
                connection = VjsConnection();
                SqlCommand command = CreateCommand(connection, sp);
                command.Parameters.Add(new SqlParameter("@EmployeeId", employeeId));

                var workOrderExists = false;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    workOrderExists = reader.HasRows;
                }

                command.Dispose();
                return workOrderExists;
            }
            finally
            {
                CloseConnection(connection);
            }
        }
    }
}