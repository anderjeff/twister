using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using Msg = Twister.Business.Shared.Messages;

namespace Twister.Business.Database
{
    internal class TableBaseDb
    {
        private static readonly string vjsPath = @"G:\Engineering\Programs\Shared\Connections\vjs.txt";
        private static readonly string twisterPath = @"G:\Engineering\Programs\Shared\Connections\Twister2015.txt";

        private static SqlConnection vjsConn;
        private static SqlConnection twisterConn;

        /// <summary>
        ///     Establishes an open connection to the Twister database.
        /// </summary>
        /// <returns>
        ///     If the connection is made, it returns an open
        ///     connection to the Twister database.  If an exception
        ///     occurs, it returns null.
        /// </returns>
        internal static SqlConnection TwisterConnection()
        {
			try
			{
				twisterConn = new SqlConnection();
				twisterConn.ConnectionString = File.Exists(twisterPath) ? TwisterConnectionString() : ConfigurationManager.ConnectionStrings["Twister"].ConnectionString;
				twisterConn.Open();
                return twisterConn;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "TwisterConnection"));
                return null;
            }
        }

        /// <summary>
        ///     Establishes a connection to the visual jobshop database.
        /// </summary>
        /// <returns>
        ///     If the connection is made, it returns an open
        ///     connection to the visual jobshop database.  If an exception
        ///     occurs, it returns null.
        /// </returns>
        protected static SqlConnection VjsConnection()
        {
            try
            {
                vjsConn = new SqlConnection();
                vjsConn.ConnectionString = File.Exists(vjsPath) ? VjsConnectionString() : ConfigurationManager.ConnectionStrings["Vjs"].ConnectionString;
				vjsConn.Open();
                return vjsConn;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "VjsConnection"));
                return null;
            }
        }

        private static string VjsConnectionString()
        {
            try
            {
                using (StreamReader sr = new StreamReader(vjsPath))
                {
                    return sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private static string TwisterConnectionString()
        {
            try
            {
                using (StreamReader sr = new StreamReader(twisterPath))
                {
                    return sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "TwisterConnectionString"));
                throw ex;
            }
        }

        internal static SqlCommand CreateCommand(SqlConnection conn, string storedProcedure)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = storedProcedure;
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;

                return cmd;
            }
            catch (InvalidOperationException opexcep)
            {
                Debug.WriteLine(Msg.GeneralExceptionMessage(opexcep, "CreateCommand"));
                return null;
            }
        }

        /// <summary>
        ///     Close an open, non-null SqlConnection.
        /// </summary>
        /// <param name="conn">
        ///     An open, non-null connection.  The method will verify both conditions
        ///     before attempting to close the connection.
        /// </param>
        internal static void CloseConnection(SqlConnection conn)
        {
            if (conn != null &&
                conn.State == ConnectionState.Open)
                try
                {
                    if (conn != null)
                        conn.Close();
                }
                catch (SqlException ex)
                {
                    Debug.WriteLine(Msg.GeneralExceptionMessage(ex, "CloseConnection"));
                }
        }
    }
}