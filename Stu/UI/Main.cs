﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Stu.Class;
using System.Collections;
using Stu.Utils;
using Stu.Manager;
using System.Diagnostics;
using System.IO;

namespace Stu.UI
{
    public partial class Main : Form
    {
        BluetoothList bluetooth_list = null;
        public Main()
        {
            InitializeComponent();
            radioTest.Checked = true;
            this.bluetooth_list = new BluetoothList();
            //bluetooth_list.Location = new Point(350, 40);
            //bluetooth_list.TopLevel = false;
            //this.Controls.Add(bluetooth_list);
            bluetooth_list.Show();
            bluetooth_list.Location = new Point(0, 0);
            bluetooth_list.hideButton();
            outputText.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //ArrayList list = new ArrayList();
            //for (double i = 1; i <= 3; i++)
            //{
            //    list.Add(i);
            //}
            //double res = Calculate.norm(list);
            //Console.WriteLine(res);
        }

        private void addDataFile()
        {
            string brainRoot =  outputText.Text + "/BrainResult";
            foreach (string deviceFolder in Directory.GetFileSystemEntries(brainRoot))
            {
                foreach (string orderID in Directory.GetFileSystemEntries(deviceFolder))
                {
                    WriteFile wfile = new WriteFile(orderID);
                    wfile.BrainToNBrain();
                }
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (outputText.Text.Length == 0)
            {
                FolderBrowserDialog path = new FolderBrowserDialog();
                path.ShowDialog();
                outputText.Text = path.SelectedPath;
            }
            if (outputText.Text.Length == 0)
            {
                MessageBox.Show("尚未選擇輸出路徑!");
                return;
            }
            if (textTestTime.Text.Length == 0 || textUserName.Text.Length == 0 || textUserYearOld.Text.Length == 0 || textWordNum.Text.Length == 0)
            {
                MessageBox.Show("欄位都為必填!");
                return;
            }
            ArrayList list = bluetooth_list.getResult();
            if (list.Count == 0)
            {
                MessageBox.Show("尚未選擇Device!");
                return;
            }
            ChromeUtils.closeChrome();
            Boolean isclient = checkBoxClient.Checked;
            if (isclient)
            {
                BluetoothDeviceManager manager = (BluetoothDeviceManager)list[0];
                string order_id = DateTime.Now.ToString("yyyyMMddHHmmss");
                ConfigManager config_manager = new ConfigManager(order_id, outputText.Text, int.Parse(textTestTime.Text), manager, true, isclient, textUserName.Text, textUserYearOld.Text);
                MessageBox.Show("準備好了?確定後開始測試");
                ArrayList formList = new ArrayList();
                formList.Add(this);
                formList.Add(bluetooth_list);
                new Memory(config_manager,formList);
            }
            else
            {
                BluetoothDeviceManager manager = (BluetoothDeviceManager)list[0];
                HttpWorker httpWorker = new HttpWorker(HttpWorker.orderCreate, httpResponse);
                JSONObject form = new JSONObject();
                form.setString("deviceAddress", manager.getDeviceAddress());
                form.setString("userName", textUserName.Text);
                form.setString("userYearOld", textUserYearOld.Text);
                form.setString("wordNum", textWordNum.Text);
                form.setString("testTime", textTestTime.Text);
                httpWorker.setData(form);
                httpWorker.httpWorker();
                WaitDialog.show();
            }
        }

        private void httpResponse(JSONObject response)
        {
            WaitDialog.close();
            int error_code = response.getInt("error_code");
            if (error_code == 0)
            {
                Boolean isTest = radioTest.Checked;
                Boolean isclient = checkBoxClient.Checked;
                ArrayList list = bluetooth_list.getResult();
                BluetoothDeviceManager manager = (BluetoothDeviceManager)list[0];
                string order_id = response.getString("orderID");
                ConfigManager config_manager = new ConfigManager(order_id, outputText.Text, int.Parse(textTestTime.Text), manager, isTest, isclient, textUserName.Text, textUserYearOld.Text);
                if (!config_manager.getIsTest())
                {
                    ShowExDialog.show("第一步、選擇單字", Properties.Resources.choose);
                    string chooseUrl = ChromeUtils.chooseURL + order_id;
                    ChromeUtils.openChrome(chooseUrl);
                    Choose choose = new Choose(config_manager);
                    choose.Show();
                    choose.Location = new Point(0, 0);
                    this.WindowState = FormWindowState.Minimized; 
                }
                else
                {
                    MessageBox.Show("準備好了?確定後開始測試");
                    ArrayList formList = new ArrayList();
                    formList.Add(this);
                    formList.Add(bluetooth_list);
                    new Memory(config_manager, formList);
                }
            }
            else
            {
                string message = response.getString("message");
                MessageBox.Show(message);
            }
        }

        private void chooseFolderBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            outputText.Text = path.SelectedPath;
        }

        private void historyBtn_Click(object sender, EventArgs e)
        {
            if (outputText.Text.Length == 0)
            {
                FolderBrowserDialog path = new FolderBrowserDialog();
                path.ShowDialog();
                outputText.Text = path.SelectedPath;
            }
            if (outputText.Text.Length == 0)
            {
                MessageBox.Show("尚未選擇輸出路徑!");
                return;
            }
            OrderList list = new OrderList(outputText.Text, checkBoxClient.Checked);
            list.Show();
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            textTestTime.Text = ""; 
            textUserName.Text = "";
            textUserYearOld.Text = "";
            textWordNum.Text = "";
            outputText.Text = "";
        }

        private void exBtn_Click(object sender, EventArgs e)
        {
            int test = 0;
            if (test == 1)
            {
                if (outputText.Text.Length == 0)
                {
                    FolderBrowserDialog path = new FolderBrowserDialog();
                    path.ShowDialog();
                    outputText.Text = path.SelectedPath;
                }
                string p = outputText.Text + "/" + "BrainResult" + "/" + "8CDE52929277";
                foreach (string fname in Directory.GetFileSystemEntries(p)) 
                {
                    string file = fname + "/" + "FFT.csv";
                } 
            }
            ChromeUtils.openChrome(ChromeUtils.exURL);
        }

        private void radioEn_CheckedChanged(object sender, EventArgs e)
        {
            label3.Visible = true;
            textWordNum.Visible = true;
            textTestTime.Text = "600";
            checkBoxClient.Checked = false;
            checkBoxClient.Enabled = false;
        }

        private void radioTest_CheckedChanged(object sender, EventArgs e)
        {
            label3.Visible = false;
            textWordNum.Visible = false;
            textTestTime.Text = "180";
            checkBoxClient.Checked = true;
            checkBoxClient.Enabled = true;
        }

        private void checkBoxClient_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxClient.Checked)
            {
                btnCheck.Text = "開始測試-離線測試";
                historyBtn.Text = "歷史紀錄-只查詢本地";
            }
            else
            {
                btnCheck.Text = "開始測試";
                historyBtn.Text = "歷史紀錄";
            }
        }
    }
}
