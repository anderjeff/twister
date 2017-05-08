using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Transactions;
using Twister.Business.Data;
using Twister.Business.Tests;
using IsolationLevel = System.Transactions.IsolationLevel;
using Msg = Twister.Business.Shared.Messages;

namespace Twister.Business.Database
{
    internal class TorqueTestDb : TableBaseDb
    {
        internal static int SaveToDatabase(TorqueTest test)
        {
            SqlConnection conn = null;
            try
            {
                var rowsAffected = 0;
                var sel = "up_SaveTwisterTest";
                conn = TwisterConnection();

                SqlCommand cmd = CreateCommand(conn, sel);
                SetTorqeTestParameters(cmd, test);

                TransactionOptions options = new TransactionOptions();
                /* It allow shared locks and read only committed data. That means 
                   never read changed data that are in the middle of any transaction.
                   */
                options.IsolationLevel = IsolationLevel.ReadCommitted;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
                {
                    // save the test.
                    rowsAffected += cmd.ExecuteNonQuery();

                    var spDtPt = "up_SaveDataPoint";
                    cmd.Dispose();
                    cmd = CreateCommand(conn, spDtPt);

                    // save each data point.
                    foreach (Sample dataPoint in test.Data)
                    {
                        if (cmd.Parameters.Count == 0)
                            SetTestSampleParameters(cmd, dataPoint, test.TestId);
                        else
                            UpdateTestSampleParams(cmd, dataPoint);

                        // increment number of rows affected.
                        rowsAffected += cmd.ExecuteNonQuery();
                    }

                    scope.Complete();
                }

                cmd.Dispose();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "SaveToDatabase"));
                return 0;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        private static void UpdateTestSampleParams(SqlCommand cmd, Sample dataPoint)
        {
            try
            {
                cmd.Parameters["@SampleTime"].Value = dataPoint.SampleTime;
                cmd.Parameters["@Torque"].Value = dataPoint.Torque;
                cmd.Parameters["@Angle"].Value = dataPoint.Angle;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "UpdateTestSampleParams"));
                throw;
            }
        }

        /// <summary>
        ///     Determines if the testId has already been used.
        /// </summary>
        /// <param name="testId">
        ///     A unique identifier for the test.
        /// </param>
        /// <returns>
        /// </returns>
        internal static bool InvalidTestId(string testId)
        {
            try
            {
                TorqueTest testInDb = GetTestById(testId);
                var oldTestFound = testInDb != null;

                if (oldTestFound) return true;
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///     For a given testId, this method will get you a TorqueTest from
        ///     the Twister database.
        /// </summary>
        /// <param name="testId">
        ///     The TestId for the test performed.
        /// </param>
        /// <returns>
        ///     A TorqueTest with properties populated from the database.  If
        ///     the record is not found in the database, return null.
        /// </returns>
        private static TorqueTest GetTestById(string testId)
        {
            SqlConnection conn = null;
            FullyReversedTorqueTest existingTest = null;
            var sel = "up_GetTestById";

            try
            {
                conn = TwisterConnection();

                SqlCommand cmd = CreateCommand(conn, sel);
                cmd.Parameters.AddWithValue("@testId", testId);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        existingTest = new FullyReversedTorqueTest();
                        existingTest.TestId = dr["TestId"] as string;
                        existingTest.TestTemplateId = (int) dr["TestTemplateId"];

                        // get the employee than ran the test, the class loads the rest.
                        BenchOperator employee = new BenchOperator(dr["EmployeeNumber"] as string);

                        existingTest.WorkOrder = dr["WorkId"] as string;

                        if (dr["TestDate"] == DBNull.Value)
                            existingTest.StartDate = null;
                        else
                            existingTest.StartDate = (DateTime) dr["TestDate"];

                        if (dr["StartTime"] != DBNull.Value)
                            existingTest.StartTime = (TimeSpan) dr["StartTime"];
                        if (dr["FinishTime"] != DBNull.Value)
                            existingTest.FinishTime = (TimeSpan) dr["FinishTime"];
                    }
                }
                cmd.Dispose();

                return existingTest;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        internal static void SetTorqeTestParameters(SqlCommand cmd, TorqueTest torqueTest)
        {
            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@TestId",
                SqlDbType = SqlDbType.NVarChar,
                Value = torqueTest.TestId
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@TestTemplateId",
                SqlDbType = SqlDbType.Int,
                Value = torqueTest.TestTemplateId
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@EmployeeNumber",
                SqlDbType = SqlDbType.NVarChar,
                Value = torqueTest.Operator.ClockId
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@WorkId",
                SqlDbType = SqlDbType.NVarChar,
                Value = torqueTest.WorkOrder
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@TestDate",
                SqlDbType = SqlDbType.Date,
                Value = torqueTest.StartDate.Value.Date
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@StartTime",
                SqlDbType = SqlDbType.Time,
                Value = torqueTest.StartTime
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@FinishTime",
                SqlDbType = SqlDbType.Time,
                Value = torqueTest.FinishTime
            });

            // now see if it's a unidirectional test, so the direction must be know.
            var direction = "BD"; // both directions is default, see TestDirections table.
            UnidirectionalTorqueTest test = torqueTest as UnidirectionalTorqueTest;
            if (test != null)
                if (test.Direction == TestDirection.Clockwise)
                    direction = "CW";
                else if (test.Direction == TestDirection.Counterclockwise)
                    direction = "CCW";

            // now add the parameter
            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@Direction",
                SqlDbType = SqlDbType.NVarChar,
                Precision = 5,
                Value = direction
            });
        }

        internal static void SetTestSampleParameters(SqlCommand cmd, Sample sample, string testId)
        {
            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@TestId",
                SqlDbType = SqlDbType.NVarChar,
                Value = testId
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@SampleTime",
                SqlDbType = SqlDbType.Time,
                Value = sample.SampleTime
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@Torque",
                SqlDbType = SqlDbType.Int,
                Value = sample.Torque
            });

            cmd.Parameters.Add(new SqlParameter
            {
                ParameterName = "@Angle",
                SqlDbType = SqlDbType.Decimal,
                Value = sample.Angle
            });
        }
    }
}