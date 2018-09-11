using System;
using System.Data.SqlClient;
using log4net;
using Twister.Business.Data;

namespace Twister.Business.Database
{
    internal class ShopfloorWorkOrderDb : TableBaseDb
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ShopfloorWorkOrderDb));

        internal static void GetWorkOrderInfo(WorkOrderInfo workOrderInfo)
        {
            SqlConnection connection = null;

            try
            {
                var sp = "up_PartNumberFromWorkOrder";
                connection = VjsConnection();
                SqlCommand command = CreateCommand(connection, sp);
                command.Parameters.Add(new SqlParameter("@WorkId", workOrderInfo.WorkId));

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        workOrderInfo.PartNumber = reader["PartNumber"] as string;
                        workOrderInfo.Revision = reader["Revision"] as string;
                    }
                }

                command.Dispose();
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(ex.ToString());
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        internal static bool WorkIdExists(string workId)
        {
            SqlConnection connection = null;

            try
            {
                var sp = "up_ValidWorkOrder";
                connection = VjsConnection();
                SqlCommand command = CreateCommand(connection, sp);
                command.Parameters.Add(new SqlParameter("@WorkId", workId));

                var workOrderExists = false;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    workOrderExists = reader.HasRows;
                }

                command.Dispose();
                return workOrderExists;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(ex.ToString());
                return false;
            }
            finally
            {
                CloseConnection(connection);
            }
        }
    }
}