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

namespace SMSManagement
{
    public partial class Form1 : Form
    {
        string strCommand = "";
        SerialPort port = new SerialPort();
        MODEL_SMS objMODEL_SMS = new MODEL_SMS();
        List<MODEL_SMS> objListMODEL_SMS = new List<MODEL_SMS>();
        SMS_FUNCTION objSMS_FUNCTION = new SMS_FUNCTION();
        ComPortConnectionClass objComPortConnectionClass = new ComPortConnectionClass();
  
        public Form1()
        {
            InitializeComponent();
            BindPort();
            txtStatus.BackColor = System.Drawing.Color.Red;
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

        private void GetWritePort()
        {
            this.port = objComPortConnectionClass.OpenPort(this.cboPortName.Text, Convert.ToInt32(this.cboBaudRate.Text), 8,300, 300);
            if (this.port != null)
            {
                txtStatus.BackColor = System.Drawing.Color.Green;
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                gbPortInfo.Enabled = false;
                readSMS();
            }

        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            GetWritePort();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            objComPortConnectionClass.ClosePort(this.port);
            txtStatus.BackColor = System.Drawing.Color.Red;
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            gbPortInfo.Enabled = true;
        }

        private void readSMS()
        {
            lvwMessages.Items.Clear();
            try
            {
                //count SMS 
                int uCountSMS = objSMS_FUNCTION.CountSMSmessages(this.port);
                if (uCountSMS >= 0)
                {

                    #region Command
                    //strCommand = "AT+CMGR=\"ALL\"";
                    strCommand = "AT+CMGL=\"ALL\"";
                    //strCommand = "AT+CMGL=\"REC UNREAD\"";
                    //strCommand = "AT+CMGL=\"STO SENT\"";
                    //strCommand = "AT+CMGL=\"STO UNSENT\"";
                    #endregion

                    // If SMS exist then read SMS
                    #region Read SMS
                    //.............................................. Read all SMS ....................................................
                    objListMODEL_SMS = objSMS_FUNCTION.ReadSMS(this.port, strCommand);
                    foreach (MODEL_SMS msg in objListMODEL_SMS)
                    {
                        ListViewItem item = new ListViewItem(new string[] { msg.INDEX, msg.SENT, msg.SENDER, msg.MESSAGE, 
                            msg.RECIEVED_AMOUNT.ToString(), 
                            msg.PHONE_NUMBER_FROM,
                            msg.REF,
                            msg.FEE.ToString(),
                            msg.BALANCE.ToString(),
                            msg.TRX_ID,                            
                            msg.RECIEVED_TIME,
                            msg.RECIEVED_DATE});
                        item.Tag = msg;
                        lvwMessages.Items.Add(item);
                    }
                    txtCountedSMS.Text = uCountSMS.ToString();
                    #endregion

                }
                else
                {
                    lvwMessages.Clear();
                    //MessageBox.Show("There is no message in SIM");
                    txtCountedSMS.Text = "0";

                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private void btnCountSMS_Click(object sender, EventArgs e)
        {
            FindActivePOrt();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendSMS obj = new SendSMS();
            obj.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
