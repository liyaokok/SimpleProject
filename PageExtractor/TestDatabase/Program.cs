using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Basic Demo
            /*
            //连接字符串
            //string connConfig = "Data Source=liyaokokPC\\SQLEXPRESS;Initial Catalog=TestDatabase;User ID=sa;Password=phones1";
            string connConfig = "Data Source=.\\SQLEXPRESS;Initial Catalog=TestDatabase;User ID=sa;Password=phones1";
            //连接对象
            SqlConnection sql_Connection = new SqlConnection(connConfig);

            try
            {
                sql_Connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database Connection Error:" + ex.ToString());
            }

            //定义SQL查询语句
            string sql_query;

            sql_query = "select * from [TestTable]";
            
            //定义命令对象
            SqlCommand sql_command = new SqlCommand(sql_query, sql_Connection);

            //定义数据读取器
            SqlDataReader sql_dataReader = null;

            sql_dataReader = sql_command.ExecuteReader();

            //输出内容
            Console.WriteLine("Address0\tIndex\tName");

            while (sql_dataReader.Read())
            {
                Console.WriteLine(sql_dataReader[0] + "\t" + sql_dataReader["Index"] + "\t" + sql_dataReader["Name"]);
            }

            //关闭数据读取器
            sql_dataReader.Close();

            //关闭数据库连接
            sql_Connection.Close();

            Console.Read();
             */ 
            #endregion

            #region Dataset

            /*SQLDatabase my_sql_database = new SQLDatabase(".\\SQLEXPRESS", "TestDatabase", "sa", "phones1");

            string sql_query = "select * from [TestTable]";

            DataSet my_DataSet = my_sql_database.Execute_select_with_DataSet(sql_query);

          
            foreach   (DataTable   dt   in   my_DataSet.Tables)   //遍历所有的datatable
            {
                foreach (DataRow dr in dt.Rows)   ///遍历所有的行
                    foreach (DataColumn dc in dt.Columns)   //遍历所有的列
                        Console.WriteLine(dt.TableName +" "+dc.ColumnName + " " + dr[dc]);   //表名, 列名,单元格数据
            }*/
            #endregion

            SQLDatabase my_sql_database = new SQLDatabase(".\\SQLEXPRESS", "TestDatabase", "sa", "phones1");

            //string sql_query = "insert into [TestDatabase].dbo.TestTable values (4, 'Product4')";
            //string sql_query = "delete from [TestDatabase].dbo.TestTable where [Index] = 4";
            string sql_query = "INSERT INTO OfficeDepot ([index],URL,ProductName,CurrentPrice,ImageURL,ProductDescription,BrandName,ManufacturerNumber,ManufacturerName,UPCNumber,Model) VALUES (0,'root URL','test_name',19.99,'image.google.con','product description','Dell','123456789','Manufacture Name','0123456','Inspiration1530')";

            int number_affected = my_sql_database.ExecuteNonQuery(sql_query);

            Console.WriteLine(number_affected.ToString() + " row(s) affected");

            Console.Read();
        }
    }
}
