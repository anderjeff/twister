using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using Twister.Business.Tests;

namespace Twister.Business.Database
{
    internal class CalibratedPartsDb : TableBaseDb
    {
        internal static int Save(Calibration calibration)
        {
            var recordsAffected = 0;
            SqlConnection conn = null;
            SqlCommand cmd = null;

            try
            {
                conn = TwisterConnection();
                cmd = CreateCommand(conn, "up_SaveCalibratedParts");
                AddParameters(cmd, calibration);
                recordsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                // do the cleanup
                if (cmd != null)
                    cmd.Dispose();
                CloseConnection(conn);
            }
            return recordsAffected;
        }

        private static void AddParameters(SqlCommand cmd, Calibration calibration)
        {
            try
            {
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@PartNumber",
                    Value = calibration.PartNumber == null ? DBNull.Value : (object) calibration.PartNumber,
                    SqlDbType = SqlDbType.NVarChar
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@Revision",
                    Value = calibration.Revision == null ? DBNull.Value : (object) calibration.Revision,
                    SqlDbType = SqlDbType.NVarChar
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@DateCalibrated",
                    Value = calibration.DateCalibrated == null ? DateTime.Now : calibration.DateCalibrated,
                    SqlDbType = SqlDbType.NVarChar
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@CalibratedBy",
                    Value = calibration.CalibratedByClockId == null
                        ? DBNull.Value
                        : (object) calibration.CalibratedByClockId,
                    SqlDbType = SqlDbType.NVarChar
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@NominalCwDeflection",
                    Value = calibration.NominalCwDeflection == null
                        ? DBNull.Value
                        : (object) calibration.NominalCwDeflection,
                    SqlDbType = SqlDbType.Decimal
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@NominalCcwDeflection",
                    Value = calibration.NominalCcwDeflection == null
                        ? DBNull.Value
                        : (object) calibration.NominalCcwDeflection,
                    SqlDbType = SqlDbType.Decimal
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@CwTestTorque",
                    Value = calibration.CwTestTorque == null ? DBNull.Value : (object) calibration.CwTestTorque,
                    SqlDbType = SqlDbType.Int
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@CcwTestTorque",
                    Value = calibration.CcwTestTorque == null ? DBNull.Value : (object) calibration.CcwTestTorque,
                    SqlDbType = SqlDbType.Int
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@Username",
                    Value = Environment.UserName, // keep record of username they are logging in with
                    SqlDbType = SqlDbType.NVarChar
                });
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("EXCEPTION THROWN IN CalibratedPartsDb.AddParameters()");
                sb.AppendFormat("  Command Parameter Count = {0}\n", cmd.Parameters.Count);

                Debug.WriteLine(ex.ToString());
            }
        }

        internal static int Delete(Calibration calibration)
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;
            var recordsAffected = 0;

            try
            {
                conn = TwisterConnection();
                cmd = CreateCommand(conn, "up_DeleteCalibration");

                // add the two parameters needed.
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@PartNumber",
                    Value = calibration.PartNumber == null ? DBNull.Value : (object) calibration.PartNumber,
                    SqlDbType = SqlDbType.NVarChar
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@Revision",
                    Value = calibration.Revision == null ? DBNull.Value : (object) calibration.Revision,
                    SqlDbType = SqlDbType.NVarChar
                });

                recordsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                // do the cleanup
                if (cmd != null)
                    cmd.Dispose();
                CloseConnection(conn);
            }

            return recordsAffected;
        }

        internal static void Load(Calibration calibration)
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;

            try
            {
                conn = TwisterConnection();
                cmd = CreateCommand(conn, "up_LoadPreviousCalibration");

                // add the two parameters needed.
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@PartNumber",
                    Value = calibration.PartNumber == null ? DBNull.Value : (object) calibration.PartNumber,
                    SqlDbType = SqlDbType.NVarChar
                });
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@Revision",
                    Value = calibration.Revision == null ? DBNull.Value : (object) calibration.Revision,
                    SqlDbType = SqlDbType.NVarChar
                });

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        calibration.DateCalibrated = dr["DateCalibrated"] as DateTime?;
                        calibration.CalibratedByClockId = dr["CalibratedBy"] as string;
                        calibration.NominalCwDeflection = dr["NominalCwDeflection"] as decimal?;
                        calibration.NominalCcwDeflection = dr["NominalCcwDeflection"] as decimal?;
                        calibration.CwTestTorque = dr["CwTestTorque"] as int?;
                        calibration.CcwTestTorque = dr["CcwTestTorque"] as int?;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                // do the cleanup
                if (cmd != null)
                    cmd.Dispose();
                CloseConnection(conn);
            }
        }
    }
}