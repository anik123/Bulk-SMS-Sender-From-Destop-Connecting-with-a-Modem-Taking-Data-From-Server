using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMS.Helper
{
    [Serializable]
    public class MODEL_SMS
    {  
        #region Private Variables
        private string _INDEX;
        private string _STATUS;
        private string _SENDER;
        private string _ALPHABET;
        private string _SENT;
        private string _MESSAGE;
        private decimal _RECIEVED_AMOUNT;
        private string _PHONE_NUMBER_FROM;
        private string _REF;
        private decimal _FEE;
        private decimal _BALANCE;
        private string _TRX_ID;
        private string _RECIEVED_DATE;
        private string _RECIEVED_TIME;
       
        
        #endregion

        #region Public Properties

        public string INDEX
        {
            get { return _INDEX; }
            set { _INDEX = value; }
        }

        public string STATUS
        {
            get { return _STATUS; }
            set { _STATUS = value; }
        }

        public string SENDER
        {
            get { return _SENDER; }
            set { _SENDER = value; }
        }

        public string ALPHABET
        {
            get { return _ALPHABET; }
            set { _ALPHABET = value; }
        }

        public string SENT
        {
            get { return _SENT; }
            set { _SENT = value; }
        }

        public string MESSAGE
        {
            get { return _MESSAGE; }
            set { _MESSAGE = value; }
        }

        public decimal RECIEVED_AMOUNT
        {
            get { return _RECIEVED_AMOUNT; }
            set { _RECIEVED_AMOUNT = value; }
        }

        public string PHONE_NUMBER_FROM
        {
            get { return _PHONE_NUMBER_FROM; }
            set { _PHONE_NUMBER_FROM = value; }
        }

        public string REF
        {
            get { return _REF; }
            set { _REF = value; }
        }
        public decimal FEE
        {
            get { return _FEE; }
            set { _FEE = value; }
        }
        public decimal BALANCE
        {
            get { return _BALANCE; }
            set { _BALANCE = value; }
        }

        public string TRX_ID
        {
            get { return _TRX_ID; }
            set { _TRX_ID = value; }
        }

        public string RECIEVED_DATE
        {
            get { return _RECIEVED_DATE; }
            set { _RECIEVED_DATE = value; }
        }


        public string RECIEVED_TIME
        {
            get { return _RECIEVED_TIME; }
            set { _RECIEVED_TIME = value; }
        }
        #endregion
    }
}
