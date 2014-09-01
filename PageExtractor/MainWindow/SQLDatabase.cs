using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDatabase
{
    class SQLDatabase
    {
        private string _DataSource; //Database brand
        private string _DatabaseName; //Database Name under the engine
        private string _UserName;
        private string _Passcode;
        private SqlConnection sql_Connection;

        public SQLDatabase(string data_source, string database_name, string user_name, string passcode)
        {
            _DataSource = data_source;
            _DatabaseName = database_name;
            _UserName = user_name;
            _Passcode = passcode; 
        }

        private string DataSource
        {
            get 
            { 
                return _DataSource;
            }
            set 
            {
                _DataSource = value;
            }        
        }
        private string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }
            set
            {
                _DatabaseName = value;
            }
        }
        private string UserName
        {
            get
            {
                return _UserName;
            }
            set 
            {
                _UserName = value;
            }
        }
        private string Passcode
        {
            get 
            {
                return _Passcode;
            }
            set 
            {
                _Passcode = value;                
            }
        }

        private bool Connect_Database()
        {
            string connConfig = "Data Source=" + DataSource +";"+"Initial Catalog="+DatabaseName+";"+"User ID="+UserName+";"+"Password=" + Passcode;

            sql_Connection = new SqlConnection(connConfig);

            try
            {
                sql_Connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database Connection Error:" + ex.ToString());
                return false;
            }
        }

        public bool Disconnect_database()
        {
            if (sql_Connection != null)
            {
                sql_Connection.Close();
                return true;
            }
            else
                return false;
        }

        public SqlConnection get_sql_Connection()
        {
            return sql_Connection;
        }

        public SqlDataReader get_SqlDataReader(string sql_query)
        {
            Connect_Database();

            //定义命令对象
            SqlCommand sql_command = new SqlCommand(sql_query, sql_Connection);

            //定义数据读取器
            SqlDataReader sql_dataReader = null;

            sql_dataReader = sql_command.ExecuteReader();

            return sql_dataReader;
        }


        public int ExecuteNonQuery(string _sql_query) // insert, update, delete
        {
            int _return_value = -1;

            try
            {
                Connect_Database();

                SqlCommand MyCommand = new SqlCommand(_sql_query, sql_Connection);

                _return_value = MyCommand.ExecuteNonQuery();

                Disconnect_database();
            }
            catch (Exception)
            {
 
            }

            return _return_value; 
        }

        public DataSet Execute_select_with_DataSet(string _sql_query) // select
        {
            DataSet MyDataSet = null;

            try
            {
                Connect_Database();

                SqlCommand MyCommand = new SqlCommand(_sql_query, sql_Connection);

                SqlDataAdapter Select_Adapter = new SqlDataAdapter();
                Select_Adapter.SelectCommand = MyCommand;
                MyDataSet = new DataSet();
                Select_Adapter.SelectCommand.ExecuteNonQuery();

                Disconnect_database();

                Select_Adapter.Fill(MyDataSet);
            }
            catch(Exception)
            {

            }

            return MyDataSet;
        }

    }
}
