using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows.Forms;
using Fibertower_Common;

namespace XFP模块测试程序
{
    public partial class I2C_Form : Form
    {
        BackgroundWorker backgroundWorkerSeacherSqlDataSource;
        public static Communication c = new Communication();
        I2C i2c;

        public I2C_Form()
        {
            InitializeComponent();
        }

        private void OK_button_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                i2c = new CP2112() as I2C;
            if (radioButton2.Checked)
                i2c = new TWI() as I2C;
            try
            {
                if (i2c.TWI_Open() == false)
                {
                    throw new Exception();
                }
                i2c.TWI_Close();
            }
            catch
            {
                MessageBox.Show("通信失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }

            SqlConnection sqlconnection = new SqlConnection("server=" + sqlserver_comboBox.Text + ";uid=sa;pwd=fiber123;database=SFP");

            c.Send(i2c, sqlconnection);
        }

        private void backgroundWorkerSeacherSqlDataSource_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                e.Result = instance.GetDataSources();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void backgroundWorkerSeacherSqlDataSource_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                sqlserver_comboBox.Items.Clear();
                DataTable table = e.Result as DataTable;
                if (table != null)
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        sqlserver_comboBox.Items.Add(table.Rows[i]["ServerName"]);
                    }
                    if (sqlserver_comboBox.Items.Count > 0)
                        sqlserver_comboBox.SelectedIndex = 0;
                }
            }
        }

        private void update_button_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerSeacherSqlDataSource.IsBusy)
            {
                return;
            }
            backgroundWorkerSeacherSqlDataSource.RunWorkerAsync();
        }

        private void testSQL_button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sqlserver_comboBox.Text))
            {
                MessageBox.Show("请选择SQL数据库服务器", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SqlConnection sqlconnection = new SqlConnection("server=" + sqlserver_comboBox.Text + ";uid=sa;pwd=fiber123;database=SFP");
            try
            {
                sqlconnection.Open();
                sqlconnection.Close();
                MessageBox.Show("测试连接成功", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show("测试连接失败\r\n" + exception.Message, "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void I2C_Form_Load(object sender, EventArgs e)
        {
            backgroundWorkerSeacherSqlDataSource = new BackgroundWorker();
            backgroundWorkerSeacherSqlDataSource.DoWork += new DoWorkEventHandler(backgroundWorkerSeacherSqlDataSource_DoWork);
            backgroundWorkerSeacherSqlDataSource.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerSeacherSqlDataSource_RunWorkerCompleted);
        }      
    }

    public class Communication
    {
        public delegate void MyEnentHander(I2C i2c, SqlConnection sqlconnection);

        public event MyEnentHander OnSendMsg;

        public void Send(I2C i2c, SqlConnection sqlconnection)
        {
            if (sqlconnection != null && i2c != null)
                OnSendMsg(i2c, sqlconnection);
        }
    }
}
