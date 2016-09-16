using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using SMS.Helper;

namespace SMSManagement
{
    public partial class SendSMS : Form
    {
        string strCommand = "";
        SerialPort port = new SerialPort();
        MODEL_SMS objMODEL_SMS = new MODEL_SMS();
        List<MODEL_SMS> objListMODEL_SMS = new List<MODEL_SMS>();
        SMS_FUNCTION objSMS_FUNCTION = new SMS_FUNCTION();
        ComPortConnectionClass objComPortConnectionClass = new ComPortConnectionClass();
        //  BackgroundWorker loader = new BackgroundWorker();

        public SendSMS()
        {
            InitializeComponent();
            BindPort();
            txtStatus.BackColor = System.Drawing.Color.Red;
            FindActivePOrt();

            //    loader.do
            // loader.CancelAsync();


        }
        private void BindPort()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                // Add all port names to the combo box:
                foreach (string port in ports)
                {
                    this.cboPortName.Items.Add(port);
                }
            }
            catch { }
        }
        private void FindActivePOrt()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    this.port = objComPortConnectionClass.OpenPort(port, Convert.ToInt32(this.cboBaudRate.Text), 8, 300, 300);
                    if (this.port != null)
                    {
                        try
                        {
                            int uCountSMS = objSMS_FUNCTION.CountSMSmessages(this.port);
                            if (uCountSMS >= 0)
                            {
                                lblActivePortName.Text = port;
                                cboPortName.Text = port;
                                txtCountedSMS.Text = uCountSMS.ToString();
                                objComPortConnectionClass.ClosePort(this.port);
                                break;
                            }
                            else
                            {
                                lblActivePortName.Text = "None";
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void GetWritePort(string portname, string cbobaudrat)
        {
            this.port = objComPortConnectionClass.OpenPort(portname, Convert.ToInt32(cbobaudrat), 8, 300, 300);
            if (this.port != null)
            {
                MethodInvoker action = () => txtStatus.BackColor = System.Drawing.Color.Green;
                picLoading.BeginInvoke(action);
                action = () => btnConnect.Enabled = false;

                btnConnect.BeginInvoke(action);

                action = () => btnDisconnect.Enabled = true;

                btnDisconnect.BeginInvoke(action);


                action = () => gbPortInfo.Enabled = false;

                gbPortInfo.BeginInvoke(action);


                //readSMS();
            }

        }
        private void DisconnectPort()
        {
            objComPortConnectionClass.ClosePort(this.port);


            MethodInvoker action = () => txtStatus.BackColor = System.Drawing.Color.Red;
            picLoading.BeginInvoke(action);
            action = () => btnConnect.Enabled = true;

            btnConnect.BeginInvoke(action);

            action = () => btnDisconnect.Enabled = false;

            btnDisconnect.BeginInvoke(action);


            action = () => gbPortInfo.Enabled = true;

            gbPortInfo.BeginInvoke(action);





        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            picLoading.Visible = true;
            Start();
        }


        private void btnConnect_Click(object sender, EventArgs e)
        {
            GetWritePort(this.cboPortName.Text, this.cboBaudRate.Text);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectPort();
        }
        private void send()
        {
            // Start();
            string connectionString = "Data Source=74.86.97.85;Initial Catalog=SMSONBD;Integrated Security=False;User ID=abhi;Connect Timeout=15;Encrypt=False;Network Library=dbmssocn;Packet Size=4096;Password=Dynamic00000@";
            SqlCommand commd = new SqlCommand();
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                List<SMS_INFO> objListSMS_INFO = new List<SMS_INFO>();

                commd.Connection = con;
                commd.CommandText = "select * from SMSON_SEND_BOX where SMS_STATUS=0";
                con.Open();
                SqlDataReader dr = commd.ExecuteReader();
                while (dr.Read())
                {
                    SMS_INFO objSMS_INFO = new SMS_INFO();
                    objSMS_INFO.PHONE_NO = dr["MOBILE_NUMBER"].ToString();
                    objSMS_INFO.MESSAGE = dr["SMS_TEXT"].ToString();
                    objListSMS_INFO.Add(objSMS_INFO);
                }
                dr.Close();

                foreach (SMS_INFO OBJ in objListSMS_INFO)
                {
                    string portname = "";
                    cboPortName.Invoke(new MethodInvoker(delegate { portname = cboPortName.SelectedItem.ToString(); }));
                    string bound = "";
                    cboBaudRate.Invoke(new MethodInvoker(delegate { bound = cboBaudRate.SelectedItem.ToString(); }));
                    GetWritePort(portname, bound);
                    objComPortConnectionClass.sendMsg(this.port, OBJ.PHONE_NO, OBJ.MESSAGE);
                    DisconnectPort();
                }

                SqlDataAdapter da = new SqlDataAdapter(commd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                MethodInvoker action = () => dataGridView1.DataSource = dt;
                dataGridView1.BeginInvoke(action);

                con.Close();

                con.Open();
                commd.CommandText = "update SMSON_SEND_BOX set SMS_STATUS=1";
                commd.ExecuteNonQuery();

            }
            catch { }
            finally { con.Close(); }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            picLoading.Visible = true;
            Start();

        }
        public void Start()
        {
            ChangeControl(false);
            //this.picLoading.Enabled = true;
            if (!loader.IsBusy)
                this.loader.RunWorkerAsync();
        }
        private void ChangeControl(bool Flag)
        {
            this.btnConnect.Enabled = Flag;
            this.btnDisconnect.Enabled = Flag;
            this.btnCountSMS.Enabled = Flag;
            this.btnSend.Enabled = Flag;
            this.cboBaudRate.Enabled = Flag;
            this.cboPortName.Enabled = Flag;

        }


        private void loader_DoWork(object sender, DoWorkEventArgs e)
        {



            send();
        }

        private void loader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MethodInvoker action = () => picLoading.Visible = false;
            picLoading.BeginInvoke(action);
            ChangeControl(true);
        }
    }
}
