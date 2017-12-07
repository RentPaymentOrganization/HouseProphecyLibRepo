using System.Data.SqlClient;
using System.Data;
using HouseProphecy.Components;
using System;
using HouseProphecy.Helpers;

namespace HouseProphecy.DataProviders
{
    public partial class DataProvider : SingleTone<DataProvider>
    {
        #region privateMembers      
        private SqlConnection sqlConnection = null;

        #endregion

        #region PublicProperties
        /// <summary>
        /// returns default connectionString
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// returns default Connection
        /// </summary>
        public SqlConnection Connection
        {
            get
            {
                if (sqlConnection == null)
                {
                    sqlConnection = new SqlConnection(ConnectionString);
                }
                if (string.IsNullOrEmpty(sqlConnection.ConnectionString))
                {
                    sqlConnection.ConnectionString = ConnectionString;
                }
                return sqlConnection;
            }
        }


        #endregion

        #region Protected Methods

        /// <summary>
        /// Create SQL command for stored procedure
        /// </summary>    
        /// <param name="spName">name of the stored procedure</param>
        /// <returns>SQL command</returns>
        /// <remarks></remarks>
        protected SqlCommand CreateSQLCommandForSP(string spName)
        {
            SqlCommand command = new SqlCommand(spName, new SqlConnection(ConnectionString));
            command.CommandType = CommandType.StoredProcedure;
            // command.Connection.Open();
            return command;
        }

        /// <summary>
        /// Create SQL command for string query
        /// </summary>    
        /// <param name="spName">name of the stored procedure</param>
        /// <returns>SQL command</returns>
        /// <remarks></remarks>
        public SqlCommand CreateSQLCommand(string query)
        {
            SqlCommand command = new SqlCommand(query, new SqlConnection(ConnectionString));
            command.CommandType = CommandType.Text;
            return command;
        }


        /// <summary>
        /// Create input SQL parametet, its name is @ and column name
        /// </summary>
        /// <param name="columnName">Column name which matches with parameter</param>
        /// <param name="dbType">Parameter type</param>
        /// <param name="value">Parameter value</param>
        /// <returns>Filled SQL parameter</returns>
        /// <remarks></remarks>
        protected SqlParameter CreateSqlParameter(string columnName, SqlDbType dbType, object value)
        {
            return CreateSqlParameter(columnName, dbType, value, ParameterDirection.Input);
        }

        /// <summary>
        /// Create SQL parametet, its name is @ and column name
        /// </summary>
        /// <param name="columnName">Column name which matches with parameter</param>
        /// <param name="dbType">Parameter type</param>
        /// <param name="value">Parameter value</param>
        /// <param name="direction">Parameter direction</param>
        /// <returns>Filled SQL parameter</returns>
        /// <remarks></remarks>
        protected SqlParameter CreateSqlParameter(string columnName, SqlDbType dbType, object value, ParameterDirection direction)
        {
            // Add parametors
            SqlParameter param = new SqlParameter(string.Format("@{0}", columnName), dbType);

            param.Direction = direction;
            param.Value = value;

            return param;
        }

        /// <summary>
        /// Makes parameterName satisfying t-sql syntax (parameterName - > @parameterName)
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        protected string SqlParametrName(string parameterName)
        {
            return string.Format("@{0}", parameterName);
        }

        #endregion

        #region getting data

        public DataSet GetPropertyListInfo(string stateName, string county, string city, string street, string streetNumber, string zipCode)
        {
            SqlCommand myCommand = CreateSQLCommandForSP(Constants.StoredProcedures.GetListInfo);
            myCommand.Parameters.Add(CreateSqlParameter("stateNameValue", SqlDbType.VarChar, stateName));
            myCommand.Parameters.Add(CreateSqlParameter("countyValue", SqlDbType.VarChar, county));
            myCommand.Parameters.Add(CreateSqlParameter("cityValue", SqlDbType.VarChar, city));
            myCommand.Parameters.Add(CreateSqlParameter("streetValue", SqlDbType.VarChar, street));
            myCommand.Parameters.Add(CreateSqlParameter("streetNumberValue", SqlDbType.VarChar, streetNumber));
            myCommand.Parameters.Add(CreateSqlParameter("zipCodeValue", SqlDbType.VarChar, zipCode));
            return GetDataSet(myCommand);
        }

        private DataSet GetDataSet(SqlCommand command)
        {
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet dataSet = new DataSet();
            try
            {
                //if use SqlDataAdapter - ew do not beed open and close connection. Adapter does own.
                da.SelectCommand = (SqlCommand)command;
                da.Fill(dataSet);
                return dataSet;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion
    }
}