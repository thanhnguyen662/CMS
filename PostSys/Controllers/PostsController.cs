﻿using Microsoft.AspNet.Identity;
using PostSys.Models;
using PostSys.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.IO;
using Ionic.Zip;

namespace PostSys.Controllers
{
	public class PostsController : Controller
	{
		private ApplicationDbContext _context;

		public PostsController()
		{
			_context = new ApplicationDbContext();
		}
		
		public ActionResult Index()
		{
			var showPost = _context.Posts.Include(m => m.Course).ToList();
			var showCourse = _context.Courses.Include(m => m.Class).Include(m => m.Student).ToList();

			return View(showPost);
		}

		[HttpGet]
		public ActionResult MinePost()
		{
			/*var getCurrentStudent = User.Identity.GetUserId();*/
			var getCurrentStudent = User.Identity.GetUserName();
			var getCourse = _context.Courses.ToList();
			var getStudent = _context.Users.ToList();

			var getStudentPost = _context.Posts.Where(m => m.Course.Student.UserName == getCurrentStudent).Include(m => m.Course);

			/*var obj = (from c in _context.Courses
					   where c.StudentId.Contains(getCurrentStudent)
					   join p in _context.Posts
					   on c.Id equals p.CourseId
					   select new
					   {
						   Id = p.Id, 
						   Name = p.Name,
						   Course = c.Name
					   }).ToList().Select(po => new PostCourseViewModel()
					   {
						   postName = po.Name,
						   postId = po.Id,
						   courseName  = po.Course
					   }
					   ).ToList();*/

			return View(getStudentPost.ToList());
		}

		[HttpGet]
		public ActionResult Delete(int Id)
		{
			var courseInDb = _context.Posts.SingleOrDefault(c => c.Id == Id);

			_context.Posts.Remove(courseInDb);
			_context.SaveChanges();

			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult DeleteMinePost (int Id)
		{
			var courseInDb = _context.Posts.SingleOrDefault(c => c.Id == Id);

			_context.Posts.Remove(courseInDb);
			_context.SaveChanges();

			return RedirectToAction("MinePost");
		}



		/*[HttpGet]
		public ActionResult ManagePost()
		{
			{
				//Get coor ID
				var currentCoorId = User.Identity.GetUserId();
				var currentCoorClass = (from cl in _context.Classes where cl.CoordinatorId == currentCoorId select cl.Id).ToList();
				var currentCoorClassId = currentCoorClass[0];

				var ojb = (from c in _context.Courses
						   where c.ClassId == currentCoorClassId
						   join p in _context.Posts
						   on c.Id equals p.CourseId
						   select new
						   {
							   postId = p.Id,
							   postName = p.Name,
							   courseName = c.Name,
							   studentName = (from st in _context.Users where st.Id == c.StudentId select st.UserName),
							   className = (from cl in _context.Classes where cl.Id == c.ClassId select cl.Name),
						   }
					).ToList().Select(p => new PostCourseViewModel()
					{
						postId = p.postId,
						postName = p.postName,
						courseName = p.courseName,
						studentName = string.Join(",", p.studentName),
						className = string.Join(",", p.className)
					}
					);

				return View(ojb);
			}
		}*/

		[HttpGet]
		public ActionResult ManageMinePost()
		{
			var getCurrentCoordinator = User.Identity.GetUserName();
			var getCourse = _context.Courses.ToList();
			var getClass = _context.Classes.ToList();
			var getStudent = _context.Users.ToList();

			var getPostOfCoordinator = _context.Posts.Where(m => m.Course.Class.Coordinator.UserName == getCurrentCoordinator).Include(m => m.Course);

			return View(getPostOfCoordinator.ToList());
		}

		public FileResult Download(Post post)
		{

			var getFileById = _context.Posts.SingleOrDefault(c => c.Id == post.Id);

			return File(getFileById.File, "file", getFileById.UrlFile);
		}


		[HttpGet]
		public ActionResult AddComment()
		{
			return View();
		}

		[HttpPost]
		public ActionResult AddComment(Post post, Comment comment)
		{
			var getPostId = _context.Posts.SingleOrDefault(m => m.Id == post.Id);

			var newComent = new Comment
			{
				PostId = getPostId.Id,
				Reply = comment.Reply
			};

			_context.Comments.Add(newComent);
			_context.SaveChanges();
			
			return View("~/Views/Home/Index.cshtml");
		}

		[HttpGet]
		public ActionResult ShowComment(Post post, int id)
		{
			var getPostId = _context.Posts.SingleOrDefault(m => m.Id == post.Id);

			var getCommentInPost = _context.Comments.Where(m => m.PostId == id).ToList();

			return View(getCommentInPost);
		}

		[HttpGet]
		public ActionResult DeleteComment(int Id)
		{
			var commentInDb = _context.Comments.SingleOrDefault(c => c.Id == Id);

			_context.Comments.Remove(commentInDb);
			_context.SaveChanges();

			return RedirectToAction("ShowComment");
		}


		[HttpGet]
		public ActionResult AddPostToPublication(Post post)
		{
			var getPostId = _context.Posts.SingleOrDefault(m => m.Id == post.Id);

			var newPublication = new Publication
			{
				PostId = getPostId.Id
			};

			_context.Publications.Add(newPublication);
			_context.SaveChanges();

			return RedirectToAction("ManageMinePost");
		}

		/*[HttpPost]
		public ActionResult AddPostToPublication(Post post)
		{
			var getPostId = _context.Posts.SingleOrDefault(m => m.Id == post.Id);

			var newPublication = new Publication
			{
				PostId = getPostId.Id
			};

			_context.Publications.Add(newPublication);
			_context.SaveChanges();

			return View(newPublication);
		}*/

		[HttpGet]
		public ActionResult ListPublication()
		{
			var getPost = _context.Posts.ToList();
			var getCourse = _context.Courses.ToList();
			var getClass = _context.Classes.ToList();
			var getCoordinator = _context.Users.ToList();

			var showListPublication = _context.Publications.Include(p => p.Post).ToList();
			return View(showListPublication);
		}

		/*[HttpGet]
		public ActionResult MinePublication()
		{
			//get all publication
			var showListPublication = _context.Publications.Include(p => p.Post).ToList();
			//Get currentId
			var currentID = User.Identity.GetUserId();
			// get current class
			var coorClass = (from co in _context.Users
							 where co.Id.Contains(currentID)
							 join cl in _context.Classes
							 on co.Id equals cl.CoordinatorId
							 select cl.Id).ToList();
			// get current classID
			var classId = coorClass[0];
			//get all course by current classID
			var currentCourse = _context.Courses.Where(m => m.ClassId == classId).ToList();
			//get all post by current Course 
			var coursePost = (from c in _context.Courses
							  where c.ClassId == classId
							  join p in _context.Posts
							  on c.Id equals p.CourseId
							  select new
							  {
								  Id = p.Id,
								  Name = p.Name,
								  Description = p.Description,
								  File = p.File,
								  UrlFile = p.UrlFile,
								  PostDate = p.PostDate,
								  CourseId = p.CourseId

							  }
							  ).ToList().Select(m => new Post()
							  {
								  Id = m.Id,
								  Name = m.Name,
								  Description = m.Description,
								  File = m.File,
								  UrlFile = m.UrlFile,
								  PostDate = m.PostDate,
								  CourseId = m.CourseId

							  }).ToList();

			//Get minePublication list post
			var publication = (from s in showListPublication
							   join cp in coursePost
							   on s.Post.Id equals cp.Id
							   select s
							   ).ToList();

		

			return View(publication);
		}*/


		/////////////////
		public ActionResult ManageMinePublication()
		{
			var getCurrentCoordinator = User.Identity.GetUserName();
			var getPost = _context.Posts.ToList();
			var getCourse = _context.Courses.ToList();
			var getClass = _context.Classes.ToList();
			var getCoordinator = _context.Users.ToList();

			var getPublicationOfCoordinator = _context.Publications.Where(m => m.Post.Course.Class.Coordinator.UserName == getCurrentCoordinator).Include(m => m.Post);

			return View(getPublicationOfCoordinator.ToList());
		}
		////////////////

		[HttpGet]
		public ActionResult DeletePublication(int Id)
		{
			var publicationInDb = _context.Publications.SingleOrDefault(c => c.Id == Id);

			_context.Publications.Remove(publicationInDb);
			_context.SaveChanges();

			return RedirectToAction("ListPublication");
		}


		[HttpGet]
		public ActionResult DeleteMinePublication(int Id)
		{
			var publicationInDb = _context.Publications.SingleOrDefault(c => c.Id == Id);

			_context.Publications.Remove(publicationInDb);
			_context.SaveChanges();

			return RedirectToAction("MinePublication");
		}

		public bool SendEmail(string toEmail, string emailSubject, string emailBody)
		{

			var senderEmail = ConfigurationManager.AppSettings["SenderEmail"].ToString();
			var senderPassword = ConfigurationManager.AppSettings["SenderPassword"].ToString();

			SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
			smtpClient.EnableSsl = true;
			smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtpClient.UseDefaultCredentials = false;
			smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

			MailMessage mailMessage = new MailMessage(senderEmail, toEmail, emailSubject, emailBody);
			mailMessage.IsBodyHtml = true;
			mailMessage.BodyEncoding = UTF8Encoding.UTF8;

			smtpClient.Send(mailMessage);

			return true;

		}

		public ActionResult SendEmailToUser()
		{
			//get coordinator email
			bool result = false;
			var getCurrentStudent = User.Identity.GetUserName();
			

			var courseStudent = (from st in _context.Users
								 where st.UserName.Contains(getCurrentStudent)
								 join c in _context.Courses
								 on st.Id equals c.StudentId
								 select c.ClassId).ToList();

			var courseStudentId = courseStudent[0];
			var classCoordinator = (from cc in _context.Classes
									where cc.Id == courseStudentId
									join co in _context.Users
									on cc.CoordinatorId equals co.Id
									select co.Email).ToList();
			var coordinatorEmail = classCoordinator[0];

			//get Course Name
			var getCurrentStudentId = User.Identity.GetUserId();
			var currentCourseNames = (from co in _context.Courses where co.StudentId.Contains(getCurrentStudentId) select co.Name).ToList();
			var currentCourseName = currentCourseNames[0];

			//get Class Name
			var classNames = (from c in _context.Courses
							  where c.StudentId.Contains(getCurrentStudentId)
							  join cl in _context.Classes
							  on c.ClassId equals cl.Id
							  select cl.Name).ToList();
			var className = classNames[0];

			result = SendEmail($"{coordinatorEmail}", "Notification Email", $"Student: {getCurrentStudent} <br> Course: {currentCourseName} <br> Class: {className} <br> Already submit a post");


			return Json(result, JsonRequestBehavior.AllowGet);
		}



		/////////////////////////////////////////
		//Only Manager can see this index
		[HttpGet]
		public ActionResult ManagerIndex()
		{
			string[] filePaths = Directory.GetFiles(Server.MapPath("~/Files/"));
			List<FileModel> files = new List<FileModel>();
			foreach (string filePath in filePaths)
			{
				////////
				/*string getFileName = Path.GetFileName(filePath);
				string[] getUsernameFromFileName = getFileName.Split('_');*/

				/*foreach (var word in getUsernameFromFileName)
				{
					System.Console.WriteLine($"<{word}>");
				}*/

				////////
				files.Add(new FileModel()
				{
					FileName = Path.GetFileName(filePath),
					FilePath = filePath,
					/*UserNameInFile = getUsernameFromFileName[0]*/
				});
			}


			return View(files);
		}

		[HttpPost]
		public ActionResult ManagerIndex(List<FileModel> files)
		{
			using (ZipFile zip = new ZipFile())
			{
				zip.AlternateEncodingUsage = ZipOption.AsNecessary;
				zip.AddDirectoryByName("Files");
				foreach (FileModel file in files)
				{
					if (file.IsSelected)
					{
						zip.AddFile(file.FilePath, "Files");
					}
				}
				string zipName = String.Format("FilesZip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
				using (MemoryStream memoryStream = new MemoryStream())
				{
					zip.Save(memoryStream);
					return File(memoryStream.ToArray(), "application/zip", zipName);
				}
			}
		}
	}
}

