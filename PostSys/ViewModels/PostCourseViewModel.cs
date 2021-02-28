using PostSys.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PostSys.ViewModels
{
	public class PostCourseViewModel
	{
		public IEnumerable<Course> Courses{ get; set; }
		public IEnumerable<Post> Posts { get; set; }
		public Course Course { get; set; }
		public Post Post { get; set; }
		
		//manage course
		public int postId { get; set; }
		public int courseId { get; set; }
		public string courseName { get; set; }
		public string className { get; set; }
		public string studentName { get; set; }
		public DateTime startDate { get; set; }
		public DateTime endDate { get; set; }

		//manage post
		public string postName { get; set; }
	}
}