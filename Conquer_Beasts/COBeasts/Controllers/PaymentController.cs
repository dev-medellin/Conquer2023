using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PhoenixConquer.Controllers
{
    public class PaymentController : Controller
    {
        private class IPNContext
        {
            public HttpRequestBase IPNRequest { get; set; }

            public string RequestBody { get; set; }

            public string Verification { get; set; } 
        }

        [Route("paymentshandler")]
        [HttpPost]
        public ActionResult Receive()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IPNContext ipnContext = new IPNContext()
            {
                IPNRequest = Request
            };

            using (StreamReader reader = new StreamReader(ipnContext.IPNRequest.InputStream, Encoding.ASCII))
            {
                ipnContext.RequestBody = reader.ReadToEnd();
            }

            //Store the IPN received from 

            LogRequest(ipnContext);

            //Fire and forget verification task
            Task.Run(() => VerifyTask(ipnContext));

            //Reply back a 200 code
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void VerifyTask(IPNContext ipnContext)
        {
            try
            {
                var verificationRequest = WebRequest.Create("https://www.paypal.com/cgi-bin/webscr");

                //Set values for the verification request
                verificationRequest.Method = "POST";
                verificationRequest.ContentType = "application/x-www-form-urlencoded";

                //Add cmd=_notify-validate to the payload
                string strRequest = "cmd=_notify-validate&" + ipnContext.RequestBody;
                verificationRequest.ContentLength = strRequest.Length;

                //Attach payload to the verification request
                using (StreamWriter writer = new StreamWriter(verificationRequest.GetRequestStream(), Encoding.ASCII))
                {
                    writer.Write(strRequest);
                }

                //Send the request to PayPal and get the response
                using (StreamReader reader = new StreamReader(verificationRequest.GetResponse().GetResponseStream()))
                {
                    ipnContext.Verification = reader.ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                LogErr(exception.ToString());

                //Capture exception for manual investigation
            }

            ProcessVerificationResponse(ipnContext);
        }


        private void LogRequest(IPNContext ipnContext)
        {
            // Persist the request values into a database or temporary data store
        }
        Dictionary<string, double> itemsBuy = new Dictionary<string, double>() {
            { "1", 10.00 },
            { "2", 20.00 },
            { "3", 30.00 },
        };

        private void ProcessVerificationResponse(IPNContext ipnContext)
        {
            if (ipnContext.Verification.Equals("VERIFIED"))
            {
                // check that Payment_status=Completed
                // check that Txn_id has not been previously processed
                // check that Receiver_email is your Primary PayPal email
                // check that Payment_amount/Payment_currency are correct
                // process payment


                LogErr("VERIFIED " + ipnContext.RequestBody);
                string[] items = ipnContext.RequestBody.Split('&');
                Dictionary<string, string> paypalObjs = new Dictionary<string, string>();
                for (int i = 0; i < items.Length; i++)
                {
                    string[] splitted_items = items[i].Split('=');
                    paypalObjs.Add(splitted_items[0], splitted_items[1]);
                }
                if (paypalObjs.ContainsKey("receiver_email") && paypalObjs["receiver_email"].Replace("%40", "@") == "m.ibrahim99x@gmail.com"
                    && paypalObjs["payment_status"] == "Completed" && itemsBuy[paypalObjs["item_number"]] == double.Parse(paypalObjs["mc_gross"]))
                {
                    try
                    {
                        using (var conn = new MySqlConnection(UserController.connectionString()))
                        using (var cmd = new MySqlCommand("INSERT into payments values (@username,@txn_id,@item_number,@item_name,@date,0,@payment_gross" +
                            ",@mc_gross,@payer_id,@payment_date,@payment_status)", conn))
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@mc_gross", paypalObjs["mc_gross"]);
                            cmd.Parameters.AddWithValue("@payer_id", paypalObjs["payer_id"]);
                            cmd.Parameters.AddWithValue("@payment_date", paypalObjs["payment_date"]);
                            cmd.Parameters.AddWithValue("@txn_id", paypalObjs["txn_id"]);
                            cmd.Parameters.AddWithValue("@payment_gross", paypalObjs["payment_gross"]);
                            cmd.Parameters.AddWithValue("@payment_status", paypalObjs["payment_status"]);
                            cmd.Parameters.AddWithValue("@username", paypalObjs["custom"]);
                            cmd.Parameters.AddWithValue("@date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@item_name", paypalObjs["item_name"]);
                            cmd.Parameters.AddWithValue("@item_number", paypalObjs["item_number"]);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        LogErr(e.ToString());
                    }
                }
                else LogErr(paypalObjs["receiver_email"]);
                //  else
                //    return;
            }
            else if (ipnContext.Verification.Equals("INVALID"))
            {
                //Log for manual investigation
                LogErr("Invalid");
            }
            else
            {
                //Log error
                LogErr("ERROR");
            }
        }

        public static void LogErr(string err)
        {
            using (var conn = new MySqlConnection(UserController.connectionString()))
            using (var cmd = new MySqlCommand("INSERT into logs (data) VALUES (@e)", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@e", err);
                cmd.ExecuteNonQuery();
            }
        }
    }
}