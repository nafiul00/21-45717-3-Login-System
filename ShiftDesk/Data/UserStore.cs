using System;
using System.Configuration;
using System.Data.SqlClient;
using ShiftDesk.Security;

namespace ShiftDesk.Data
{
    /// <summary>
    /// Every SQL statement in this application lives in this one file.
    ///
    /// The forms call the three methods below and never see a connection, a
    /// command or a password hash. Two things fall out of that:
    ///
    ///   1. The connection string is read from App.config in exactly one place,
    ///      so there is no second hardcoded copy to drift out of date.
    ///   2. Hashing happens on both the login path and the registration path
    ///      because both paths run through here. It is not something a form
    ///      can forget to do.
    /// </summary>
    internal static class UserStore
    {
        /// <summary>
        /// Read once, from App.config, the first time this class is used.
        /// Nothing in the project hardcodes a server name.
        /// </summary>
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["connString"].ConnectionString;

        /// <summary>
        /// Opens a connection, then closes it again, to answer one question up
        /// front: can this machine reach the database at all?
        ///
        /// Without this the first sign of a bad connection string is a failed
        /// sign-in, which looks exactly like a wrong password. The sign-in
        /// screen calls this when it opens and says which it is.
        ///
        /// Returns null when the connection worked, or the reason it did not.
        /// </summary>
        internal static string DescribeConnectionProblem()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// True when the username exists and its stored hash matches the hash
        /// of the supplied password. COUNT(*) comes back as a single number,
        /// which is why ExecuteScalar is the right call here.
        /// </summary>
        internal static bool CredentialsAreValid(string username, string password)
        {
            const string sql = "SELECT COUNT(*) FROM tbl_users " +
                               "WHERE username = @username AND password = @password";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                // The command text above is fixed. Whatever the user typed
                // travels separately, as a value, and is never parsed as SQL.
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        /// <summary>
        /// Used by registration to give a helpful message before attempting the
        /// insert. The UNIQUE constraint on the column is the real guarantee -
        /// this is only the polite version of it.
        /// </summary>
        internal static bool UsernameIsTaken(string username)
        {
            const string sql = "SELECT COUNT(*) FROM tbl_users WHERE username = @username";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@username", username);

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        /// <summary>
        /// Writes the new account. ExecuteNonQuery is used because an INSERT
        /// returns a count of affected rows rather than a result set.
        /// </summary>
        internal static void CreateUser(string username, string password)
        {
            const string sql = "INSERT INTO tbl_users (username, password) " +
                               "VALUES (@username, @password)";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
