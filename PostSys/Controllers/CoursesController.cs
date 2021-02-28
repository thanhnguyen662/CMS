﻿using PostSys.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using PostSys.ViewModels;
using Microsoft.AspNet.Identity;

namespace PostSys.Controllers
{
	public class CoursesController : Controller
	{
		private ApplicationDbContext _context;

		public CoursesController()
		{
			_context = new ApplicationDbContext();
		}

		// GET: Courses
		/*[Authorize(Roles = "Marketing Manager, Marketing Coodinator")]*/
		public ActionResult Index()
		{
			var getAllCourse = _context.Courses.Include(m => m.Class).Include(m => m.Student).ToList();

			return View(getAllCourse);
		}

		[HttpGet]
		/*[Authorize(Roles = "Marketing Coodinator")]*/
		public ActionResult AssignStudentToCourse()
		{
			var currentUserId = User.Identity.GetUserId();
			var showMineClass = _context.Classes.Where(m => m.CoordinatorId == currentUserId).Include(m => m.Coordinator).ToList();


			//Get account in role student
			var role = (from r in _context.Roles where r.Name.Contains("Student") select r)
															 .FirstOrDefault();
			var showAllStudentInRole = _context.Users.Where(x => x.Roles.Select(y => y.RoleId)
									  .Contains(role.Id))
									  .ToList();


			var newStudentInCourseViewModel = new StudentToCourseViewModel
			{
				Students = showAllStudentInRole,
				Classes = showMineClass
			};

			return View(newStudentInCourseViewModel);
		}

		[HttpPost]
		/*[Authorize(Roles = "Marketing Coodinator")]*/
		public ActionResult AssignStudentToCourse(Course course)
		{
			if (!ModelState.IsValid)
			{
				return View("~/Views/ErrorValidations/Null.cshtml");
			}

			/*var checkStudentInCourse = _context.Courses.Any(c => c.ClassId == course.ClassId ||
																   c.StudentId == course.StudentId);
			if (checkStudentInCourse == true)
			{
				return View("~/Views/ErrorValidations/Exist.cshtml");
			}*/

			//Get class
			/*var CurrentUserId = User.Identity.GetUserId();
			var obj = (from classid in _context.Classes where classid.CoordinatorId == CurrentUserId select classid.Id).ToList();
			var classId = obj[0];*/
			
			

			var newStudentInCourse = new Course
			{
				Name = course.Name,
				ClassId = course.ClassId,
				StartDate = course.StartDate,
				EndDate = course.EndDate,
				StudentId = course.StudentId
			};

			_context.Courses.Add(newStudentInCourse);
			_context.SaveChanges();

			return RedirectToAction("Index");

		}

		[HttpGet]
		/*[Authorize(Roles = "Marketing Coodinator, Marketing Manager")]*/
		public ActionResult Delete(int Id)
		{
			var courseInDb = _context.Courses.SingleOrDefault(c => c.Id == Id);

			_context.Courses.Remove(courseInDb);
			_context.SaveChanges();

			return RedirectToAction("Index");
		}

		public ActionResult DeleteManageMineCourse(int Id)
		{
			var courseInDb = _context.Courses.SingleOrDefault(c => c.Id == Id);

			_context.Courses.Remove(courseInDb);
			_context.SaveChanges();

			return RedirectToAction("ManageCourse");
		}

		[HttpGet]
		/*[Authorize(Roles = "Student")]*/
		public ActionResult MineCourse()
		{
			var mineId = User.Identity.GetUserId();
			var mineCourse = _context.Courses.Where(m => m.StudentId == mineId).Include(m => m.Class).Include(m => m.Student).ToList();

			return View(mineCourse);
		}

		public ActionResult ManageCourse()
		{
			var currentCoordinatorId = User.Identity.GetUserId();	

			var obj = (from cl in _context.Classes
					   where cl.CoordinatorId.Contains(currentCoordinatorId)
					   join c in _context.Courses
					   on cl.Id equals c.ClassId
					   select new
					   {
						   courseId = c.Id,
						   courseName = c.Name,
						   className = cl.Name,
						   studentName = (from st in _context.Users where st.Id.Contains(c.StudentId) select st.UserName),
						   startDate = c.StartDate,
						   endDate = c.EndDate,
					   }
					   ).ToList().Select(p => new PostCourseViewModel()
					   {
						   courseId = p.courseId,
						   courseName = p.courseName,
						   className = p.className,
						   studentName = string.Join(",", p.studentName),
						   startDate = p.startDate,
						   endDate = p.endDate
					   }
					   );
			return View(obj);
			
			
		}




		/////////////////////////////////////////
		[HttpGet]
		public ActionResult PostTopic(int id)
		{
			var currentStudent = User.Identity.GetUserId();
			var showMineCourse = _context.Courses.Where(m => m.StudentId == currentStudent).Include(m => m.Student).ToList();

			var courseInDb = _context.Courses.SingleOrDefault(c => c.Id == id);

			var newPostCourseViewModel = new PostCourseViewModel
			{
				Course = courseInDb
			};

			return View(newPostCourseViewModel);
		}


		[HttpPost]
		public ActionResult PostTopic(Post post, Course course)
		{

			var courseInDb = _context.Courses.SingleOrDefault(c => c.Id == course.Id);
			
			var newPost = new Post
			{
				CourseId = courseInDb.Id,
				Name = post.Name,
			};

			_context.Posts.Add(newPost);
			_context.SaveChanges();

			return View("~/Views/Home/Index.cshtml");
		}
	}
}