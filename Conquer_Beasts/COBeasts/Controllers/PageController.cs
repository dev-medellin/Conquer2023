using MySql.Data.MySqlClient;
using PhoenixConquer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhoenixConquer.Controllers
{
    public class PageController : Controller
    {

        [Route("stats")]
        public ActionResult Stats()
        {
            return View();
        }
        public ActionResult vote_success()
        {
            string voteIp = Request.QueryString["votingip"];
            string custom = Request.QueryString["custom"];
            using (var conn = new MySqlConnection(UserController.connectionString()))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM votes_ip where IP=@ip", conn))
                {
                    cmd.Parameters.AddWithValue("@ip", voteIp);
                    cmd.Parameters.AddWithValue("@c", custom);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var date = reader.GetDateTime("Date");
                            if (DateTime.Now > date.AddHours(12))
                            {
                                reader.Close();
                                using (var cmd2 = new MySqlCommand("update votes_ip set Date=@d where IP=@ip", conn))
                                {
                                    cmd2.Parameters.AddWithValue("@d", DateTime.Now);
                                    cmd2.Parameters.AddWithValue("@ip", voteIp);
                                    cmd2.ExecuteNonQuery();
                                }
                                #region Prizes
                                try
                                {
                                    using (var cmd2 = new MySqlCommand("insert into votes_accounts values(@c,1,1,'',@d)", conn))
                                    {
                                        cmd2.Parameters.AddWithValue("@c", custom);
                                        cmd2.Parameters.AddWithValue("@d", DateTime.Now);
                                        cmd2.ExecuteNonQuery();
                                    }
                                    RegisterVote(voteIp);

                                }
                                catch
                                {
                                    DateTime lastVote = DateTime.Now; ;
                                    using (var cmd2 = new MySqlCommand("SELECT LastClaim FROM votes_accounts where Username=@u", conn))
                                    {
                                        cmd2.Parameters.AddWithValue("@u", custom);
                                        using (var rdr = cmd2.ExecuteReader())
                                            while (rdr.Read())
                                                lastVote = rdr.GetDateTime("LastClaim");
                                    }
                                    if (DateTime.Now > lastVote.AddHours(12))
                                    {
                                        using (var cmd2 = new MySqlCommand("update votes_accounts set Claims=Claims+1,Total=Total+1,LastClaim=@d Where Username=@c", conn))
                                        {
                                            cmd2.Parameters.AddWithValue("@c", custom);
                                            cmd2.Parameters.AddWithValue("@d", DateTime.Now);
                                            cmd2.ExecuteNonQuery();
                                            Response.Write("Voted");
                                        }
                                        RegisterVote(voteIp);

                                    }
                                    else Response.Write(lastVote.ToString());
                                }
                                #endregion
                            }
                            else Response.Write("VOTED already you need to wait " + (date.AddHours(12) - DateTime.Now).TotalHours);
                        }
                        else
                        {
                            reader.Close();
                            using (var cmd2 = new MySqlCommand("insert into votes_ip values(@d,@ip)", conn))
                            {
                                cmd2.Parameters.AddWithValue("@d", DateTime.Now);
                                cmd2.Parameters.AddWithValue("@ip", voteIp);
                                cmd2.ExecuteNonQuery();
                            }
                            #region Prizes
                            try
                            {
                                using (var cmd2 = new MySqlCommand("insert into votes_accounts values(@c,1,1,'',@d)", conn))
                                {
                                    cmd2.Parameters.AddWithValue("@c", custom);
                                    cmd2.Parameters.AddWithValue("@d", DateTime.Now);
                                    cmd2.ExecuteNonQuery();

                                }
                                RegisterVote(voteIp);

                            }
                            catch
                            {
                                DateTime lastVote = DateTime.Now;
                                using (var cmd2 = new MySqlCommand("SELECT LastClaim FROM votes_accounts where Username=@u", conn))
                                {
                                    cmd2.Parameters.AddWithValue("@u", custom);
                                    using (var rdr = cmd2.ExecuteReader())
                                        while (rdr.Read())
                                            lastVote = rdr.GetDateTime("LastClaim");
                                }
                                if (DateTime.Now > lastVote.AddHours(12))
                                {
                                    using (var cmd2 = new MySqlCommand("update votes_accounts set Claims=Claims+1,Total=Total+1,LastClaim=@d Where Username=@c", conn))
                                    {
                                        cmd2.Parameters.AddWithValue("@c", custom);
                                        cmd2.Parameters.AddWithValue("@d", DateTime.Now);
                                        cmd2.ExecuteNonQuery();
                                        Response.Write("Voted");
                                    }
                                    RegisterVote(voteIp);

                                }
                                else Response.Write(lastVote.ToString());
                            }
                            #endregion
                        }
                    }
                }
            }
            return View();
        }
        public void RegisterVote(string ip)
        {
            try
            {
                using (var conn = new MySqlConnection(UserController.connectionString()))
                {
                    using (var cmd = new MySqlCommand("insert into votes_log values (@ip,@date)", conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@ip", ip);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
            }
        }
        [Route("404")]
        public ActionResult not_found()
        {
            return View();
        }
        [HttpGet]
        public ActionResult contact_form()
        {
            return View();
        }
        [HttpPost]
        public ActionResult contact_form(ContactForm model, bool CaptchaValid)
        {
            //CaptchaValid = true;
            if (!CaptchaValid)
            {
                ViewBag.Message = "Invalid Captcha. ";
                return View(model);
            }
            if (ModelState.IsValid)
            {
                if (UploadMessage(model))
                    ViewBag.Message = "Message sent..";
                else
                    ViewBag.Message = "Can`t send the message.";
            }
            return View(model);
        }

        private bool UploadMessage(ContactForm model)
        {
            try
            {
                using (var conn = new MySqlConnection(UserController.connectionString()))
                using (var cmd = new MySqlCommand("INSERT into messages values (@u,@n,@e,@m)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@u", model.Username);
                    cmd.Parameters.AddWithValue("@n", model.Name);
                    cmd.Parameters.AddWithValue("@e", model.Email);
                    cmd.Parameters.AddWithValue("@m", model.Message);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        [Route("store")]
        public ActionResult shop()
        {
            if (Session["user"] == null)
                Response.Redirect("~/login");

            return View();
        }
        public ActionResult ranks()
        {
            return View();
        }
        public enum TopsType : byte
        {
            Potency = 1,
            Money = 2,
            Virtue = 3,
            PK = 4,
            Trojan = 15,
            Warrior = 25,
            Archer = 45,
            Fire = 145,
            Water = 135
        }
        public struct Toper
        {
            public string Name;
            public object Param;
            public byte Level;
            public byte VIPLevel;
            public string Guild;
        }

        public static List<Toper> Topers(TopsType type)
        {
            var tp = new List<Toper>();
            try
            {
                using (var conn = new MySqlConnection(UserController.connectionString()))
                using (var cmd = new MySqlCommand("SELECT * from tops where toptype=@p ORDER by Param desc LIMIT 20", conn))
                {
                    conn.Open();
                    byte Type = (byte)type;
                    cmd.Parameters.AddWithValue("@p", Type);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var toper = new Toper();
                            try
                            {
                                toper.Name = rdr.GetString("Name");
                                if (toper.Name == "") continue;
                                toper.Param = rdr.GetUInt32("Param");
                                if (type != (TopsType)5)
                                {
                                    toper.Level = (byte)rdr.GetInt32("Level");
                                    toper.VIPLevel = (byte)rdr.GetInt32("VIPLevel");
                                }
                                toper.Guild = rdr.GetString("GuildName");

                                tp.Add(toper);
                            }
                            catch (Exception e)
                            { }
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            return tp;
        }
        public struct Guilds
        {
            public string Name;
            public string Leader;
            public int MembersCount;
            public ulong Fund;
        }
        public struct Events
        {
            public string Name;
            public string Winner;
            public int Count;
        }
        public struct Vote
        {
            public string Name;
            public int Votes;
        }
        public static List<Events> Events_Topers()
        {
            var tp = new List<Events>();
            try
            {
                using (var conn = new MySqlConnection(UserController.connectionString()))
                using (var cmd = new MySqlCommand("SELECT * from events", conn))
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var toper = new Events();
                            try
                            {
                                toper.Name = rdr.GetString("Event");
                                toper.Count = rdr.GetInt32("Count");
                                toper.Winner = rdr.GetString("Winner");
                                tp.Add(toper);
                            }
                            catch (Exception e)
                            { }
                        }
                    }
                }
            }
            catch { }
            return tp;
        }
        public static List<Vote> Vote_Topers()
        {
            var tp = new List<Vote>();
            try
            {
                using (var conn = new MySqlConnection(UserController.connectionString()))
                using (var cmd = new MySqlCommand("SELECT * from votes_accounts WHERE Name <> ' ' ORDER by Total desc LIMIT 5", conn))
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var toper = new Vote();
                            try
                            {

                                toper.Name = rdr.GetString("Name");
                                toper.Votes = rdr.GetInt32("Total");
                                tp.Add(toper);
                            }
                            catch (Exception e)
                            { }
                        }
                    }
                }
            }
            catch { }
            return tp;
        }
        [Route("downloads")]

        public ActionResult downloads()
        {
            return View();
        }
        [Route("home")]
        public ActionResult home()
        {
            return View();
        }
    }
}