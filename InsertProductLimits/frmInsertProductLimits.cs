using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using TTUSAPI;


namespace InsertProductLimits
{
    public partial class frmInsertProductLimits : Form
    {
        private TTUSAPI.TTUSApi m_TTUSAPI;

        private Dictionary<string, TTUSAPI.DataObjects.User> m_Users;  // User objects
        private Dictionary<int, TTUSAPI.DataObjects.AccountGroup> m_accountGroups;  // Account groups
        private Dictionary<int, TTUSAPI.DataObjects.Gateway> m_colGateways;  // Gateway information
        private Dictionary<string, TTUSAPI.DataObjects.GatewayLogin> m_GatewayLogins;  // User objects
        private Dictionary<string, TTUSAPI.DataObjects.Product> m_Products;  // User objects
        private Dictionary<int, TTUSAPI.DataObjects.MarketProductCatalog> m_MarketProductCatalogs;  // User objects

        private int piInit = 0;
        private int piProducts = 0;

        public frmInsertProductLimits()
        {
            InitializeComponent();
            initTTUSAPI();
        }

        public void initTTUSAPI()
        {
            UpdateStatusBar("Initializing...");
            
            m_TTUSAPI = new TTUSAPI.TTUSApi(TTUSAPI.TTUSApi.StartupMode.Normal);  //Create an Instance of the TTUSAPI
            //Callbacks
            m_TTUSAPI.OnConnectivityStatusUpdate += new TTUSApi.ConnectivityStatusUpdateHandler(m_TTUSAPI_OnConnectivityStatusUpdate);
            m_TTUSAPI.OnLoginStatusUpdate += new TTUSApi.LoginStatusHandler(m_TTUSAPI_OnLoginStatusUpdate);  //Login status update
            m_TTUSAPI.OnInitializeComplete += new TTUSApi.InitializeCompleteHandler(m_TTUSAPI_OnInitializeComplete); //API Initialization
            m_TTUSAPI.OnAccountGroupUpdate += new TTUSApi.AccountGroupUpdateHandler(m_TTUSAPI_OnAccountGroupUpdate); //Account group updates
            m_TTUSAPI.OnGatewayUpdate += new TTUSApi.GatewayUpdateHandler(m_TTUSAPI_OnGatewayUpdate); //Gateway information
            m_TTUSAPI.OnUserUpdate += new TTUSApi.UserUpdateHandler(m_TTUSAPI_OnUserUpdate);  //Callback for user downloads
            m_TTUSAPI.OnGatewayLoginUpdate += new TTUSApi.GatewayLoginUpdateHandler(m_TTUSAPI_OnGatewayLoginUpdate);
            m_TTUSAPI.OnProductUpdate += new TTUSApi.ProductUpdateHandler(m_TTUSAPI_OnProductUpdate);


            btnConnect.Enabled = false;
            btnUpdate.Enabled = false;
        }

        #region TTUS Callbacks

        void m_TTUSAPI_OnConnectivityStatusUpdate(object sender, ConnectivityStatusEventArgs e)
        {
            UpdateStatusBar("Found a TT User Setup Server, You can now login...");
            this.btnConnect.Enabled = true;
        }

        void m_TTUSAPI_OnProductUpdate(object sender, ProductUpdateEventArgs e)
        {
            // create product limit profile
            // lookup login
            // add product limit profile to gateway login

            m_MarketProductCatalogs = e.Products;

            UpdateStatusBar("Downloaded Products.");
            piProducts = 1;
            if (piInit == 1)
            {
                btnUpdate.Enabled = true;
            }
        }
        
        void m_TTUSAPI_OnGatewayLoginUpdate(object sender, GatewayLoginUpdateEventArgs e)
        {
            m_GatewayLogins = e.GatewayLogins;
        }

        void m_TTUSAPI_OnLoginStatusUpdate(object sender, LoginStatusEventArgs e)
        {
            if (e.LoginResultCode == TTUSAPI.LoginResultCode.Success)
            {
                UpdateStatusBar("Login was Successful");
                Console.WriteLine("Login was Successful");
                //We have successfully logged in, so request users and FA Servers...
                m_TTUSAPI.Initialize();   //Initialize the API to get all of the User, Fix Adapter, Account, and MGT data
                m_TTUSAPI.GetProducts();
            }
            else
            {
                UpdateStatusBar("Error:  Login failed");
                Environment.Exit(0);
            }
        }

        void m_TTUSAPI_OnInitializeComplete(object sender, TTUSAPI.InitializeCompleteEventArgs e)
        {
            UpdateStatusBar("Initialization Complete.");
            piInit = 1;
            if (piProducts == 1)
            {
                btnUpdate.Enabled = true;
            }
        }

        //Callback for TTUS User Updates
        void m_TTUSAPI_OnUserUpdate(object sender, UserUpdateEventArgs e)
        {
            try
            {
                if (e.Type == UpdateType.Download)
                {
                    m_Users = e.Users;
                }
                //Update dictionary with any user updates
                else if (e.Type == UpdateType.Added || e.Type == UpdateType.Changed || e.Type == UpdateType.Relationship)
                {
                    foreach (KeyValuePair<string, TTUSAPI.DataObjects.User> userItem in e.Users)
                    {
                        m_Users[userItem.Key] = userItem.Value;
                    }
                }
                //Remove user from dictionary
                else if (e.Type == UpdateType.Deleted)
                {
                    foreach (KeyValuePair<string, TTUSAPI.DataObjects.User> userItem in e.Users)
                    {
                        m_Users.Remove(userItem.Key);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an exception in the OnUserUpdate callback: " + ex.Message);
            }
        }

        void m_TTUSAPI_OnAccountGroupUpdate(object sender, AccountGroupUpdateEventArgs e)
        {
            try
            {
                // Populate dictionary with Account Groups
                if (e.Type == UpdateType.Download)
                    m_accountGroups = e.AccountGroups;
                // Add Account Group
                else if (e.Type == UpdateType.Added)
                {
                    foreach (KeyValuePair<int, TTUSAPI.DataObjects.AccountGroup> acctGroup in e.AccountGroups)
                    {
                        m_accountGroups[acctGroup.Key] = acctGroup.Value;
                    }
                }
                // Update Account Group
                else if ((e.Type == UpdateType.Changed || e.Type == UpdateType.Relationship))
                {
                    foreach (KeyValuePair<int, TTUSAPI.DataObjects.AccountGroup> acctGroup in e.AccountGroups)
                    {
                        m_accountGroups[acctGroup.Key] = acctGroup.Value;
                    }
                }
                // Delete Account Group
                else if (e.Type == UpdateType.Deleted)
                {
                    foreach (KeyValuePair<int, TTUSAPI.DataObjects.AccountGroup> acctGroup in e.AccountGroups)
                    {
                        m_accountGroups.Remove(acctGroup.Key);
                    }
                }

                //foreach (TTUSAPI.DataObjects.AccountGroup acct in m_accountGroups.Values)
                //{
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an exception in the OnAccountGroupUpdate callback: " + ex.Message);
            }
        }

        // Populate dictionary of Gateway Information
        void m_TTUSAPI_OnGatewayUpdate(object sender, GatewayUpdateEventArgs e)
        {
            m_colGateways = e.Gateways;
        }

        #endregion

        #region UpdateUIRegion



        /// <summary>
        /// Update the status bar and write the message to the console in a thread safe way.
        /// </summary>
        /// <param name="message">Message to update the status bar with.</param>
        delegate void UpdateStatusBarCallback(string message);
        public void UpdateStatusBar(string message)
        {
            if (this.InvokeRequired)
            {
                UpdateStatusBarCallback statCB = new UpdateStatusBarCallback(UpdateStatusBar);
                this.Invoke(statCB, new object[] { message });
            }
            else
            {
                // Update the status bar.
                toolStripStatusLabel1.Text = message;

                // Also write this message to the console.
                Console.WriteLine(message);
            }
        }

        #endregion


        //For a given product type, return the proper ID number
        private int getProductTypeId(string productType)
        {
            if (productType.ToUpper().Equals("FUTURE"))
                return 1;
            else if (productType.ToUpper().Equals("SPREAD"))
                return 2;
            else if (productType.ToUpper().Equals("OPTION"))
                return 3;
            else
                return -1;
        }

        //Retrieve the ID for a given exchange name
        private int getGatewayId(string exchange)
        {
            foreach (TTUSAPI.DataObjects.Gateway gw in m_colGateways.Values) //Loop through gateways to map gateway name to gateway ID
            {
            }
            return -1;
        }

        #region FormEventHandlers

        private void btnUpdate_Click(object sender, EventArgs e)
        {
  //          TTUSAPI.DataObjects.AccountGroup acctGroup;

//            if (gatewayId != -1 && productTypeId != -1 && uint.TryParse(txtMaxPosition.Text, out maxPos) && uint.TryParse(txtMaxLongShort.Text, out maxLongShort))
 //           {
            foreach (TTUSAPI.DataObjects.AccountGroup acctGroup in m_accountGroups.Values) // Loop through downloaded account groups
            {
                // Clients should type in Group name to search for
                if (acctGroup.Name.ToUpper() == txtAccountGroupName.Text.ToUpper())
                {
                    TTUSAPI.DataObjects.AccountGroupProfile acctGroupProfile = new TTUSAPI.DataObjects.AccountGroupProfile(acctGroup);

                    TTUSAPI.DataObjects.AccountGroupProductLimitProfile productLimit;
                    //bool productLimitSet = false;
                    //foreach (TTUSAPI.DataObjects.AccountGroupProductLimit prodLimit in acctGroupProfile.ProductLimits.Values) //Loop through product limits associated with the account
                    //{
                    //    //Replace account product limits that match ones in the update file
                    //    if (prodLimit.GatewayID == gatewayId && prodLimit.ProductTypeID == productTypeId && prodLimit.Product.ToUpper().Equals("ES"))
                    //    {
                    //        productLimit = new TTUSAPI.DataObjects.AccountGroupProductLimitProfile(prodLimit); //Create copy of Product Limit
                    //        productLimit.MaxPositionPerContract = maxPos; //Set Maximum Position
                    //        productLimit.MaxProductLongShort = maxLongShort; // Set Maximum Long/Short Position
                    //        acctGroupProfile.AddProductLimit(productLimit); //Add product limit to account
                    //        productLimitSet = true;
                    //        break;
                    //    }
                    //}
                    //Create new product limits if they don't currently exist for the account
                    //if (productLimitSet == false)
                    //{
                        
                    // Loop through Markets
                    foreach (TTUSAPI.DataObjects.MarketProductCatalog marketPC in m_MarketProductCatalogs.Values)
                    {
                        // Loop through GWs
                        foreach (TTUSAPI.DataObjects.GatewayProductCatalog gwPC in marketPC.GatewayProductCatalogs.Values)
                        {
                            // Loop through Product Types
                            foreach (TTUSAPI.DataObjects.ProductCatalog productTypes in gwPC.ProductCatalogs.Values)
                            {
                                // Do only for Futures
                                if (productTypes.ProductType.ProductTypeID == 1)
                                {
                                    // Loop through Products
                                    foreach (TTUSAPI.DataObjects.Product prod in productTypes.Products.Values)
                                    {
                                        productLimit = new TTUSAPI.DataObjects.AccountGroupProductLimitProfile(); //Create new Product Limit

                                        //Assign values to product limit from update file object
                                        productLimit.GatewayID = gwPC.Gateway.GatewayID;
                                        productLimit.Product = prod.Name;
                                        productLimit.ProductTypeID = 1;  // Insert for Futures
                                        productLimit.MaxOutrightsOrderQty = 1;
                                        productLimit.MaxSpreadsOrderQty = 1;
                                        productLimit.AllowTradingOutrights = true;
                                        productLimit.AllowTradingSpreads = true;

                                        //Add product limit to account group
                                        acctGroupProfile.AddProductLimit(productLimit);

                                        ResultStatus res = m_TTUSAPI.UpdateAccountGroup(acctGroupProfile); //Send updates to TTUS Server
                                        if (res.Result == ResultType.SentToServer) //On Success
                                        {
                                            UpdateStatusBar("Updated account Group; " + acctGroup.Name);
                                        }
                                        else if (res.Result == ResultType.ValuesUnchanged) //No Change
                                        {
                                            UpdateStatusBar("Account Group; " + acctGroup.Name + " unchanged");
                                        }
                                        else //On Failure
                                        {
                                            UpdateStatusBar("Failed to update account Group; " + acctGroup.Name + ", " + res.ErrorMessage);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

 

        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            // shut down the API
            m_TTUSAPI.Logoff();  // Logoff the TTUS Server
            m_TTUSAPI.Dispose();

            base.Dispose(disposing);
        }

        private void btnConnect_Click_1(object sender, EventArgs e)
        {
            m_TTUSAPI.Login(txtUsername.Text, txtPassword.Text);

        }

    }
}
