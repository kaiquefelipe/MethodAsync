using System;
using System.Collections.Generic;
using System.Data;
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
        private SqlConnection connection = null;
        private SqlCommand command = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                Button1.Click += new EventHandler(Button1_Click);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                SetLabel("Connecting...");
                #region USING BeginExecuteReader AND EndExecuteReader
                //BeginEventHandler begin_hanlder = new BeginEventHandler(BeginExecuteReader);
                //EndEventHandler end_handler = new EndEventHandler(EndExecuteReader);
                //AddOnPreRenderCompleteAsync(begin_hanlder, end_handler);
                #endregion

                #region USING BeginExecuteNonQuery AND EndExecuteNonQuery
                BeginEventHandler begin_hanlder = new BeginEventHandler(BeginNonQuery);
                EndEventHandler end_handler = new EndEventHandler(EndNonQuery);
                AddOnPreRenderCompleteAsync(begin_hanlder, end_handler);
                #endregion
            }
            catch (Exception ex)
            {
                SetLabel("Error: " + ex.Message);
                if (connection != null)
                    connection.Close();
            }
        }

        #region USING BeginExecuteReader AND EndExecuteReader
        IAsyncResult BeginExecuteReader(object sender, EventArgs e, AsyncCallback callback, object state)
        {
            connection = new SqlConnection(GetConnectionString());
            // To emulate a long-running query, wait for 
            // a few seconds before retrieving the real data.
            command = new SqlCommand(
                          @"SELECT Batch_ID, Batch_SendXml FROM eSocial_Batches",
                connection);

            connection.Open();

            SetLabel("Executing...");
            return command.BeginExecuteReader(callback, state);
        }

        void EndExecuteReader(IAsyncResult asyncResult)
        {           
            List<string> lString = new List<string>();
            List<string> lStringXml = new List<string>();
            try
            {
                SqlDataReader reader = command.EndExecuteReader(asyncResult);
                while (!reader.IsClosed && reader.HasRows && reader.Read() && asyncResult.IsCompleted)
                {
                    lString.Add(reader["Batch_ID"].ToString() + Environment.NewLine);
                    lStringXml.Add(reader["Batch_SendXml"].ToString() + Environment.NewLine);
                    //for (int i = 0; i < reader.FieldCount; i++)
                    //{
                    //    strinng += reader.GetValue(i).ToString();
                    //}
                }

                for (int i = 0; i < lString.Count; i++)
                {
                    resultsTextBox.InnerText += lString[i];
                    resultsTextBox.InnerText += lStringXml[i];
                    resultsTextBox.InnerText += "-------------------------------------------------";
                }
                ShowMessage("HAHA! DEU CERTOOO! OK");
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message);
            }
        }

        #endregion

        #region USING BeginExecuteNonQuery AND EndExecuteNonQuery
        IAsyncResult BeginNonQuery(object sender, EventArgs e, AsyncCallback callback, object state)
        {
            connection = new SqlConnection(GetConnectionString());
            // To emulate a long-running query, wait for 
            // a few seconds before retrieving the real data.
            command = new SqlCommand(
                        @"DECLARE @i int = 0
                                IF OBJECT_ID('TEMPTeste') IS NOT NULL DROP TABLE TEMPTeste
                                    CREATE TABLE TEMPTeste(
                                        batch_ID INT,
                                        batch_sendXML[xml]
                                    );
                                WHILE @i < 150
                                BEGIN
                                    INSERT INTO TEMPTeste

                                        SELECT batch_ID, batch_sendXML FROM eSocial_batches

                                    SET @i = @i + 1
                                END",
                connection);

            connection.Open();

            SetLabel("Executing...");
            return command.BeginExecuteNonQuery(callback, state);
        }

        void EndNonQuery(IAsyncResult asyncResult)
        {
            try
            {
                int rowCount = command.EndExecuteNonQuery(asyncResult);
                
                string rowText = " rows affected.";
                if (rowCount == 1)                
                    rowText = " row affected.";

                rowText = rowCount + rowText;
                
                ShowMessage(rowText);
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message);
            }
        }
        #endregion

        private void ShowMessage(string message)
        {
            Label1.Text = message;
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "Alter", "<script>alert('"+ Label1.Text + "');</script>", false);
        }

        private void SetLabel(string v)
        {
            Label1.Text = v;
        }

        private string GetConnectionString()
        {
            return string.Format("DATA SOURCE={0}; INITIAL CATALOG={1}; USER ID={2}; Password={3}; MultipleActiveResultSets=True; Asynchronous Processing=true;", "mychurch.database.windows.net", "mychurch", "mychurch", "fxm@sterb1");
        }
    }
}