using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MethodAsync
{
    public partial class Default : System.Web.UI.Page
    {

        // This flag ensures that the user does not attempt
        // to restart the command or close the form while the 
        // asynchronous command is executing.
        private bool isExecuting = false;

        // Because the overloaded version of BeginExecuteReader
        // demonstrated here does not allow you to have the connection
        // closed automatically, this example maintains the 
        // connection object externally, so that it is available for closing.
        private SqlConnection connection = null;

        // You need this delegate to update the status bar.
        private delegate void DisplayStatusDelegate(string Text);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Button1.Click += new EventHandler(Button1_Click);
            }
        }

        private void DisplayStatus(string Text)
        {
            Label1.Text = Text;
        }

        private void HandleCallback(IAsyncResult result)
        {
            try
            {
                // Retrieve the original command object, passed
                // to this procedure in the AsyncState property
                // of the IAsyncResult parameter.
                SqlCommand command = (SqlCommand)result.AsyncState;
                SqlDataReader reader = command.EndExecuteReader(result);

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // resultsTextBox.InnerText += (Environment.NewLine + "\r\nId: ");
                        resultsTextBox.InnerText += reader.GetFieldValue<int>(0).ToString();// + "\t" + ((!rdr.IsDBNull(1)) ? rdr["batch_sendXML"].ToString() : ""));
                        resultsTextBox.InnerText += (Environment.NewLine);
                        //ScriptManager.RegisterClientScriptBlock(this, GetType(), "Alter", "document.getElementById('"+ resultsTextBox.ID + "').innerHTML += "+ reader.GetFieldValue<int>(0).ToString() +"", true);
                        ScriptManager.RegisterClientScriptBlock(this, GetType(), "Alter", "alert(resultsTextBox.ID);", true);
                        //ClientScript.RegisterClientScriptBlock(GetType(), "Alter", "document.getElementById('"+ resultsTextBox.ID + "').innerHTML += "+ reader.GetFieldValue<int>(0).ToString() + ";");
                        // DoIndependentWork();
                    }
                }

                // You may not interact with the form and its contents
                // from a different thread, and this callback procedure
                // is all but guaranteed to be running from a different thread
                // than the form. Therefore you cannot simply call code that 
                // fills the grid, like this:
                // FillGrid(reader);
                // Instead, you must call the procedure from the form's thread.
                // One simple way to accomplish this is to call the Invoke
                // method of the form, which calls the delegate you supply
                // from the form's thread. 
                // FillGridDelegate del = new FillGridDelegate(FillGrid);
                // Invoke(del, reader);
                // Do not close the reader here, because it is being used in 
                // a separate thread. Instead, have the procedure you have
                // called close the reader once it is done with it.
            }
            catch (Exception)
            {
                // Because you are now running code in a separate thread, 
                // if you do not handle the exception here, none of your other
                // code catches the exception. Because there is none of 
                // your code on the call stack in this thread, there is nothing
                // higher up the stack to catch the exception if you do not 
                // handle it here. You can either log the exception or 
                // invoke a delegate (as in the non-error case in this 
                // example) to display the error on the form. In no case
                // can you simply display the error without executing a delegate
                // as in the try block here. 
                // You can create the delegate instance as you 
                // invoke it, like this:
                // Invoke(new DisplayStatusDelegate(DisplayStatus), "Error: " + ex.Message);
            }
            finally
            {
                isExecuting = false;
            }
        }

        private string GetConnectionString()
        {
            // To avoid storing the connection string in your code, 
            // you can retrieve it from a configuration file. 

            // If you do not include the Asynchronous Processing=true name/value pair,
            // you wo not be able to execute the command asynchronously.

            return string.Format("DATA SOURCE={0}; INITIAL CATALOG={1}; USER ID={2}; Password={3}; MultipleActiveResultSets=True; Asynchronous Processing=true;", "mychurch.database.windows.net", "mychurch", "mychurch", "fxm@sterb1");
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (isExecuting)
            {
                ClientScript.RegisterStartupScript(GetType(), "Detalhes Exception", "alert('Already executing. Please wait until the current query has completed.');", true);
            }
            else
            {
                SqlCommand command = null;
                try
                {
                    DisplayStatus("Connecting...");
                    connection = new SqlConnection(GetConnectionString());
                    // To emulate a long-running query, wait for 
                    // a few seconds before retrieving the real data.
                    command = new SqlCommand("WAITFOR DELAY '0:0:5';" +
                                @"DECLARE @i int = 0
                                IF OBJECT_ID('TEMPTeste') IS NOT NULL DROP TABLE TEMPTeste
                                    CREATE TABLE TEMPTeste(
                                        batch_ID INT,
                                        batch_sendXML[xml]
                                    );
                                WHILE @i < 750
                                BEGIN
                                    INSERT INTO TEMPTeste

                                        SELECT batch_ID, batch_sendXML FROM eSocial_batches

                                    SET @i = @i + 1
                                END",
                        connection);

                    connection.Open();

                    DisplayStatus("Executing...");
                    isExecuting = true;
                    // Although it is not required that you pass the 
                    // SqlCommand object as the second parameter in the 
                    // BeginExecuteReader call, doing so makes it easier
                    // to call EndExecuteReader in the callback procedure.
                    // AsyncCallback callback = new AsyncCallback(HandleCallback);
                    // command.BeginExecuteReader(callback, command);
                    command.BeginExecuteReader();


                    #region SELECT ASYNC
                    //command = new SqlCommand(@"WAITFOR DELAY '0:0:5'; SELECT TOP 10 batch_ID FROM TEMPTeste", connection);

                    //DisplayStatus("Executing the SELECT...");
                    //isExecuting = true;
                    //// Although it is not required that you pass the 
                    //// SqlCommand object as the second parameter in the 
                    //// BeginExecuteReader call, doing so makes it easier
                    //// to call EndExecuteReader in the callback procedure.
                    //AsyncCallback callbackSelect = new AsyncCallback(HandleCallback);
                    //command.BeginExecuteReader(callbackSelect, command);
                    #endregion

                }
                catch (Exception ex)
                {
                    DisplayStatus("Error: " + ex.Message);
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}