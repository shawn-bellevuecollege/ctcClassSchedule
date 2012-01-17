﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Common;
using CTCClassSchedule.Models;
using CTCClassSchedule.Properties;
using System;









namespace CTCClassSchedule.Controllers
{
	public class ApiController : Controller
	{
		public ApiController()
		{
			ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		//
		// GET: /Api/Subjects
		/// <summary>
		/// Returns an array of <see cref="Course"/> Subjects
		/// </summary>
		/// <param name="format"></param>
		///<param name="YearQuarter"></param>
		///<returns>
		///		Either a <see cref="PartialViewResult"/> which can be embedded in an MVC View,
		///		or the list of <see cref="Course"/> Subjects as a JSON array.
		/// </returns>
		/// <remarks>
		///		To receive the list as a JSON array call this method with <i>format=json</i>:
		///		<example>
		///			http://localhost/Api/Subjects?format=json
		///		</example>
		/// </remarks>
		[HttpGet]
		public ActionResult Subjects(string format, string YearQuarter)
		{
			using (OdsRepository db = new OdsRepository(HttpContext))
			{
				IList<CoursePrefix> data;
				data = string.IsNullOrWhiteSpace(YearQuarter) || YearQuarter.ToUpper() == "ALL" ? db.GetCourseSubjects() : db.GetCourseSubjects(Ctc.Ods.Types.YearQuarter.FromFriendlyName(YearQuarter));

				IList<vw_ProgramInformation> progInfo;
				using (ClassScheduleDb classScheduleDb = new ClassScheduleDb())
				{
					progInfo = (from s in classScheduleDb.vw_ProgramInformation select s).ToList();
				}
				IList<ScheduleCoursePrefix> subjectList = (from p in progInfo
																									where data.Select(c => c.Subject).Contains(p.Abbreviation.TrimEnd('&'))
																									select new ScheduleCoursePrefix
																														{
																															Subject = p.URL,
																															Title = p.Title
																														})
																									.OrderBy(s => s.Title)
																									.Distinct()
																									.ToList();

				if (format == "json")
				{
					// NOTE: AllowGet exposes the potential for JSON Hijacking (see http://haacked.com/archive/2009/06/25/json-hijacking.aspx)
					// but is not an issue here because we are receiving and returning public (e.g. non-sensitive) data
					return Json(subjectList, JsonRequestBehavior.AllowGet);
				}

				ViewBag.LinkParams = Helpers.getLinkParams(Request);
				ViewBag.SubjectsColumns = 2;

				return PartialView(subjectList);
			}
		}


		//Generation of the
		public ActionResult SectionEdit(string itemNumber, string yrq, string subject, string classNum)
		{
			string courseIdPlusYRQ = itemNumber + yrq;

			using (OdsRepository respository = new OdsRepository(HttpContext))
			{
				IList<YearQuarter> yrqRange = Helpers.getYearQuarterListForMenus(respository);
				ViewBag.QuarterNavMenu = yrqRange;

				ICourseID courseID = CourseID.FromString(subject, classNum);
				IList<Section> sections;
				sections = respository.GetSections(courseID);

				Section editSection = null;
				foreach (Section section in sections)
				{
					if (section.ID.ToString() == courseIdPlusYRQ)
					{
						editSection = section;
					}
				}

				sections.Clear();
				sections.Add(editSection);

				IEnumerable<SectionWithSeats> sectionsEnum;
				using (ClassScheduleDb db = new ClassScheduleDb())
				{
					sectionsEnum = Helpers.getSectionsWithSeats(yrqRange[0].ID, sections, db);

					return PartialView(sectionsEnum);
				}
			}

			return PartialView();
		}




		//
		// POST after submit is clicked

		[HttpPost]
		public ActionResult SectionEdit(FormCollection collection)
		{

			string CourseSubject = collection["CourseSubject"];
			string CourseNumber = collection["CourseNumber"];
			string ItemNumber = collection["ItemNumber"];
			string Yrq = collection["Yrq"];
			string Username = collection["Username"];
			string SectionFootnotes = collection["section.SectionFootnotes"];

			try
			{
				// TODO: Add update logic here




/*
 * There's no Index View for Api. You'll want to include the Controller name here too.
 */
				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}


	}
}
