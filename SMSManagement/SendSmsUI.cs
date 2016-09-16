using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using SMS.Helper;
using System.Data.SqlClient;

namespace SMSManagement
{
    public partial class SendSmsUI : Form
    {
        string strCommand = "";
        SerialPort port = new SerialPort();
        MODEL_SMS objMODEL_SMS = new MODEL_SMS();
        List<MODEL_SMS> objListMODEL_SMS = new List<MODEL_SMS>();
        SMS_FUNCTION objSMS_FUNCTION = new SMS_FUNCTION();
        ComPortConnectionClass objComPortConnectionClass = new ComPortConnectionClass();
        List<string> cboPortName = new List<string>();



        public static int count = 0;
        public SendSmsUI()
        {
            InitializeComponent();
            BindTime();
            /* Port Binding */
            BindPort();
            //txtStatus.BackColor = System.Drawing.Color.Red;
            FindActivePOrt();
        }
        private void BindPort()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                // Add all port names to the combo box:
                foreach (string port in ports)
                {
                    this.cboPortName.Add(port);
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
                    this.port = objComPortConnectionClass.OpenPort(port, Convert.ToInt32(9600), 8, 300, 300);
                    if (this.port != null)
                    {
                        try
                        {
                            int uCountSMS = objSMS_FUNCTION.CountSMSmessages(this.port);
                            if (uCountSMS >= 0)
                            {

                                lblActivePortName.Text = port;

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
        public void BindTime()
        {
            this.time.Text = DateTime.Now.ToString("hh:mm:ss tt");

        }
        private void GetWritePort(string portname, string cbobaudrat)
        {
            this.port = objComPortConnectionClass.OpenPort(portname, Convert.ToInt32(cbobaudrat), 8, 300, 300);
            if (this.port != null)
            {



                //readSMS();
            }

        }
        private void watch_Tick(object sender, EventArgs e)
        {
            BindTime();
        }
        private void DisconnectPort()
        {
            objComPortConnectionClass.ClosePort(this.port);








        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            picLoading.Visible = true;
            sucess.Visible = false;
            Start();
        }
        public void Start()
        {
            //ChangeControl(false);
            //this.picLoading.Enabled = true;
            if (!loader.IsBusy)
                this.loader.RunWorkerAsync();
        }

        private void loader_DoWork(object sender, DoWorkEventArgs e)
        {
            send();
        }

        private void loader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MethodInvoker action = () => picLoading.Visible = false;
            picLoading.BeginInvoke(action);

            action = () => sucess.Visible = true;
            sucess.BeginInvoke(action);


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
                    count++;
                }
                dr.Close();

                foreach (SMS_INFO OBJ in objListSMS_INFO)
                {
                    string portname = "";
                    lblActivePortName.Invoke(new MethodInvoker(delegate { portname = lblActivePortName.Text.ToString(); }));
                    string bound = "9600";

                    GetWritePort(portname, bound);
                    objComPortConnectionClass.sendMsg(this.port, OBJ.PHONE_NO, OBJ.MESSAGE);
                    DisconnectPort();
                }



                con.Close();

                con.Open();
                commd.CommandText = "update SMSON_SEND_BOX set SMS_STATUS=1";
                commd.ExecuteNonQuery();

            }
            catch { }
            finally
            {
                con.Close();
                MethodInvoker action = () => lblSent.Text = count.ToString();
                lblSent.BeginInvoke(action);


            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = true;
            btnStart.Enabled = false;
            timer1.Enabled = true;
            btnStart.Text = "Running ...";

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            timer1.Enabled = false;
            loader.CancelAsync();
            btnStart.Text = "Start";
        }
    }
}
