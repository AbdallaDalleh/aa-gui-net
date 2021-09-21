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
        const string RequestDataCSV = "http://10.1.100.9:17668/retrieval/data/getData.csv?pv={0}_{1}({2})&from={3}&to={4}";
        const string DateTimeLongFormat = "hh:mm:ss dd/MM/yyyy tt";
        const string DateTimeISOFormat = "yyyy-MM-ddThh:mm:ss.fffZ";

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
            dtFrom.Value = DateTime.Now.AddHours(-1);
            dtFrom.Format = DateTimePickerFormat.Custom;
            dtFrom.CustomFormat = DateTimeLongFormat;
            dtTo.Value = DateTime.Now;
            dtTo.Format = DateTimePickerFormat.Custom;
            dtTo.CustomFormat = DateTimeLongFormat;
            saveFileDialog.Filter = "AA Templates|*.aat";
            saveFileDialog.Title = "Save AA template";
            openFileDialog.Filter = "AA Templates|*.aat";
            openFileDialog.Title = "Load AA Template";
            cbMethod.SelectedIndex = 0;

            btnNow.Click += (object s, EventArgs e) => dtTo.Value = DateTime.Now;
            btnAdd.Click += (object s, EventArgs e) => listData.Items.Add(listBuffer.SelectedItem);
            btnFetch.Click += (object s, EventArgs e) => FetchPVs();
            btnRemove.Click += (object s, EventArgs e) => listData.Items.Remove(listData.SelectedItem);
            btnRemoveAll.Click += (object s, EventArgs e) => listData.Items.Clear();
            btnAddAll.Click += (object s, EventArgs e) =>
            {
                foreach (var item in listBuffer.SelectedItems)
                {
                    if(!listData.Items.Contains(item))
                        listData.Items.Add(item);
                }
            };

            FetchPVs();
        }

        private void FetchPVs()
        {
            listBuffer.Items.Clear();
            request = (HttpWebRequest)WebRequest.Create(RequestPVsList);
            response = (HttpWebResponse)request.GetResponse();
            String content = new StreamReader(response.GetResponseStream()).ReadToEnd();
            List<String> pvList = JsonSerializer.Deserialize<List<String>>(content);

            foreach (String pv in pvList)
            {
                listBuffer.Items.Add(pv);
            }
        }

        private void btnSaveTemplate_Click(object sender, EventArgs e)
        {
            if(listData.Items.Count <= 0)
            {
                MessageBox.Show("Empty PVs List", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(dtFrom.Value.Ticks >= dtTo.Value.Ticks)
            {
                MessageBox.Show("Invalid start/end timestamps", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            saveFileDialog.ShowDialog();
            if(saveFileDialog.FileName != String.Empty)
            {
                StreamWriter template = new StreamWriter(saveFileDialog.FileName);
                foreach (var item in listData.Items)
                {
                    template.WriteLine("pv " + item.ToString());
                }
                template.WriteLine("from " + dtFrom.Value.ToUniversalTime().ToString(DateTimeISOFormat));
                template.WriteLine("to " + dtTo.Value.ToUniversalTime().ToString(DateTimeISOFormat));
                template.Close();

                MessageBox.Show("Template " + saveFileDialog.FileName + " saved successfulyl", "Save Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Filename was not selected. Template was not saved", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnLoadTemplate_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            if(openFileDialog.FileName != String.Empty)
            {
                StreamReader template = new StreamReader(openFileDialog.FileName);
                while(template.Peek() != -1)
                {
                    string line = template.ReadLine();
                    string[] items = line.Split(' ');
                    if (items[0] == "pv")
                        listData.Items.Add(items[1]);
                    else if (items[0] == "from")
                        dtFrom.Value = DateTime.Parse(items[1]).ToLocalTime();
                    else if(items[0] == "to")
                        dtTo.Value = DateTime.Parse(items[1]).ToLocalTime();
                    else
                    {
                        MessageBox.Show("Invalid configuration", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        template.Close();
                    }
                }

                template.Close();
            }
        }

        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            if(listData.Items.Count <= 0)
            {
                MessageBox.Show("Empty PVs List", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if(dtFrom.Value.Ticks >= dtTo.Value.Ticks)
            {
                MessageBox.Show("Start time is greater than or equal to end time", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            saveFileDialog.ShowDialog();
            if(saveFileDialog.FileName != String.Empty)
            {
                StreamWriter csvFile = new StreamWriter(saveFileDialog.FileName);
                int samplingRate;
                if (rbSeconds.Checked)
                    samplingRate = (int)numPeriod.Value;
                else if (rbMinutes.Checked)
                    samplingRate = (int)numPeriod.Value * 60;
                else
                    samplingRate = (int)numPeriod.Value * 3600;

                int requestInterval = (int)(dtTo.Value.Ticks - dtFrom.Value.Ticks) / samplingRate;
                string samplingMethod = cbMethod.SelectedIndex == 0 ? "firstFill" : cbMethod.Text;

                string url = String.Format(RequestDataCSV,
                    samplingMethod,
                    samplingRate,
                    listData.Items[0], 
                    dtFrom.Value.ToUniversalTime().ToString(DateTimeISOFormat),
                    dtTo.Value.ToUniversalTime().ToString(DateTimeISOFormat));

                MessageBox.Show(url);
            }
        }
    }
}
