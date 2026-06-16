using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CalcDemo
{
    public partial class Form1 : Form
    {
        // 全局变量：第一个数、运算符、标记是否输入新数字
        double num1 = 0;
        string op = "";
        bool isNewNum = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 窗体加载无需额外代码
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "6";
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "9";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtResult.Clear();
            isNewNum = false;
            op = "";
            num1 = 0;
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            // 如果是新数字，先清空文本框
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "0";
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            // 如果是新数字，先清空文本框
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "1";
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            // 如果是新数字，先清空文本框
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "2";
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            // 如果是新数字，先清空文本框
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "3";
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            // 如果是新数字，先清空文本框
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "4";
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            // 如果是新数字，先清空文本框
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "5";
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "7";
        }

        private void btn8_Click(object sender, EventArgs e)
        {
            if (isNewNum)
            {
                txtResult.Clear();
                isNewNum = false;
            }
            txtResult.Text += "8";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            num1 = Convert.ToDouble(txtResult.Text);
            op = "+";
            isNewNum = true;
        }

        private void btnSub_Click(object sender, EventArgs e)
        {
            num1 = Convert.ToDouble(txtResult.Text);
            op = "-";
            isNewNum = true;
        }

        private void btnMul_Click(object sender, EventArgs e)
        {
            num1 = Convert.ToDouble(txtResult.Text);
            op = "*";
            isNewNum = true;
        }

        private void btnDiv_Click(object sender, EventArgs e)
        {
            num1 = Convert.ToDouble(txtResult.Text);
            op = "/";
            isNewNum = true;
        }

        private void btnEqual_Click(object sender, EventArgs e)
        {
            // 无运算符/无数字，直接返回
            if (string.IsNullOrEmpty(op) || string.IsNullOrEmpty(txtResult.Text))
                return;

            double num2 = Convert.ToDouble(txtResult.Text);
            double result = 0;

            switch (op)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "*":
                    result = num1 * num2;
                    break;
                case "/":
                    if (num2 == 0)
                    {
                        MessageBox.Show("除数不能为0");
                        txtResult.Clear();
                        return;
                    }
                    result = num1 / num2;
                    break;
            }
            txtResult.Text = result.ToString();
            num1 = result;       // 关键：结果赋值给num1，支持连续计算
            isNewNum = true;
        }

        private void txtResult_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
