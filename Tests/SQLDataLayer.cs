#region Copyright Pyrotek-Inc. 2009
/*
All rights are reserved.  Reproduction or transmission in whole or in part, 
in any form or by any means, electronic, mechanical or otherwise, is prohibited
without the prior written consent of the copyright owner.

Filename: SQLDataLayer.cs
*/
#endregion
using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Threading.Tasks;
using System.Data.Common;

#region Update history
//Created by: Alejandro Palacios
//Creation date: Jan 02, 2009
//Updates:
//  added method ExecuteSQL to run queries
//  added capability of handling SQLDBType.Text appropriately
//  added BeginTransaction overloading method that receives transaction isolation level
//  (May, 29, 2013) Alejandro Palacios added Disposable(bool) method to dispose appropriately
//  (May, 29, 2013) Alejandro Palacios modify Double.Nan to Double.IsNaN
//Comments/Suggestions: alepal@pyrotek-inc.com
#endregion

namespace Tests
{
    public class SQLDataLayer : IDisposable
    {
        private object _con_trans_lock = new Object();	// for synronizing transaction object access
        SqlConnection _con = null;
        private SqlTransaction _con_trans = null;
        private static string[] _rc_msg = null;				// store all DataLayer return code messages (0-100)
        private int _last_return_code;
        private int _last_execution_time;
        /// <summary>
        /// Static constructor.
        /// </summary>
        static SQLDataLayer()
        {
            // DataLayer return code messages (0-100)
            _rc_msg = new string[101];
            _rc_msg[0] = "The operation completed successfully.";
            _rc_msg[1] = "Error creating connection to database.";
            _rc_msg[2] = "Parameter name cannot be null.";
            _rc_msg[3] = "Parameter name cannot be empty string.";
            _rc_msg[4] = "Input parameter value cannot be null. If you want to use database NULL, use DBNull.Value instead.";
            _rc_msg[5] = "VARCHAR2 parameter length too long. Limit is 4000 characters.";
            _rc_msg[6] = "Unsupported SQLDBType for CreateParam().";
            _rc_msg[7] = "Stored Procedure name cannot be null.";
            _rc_msg[8] = "Stored Procedure name cannot be an empty string.";
            _rc_msg[9] = "ExecuteSP parameters cannot be null.";
            _rc_msg[10] = "DataLayer was already disposed and cannot be reused again.";
            _rc_msg[11] = "Error executing stored procedure.";
            _rc_msg[12] = "A transaction has already begun.";
            _rc_msg[13] = "No open transaction.";
            _rc_msg[14] = "SQL statement cannot be null.";
            _rc_msg[15] = "SQL statement cannot be an empty string.";
            _rc_msg[16] = "ExecuteSQL parameters cannot be null.";
            _rc_msg[17] = "Error executing Dynamic SQL.";
            _rc_msg[100] = "Unknown error.";
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection_string"></param>
        public SQLDataLayer(string connection_string)
        {

            //when the object is instantiated, we need to initialize connection.
            // setup database connection
            _con = new SqlConnection(connection_string);
            //_dec2int_off = new ArrayList();
        }
        /// <summary>
        /// Return an input parameter for a VARCHAR2 with the specified name.
        /// </summary>
        /// <param name="name">IN parameter name</param>
        /// <param name="value">string object.
        ///	string must be less than or equal to 4000 chars due to database limit.
        ///	DBNull.Value will be assumed if value=null.
        /// </param>
        /// <returns>SQL parameter</returns>
        public SqlParameter CreateParam(string name, string value)
        {
            // check if length is too long for this datatype
            if (value != null && value.Length > 4000)
            {
                _last_return_code = 5;
                throw new ArgumentException(_rc_msg[5] + " Parameter name '" + name + "'.");
            }

            // check for null and use DBNull.Value instead
            object obj = value;
            int length = 0;
            if (value == null)
                obj = DBNull.Value;
            else
                length = Math.Max(value.Length, 1);

            return CreateParam(name, obj, length, SqlDbType.VarChar, ParameterDirection.Input);
        }
        /// <summary>
        /// Return an input parameter for a NUMBER with the specified name.
        /// </summary>
        /// <param name="name">IN parameter name</param>
        /// <param name="value">int value</param>
        /// <returns>SQL parameter</returns>
        public SqlParameter CreateParam(string name, int value)
        {
            return CreateParam(name, Convert.ToDecimal(value), 0, SqlDbType.Int, ParameterDirection.Input);
        }
        /// <summary>
        /// Return an input parameter for a boolean (DbType.Bit) with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlParameter CreateParam(string name, bool value)
        {
            return CreateParam(name, value, 0, SqlDbType.Bit, ParameterDirection.Input);
        }
        /// <summary>
        /// Return an input parameter for a NUMBER with the specified name.
        /// </summary>
        /// <param name="name">IN parameter name</param>
        /// <param name="value">Decimal value</param>
        /// <returns>SQL parameter</returns>
        public SqlParameter CreateParam(string name, Decimal value)
        {
            return CreateParam(name, value, 0, SqlDbType.Decimal, ParameterDirection.Input);
        }
        /// <summary>
        /// Return an input parameter for a DOUBLE with the specified name.
        /// </summary>
        /// <param name="name">IN parameter name</param>
        /// <param name="value">double value.
        ///	DBNull.Value will be assumed if value=Double.NaN.
        ///	</param>
        /// <returns>SQL parameter</returns>
        public SqlParameter CreateParam(string name, double value)
        {
            // check for Double.NaN and use DBNull.Value instead
            object obj = value;
            //if (value == Double.NaN) replaced by Alejandro Palacios, May, 29, 2012
            if (Double.IsNaN(value))
                obj = DBNull.Value;

            return CreateParam(name, obj, 0, SqlDbType.Decimal, ParameterDirection.Input);
        }
        /// <param name="name">parameter name</param>
        /// <param name="value">input value (use DBNull.Value for database NULL input; use null for output value)</param>
        /// <param name="type">SQL type info</param>
        /// <param name="direction">parameter direction is input/output or both</param>
        /// <returns>SQL parameter</returns>
        public SqlParameter CreateParam(string name, object value, int size, SqlDbType type, ParameterDirection direction)
        {
            // make sure method parameters are workable
            if (name == null)
            {
                _last_return_code = 2;
                throw new ArgumentNullException(_rc_msg[2]);
            }

            if (name == "")
            {
                _last_return_code = 3;
                throw new ArgumentException(_rc_msg[3]);
            }

            if (direction == ParameterDirection.Input && value == null)
            {
                _last_return_code = 4;
                throw new ArgumentNullException(_rc_msg[4] + " Parameter name '" + name + "'.");
            }

            // set output parameters
            _last_return_code = 100;	// in case any unknown error occur (remote possibility)
            SqlParameter op = new SqlParameter();
            op.ParameterName = name;
            op.Value = value;
            op.SqlDbType = type;
            op.Size = size;
            op.Direction = direction;
            _last_return_code = 0;
            return op;
        }
        /// <summary>
        /// Return an input parameter for a DATETIME with the specified name.
        /// </summary>
        /// <param name="name">IN parameter name</param>
        /// <param name="value">datetime object.
        ///	DBNull.Value will be assumed if value=null.
        ///	</param>
        /// <returns>SQL parameter</returns>
        public SqlParameter CreateParam(string name, DateTime value)
        {
            // check for null and use DBNull.Value instead
            object obj = value;
            if (obj == null)
                obj = DBNull.Value;

            return CreateParam(name, obj, 0, SqlDbType.DateTime, ParameterDirection.Input);
        }
        /// <summary>
        /// Return an input parameter for a TEXT with the specified name.
        /// </summary>
        /// <param name="name">IN parameter name</param>
        /// <param name="value">string for Text;</param>
        /// <param name="type">Text</param>
        /// <returns>SQL parameter</returns>
        public SqlParameter CreateParam(string name, object value, SqlDbType type)
        {
            // check for supported SQLType
            //value = value is string ? ChangeCharacters(value) : value;
            if (type == SqlDbType.Text)
            {
                // check if object disposed and if there is an open transaction
                if (_con == null)
                {
                    _last_return_code = 10;
                    throw new ObjectDisposedException(_rc_msg[10]);
                }

                if (_con_trans == null)
                {
                    _last_return_code = 13;
                    throw new InvalidOperationException(_rc_msg[13]);
                }

                byte[] data = null;
                if (value == null || value == DBNull.Value)
                    data = new byte[0];
                else if (type == SqlDbType.Text && value.GetType() == typeof(byte[]))
                    data = (byte[])value;
                else
                {
                    UnicodeEncoding ue = new UnicodeEncoding();
                    data = ue.GetBytes(value.ToString());
                }
                /* test without specifying size, and see if it truncates or not...
                object obj = value;
                int length = 0;
                if (value == null)
                    obj = DBNull.Value;
                else
                    length = Math.Max(obj.ToString().Length, 1);

                SqlParameter op = new SqlParameter(name, SqlDbType.Text, length);
                op.Value = value.ToString();
                */
                SqlParameter op = new SqlParameter(name, SqlDbType.Text);
                op.Value = value.ToString();
                return op;
            }
            else
            {
                if (value == null || value == DBNull.Value)
                    return CreateParam(name, DBNull.Value, 0, type, ParameterDirection.Input);
                else
                {
                    _last_return_code = 6;
                    throw new ArgumentException(_rc_msg[6] + " Parameter name '" + name + "'.");
                }
            }
        }
        /// <summary>
        /// This method changes the special character so logical characters which the database is 
        /// capable of storing. Need to remove it if the database starts storing Unicode characters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        //private object ChangeCharacters(object input)
        //{
        //    //char ch = '\u2019';
        //    char[] chars = input.ToString().ToCharArray();
        //    for (int i = 0; i < chars.Length; ++i)
        //    {
        //        if (charTable.ContainsKey(chars[i]))
        //            chars[i] = (char)charTable[chars[i]];
        //    }

        //    return new string(chars);
        //}
        /// <summary>
        /// Error code from the last DataLayer CreateParam() or Execute*() call
        /// </summary>
        public int last_return_code
        {
            get
            {
                return _last_return_code;
            }
            set
            {
                _last_return_code = value;
            }
        }
        /// <summary>
        /// Execution time in milliseconds from the last DataLayer Execute*() call
        /// </summary>
        public int last_execution_time
        {
            get
            {
                return _last_execution_time;
            }
        }
        /// <summary>
        /// Execute stored procedure (require use of BeginTransaction() beforehand and Commit() / Rollback() afterwards).
        /// </summary>
        /// <param name="sp_name">full stored procedure name (case insensitive)</param>
        /// <param name="parameters">array of SQLParameters</param>
        /// <returns>dataset; null if no output from stored procedure or internal stored procedure error</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public DataSet ExecuteSP(string sp_name, params DbParameter[] parameters)
        {
            lock (this._con_trans_lock)
            {
                // for recording execution time
                DateTime datetime = DateTime.Now;

                // check if there is an open transaction
                if (_con_trans == null)
                {
                    _last_return_code = 13;
                    throw new InvalidOperationException(_rc_msg[13]);
                }

                // make sure sp_name is valid, parameters is not null and current instance is not disposed
                if (sp_name == null)
                {
                    _last_return_code = 7;
                    throw new ArgumentNullException(_rc_msg[7]);
                }

                if (sp_name == "")
                {
                    _last_return_code = 8;
                    throw new ArgumentException(_rc_msg[8]);
                }

                if (parameters == null)
                {
                    _last_return_code = 9;
                    throw new ArgumentNullException(_rc_msg[9]);
                }

                if (_con == null)
                {
                    _last_return_code = 10;
                    throw new ObjectDisposedException(_rc_msg[10]);
                }

                _last_return_code = 100;	// in case any unknown error occur (remote possibility)

                // translate sp_name if necessary
                //string translated_sp_name = null;
                //if (_ht_sp_name.Contains(sp_name.ToUpper()))
                //    translated_sp_name = (string)_ht_sp_name[sp_name.ToUpper()];
                //else
                //    translated_sp_name = sp_name;

                // create DataSet variable for return
                DataSet ds = null;

                // create the command
                using (SqlCommand cmd = _con.CreateCommand())
                {
                    cmd.CommandTimeout = 300;//5 mins before it times out
                    cmd.CommandText = sp_name;//translated_sp_name;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Transaction = _con_trans;

                    // add default errorcode output parameter (parameter name is hardcoded)                    
                    // since CreateParam resets _last_return_code, we have to set it again
                    cmd.Parameters.Add(CreateParam("poReturnCode", null, 0, SqlDbType.Int, ParameterDirection.Output));

                    _last_return_code = 100; // in case any unknown error occur (remote possibility)

                    // create array list to hold all other output parameter names
                    ArrayList a_outparam = new ArrayList();

                    // add all user specified parameters
                    foreach (SqlParameter op in parameters)
                    {
                        if (op == null)
                        {
                            _last_return_code = 9;
                            throw new ArgumentNullException(_rc_msg[9]);
                        }

                        // add the parameter
                        cmd.Parameters.Add(op);

                        // record if it's output parameter for generating DataTables
                        if (op.Direction == ParameterDirection.Output || op.Direction == ParameterDirection.InputOutput)
                            a_outparam.Add(op.ParameterName);
                    }

                    try
                    {
                        // create DataSet with name same as the stored procedure
                        ds = new DataSet(sp_name);

                        SqlDataReader sqldr;
                        sqldr = cmd.ExecuteReader();

                        addDataReader(sqldr, "dsResults", ds);

                        //Close the datareader
                        sqldr.Close();

                        // get the internal stored procedure return value
                        Int32 errorcode = (Int32)cmd.Parameters["poReturnCode"].Value;
                        _last_return_code = Convert.ToInt32(errorcode);

                        if (_last_return_code == 0 && a_outparam.Count > 0)
                        {
                            // assemble the dataset
                            foreach (string param_name in a_outparam)
                            {
                                SqlParameter op = cmd.Parameters[param_name];
                                //object param_value = op.Value;
                                //Type param_type = param_value.GetType();
                                DataTable dt = new DataTable(param_name);
                                dt.Columns.Add(op.Value.GetType().FullName, op.Value.GetType());
                                dt.Rows.Add(new object[1] { op.Value });
                                ds.Tables.Add(dt);
                            }
                        }
                    }
                    catch
                    {
                        _last_return_code = 11;
                        throw;
                    }
                }

                // record execution time
                _last_execution_time = DateTime.Now.Subtract(datetime).Milliseconds;

                return ds;
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public DbCommand CreateCommandPQ(string sp_name, params DbParameter[] parameters)
        {
            // check if there is an open transaction
            if (_con_trans == null)
            {
                _last_return_code = 13;
                throw new InvalidOperationException(_rc_msg[13]);
            }

            // make sure sp_name is valid, parameters is not null and current instance is not disposed
            if (sp_name == null)
            {
                _last_return_code = 7;
                throw new ArgumentNullException(_rc_msg[7]);
            }

            if (string.IsNullOrEmpty(sp_name))
            {
                _last_return_code = 8;
                throw new ArgumentException(_rc_msg[8]);
            }

            if (parameters == null)
            {
                _last_return_code = 9;
                throw new ArgumentNullException(_rc_msg[9]);
            }

            if (_con == null)
            {
                _last_return_code = 10;
                throw new ObjectDisposedException(_rc_msg[10]);
            }

            _last_return_code = 100;	// in case any unknown error occur (remote possibility)

            // create the command
            DbCommand cmd = _con.CreateCommand();
            cmd.CommandTimeout = 300;//5 mins before it times out
            cmd.CommandText = sp_name;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = _con_trans;

            // add all user specified parameters
            foreach (SqlParameter op in parameters)
            {
                if (op == null)
                {
                    _last_return_code = 9;
                    throw new ArgumentNullException(_rc_msg[9]);
                }

                // add the parameter
                cmd.Parameters.Add(op);
            }

            return cmd;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public DbCommand CreateCommandSP(string sp_name, params DbParameter[] parameters)
        {
            // check if there is an open transaction
            if (_con_trans == null)
            {
                _last_return_code = 13;
                throw new InvalidOperationException(_rc_msg[13]);
            }

            // make sure sp_name is valid, parameters is not null and current instance is not disposed
            if (sp_name == null)
            {
                _last_return_code = 7;
                throw new ArgumentNullException(_rc_msg[7]);
            }

            if (string.IsNullOrEmpty(sp_name))
            {
                _last_return_code = 8;
                throw new ArgumentException(_rc_msg[8]);
            }

            if (parameters == null)
            {
                _last_return_code = 9;
                throw new ArgumentNullException(_rc_msg[9]);
            }

            if (_con == null)
            {
                _last_return_code = 10;
                throw new ObjectDisposedException(_rc_msg[10]);
            }

            _last_return_code = 100;	// in case any unknown error occur (remote possibility)

            // create the command
            DbCommand cmd = _con.CreateCommand();
            cmd.CommandTimeout = 300;//5 mins before it times out
            cmd.CommandText = sp_name;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Transaction = _con_trans;

            // add default errorcode output parameter (parameter name is hardcoded)                    
            // since CreateParam resets _last_return_code, we have to set it again
            cmd.Parameters.Add(CreateParam("poReturnCode", null, 0, SqlDbType.Int, ParameterDirection.Output));

            // add all user specified parameters
            foreach (SqlParameter op in parameters)
            {
                if (op == null)
                {
                    _last_return_code = 9;
                    throw new ArgumentNullException(_rc_msg[9]);
                }

                // add the parameter
                cmd.Parameters.Add(op);
            }

            return cmd;
        }
        /// <summary>
        /// Runs a parameterized query
        /// </summary>
        /// <param name="sp_name">full stored procedure name (case insensitive)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public DataSet ExecutePQ(string sp_name, params DbParameter[] parameters)
        {
            lock (this._con_trans_lock)
            {
                // for recording execution time
                DateTime datetime = DateTime.Now;

                // check if there is an open transaction
                if (_con_trans == null)
                {
                    _last_return_code = 13;
                    throw new InvalidOperationException(_rc_msg[13]);
                }

                // make sure sp_name is valid, parameters is not null and current instance is not disposed
                if (sp_name == null)
                {
                    _last_return_code = 7;
                    throw new ArgumentNullException(_rc_msg[7]);
                }

                if (sp_name == "")
                {
                    _last_return_code = 8;
                    throw new ArgumentException(_rc_msg[8]);
                }

                //if (parameters == null)
                //{
                //    _last_return_code = 9;
                //    throw new ArgumentNullException(_rc_msg[9]);
                //}

                if (_con == null)
                {
                    _last_return_code = 10;
                    throw new ObjectDisposedException(_rc_msg[10]);
                }

                _last_return_code = 100;	// in case any unknown error occur (remote possibility)

                // translate sp_name if necessary
                //string translated_sp_name = null;
                //if (_ht_sp_name.Contains(sp_name.ToUpper()))
                //    translated_sp_name = (string)_ht_sp_name[sp_name.ToUpper()];
                //else
                //    translated_sp_name = sp_name;

                // create DataSet variable for return
                DataSet ds = null;

                // create the command
                using (SqlCommand cmd = _con.CreateCommand())
                {
                    cmd.CommandTimeout = 300;//5 mins before it times out
                    cmd.CommandText = sp_name;//translated_sp_name;
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = _con_trans;

                    // add default errorcode output parameter (parameter name is hardcoded)                    
                    // since CreateParam resets _last_return_code, we have to set it again
                    //cmd.Parameters.Add(CreateParam("poReturnCode", null, 0, SqlDbType.Int, ParameterDirection.Output));

                    _last_return_code = 100; // in case any unknown error occur (remote possibility)

                    // create array list to hold all other output parameter names
                    ArrayList a_outparam = new ArrayList();

                    // add all user specified parameters
                    foreach (SqlParameter op in parameters)
                    {
                        if (op == null)
                        {
                            _last_return_code = 9;
                            throw new ArgumentNullException(_rc_msg[9]);
                        }

                        // add the parameter
                        cmd.Parameters.Add(op);

                        // record if it's output parameter for generating DataTables
                        if (op.Direction == ParameterDirection.Output || op.Direction == ParameterDirection.InputOutput)
                            a_outparam.Add(op.ParameterName);
                    }

                    try
                    {
                        // create DataSet with name same as the stored procedure
                        ds = new DataSet(sp_name);

                        SqlDataReader sqldr;
                        sqldr = cmd.ExecuteReader();

                        addDataReader(sqldr, "dsResults", ds);

                        //Close the datareader
                        sqldr.Close();

                        // get the internal stored procedure return value
                        //Int32 errorcode = (Int32)cmd.Parameters["poReturnCode"].Value;
                        _last_return_code = 0;

                        if (_last_return_code == 0 && a_outparam.Count > 0)
                        {
                            // assemble the dataset
                            foreach (string param_name in a_outparam)
                            {
                                SqlParameter op = cmd.Parameters[param_name];
                                //object param_value = op.Value;
                                //Type param_type = param_value.GetType();
                                DataTable dt = new DataTable(param_name);
                                dt.Columns.Add(op.Value.GetType().FullName, op.Value.GetType());
                                dt.Rows.Add(new object[1] { op.Value });
                                ds.Tables.Add(dt);
                            }
                        }
                    }
                    catch
                    {
                        _last_return_code = 11;
                        throw;
                    }
                }

                // record execution time
                _last_execution_time = DateTime.Now.Subtract(datetime).Milliseconds;

                return ds;
            }
        }
        /// <summary>
        /// Execute a SQL statement (require use of BeginTransaction() beforehand and Commit() / Rollback() afterwards).
        /// </summary>
        /// <param name="sql">SQL statement(s); use ":pParamName" convention for named parameters inside SQL statement(s)</param>
        /// <param name="parameters">comma delimited list or array of SqlParameters.
        /// note: for OUT parameters, you have to hold on to the SqlParameters you passed in to get their Values back.
        /// </param>
        /// <returns>DataSet of the result of query.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public DataSet ExecuteSQL(string sql)
        {
            lock (this._con_trans_lock)
            {
                // for recording execution time
                DateTime datetime = DateTime.Now;

                // check if there is an open transaction
                if (_con_trans == null)
                {
                    _last_return_code = 13;
                    throw new InvalidOperationException(_rc_msg[13]);
                }

                // make sure sql is valid, parameters is not null and current instance is not disposed
                if (sql == null)
                {
                    _last_return_code = 14;
                    throw new ArgumentNullException(_rc_msg[14]);
                }

                if (sql == "")
                {
                    _last_return_code = 15;
                    throw new ArgumentException(_rc_msg[15]);
                }
                /*
                if (parameters == null)
                {
                    _last_return_code = 16;
                    throw new ArgumentNullException(_rc_msg[16]);
                }
                */
                if (_con == null)
                {
                    _last_return_code = 10;
                    throw new ObjectDisposedException(_rc_msg[10]);
                }

                _last_return_code = 100;	// in case any unknown error occur (remote possibility)
                SqlDataReader sqldr = null;
                // create DataSet variable for return
                DataSet ds = new DataSet();

                using (SqlCommand cmd = _con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = _con_trans;

                    try
                    {
                        // execute the query
                        sqldr = cmd.ExecuteReader();

                        addDataReader(sqldr, "dsResults", ds);

                        //Close the datareader
                        sqldr.Close();

                    }
                    catch
                    {
                        _last_return_code = 11;
                        throw;
                    }
                }

                _last_return_code = 0;

                // record execution time
                _last_execution_time = DateTime.Now.Subtract(datetime).Milliseconds;

                return ds;
            }
        }
        public async Task<DataSet> ExecuteSPAsync(string sp_name, params DbParameter[] parameters)
        {
            // for recording execution time
            DateTime datetime = DateTime.Now;

            // check if there is an open transaction
            if (_con_trans == null)
            {
                _last_return_code = 13;
                throw new InvalidOperationException(_rc_msg[13]);
            }

            // make sure sp_name is valid, parameters is not null and current instance is not disposed
            if (sp_name == null)
            {
                _last_return_code = 7;
                throw new ArgumentNullException(_rc_msg[7]);
            }

            if (sp_name == "")
            {
                _last_return_code = 8;
                throw new ArgumentException(_rc_msg[8]);
            }

            if (parameters == null)
            {
                _last_return_code = 9;
                throw new ArgumentNullException(_rc_msg[9]);
            }

            if (_con == null)
            {
                _last_return_code = 10;
                throw new ObjectDisposedException(_rc_msg[10]);
            }

            _last_return_code = 100;	// in case any unknown error occur (remote possibility)

            // translate sp_name if necessary
            //string translated_sp_name = null;
            //if (_ht_sp_name.Contains(sp_name.ToUpper()))
            //    translated_sp_name = (string)_ht_sp_name[sp_name.ToUpper()];
            //else
            //    translated_sp_name = sp_name;

            // create DataSet variable for return
            DataSet ds = null;

            // create the command
            using (SqlCommand cmd = _con.CreateCommand())
            {
                cmd.CommandTimeout = 300;//5 mins before it times out
                cmd.CommandText = sp_name;//translated_sp_name;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Transaction = _con_trans;

                // add default errorcode output parameter (parameter name is hardcoded)                    
                // since CreateParam resets _last_return_code, we have to set it again
                cmd.Parameters.Add(CreateParam("poReturnCode", null, 0, SqlDbType.Int, ParameterDirection.Output));

                _last_return_code = 100; // in case any unknown error occur (remote possibility)

                // create array list to hold all other output parameter names
                ArrayList a_outparam = new ArrayList();

                // add all user specified parameters
                foreach (SqlParameter op in parameters)
                {
                    if (op == null)
                    {
                        _last_return_code = 9;
                        throw new ArgumentNullException(_rc_msg[9]);
                    }

                    // add the parameter
                    cmd.Parameters.Add(op);

                    // record if it's output parameter for generating DataTables
                    if (op.Direction == ParameterDirection.Output || op.Direction == ParameterDirection.InputOutput)
                        a_outparam.Add(op.ParameterName);
                }

                try
                {
                    // create DataSet with name same as the stored procedure
                    ds = new DataSet(sp_name);

                    SqlDataReader sqldr;
                    sqldr = await cmd.ExecuteReaderAsync();

                    addDataReader(sqldr, "dsResults", ds);

                    //Close the datareader
                    sqldr.Close();

                    // get the internal stored procedure return value
                    Int32 errorcode = (Int32)cmd.Parameters["poReturnCode"].Value;
                    _last_return_code = Convert.ToInt32(errorcode);

                    if (_last_return_code == 0 && a_outparam.Count > 0)
                    {
                        // assemble the dataset
                        foreach (string param_name in a_outparam)
                        {
                            SqlParameter op = cmd.Parameters[param_name];
                            //object param_value = op.Value;
                            //Type param_type = param_value.GetType();
                            DataTable dt = new DataTable(param_name);
                            dt.Columns.Add(op.Value.GetType().FullName, op.Value.GetType());
                            dt.Rows.Add(new object[1] { op.Value });
                            ds.Tables.Add(dt);
                        }
                    }
                }
                catch
                {
                    _last_return_code = 11;
                    throw;
                }

                // record execution time
                _last_execution_time = DateTime.Now.Subtract(datetime).Milliseconds;

                return ds;
            }
        }
        /// <summary>
        /// Runs a parameterized query asynchronously
        /// </summary>
        /// <param name="sp_name">full stored procedure name (case insensitive)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public async Task<DataSet> ExecutePQAsync(string sp_name, params DbParameter[] parameters)
        {
            // for recording execution time
            DateTime datetime = DateTime.Now;

            // check if there is an open transaction
            if (_con_trans == null)
            {
                _last_return_code = 13;
                throw new InvalidOperationException(_rc_msg[13]);
            }

            // make sure sp_name is valid, parameters is not null and current instance is not disposed
            if (sp_name == null)
            {
                _last_return_code = 7;
                throw new ArgumentNullException(_rc_msg[7]);
            }

            if (sp_name == "")
            {
                _last_return_code = 8;
                throw new ArgumentException(_rc_msg[8]);
            }

            //if (parameters == null)
            //{
            //    _last_return_code = 9;
            //    throw new ArgumentNullException(_rc_msg[9]);
            //}

            if (_con == null)
            {
                _last_return_code = 10;
                throw new ObjectDisposedException(_rc_msg[10]);
            }

            _last_return_code = 100;	// in case any unknown error occur (remote possibility)

            // translate sp_name if necessary
            //string translated_sp_name = null;
            //if (_ht_sp_name.Contains(sp_name.ToUpper()))
            //    translated_sp_name = (string)_ht_sp_name[sp_name.ToUpper()];
            //else
            //    translated_sp_name = sp_name;

            // create DataSet variable for return
            DataSet ds = null;

            // create the command
            using (SqlCommand cmd = _con.CreateCommand())
            {
                cmd.CommandTimeout = 300;//5 mins before it times out
                cmd.CommandText = sp_name;//translated_sp_name;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = _con_trans;

                // add default errorcode output parameter (parameter name is hardcoded)                    
                // since CreateParam resets _last_return_code, we have to set it again
                //cmd.Parameters.Add(CreateParam("poReturnCode", null, 0, SqlDbType.Int, ParameterDirection.Output));

                _last_return_code = 100; // in case any unknown error occur (remote possibility)

                // create array list to hold all other output parameter names
                ArrayList a_outparam = new ArrayList();

                // add all user specified parameters
                foreach (SqlParameter op in parameters)
                {
                    if (op == null)
                    {
                        _last_return_code = 9;
                        throw new ArgumentNullException(_rc_msg[9]);
                    }

                    // add the parameter
                    cmd.Parameters.Add(op);

                    // record if it's output parameter for generating DataTables
                    if (op.Direction == ParameterDirection.Output || op.Direction == ParameterDirection.InputOutput)
                        a_outparam.Add(op.ParameterName);
                }

                try
                {
                    // create DataSet with name same as the stored procedure
                    ds = new DataSet(sp_name);

                    SqlDataReader sqldr;
                    sqldr = await cmd.ExecuteReaderAsync();

                    addDataReader(sqldr, "dsResults", ds);

                    //Close the datareader
                    sqldr.Close();

                    // get the internal stored procedure return value
                    //Int32 errorcode = (Int32)cmd.Parameters["poReturnCode"].Value;
                    _last_return_code = 0;

                    if (_last_return_code == 0 && a_outparam.Count > 0)
                    {
                        // assemble the dataset
                        foreach (string param_name in a_outparam)
                        {
                            SqlParameter op = cmd.Parameters[param_name];
                            //object param_value = op.Value;
                            //Type param_type = param_value.GetType();
                            DataTable dt = new DataTable(param_name);
                            dt.Columns.Add(op.Value.GetType().FullName, op.Value.GetType());
                            dt.Rows.Add(new object[1] { op.Value });
                            ds.Tables.Add(dt);
                        }
                    }
                }
                catch
                {
                    _last_return_code = 11;
                    throw;
                }
            }

            // record execution time
            _last_execution_time = DateTime.Now.Subtract(datetime).Milliseconds;

            return ds;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public async Task<DataSet> ExecuteSQLAsync(string sql)
        {
            // for recording execution time
            DateTime datetime = DateTime.Now;

            // check if there is an open transaction
            if (_con_trans == null)
            {
                _last_return_code = 13;
                throw new InvalidOperationException(_rc_msg[13]);
            }

            // make sure sql is valid, parameters is not null and current instance is not disposed
            if (sql == null)
            {
                _last_return_code = 14;
                throw new ArgumentNullException(_rc_msg[14]);
            }

            if (sql == "")
            {
                _last_return_code = 15;
                throw new ArgumentException(_rc_msg[15]);
            }
            /*
            if (parameters == null)
            {
                _last_return_code = 16;
                throw new ArgumentNullException(_rc_msg[16]);
            }
            */
            if (_con == null)
            {
                _last_return_code = 10;
                throw new ObjectDisposedException(_rc_msg[10]);
            }

            _last_return_code = 100;	// in case any unknown error occur (remote possibility)
            SqlDataReader sqldr = null;
            // create DataSet variable for return
            DataSet ds = new DataSet();

            using (SqlCommand cmd = _con.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = _con_trans;

                try
                {
                    // execute the query
                    sqldr = await cmd.ExecuteReaderAsync();

                    addDataReader(sqldr, "dsResults", ds);

                    //Close the datareader
                    sqldr.Close();

                }
                catch
                {
                    _last_return_code = 11;
                    throw;
                }
            }

            _last_return_code = 0;

            // record execution time
            _last_execution_time = DateTime.Now.Subtract(datetime).Milliseconds;

            return ds;
        }
        /// <summary>
        /// Open connection to database and begin a new transaction.
        /// </summary>
        /// <param name="isolation_level">Transaction isolation level for connection</param>
        public void BeginTransaction(IsolationLevel? isolation_level = null)
        {
            lock (this._con_trans_lock)
            {
                if (_con == null)
                    throw new ObjectDisposedException(_rc_msg[10]);

                if (_con_trans != null)
                    throw new InvalidOperationException(_rc_msg[12]);

                try
                {
                    _con.Open();
                    if (isolation_level.HasValue)
                        _con_trans = _con.BeginTransaction(isolation_level.Value);
                    else
                        _con_trans = _con.BeginTransaction(IsolationLevel.ReadCommitted);//DEFAULT TO READ COMMITTED
                }
                catch
                {
                    _con_trans = null;
                    _con.Close();
                    throw;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isolation_level"></param>
        /// <returns></returns>
        public async Task BeginTransactionAsync(IsolationLevel? isolation_level = null)
        {
            if (_con == null)
                throw new ObjectDisposedException(_rc_msg[10]);

            if (_con_trans != null)
                throw new InvalidOperationException(_rc_msg[12]);

            try
            {
                await _con.OpenAsync();
                if (isolation_level.HasValue)
                    _con_trans = _con.BeginTransaction(isolation_level.Value);
                else
                    _con_trans = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch
            {
                _con_trans = null;
                _con.Close();
                throw;
            }
        }
        /// <summary>
        /// Generic method to add table(s) from a DataReader into the DataSet.
        /// Each DataReader's ResultSet will correspond to 1 DataTable inside DataSet.
        /// Note1: if the DataReader have more than 1 ResultSet, the table name of the 1st ResultSet
        /// is equal to the base_table_name. Table name for subsequence ResultSet(s) will be in the
        /// form base_table_name__1, base_table_name__2, and so on (note double underscore).
        /// Note2: if the DataSet already contains a table with the same name,
        /// it will throw a DuplicateNameException.
        /// </summary>
        /// <param name="dr">data reader to be added</param>
        /// <param name="base_table_name">table name for the first ResultSet, or base table name for ResultSets thereafter</param>
        /// <param name="ds">data set target</param>
        /// <param name="decimal2int">indicate whether to auto convert all Decimal to Int32 or not</param>
        /// <returns>number of DataTables added to DataSet</returns>
        /// 
        int addDataReader(IDataReader dr, string base_table_name, DataSet ds)
        {
            int count = 0;	// current number of ResultSet under process
            do
            {
                // Create new data table
                DataTable schemaTable = dr.GetSchemaTable();
                DataTable dt = new DataTable();
                dt.TableName = base_table_name;
                if (count > 0)
                    dt.TableName += "__" + count.ToString();
                count++;

                if (schemaTable != null)
                {
                    // query returning records was executed
                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        DataRow dataRow = schemaTable.Rows[i];
                        dt.Columns.Add(new DataColumn((string)dataRow["ColumnName"], (Type)dataRow["DataType"]));
                    }

                    // Fill the data table we just created
                    while (dr.Read())
                    {
                        DataRow dataRow = dt.NewRow();
                        for (int i = 0; i < dr.FieldCount; i++)
                            dataRow[i] = dr.GetValue(i);
                        dt.Rows.Add(dataRow);
                    }

                    ds.Tables.Add(dt);
                }
                else
                {
                    // No records were returned
                    DataColumn dc = new DataColumn("RowsAffected", Type.GetType("System.Int32"));
                    dt.Columns.Add(dc);
                    DataRow dataRow = dt.NewRow();
                    dataRow[0] = dr.RecordsAffected;
                    dt.Rows.Add(dataRow);
                    ds.Tables.Add(dt);
                }
            }
            while (dr.NextResult());
            return count;
        }
        /// <summary>
        /// Commit current transaction and close the connection.
        /// </summary>
        public void Commit()
        {
            lock (this._con_trans_lock)
            {
                if (_con == null)
                    throw new ObjectDisposedException(_rc_msg[10]);

                if (_con_trans == null)
                    throw new InvalidOperationException(_rc_msg[13]);

                try
                {
                    _con_trans.Commit();
                }
                finally
                {
                    _con_trans = null;
                    _con.Close();
                }
            }
        }
        /// <summary>
        /// Rollback current transaction and close the connection.
        /// An application can call Rollback more than one time without generating an exception.
        /// </summary>
        public void Rollback()
        {
            lock (this._con_trans_lock)
            {
                if (_con_trans != null)
                {
                    _con_trans.Rollback();
                    _con_trans = null;
                    _con.Close();
                }
            }
        }
        #region IDisposable Members
        /// <summary>
        /// using block requires the class to implement IDisposable:
        /// using (SQLDataLayer dl = new SQLDataLayer(""))
        ///{
        ///
        ///}
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_con != null)
                {
                    if (_con_trans != null)
                        Rollback();
                    _con.Dispose();
                    _con = null;
                }
            }

            //release native resources here...            
        }
        #endregion
    }
}