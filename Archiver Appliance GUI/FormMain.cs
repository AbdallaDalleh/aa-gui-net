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
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            listBuffer.Sorted = true;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("http://10.1.100.9:17665/mgmt/bpl/getAllPVs?limit=-1");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            String content = new StreamReader(response.GetResponseStream()).ReadToEnd();
            List<String> list = JsonSerializer.Deserialize<List<String>>(content);
            foreach (String item in list)
            {
                listBuffer.Items.Add(item);
            }
        }
    }
}
