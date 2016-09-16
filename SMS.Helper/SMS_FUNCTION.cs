using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO.Ports;

namespace SMS.Helper
{
    public class SMS_FUNCTION
    {
        #region Read SMS

        public AutoResetEvent receiveNow;

        public List<MODEL_SMS> ReadSMS(SerialPort port, string p_strCommand)
        {

            // Set up the phone and read the messages
            List<MODEL_SMS> messages = null;
            try
            {

                #region Execute Command
                // Check connection
                ComPortConnectionClass.ExecCommand(port, "AT", 300, "No phone connected");
                // Use message format "Text mode"
                ComPortConnectionClass.ExecCommand(port, "AT+CMGF=1", 300, "Failed to set message format.");
                // Use character set "PCCP437"

                ///ComPortConnectionClass.ExecCommand(port,"AT+CSCS=\"PCCP437\"", 300, "Failed to set character set.");


                //string manufacturer = ComPortConnectionClass.ExecCommand(port, "AT+CGMI", 300, "Failed to select message storage.");
                //string model = ComPortConnectionClass.ExecCommand(port, "AT+CGMM", 300, "Failed to select message storage.");
                //string IMEI = ComPortConnectionClass.ExecCommand(port, "AT+CGSN", 300, "Failed to select message storage.");
                //string software = ComPortConnectionClass.ExecCommand(port, "AT+CGMR", 300, "Failed to select message storage.");
                ////string MSISDN = ComPortConnectionClass.ExecCommand(port, "AT+CNUM", 300, "Failed to select message storage.");
                //string IMSI = ComPortConnectionClass.ExecCommand(port, "AT+CIMI", 300, "Failed to select message storage.");

                ////string Pstatus = ComPortConnectionClass.ExecCommand(port, "AT+CPAS", 300, "Failed to select message storage.");
                ////string Nstatus = ComPortConnectionClass.ExecCommand(port, "AT+CREG", 300, "Failed to select message storage.");
                //string battery = ComPortConnectionClass.ExecCommand(port, "AT+CBC", 300, "Failed to select message storage.");
                ////string read = ComPortConnectionClass.ExecCommand(port, "AT+CMGR", 300, "Failed to select message storage.");
                ////string IMSI = ComPortConnectionClass.ExecCommand(port, "AT+CIMI", 300, "Failed to select message storage.");


                // Select SIM storage
                //string input1 = ComPortConnectionClass.ExecCommand(port, "AT+CPMS=\"SM\"", 300, "Failed to select message storage.");               
                // Read the messages
                //string input = ComPortConnectionClass.ExecCommand(port, p_strCommand, 5000, "Failed to read the messages.");

                string manufacturer = ComPortConnectionClass.ExecCommand(port, "AT", 300, "Failed to select message storage.");
                string model = ComPortConnectionClass.ExecCommand(port, "ATE1", 300, "Failed to select message storage.");
                string IMEI = ComPortConnectionClass.ExecCommand(port, "AT+IPR=115200", 300, "Failed to select message storage.");
                string software = ComPortConnectionClass.ExecCommand(port, "AT&W", 300, "Failed to select message storage.");


                string manufacturer1 = ComPortConnectionClass.ExecCommand(port, "AT", 300, "Failed to select message storage.");
                string model1 = ComPortConnectionClass.ExecCommand(port, "AT+CMGF=1", 300, "Failed to select message storage.");
                //string input = ComPortConnectionClass.ExecCommand(port, "AT+CMGL=\"ALL\"", 300, "Failed to select message storage.");
                //string input = ComPortConnectionClass.ExecCommand(port, "AT+CMGL=\"REC UNREAD\"", 300, "Failed to select message storage.");
                string input = ComPortConnectionClass.ExecCommand(port, p_strCommand, 300, "Failed to select message storage.");

                #endregion

                #region Parse messages
                messages = ParseMessages(input);
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (messages != null)
                return messages;
            else
                return null;

        }
        public List<MODEL_SMS> ParseMessages(string input)
        {
            List<MODEL_SMS> messages = new List<MODEL_SMS>();
            try
            {
                Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n");
                Match m = r.Match(input);
                while (m.Success)
                {
                    MODEL_SMS msg = new MODEL_SMS();
                    msg.INDEX = m.Groups[1].Value;
                    msg.STATUS = m.Groups[2].Value;
                    msg.SENDER = m.Groups[3].Value;
                    msg.ALPHABET = m.Groups[4].Value;
                    msg.SENT = m.Groups[5].Value;
                    msg.MESSAGE = m.Groups[6].Value;
                    string msg_MESSAGE = msg.MESSAGE;//.Replace(" ","&nbps");
                    bool messageStartType1=msg.MESSAGE.StartsWith("You have recieved tk",System.StringComparison.OrdinalIgnoreCase);
                    bool messageStartType2=msg.MESSAGE.StartsWith("Cash in tk",System.StringComparison.OrdinalIgnoreCase);
                    if (messageStartType1)
                    {
                        #region received amount
                        /////find received amount from text message
                        int first = msg_MESSAGE.IndexOf("tk", System.StringComparison.OrdinalIgnoreCase);
                        //int last =msg_MESSAGE.LastIndexOf("tk", System.StringComparison.OrdinalIgnoreCase);
                        int IndexOfFrom = msg_MESSAGE.IndexOf("from", System.StringComparison.OrdinalIgnoreCase);
                        int IndexRef = msg_MESSAGE.IndexOf("Ref", System.StringComparison.OrdinalIgnoreCase);
                        int IndexFee = msg_MESSAGE.IndexOf("Fee", System.StringComparison.OrdinalIgnoreCase);
                        int IndexBalance = msg_MESSAGE.IndexOf("Balance", System.StringComparison.OrdinalIgnoreCase);
                        int IndexTrxID = msg_MESSAGE.IndexOf("TrxID", System.StringComparison.OrdinalIgnoreCase);
                        int Indexat = msg_MESSAGE.IndexOf("at", System.StringComparison.OrdinalIgnoreCase);
                        int IndexColon = msg_MESSAGE.IndexOf(":", System.StringComparison.OrdinalIgnoreCase);
                        string newstring = msg_MESSAGE.Substring(first + 2, IndexOfFrom - first - 2);
                        msg.RECIEVED_AMOUNT = Convert.ToDecimal(newstring.Trim());
                        #endregion

                        #region number
                        msg.PHONE_NUMBER_FROM = msg_MESSAGE.Substring(IndexOfFrom + 4, IndexRef - IndexOfFrom - 6);
                        #endregion

                        #region Ref
                        msg.REF = msg_MESSAGE.Substring(IndexRef + 3, IndexFee - IndexRef - 5);
                        #endregion

                        #region Fee
                        string fee = msg_MESSAGE.Substring(IndexFee + 6, IndexBalance - IndexFee - 8);
                        msg.FEE = Convert.ToDecimal(fee.Trim());
                        #endregion

                        #region Balance
                        string BALANCE = msg_MESSAGE.Substring(IndexBalance + 10, IndexTrxID - IndexBalance - 12);
                        msg.BALANCE = Convert.ToDecimal(BALANCE.Trim());
                        #endregion

                        #region Fee
                        msg.TRX_ID = msg_MESSAGE.Substring(IndexTrxID + 6, Indexat - IndexTrxID - 6).Trim();
                        #endregion

                        #region Date
                        msg.RECIEVED_DATE = msg_MESSAGE.Substring(Indexat + 2, IndexColon - Indexat - 4).Trim();
                        //msg.RECIEVED_DATE = Convert.ToDateTime(date);
                        #endregion

                        #region Date
                        msg.RECIEVED_TIME = msg_MESSAGE.Substring(IndexColon - 2, IndexColon-IndexColon+5).Trim();
                        #endregion
                    }
                    else if (messageStartType2)
                    {
                        #region received amount
                        int first = msg_MESSAGE.IndexOf("tk", System.StringComparison.OrdinalIgnoreCase);
                        int IndexOfFrom = msg_MESSAGE.IndexOf("from", System.StringComparison.OrdinalIgnoreCase);
                        int Indexsuccessful = msg_MESSAGE.IndexOf("successful", System.StringComparison.OrdinalIgnoreCase);
                        //int IndexRef = msg_MESSAGE.IndexOf("Ref", System.StringComparison.OrdinalIgnoreCase);
                        int IndexFee = msg_MESSAGE.IndexOf("Fee", System.StringComparison.OrdinalIgnoreCase);
                        int IndexBalance = msg_MESSAGE.IndexOf("Balance", System.StringComparison.OrdinalIgnoreCase);
                        int IndexTrxID = msg_MESSAGE.IndexOf("TrzID", System.StringComparison.OrdinalIgnoreCase);
                        //int IndexTrxID = msg_MESSAGE.IndexOf("TrzID", System.StringComparison.OrdinalIgnoreCase);
                        int Indexat = msg_MESSAGE.IndexOf("at", System.StringComparison.OrdinalIgnoreCase);
                        int IndexColon = msg_MESSAGE.IndexOf(":", System.StringComparison.OrdinalIgnoreCase);
                        string newstring = msg_MESSAGE.Substring(first + 2, IndexOfFrom - first - 2);
                        msg.RECIEVED_AMOUNT = Convert.ToDecimal(newstring.Trim());
                        #endregion
                        #region number
                        msg.PHONE_NUMBER_FROM = msg_MESSAGE.Substring(IndexOfFrom + 4, Indexsuccessful - IndexOfFrom - 4);
                        #endregion

                        //#region Ref
                        //msg.REF = msg_MESSAGE.Substring(IndexRef + 3, IndexFee - IndexRef - 5);
                        //#endregion

                        #region Fee
                        string fee = msg_MESSAGE.Substring(IndexFee + 6, IndexBalance - IndexFee - 8);
                        msg.FEE = Convert.ToDecimal(fee.Trim());
                        #endregion

                        #region Balance
                        string BALANCE = msg_MESSAGE.Substring(IndexBalance + 10, IndexTrxID - IndexBalance - 12);
                        msg.BALANCE = Convert.ToDecimal(BALANCE.Trim());
                        #endregion

                        #region Fee
                        msg.TRX_ID = msg_MESSAGE.Substring(IndexTrxID + 6, Indexat - IndexTrxID - 6).Trim();
                        #endregion

                        #region Date
                        msg.RECIEVED_DATE = msg_MESSAGE.Substring(Indexat + 2, IndexColon - Indexat - 4).Trim();
                        //msg.RECIEVED_DATE = Convert.ToDateTime(date);
                        #endregion

                        #region Date
                        msg.RECIEVED_TIME = msg_MESSAGE.Substring(IndexColon - 2, IndexColon - IndexColon + 5).Trim();
                        #endregion
                    }
                    else
                    {
                        msg.RECIEVED_AMOUNT = 0;
                    }
                    //int first = str.IndexOf("silly");
                    //int last = str.LastIndexOf("silly");
                    //string str2 = str.Substring(first, last - first);
                    //if (Regex.IsMatch("testtest", @"^\w+$"))
                    //{
                    //    string.
                    //    // Do something here
                    //}
                    //msg.RECIEVED_AMOUNT =
                    //msg.PHONE_NUMBER_FROM =
                    //msg.REF =
                    //msg.BALANCE =
                    //msg.TRX_ID =
                    //msg.RECIEVED_DATE =
                    //msg.RECIEVED_TIME=

                    messages.Add(msg);

                    m = m.NextMatch();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return messages;
        }

        #endregion
        public int CountSMSmessages(SerialPort port)
        {
            int CountTotalMessages = 0;
            try
            {

                #region Execute Command

                string recievedData = ComPortConnectionClass.ExecCommand(port, "AT", 300, "No phone connected at ");
                recievedData = ComPortConnectionClass.ExecCommand(port, "AT+CMGF=1", 300, "Failed to set message format.");
                String command = "AT+CPMS?";
                recievedData = ComPortConnectionClass.ExecCommand(port, command, 1000, "Failed to count SMS message");
                int uReceivedDataLength = recievedData.Length;

                #endregion

                #region If command is executed successfully
                if ((recievedData.Length >= 45))//&& (recievedData.StartsWith("AT+CPMS?")))
                {

                    #region Parsing SMS
                    string[] strSplit = recievedData.Split(',');
                    string strMessageStorageArea1 = strSplit[0];     //SM
                    string strMessageExist1 = strSplit[1];           //Msgs exist in SM
                    #endregion

                    #region Count Total Number of SMS In SIM
                    CountTotalMessages = Convert.ToInt32(strMessageExist1);
                    #endregion

                }
                #endregion

                #region If command is not executed successfully
                else if (recievedData.Contains("ERROR"))
                {

                    #region Error in Counting total number of SMS
                    string recievedError = recievedData;
                    recievedError = recievedError.Trim();
                    recievedData = "Following error occured while counting the message" + recievedError;
                    #endregion

                }
                #endregion

                return CountTotalMessages;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
