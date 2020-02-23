using Quizz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Data.Entity;

namespace Quizz.Controllers
{
    public class HomeController : Controller
    {
        DBQUIZEntities db = new DBQUIZEntities();

        [HttpGet]
        public ActionResult sregister()
        {
            return View();
        }

        [HttpPost]
        public ActionResult sregister(TBL_STUDENT svw, HttpPostedFileBase imagefile)
        {
            TBL_STUDENT s = new TBL_STUDENT();

            try
            {
                s.S_NAME = svw.S_NAME;
                s.S_PASSWORD = svw.S_PASSWORD;
                s.S_IMAGE = uploadimage(imagefile);
                db.TBL_STUDENT.Add(s);
                db.SaveChanges();
                return RedirectToAction("slogin");
            }
            catch (Exception)
            {
                ViewBag.msg = "Data could not be inserted.....";
            }


            return View();
        }

        public string uploadimage(HttpPostedFileBase imgfile)
        {
            string path = "-1";

            try
            {
                if (imgfile != null && imgfile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(imgfile.FileName);
                    if (extension.ToLower().Equals("jpg") || extension.ToLower().Equals("jpeg") || extension.ToLower().Equals(""))
                    {
                        Random r = new Random();
                        Path.Combine(Server.MapPath("~/Content/img"), r + Path.GetFileName(imgfile.FileName));
                        imgfile.SaveAs(path);
                        path = "~/Content/img" + r + Path.GetFileName(imgfile.FileName);
                    }
                }
                else
                {

                }

            }
            catch (Exception)
            {

            }
            return path;
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Index");
        }

        public ActionResult slogin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult slogin(TBL_STUDENT s)
        {
            TBL_STUDENT std = db.TBL_STUDENT.Where(x => x.S_NAME == s.S_NAME && x.S_PASSWORD == s.S_PASSWORD).SingleOrDefault();
            if (std == null)
            {
                ViewBag.msg = "Invalid Email or Password";
            }
            else
            {
                Session["std_id"] = std.S_ID;
                return RedirectToAction("StudentDashboard");

            }
            return View();
        }

        public ActionResult StudentDashboard()
        {
            if (Session["std_id"] == null)
            {
                return RedirectToAction("Index");
            }

            return View();

        }
        public ActionResult Edit()
        {
            if (Session["std_id"] == null)
            {
                return RedirectToAction("Index");
            }

            return View();

        }

        public ActionResult StudentExam()
        {
            if (Session["std_id"] == null)
            {
                return RedirectToAction("slogin");
            }

            return View();

        }

        [HttpPost]
        public ActionResult StudentExam(string room)
        {
            List<TBL_CATEGORY> list = db.TBL_CATEGORY.ToList();
            foreach (var item in list)
            {
                if (item.CAT_ENCRYPTEDSTRING == room)
                {
                    List<TBL_QUESTIONS> li = db.TBL_QUESTIONS.Where(x => x.Q_FK_CATID == item.CAT_ID).ToList();
                    Queue<TBL_QUESTIONS> queue = new Queue<TBL_QUESTIONS>();
                    foreach (TBL_QUESTIONS a in li)
                    {
                        queue.Enqueue(a);
                    }
                    TempData["examname"] = item.CAT_NAME;
                    TempData["examid"] = item.CAT_ID;
                    TempData["questions"] = queue;
                    TempData["score"] = 0;
                    TempData.Keep();
                    return RedirectToAction("QuizzStart");
                }
                else
                {
                    ViewBag.error = "No Room Found ...";
                }
            }
            return View();

        }

        public ActionResult QuizzStart()
        {
            if (Session["std_id"] == null)
            {
                return RedirectToAction("slogin");
            }



            TBL_QUESTIONS q = null;

            if (TempData["questions"] != null)
            {
                Queue<TBL_QUESTIONS> qlist = (Queue<TBL_QUESTIONS>)TempData["questions"];
                if (qlist.Count > 0)
                {
                    q = qlist.Peek();
                    qlist.Dequeue();
                    TempData["questions"] = qlist;
                    TempData.Keep();
                }
                else
                {
                    return RedirectToAction("EndExam");
                }
            }
            else
            {
                return RedirectToAction("StudentExam");
            }
            return View(q);



        }

        [HttpPost]
        public ActionResult QuizzStart(TBL_QUESTIONS q)
        {
            string correctans = null;
            if (q.OPA != null)
            {
                correctans = "A";
            }
            else if (q.OPB != null)
            {
                correctans = "B";
            }
            else if (q.OPC != null)
            {
                correctans = "C";
            }
            else if (q.OPD != null)
            {
                correctans = "D";
            }
            if (correctans.Equals(q.COP))
            {
                TempData["score"] = Convert.ToInt32(TempData["score"]) + 1;
            }

            TempData.Keep();


            return RedirectToAction("QuizzStart");
        }

        public ActionResult EndExam()
        {
            TBL_SETEXAM s = new TBL_SETEXAM();
            s.EXAM_DATE = DateTime.Now;
            s.EXAM_NAME = Convert.ToString(TempData["examname"]);
            s.STD_SCORE = Convert.ToInt32(TempData["score"]);
            s.EXAM_FK_STU = Convert.ToInt32(Session["std_id"].ToString());
            db.TBL_SETEXAM.Add(s);
            db.SaveChanges();
            return View();
        }

        [HttpGet]
        public ActionResult tlogin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult tlogin(TBL_ADMIN a)
        {
            TBL_ADMIN ad = db.TBL_ADMIN.Where(x => x.AD_NAME == a.AD_NAME && x.AD_PASSWORD == a.AD_PASSWORD).SingleOrDefault();
            if (ad != null)
            {
                Session["ad_id"] = ad.AD_ID;
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.msg = "Invalid username or password";
            }
            return View();
        }
        public ActionResult Dashboard()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
        public ActionResult Addcategory()
        {
            List<TBL_CATEGORY> li = db.TBL_CATEGORY.OrderByDescending(x => x.CAT_ID).ToList();
            ViewData["list"] = li;
            return View();
        }
        [HttpPost]
        public ActionResult Addcategory(TBL_CATEGORY cat)
        {
            // Session["ad_id"] = 1;// we will remove it soon....
            int adid = Convert.ToInt32(Session["ad_id"].ToString());
            List<TBL_CATEGORY> li = db.TBL_CATEGORY.Where(x => x.CAT_FK_ADID == adid).OrderByDescending(x => x.CAT_ID).ToList();
            ViewData["list"] = li;
            Random r = new Random();
            TBL_CATEGORY c = new TBL_CATEGORY();
            c.CAT_NAME = cat.CAT_NAME;
            c.CAT_ENCRYPTEDSTRING = crypto.Encrypt(cat.CAT_NAME.Trim() + r.Next().ToString(), true);
            c.CAT_FK_ADID = Convert.ToInt32(Session["ad_id"].ToString());
            db.TBL_CATEGORY.Add(c);
            db.SaveChanges();


            return RedirectToAction("Addcategory");
        }



        public ActionResult Index()
        {
            if (Session["ad_id"] != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }
        [HttpGet]
        public ActionResult Addquestion()
        {
            int sid = Convert.ToInt32(Session["ad_id"]);
            List<TBL_CATEGORY> li = db.TBL_CATEGORY.Where(x => x.CAT_FK_ADID == sid).ToList();
            ViewBag.list = new SelectList(li, "cat_id", "cat_name");
            return View();
        }

        [HttpPost]
        public ActionResult Addquestion(TBL_QUESTIONS q)
        {
            int sid = Convert.ToInt32(Session["ad_id"]);
            List<TBL_CATEGORY> li = db.TBL_CATEGORY.Where(x => x.CAT_FK_ADID == sid).ToList();
            ViewBag.list = new SelectList(li, "cat_id", "cat_name");

            TBL_QUESTIONS QA = new TBL_QUESTIONS();
            QA.Q_TEXT = q.Q_TEXT;
            QA.OPA = q.OPA;
            QA.OPB = q.OPB;
            QA.OPC = q.OPC;
            QA.OPD = q.OPD;
            QA.COP = q.COP;
            QA.Q_FK_CATID = q.Q_FK_CATID;

            db.TBL_QUESTIONS.Add(QA);
            db.SaveChanges();
            TempData["msg"] = "Question added successfully...";
            TempData.Keep();

            return RedirectToAction("Addquestion");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult viewAllquestions(int? id)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("tlogin");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }



            return View(db.TBL_QUESTIONS.Where(x => x.Q_FK_CATID == id).ToList());
        }
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TBL_QUESTIONS questions = db.TBL_QUESTIONS.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QUESTION_ID,Q_TEXT,OPA,OPB,OPC,OPD,COP,Q_FK_CATID")] TBL_QUESTIONS questions)
        {
            if (ModelState.IsValid)
            {
                db.Entry(questions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("viewAllquestions");
            }
            return View(questions);
        }
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TBL_QUESTIONS questions = db.TBL_QUESTIONS.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        // POST: /Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TBL_QUESTIONS questions = db.TBL_QUESTIONS.Find(id);
            db.TBL_QUESTIONS.Remove(questions);
            db.SaveChanges();
            return RedirectToAction("viewAllquestions");
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}