using MySql.Data.MySqlClient;
using PhoenixConquer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace PhoenixConquer.Controllers
{
    public class UserController : Controller
    {
        internal static string connectionString()
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["accountsDB"];
        }

        #region Register
        [Route("register")]
        [HttpGet]
        public ActionResult register()
        {
            if (Session["user"] != null)
                Response.Redirect("~/home");
            return View();
        }
        private bool RegisterAccount(RegisterAccount model)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString()))
                using (var cmd = new MySqlCommand("INSERT into accounts (Username,Password,Email,Creation) values (@u,@p,@e,@c)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@u", model.Username);
                    cmd.Parameters.AddWithValue("@p", model.Password);
                    cmd.Parameters.AddWithValue("@e", model.Email);
                    cmd.Parameters.AddWithValue("@c", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        [Route("register")]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(RegisterAccount model, bool CaptchaValid)
        {
            if (!CaptchaValid)
            {
                ViewBag.Message = "Invalid Captcha. ";
                return View(model);
            }
            if (ModelState.IsValid)
            {
                if (RegisterAccount(model))
                    ViewBag.Message = "Successfully registered";
                else
                    ViewBag.Message = "Please choose another username.";
            }
            return View(model);
        }
        #endregion Register
        #region login
        [HttpGet]
        [Route("Login")]
        public ActionResult login()
        {
            if (Session["user"] != null)
                Response.Redirect("~/home");
            return View();
        }
        private bool LoginAccount(LoginUser model)
        {
            bool valid = false;
            try
            {
                using (var conn = new MySqlConnection(connectionString()))
                using (var cmd = new MySqlCommand("select * from accounts where Username=@u AND Password=@p", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@u", model.Username);
                    cmd.Parameters.AddWithValue("@p", model.Password);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            valid = true;
                    }
                }
                return valid;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        [HttpPost]
        [Route("login")]
        public ActionResult login(LoginUser model, bool CaptchaValid = false)
        {
            //CaptchaValid = true;
            //if (!CaptchaValid)
            //{
            //    ViewBag.Message = "Invalid Captcha. ";
            //    return View(model);
            //}
            if (ModelState.IsValid)
            {
                if (LoginAccount(model))
                {
                    Session["user"] = model.Username;
                    Response.Redirect("~/home", false);
                }
                else
                    ViewBag.Message = "Invalid username or password.";
            }
            return View(model);
        }
        #endregion login
        #region logout
        [Route("logout")]

        public ActionResult logout()
        {
            if (Session["user"] != null)
                Session["user"] = null;
            Response.Redirect("~/home");
            return View();
        }
        #endregion
        #region changepassword
        [HttpGet]
        [Route("change_password")]

        public ActionResult change_password()
        {
            if (Session["user"] == null)
                Response.Redirect("~/login");
            return View();
        }
        private bool ChangePass(ChangePassModel model, string username)
        {
            bool valid = false;
            try
            {
                using (var conn = new MySqlConnection(connectionString()))
                using (var cmd = new MySqlCommand("update accounts set Password=@newp where Username=@u AND Password=@p", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", model.OldPassword);
                    cmd.Parameters.AddWithValue("@newp", model.Password);
                    valid = cmd.ExecuteNonQuery() > 0;
                }
                return valid;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("change_password")]

        public ActionResult change_password(ChangePassModel model, bool CaptchaValid = false)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("~/login");
                return View();
            }
            //CaptchaValid = true;
            if (!CaptchaValid)
            {
                ViewBag.Message = "Invalid Captcha. ";
                return View(model);
            }
            if (ModelState.IsValid)
            {
                if (ChangePass(model, Session["user"].ToString()))
                    ViewBag.Message = "Password successfully changed.";
                else
                    ViewBag.Message = "Invalid information .";
            }
            return View(model);
        }
        #endregion changepassword
        #region forgotpassword
        [HttpGet]
        [Route("forgot_password")]
        public ActionResult forgot_password()
        {
            return View();
        }
        string randstr = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        Random rnd = new Random();
        public string RandomToken(int n)
        {
            string rndTok = "";
            for (int i = 0; i < n; i++)
                rndTok += randstr[rnd.Next(0, randstr.Length)];
            return rndTok;

        }
        private bool ForgotPass(ForgotPassModel model)
        {
            bool valid = false;
            string token = RandomToken(100);
            try
            {
                using (var conn = new MySqlConnection(connectionString()))
                using (var cmd = new MySqlCommand("update accounts set TokenChangePass=@t WHERE username=@u AND email=@e", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@u", model.Username);
                    cmd.Parameters.AddWithValue("@e", model.Email);
                    cmd.Parameters.AddWithValue("@t", token);
                    valid = cmd.ExecuteNonQuery() > 0;
                }
                if (valid)
                {
                    string content = "Hello " + model.Username + ",\n\nHere is your password recovery link.\n"
            + "\n\n\nhttp://conquer.zone/reset_password?user=" + model.Username + "&token=" + token +
            "\n\nPlease dont give your info to anyone.\n\nConquerZone."
            + "\n\n\nTHIS IS A NO REPLY MESSAGE. ANY REPLY TO THIS WILL NOT BE READ.";
                    using (SmtpClient client = new SmtpClient())
                    {
                        client.Host = "smtp.gmail.com";
                        client.Port = 587;
                        client.EnableSsl = true;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.Credentials = new System.Net.NetworkCredential(System.Web.Configuration.WebConfigurationManager.AppSettings["RestoreGmail"].ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["RestoreGmailPassword"].ToString());
                        client.Timeout = 600000;
                        using (MailMessage mm = new MailMessage(System.Web.Configuration.WebConfigurationManager.AppSettings["RestoreGmail"].ToString(), model.Email, "ConquerZone - Password Recovery for " + model.Username, content))
                            client.Send(mm);
                    }
                }
                return valid;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("forgot_password")]
        public ActionResult forgot_password(ForgotPassModel model, bool CaptchaValid = false)
        {
            ////CaptchaValid = true;
            if (!CaptchaValid)
            {
                ViewBag.Message = "Invalid Captcha. ";
                return View(model);
            }
            if (ModelState.IsValid)
            {
                if (ForgotPass(model))
                    ViewBag.Message = "Sent the reset link to your email.";
                else
                    ViewBag.Message = "Invalid information .";
            }
            return View(model);
        }
        #endregion changepassword
        #region restore
        [Route("reset_password")]
        [HttpGet]
        public ActionResult reset()
        {
            if (Request.QueryString["token"] == null || Request.QueryString["user"] == null || Request.QueryString["token"].ToString() == "")
                Response.Redirect("~/home");
            return View();
        }
        private bool RestorePass(RestorePasswordModel model, string user, string token)
        {
            if (token == "") return false;
            bool valid = false;
            try
            {
                using (var conn = new MySqlConnection(connectionString()))
                {
                    using (var cmd = new MySqlCommand("update accounts set Password=@p, TokenChangePass='' WHERE TokenChangePass=@t AND Username=@u;", conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@p", model.Password);
                        cmd.Parameters.AddWithValue("@u", user);
                        cmd.Parameters.AddWithValue("@t", token);
                        valid = cmd.ExecuteNonQuery() > 0;
                    }

                }
                return valid;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        [HttpPost]
        [Route("reset_password")]
        [AllowAnonymous]
        public ActionResult reset(string t, string us, RestorePasswordModel model, bool CaptchaValid = false)
        {
            try
            {
                if (!CaptchaValid)
                {
                    ViewBag.Message = "Invalid Captcha.";
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    if (RestorePass(model, model.User, model.Token))
                        ViewBag.Message = "Password changed successfully.";
                    else
                        ViewBag.Message = "Expired/Invalid token please try again using another token.";
                }
                return View();

            }
            catch (Exception e)
            {
                ViewBag.Message = e.ToString();
            }
            return View(model);
        }

        #endregion changepassword
    }
}