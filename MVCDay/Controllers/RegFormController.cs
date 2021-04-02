using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCDay.Models;
using System.Data.Objects;

namespace MVCDay.Controllers
{
    public class RegFormController : Controller
    {
        LTIMVCEntities1 db = new LTIMVCEntities1();
//----------------------------------insertnew employee----------------------------------
        [HttpGet]
        public ActionResult InsertNewEmployee()
        {
            //this code will retrieve data from db and bind it to DropDownList
            //frm where we r selecting data-selecting from projectinfo
            //projid-->value member
            //projname-->display memeber--end user will see project names
            ViewData["proj"] = new SelectList(db.ProjectInfoes.ToList(),"projid","projname");
            return View();
        }
        //method name and view name must be same
        [HttpPost]
        public ActionResult InsertNewEmployee(Employee emp)
        {

            emp.EmpID = Convert.ToInt32(Request.Form["txtempid"]);
            emp.EmpName = Request.Form["txtempname"];
            emp.Dept = Request.Form["depart"];
            emp.Desg = Request.Form["ddldesg"];
            emp.Salary = Convert.ToDecimal(Request.Form["txtSalary"]);
            emp.projid = Convert.ToInt32(Request.Form["ddlpid"]);

            //this data has to be inserted to DB
            //ado.net or new technology called entity framework
            //ModelState.AddModelError() display any msg from controller
            //in the view
            ModelState.AddModelError("", emp.EmpID + "," + emp.EmpName + "," + emp.Dept + "," + emp.Desg + "," + emp.Salary+","+emp.projid);
            //1. obj of sqlconnection
            //2.open connection 
            //3.

            //insert data to emp table through entity framework
            db.Employees.Add(emp);
            int res = db.SaveChanges();
            if (res > 0)
                ModelState.AddModelError("", "new employee inserted");
            return RedirectToAction("GetEmployees"); //1st way
            
        }
//---------------------------------------get employees---------------------------------------------------------------------------------------
        public ActionResult GetEmployees()
        {
            var data = db.Employees.ToList();
            return View(data);//model binding

        }
//-----------------------------get emp by dept and salary----------------------------------------------------------------------------
        //retrieve emp details from the db on condidtion
        [HttpGet]
        public ActionResult GetEmployeeByDeptSalary()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetEmployeeByDeptSalary(string dept,decimal? salary)
        {
            dept = Request.Form["dept"];
            salary = Convert.ToDecimal(Request.Form["Salary"]);
            var query = from t in db.Employees
                        where t.Dept == dept && t.Salary >= salary
                        select t;
            var lambda = db.Employees.Where(x => x.Dept == dept && x.Salary >= salary);
            if(query.Count()==0)
            {
                ModelState.AddModelError("", "No data found for the parameters passed");
                return View();
            }
            else
            {
                //this will pass data from controller 
                //to view
                Session["data"] = query;
            }
            return View();
        }
//----------------------------------------update employee---------------------------------------------------------------------

        [HttpGet]
        public ActionResult UpdateEmployee(int id)
        {
            var data = db.Employees.Where(x => x.EmpID == id).SingleOrDefault();

            return View(data);
        }
        [HttpPost]
        public ActionResult UpdateEmployee()
        {
            int id = Convert.ToInt32(Request.Form["eid"]);
            var olddata = db.Employees.Where(x => x.EmpID == id).SingleOrDefault();
            var newname = Request.Form["name"];
            var newdept = Request.Form["dept"];
            var newdesg = Request.Form["desg"];
            var newsal=Convert.ToDecimal (Request.Form["sal"]);
            var newpid = Convert.ToInt32(Request.Form["pid"]);
            olddata.EmpName = newname;
            olddata.Dept = newdept;
            olddata.Desg = newdesg;
            olddata.Salary = newsal;
            olddata.projid = newpid;
            olddata.EmpID = id;
            
            var res = db.SaveChanges();
            if (res > 0)
                return RedirectToAction("GetEmployees");
            return RedirectToAction("InsertNewEmployee");
            
        }
//---------------------------------------delete employee--------------------------------------------------------
        [HttpGet]
        public ActionResult DeleteEmployee(int id)
        {
            var data = db.Employees.Where(x => x.EmpID == id).SingleOrDefault();

            return View(data);
        }
        [HttpPost]
        public ActionResult DeleteEmployee()
        {
            int id = Convert.ToInt32(Request.Form["eid"]);
            var delrow = db.Employees.Where(x => x.EmpID == id).SingleOrDefault();
            db.Employees.Remove(delrow);
            var res = db.SaveChanges();
            if (res > 0)
                return RedirectToAction("GetEmployees");
            return RedirectToAction("GetEmployees");
        }
//---------------------------------------insert project--------------------------------------------------------

        [HttpGet]
        public ActionResult InsertProject()
        {
            return View();
        }
        [HttpPost]
        public ActionResult InsertProject(ProjectInfo pinfo)
        {
            if(ModelState.IsValid)
            {
                //if there is no validation error,then 
                //Isvalid will return true
                //if errors IsValid return false
                db.ProjectInfoes.Add(pinfo);
                var res = db.SaveChanges();
                if (res > 0)
                {
                    Response.Write("<script type='text/JavaScript'>" + "alert('New Project Crreated')" + "</script");
                    ModelState.AddModelError("", "New Project Added");
                }//res
            }//isvalid
            return View();
        }
//_-----------------------------------------Get Emp Project-----------------------------------------
        [HttpGet]
        public ActionResult GetEmpProject()
        {
            var data = (from e in db.Employees
                       join p in db.ProjectInfoes
                       on e.projid equals p.projid
                       select new CustomEmpProject { EmpId = e.EmpID, Name = e.EmpName, Dept = e.Dept, ProjName = p.projname, Domain = p.domain }).ToList();
            return View(data);
        }
//---------------------------storded procedure--------------------------------------------------
        [HttpGet]
        public ActionResult SelectAndUpdateProjectSP()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SelectandUpdateProjectSP(string command)
        {
            if (command == "Select")
            {
                int? id = int.Parse(Request.Form["pid"]);
                var result = db.sp_SelectProjectById(id).FirstOrDefault();
                if (result == null)
                    ModelState.AddModelError("", "Invalid project id");
                else
                
                    ModelState.AddModelError("", result.projid + "," + result.projname + "," + result.domain);
                
                ViewBag.data = result;
            }
            if (command == "Update")
            {
                string newpname = Request.Form["pname"];
                string newdomain = Request.Form["domain"];
                int? pid = int.Parse(Request.Form["projid"]);
                var res = db.sp_UpdateProject(pid, newpname, newdomain);
                if (res > 0)
                    ModelState.AddModelError("", "one row affected");
            }

            return View();

        }
    }

}
