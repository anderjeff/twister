using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Twister.Business.Tests;
using Msg = Twister.Business.Shared.Messages;


namespace Twister.Business.Database
{
    internal class TestTemplateDb : TableBaseDb
    {
        /// <summary>
        ///     Populate a template that already has the TemplateId property determined.
        /// </summary>
        /// <param name="template">A completed TestTemplate.</param>
        internal static void LoadTemplateThatHasId(TestTemplate template)
        {
            SqlConnection conn = null;
            try
            {
                var sel = "up_LoadTestSettings";
                conn = TwisterConnection();

                SqlCommand cmd = CreateCommand(conn, sel);
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@templateId",
                    Value = template.Id
                });

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        template.Description = dr["TestDescription"] as string;
                        template.ClockwiseTorque = (int) dr["ClockwiseTorque"];
                        template.CounterclockwiseTorque = (int) dr["CounterclockwiseTorque"];
                        template.RunSpeed = (int) dr["RunSpeed"];
                        template.MoveSpeed = (int) dr["MoveSpeed"];
                    }
                }

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "SaveToDatabase"));
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        internal static void UpdateSpeedSettings(int runSpeed, int moveSpeed, int testId)
        {
            SqlConnection conn = null;
            try
            {
                var sel = "up_SaveTestSettings";
                conn = TwisterConnection();

                SqlCommand cmd = CreateCommand(conn, sel);
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@runSpeed",
                    Value = runSpeed
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@moveSpeed",
                    Value = moveSpeed
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@testId",
                    Value = testId
                });

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected != 1)
                {
                    string message = $"Error saving speed settings. Expected 1 update using " +
                                     $"run speed = {runSpeed} and move speed = {moveSpeed}.";
                    throw new Exception(message);
                }

                cmd.Dispose();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "SaveToDatabase"));
            }
            finally
            {
                CloseConnection(conn);
            }
        }
    }
}