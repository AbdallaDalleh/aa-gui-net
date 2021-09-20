using System;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Archiver_Appliance_GUI
{
    public partial class FormMain : Form
    {
        const string RequestPVsList = "http://10.1.100.9:17665/mgmt/bpl/getAllPVs?limit=-1";

        HttpWebRequest request;
        HttpWebResponse response;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            listBuffer.Sorted = true;
            listBuffer.SelectionMode = SelectionMode.MultiSimple;
            listData.SelectionMode = SelectionMode.MultiSimple;

            request = (HttpWebRequest) WebRequest.Create(RequestPVsList);
            response = (HttpWebResponse)request.GetResponse();
            String content = new StreamReader(response.GetResponseStream()).ReadToEnd();
            List<String> list = JsonSerializer.Deserialize<List<String>>(content);
            foreach (String item in list)
            {
                listBuffer.Items.Add(item);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            listData.Items.Add(listBuffer.SelectedItem);
        }

        private void btnAddAll_Click(object sender, EventArgs e)
        {
            foreach (var item in listBuffer.SelectedItems)
            {
                listData.Items.Add(item);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            listData.Items.Remove(listData.SelectedItem);
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            listData.Items.Clear();
        }
    }
}
