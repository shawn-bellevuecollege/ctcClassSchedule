﻿using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using CTCClassSchedule.Models;
using CtcApi.Web.Security;
using MvcMiniProfiler;
using System.Configuration;
using System.Text;

namespace CTCClassSchedule.Common
{
	public static class Helpers
	{
		/// <summary>
		/// Useful if a helper method works with a timespan and should default to
		/// either 12:00am or 23:59pm (start time/end time).
		/// </summary>
		private enum DefaultTimeResult
		{
			StartTime,
			EndTime
		};

		public static string getBodyClasses(HttpContextBase Context)
		{
			string classes = "";

			if (Context.User.Identity.IsAuthenticated)
			{
				string appSetting = ConfigurationManager.AppSettings["ApplicationDeveloper"];
				if (!string.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? "role-developer" : "";
				}

				appSetting = ConfigurationManager.AppSettings["ApplicationAdmin"];
				if (!string.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? " role-schedule-editors " : "";
				}

				appSetting = ConfigurationManager.AppSettings["ApplicationEditor"];
				if (!string.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					classes += ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? " role-admin " : "";
				}
			}
			else
			{
				classes = "not-logged-in";
			}

			return classes;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Context"></param>
		/// <returns></returns>
		public static bool isAdmin(HttpContextBase Context)
		{
			bool isAdmin = false;
			if (Context.User.Identity.IsAuthenticated)
			{
				string appSetting = ConfigurationManager.AppSettings["ApplicationAdmin"];
				if (!string.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					isAdmin = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? true : false;
				}
			}
			return isAdmin;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Context"></param>
		/// <returns></returns>
		public static bool isEditor(HttpContextBase Context)
		{
			bool isEditor = false;
			if (Context.User.Identity.IsAuthenticated)
			{
				string appSetting = ConfigurationManager.AppSettings["ApplicationEditor"];
				if (!string.IsNullOrWhiteSpace(appSetting))
				{
					string[] roles = appSetting.Split(',');
					isEditor = ActiveDirectoryRoleProvider.IsUserInRoles(Context.User, roles) ? true : false;
				}
			}
			return isEditor;
		}


		// TODO: Jeremy, another optional/BCC specific way of getting data - find another way?
		public static String getProfileURL(string SID)
		{
			Encryption64 en = new Encryption64();

			string returnString = "";
			returnString = "http://bellevuecollege.edu/directory/PersonDetails.aspx?PersonID=" + en.Encrypt(SID,"!#$a54?5");
			return returnString;

		}

		// TODO: Jeremy, another optional/BCC specific way of getting data - find another way?
		public static string getBookstoreBooksLink(List<SectionWithSeats> linkedSections)
		{
			StringBuilder resultURL = new StringBuilder("http://bellevue.verbacompare.com/comparison?id=");
			for (int i = 0; i < linkedSections.Count; i++)
			{
				SectionWithSeats sec = linkedSections[i];
				resultURL.AppendFormat("{0}{1}__{2}__{3}{4}__{5}", (i > 0? "%2C" : string.Empty),
																	getYRQValueForBookstoreURL(sec.Yrq),
																	sec.CourseSubject,
																	sec.CourseNumber,
																	(sec.IsCommonCourse ? "%26" : string.Empty),
																	sec.ID.ItemNumber);
			}

			return resultURL.ToString();
		}

		/// <summary>
		/// returns an IList<ISectionFacet> that contains all of the facet information
		/// passed into the app by the user clicking on the faceted search left pane
		/// facets accepted: flex, time, days, availability
		/// </summary>
		static public IList<ISectionFacet> addFacets(string timestart, string timeend, string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s, string f_oncampus, string f_online, string f_hybrid, string f_telecourse, string avail, string latestart, string numcredits)
		{
			IList<ISectionFacet> facets = new List<ISectionFacet>();

			//add the class format facet options (online, hybrid, telecourse, on campus)
			ModalityFacet.Options modality = ModalityFacet.Options.All;	// default

			if (!String.IsNullOrWhiteSpace(f_online))
			{
				modality = (modality | ModalityFacet.Options.Online);
			}
			if (!String.IsNullOrWhiteSpace(f_hybrid))
			{
				modality = (modality | ModalityFacet.Options.Hybrid);
			}
			if (!String.IsNullOrWhiteSpace(f_telecourse))
			{
				modality = (modality | ModalityFacet.Options.Telecourse);
			}
			if (!String.IsNullOrWhiteSpace(f_oncampus))
			{
				modality = (modality | ModalityFacet.Options.OnCampus);
			}
			facets.Add(new ModalityFacet(modality));


			//determine integer values for start/end time hours and minutes
			int startHour, startMinute;
			int endHour, endMinute;
			getHourAndMinuteFromString(timestart, out startHour, out startMinute);
			getHourAndMinuteFromString(timeend, out endHour, out endMinute, DefaultTimeResult.EndTime);

			//add the time facet
			facets.Add(new TimeFacet(new TimeSpan(startHour, startMinute, 0), new TimeSpan(endHour, endMinute, 0)));

			//day of the week facets
			DaysFacet.Options days = DaysFacet.Options.All;	// default

			if (!String.IsNullOrWhiteSpace(day_su))
			{
				days = (days | DaysFacet.Options.Sunday);
			}
			if (!String.IsNullOrWhiteSpace(day_m))
			{
				days = (days | DaysFacet.Options.Monday);
			}
			if (!String.IsNullOrWhiteSpace(day_t))
			{
				days = (days | DaysFacet.Options.Tuesday);
			}
			if (!String.IsNullOrWhiteSpace(day_w))
			{
				days = (days | DaysFacet.Options.Wednesday);
			}
			if (!String.IsNullOrWhiteSpace(day_th))
			{
				days = (days | DaysFacet.Options.Thursday);
			}
			if (!String.IsNullOrWhiteSpace(day_f))
			{
				days = (days | DaysFacet.Options.Friday);
			}
			if (!String.IsNullOrWhiteSpace(day_s))
			{
				days = (days | DaysFacet.Options.Saturday);
			}
			facets.Add(new DaysFacet(days));


			if (!String.IsNullOrWhiteSpace(avail))
			{
				if (avail == "All")
				{
					facets.Add(new AvailabilityFacet(AvailabilityFacet.Options.All));
				}

				if (avail == "Open")
				{
					facets.Add(new AvailabilityFacet(AvailabilityFacet.Options.Open));
				}
			}

			if (!String.IsNullOrWhiteSpace(latestart))
			{
				if (latestart == "true")
				{
					facets.Add(new LateStartFacet());
				}
			}


			if (!String.IsNullOrWhiteSpace(numcredits))
			{
				if (numcredits != "Any")
				{
					int credits;
					try
					{
						credits = Convert.ToInt16(numcredits);
						facets.Add(new CreditsFacet(credits));
					}
					catch
					{
						throw new System.FormatException("Number of credits was not a valid integer");
					}
				}

			}




			return facets;
		}

		/// <summary>
		/// Returns a friendly time in sentance form given a datetime. This value
		/// is create by subtracting the input datetime from the current datetime.
		/// example: 6/8/2011 07:23:123 -> about 4 hours ago
		/// </summary>
		public static string getFriendlyTime(DateTime theDate)
		{
			const int SECOND = 1;
			const int MINUTE = 60 * SECOND;
			const int HOUR = 60 * MINUTE;
			const int DAY = 24 * HOUR;
			const int MONTH = 30 * DAY;

			var deltaTimeSpan = new TimeSpan(DateTime.Now.Ticks - theDate.Ticks);

			var delta = deltaTimeSpan.TotalSeconds;

			if (delta < 0)
			{
				return "not yet";
			}

			if (delta < 1 * MINUTE)
			{
				return deltaTimeSpan.Seconds == 1 ? "one second ago" : deltaTimeSpan.Seconds + " seconds ago";
			}
			if (delta < 2 * MINUTE)
			{
				return "a minute ago";
			}
			if (delta < 45 * MINUTE)
			{
				return deltaTimeSpan.Minutes + " minutes ago";
			}
			if (delta < 90 * MINUTE)
			{
				return "an hour ago";
			}
			if (delta < 24 * HOUR)
			{
				return deltaTimeSpan.Hours + " hours ago";
			}
			if (delta < 48 * HOUR)
			{
				return "yesterday";
			}
			if (delta < 30 * DAY)
			{
				return deltaTimeSpan.Days + " days ago";
			}
			if (delta < 12 * MONTH)
			{
				int months = Convert.ToInt32(Math.Floor((double)deltaTimeSpan.Days / 30));
				return months <= 1 ? "one month ago" : months + " months ago";
			}
			else
			{
				int years = Convert.ToInt32(Math.Floor((double)deltaTimeSpan.Days / 365));
				return years <= 1 ? "one year ago" : years + " years ago";
			}
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="fieldID"></param>
		/// <param name="fieldTitle"></param>
		/// <param name="fieldValue"></param>
		/// <returns></returns>
		static public GeneralFacetInfo getFacetInfo(string fieldID, string fieldTitle, string fieldValue)
		{
			return new GeneralFacetInfo {
			  ID = fieldID,
			  Title = fieldTitle,
				Value = fieldValue,
			  Selected = Utility.SafeConvertToBool(fieldValue)
			};
		}



		static public IList<GeneralFacetInfo> ConstructModalityList(string f_oncampus, string f_online, string f_hybrid, string f_telecourse)
		{
			IList<GeneralFacetInfo> modality = new List<GeneralFacetInfo>(4);

			modality.Add(getFacetInfo("f_oncampus", "On Campus", f_oncampus));
			modality.Add(getFacetInfo("f_online", "Online", f_online));
			modality.Add(getFacetInfo("f_hybrid", "Hybrid", f_hybrid));
			modality.Add(getFacetInfo("f_telecourse", "Telecourse", f_telecourse));

			return modality;
		}


		static public IList<GeneralFacetInfo> ConstructDaysList(string day_su, string day_m, string day_t, string day_w, string day_th, string day_f, string day_s)
		{
			IList<GeneralFacetInfo> modality = new List<GeneralFacetInfo>(7);

			modality.Add(getFacetInfo("day_su", "S", day_su));
			modality.Add(getFacetInfo("day_m", "M", day_m));
			modality.Add(getFacetInfo("day_t", "T", day_t));
			modality.Add(getFacetInfo("day_w", "W", day_w));
			modality.Add(getFacetInfo("day_th", "T", day_th));
			modality.Add(getFacetInfo("day_f", "F", day_f));
			modality.Add(getFacetInfo("day_s", "S", day_s));

			return modality;
		}

		/// <summary>
		/// Gets the <see cref="YearQuarter"/> for the current, and previous 3 quarters.
		/// This drives the dynamic YRQ navigation bar
		/// </summary>
		static public IList<YearQuarter> getYearQuarterListForMenus(OdsRepository repository)
		{
			IList<YearQuarter> currentFutureQuarters = repository.GetRegistrationQuarters(4);
			return currentFutureQuarters;
		}

		/// <summary>
		/// Gets the <see cref="YearQuarter"/> for the current, and future 2 quarters.
		/// </summary>
		static public IList<YearQuarter> getFutureQuarters(OdsRepository repository)
		{
			IList<YearQuarter> futureQuarters = repository.GetFutureQuarters(3);
			return futureQuarters;
		}

		/// <summary>
		/// Gets the current http get/post params and assigns them to an IDictionary<string, object>
		/// This is mainly used to set a Viewbag variable so these can be passed into action links in the views.
		/// </summary>
		/// <param name="httpRequest"></param>
		/// <param name="ignoreKeys">List of key values to ignore.</param>
		static public IDictionary<string, object> getLinkParams(HttpRequestBase httpRequest, params string[] ignoreKeys)
		{
			IDictionary<string, object> linkParams = new Dictionary<string, object>(httpRequest.QueryString.Count);
			foreach (string key in httpRequest.QueryString.AllKeys)
			{
				if (key != "X-Requested-With" && !ignoreKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
				{
					if (linkParams.ContainsKey(key))
					{
						linkParams[key] = httpRequest.QueryString[key];
					}
					else
					{
						linkParams.Add(key, httpRequest.QueryString[key]);
					}
				}
			}
			foreach (string key in httpRequest.Form.AllKeys)
			{
				if (!ignoreKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
				{
					if (linkParams.ContainsKey(key))
					{
						linkParams[key] = httpRequest.Form[key];
					}
					else
					{
						linkParams.Add(key, httpRequest.Form[key]);
					}
				}
			}
			return linkParams;
		}

		/// <summary>
		/// Common method to retrieve <see cref="SectionWithSeats"/> records
		/// </summary>
		/// <param name="currentYrq"></param>
		/// <param name="sections"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static IList<SectionWithSeats> GetSectionsWithSeats(string currentYrq, IList<Section> sections, ClassScheduleDb db)
		{
			MiniProfiler profiler = MiniProfiler.Current;

			// ensure we're ALWAYS getting the latest data from the database (i.e. ignore any cached data)
			// Reference: http://forums.asp.net/post/2848021.aspx
			db.vw_Class.MergeOption = MergeOption.OverwriteChanges;

			IList<vw_Class> classes;
      using (profiler.Step("API::Get Class Schedule Specific Data()"))
      {
				classes = db.vw_Class.Where(c => c.YearQuarterID == currentYrq).ToList();
      }


			IList<SectionWithSeats> sectionsEnum;
      using (profiler.Step("Joining all data"))
      {
          sectionsEnum = (
			      from c in sections
                      join d in classes on c.ID.ToString() equals d.ClassID into t1
			      from d in t1.DefaultIfEmpty()
											join ss in db.SectionSeats on (d != null ? d.ClassID : "") equals ss.ClassID into t2
											from ss in t2.DefaultIfEmpty()
											join sm in db.SectionsMetas on (d != null ? d.ClassID : "") equals sm.ClassID into t3
											from sm in t3.DefaultIfEmpty()
                      // NOTE:  This logic assumes that data will only be saved in ClassScheduleDb after having come through
                      //        the filter of the CtcApi - which normalizes spacing of the CourseID field data.
                      join cm in db.CourseMetas on (d != null ? d.CourseID : "") equals cm.CourseID into t4
											from cm in t4.DefaultIfEmpty()
			      orderby c.Yrq.ID descending
			      select new SectionWithSeats {
								ParentObject = c,
								SeatsAvailable = ss != null ? ss.SeatsAvailable : int.MinValue,	// allows us to identify past quarters (with no availability info)
								LastUpdated = (d != null ? d.LastUpdated.GetValueOrDefault() : DateTime.MinValue).ToString("h:mm tt").ToLower(),
													SectionFootnotes = sm != null && !string.IsNullOrWhiteSpace(sm.Footnote) ? sm.Footnote : string.Empty,
													CourseFootnotes = cm != null && !string.IsNullOrWhiteSpace(cm.Footnote) ? cm.Footnote : string.Empty,
													CourseTitle = sm != null && !string.IsNullOrWhiteSpace(sm.Title) ? sm.Title : c.CourseTitle,
													CustomDescription = sm != null && !string.IsNullOrWhiteSpace(sm.Description) ? sm.Description : string.Empty,
											}).OrderBy(s => s.CourseNumber).ThenBy(s => s.CourseTitle).ToList();

        // Flag sections that are cross-linked with a Course
        foreach (string sec in db.SectionCourseCrosslistings.Select(x => x.ClassID).Distinct())
        {
          sectionsEnum.Single(s => s.ID.ToString() == sec).IsCrossListed = true;
        }
      }

			return sectionsEnum;
		}

		public static string getFriendlyDayRange(string dayString, Dictionary<string, string> dict)
		{
			string friendlyName;
			if (dict.ContainsKey(dayString))
			{
				friendlyName = dict[dayString];
			}
			else
			{
				friendlyName = dayString;
			}
			return friendlyName;
		}

		/// <summary>
		/// Retrieves all active and future course descriptions from a <see cref="Course"/>
		/// </summary>
		/// <param name="course">The course to process</param>
		/// <param name="currentYrq">The current YRQ</param>
		/// <returns>An ordered list of CourseDescriptions sorted current first</returns>
		public static IList<CourseDescription> getActiveCourseDescriptions(Course course, YearQuarter currentYrq)
		{
			IList<CourseDescription> results = new List<CourseDescription>();
			if (course.Descriptions != null && course.Descriptions.Count() > 0)
			{
				IList<CourseDescription> descriptions = course.Descriptions.Reverse().ToList();
				results = descriptions.Where(q => String.Compare(q.YearQuarterBegin.ID, currentYrq.ID) > 0).ToList();
				if (results.Count == 0) { results.Add(descriptions.First()); }
			}

			return results;
		}

		public static Dictionary<string, string> getDayDictionary(){
			Dictionary<string, string> dict = new Dictionary<string, string>();

			dict.Add("Online", "Online");
			dict.Add("M", "Monday");
			dict.Add("T", "Tuesday");
			dict.Add("W", "Wednesday");
			dict.Add("Th", "Thursday");
			dict.Add("F", "Friday");
			dict.Add("Sa", "Saturday");
			dict.Add("DAILY", "Daily");
			dict.Add("MWF", "Monday/Wednesday/Friday");
			dict.Add("TTh", "Tuesday/Thursday");
			dict.Add("MWThF", "Monday/Wednesday/Thursday/Friday");
			dict.Add("MTWTh", "Monday/Tuesday/Wednesday/Thursday");
			dict.Add("MW", "Monday/Wednesday");
			dict.Add("WF", "Wednesday/Friday");
			dict.Add("TF", "Tuesday/Friday");
			dict.Add("MF", "Monday/Friday");
			dict.Add("ThF", "Thursday/Friday");
			dict.Add("MWTh", "Monday/Wednesday/Thursday");
			dict.Add("MTWF", "Monday/Tuesday/Wednesday/Friday");
			dict.Add("MTTh", "Monday/Tuesday/Thursday");
			dict.Add("WTh", "Wednesday/Thursday");
			dict.Add("MTF", "Monday/Tuesday/Friday");
			dict.Add("MT", "Monday/Tuesday");
			dict.Add("TWThF", "Tuesday/Wednesday/Thursday/Friday");
			dict.Add("ARRANGED", "Arranged");
			dict.Add("Su", "Sunday");
			dict.Add("TThF", "Tuesday/Thursday/Friday");
			dict.Add("TW", "Tuesday/Wednesday");
			dict.Add("MTh", "Monday/Thursday");
			dict.Add("MTW", "Monday/Tuesday/Wednesday");
			dict.Add("MThF", "Monday/Thursday/Friday");
			dict.Add("TWTh", "Tuesday/Wednesday/Thursday");
			dict.Add("TWF", "Tuesday/Wednesday/Friday");
			dict.Add("WThF", "Wednesday/Thursday/Friday");
			dict.Add("MTThF", "Monday/Tuesday/Thursday/Friday");
			dict.Add("TSa", "Tuesday/Saturday");
			dict.Add("ThSa", "Thursday/Saturday");
			dict.Add("FSa", "Friday/Saturday");
			dict.Add("ThSu", "Thursday/Sunday");
			dict.Add("MSu", "Monday/Sunday");
			dict.Add("WSa", "Wednesday/Saturday");
			dict.Add("MWSa", "Monday/Wednesday/Saturday");
			dict.Add("TThSa", "Tuesday/Thursday/Saturday");
			dict.Add("WThSa", "Wednesday/Thursday/Saturday");
			dict.Add("SaSu", "Saturday/Sunday");
			dict.Add("WSu", "Wednesday/Sunday");
			dict.Add("TWThSa", "Tuesday/Wednesday/Thursday/Saturday");
			dict.Add("MTWThFSaSu", "Monday/Tuesday/Wednesday/Thursday/Friday/Saturday/Sunday");
			dict.Add("WFSa", "Wednesday/Friday/Saturday");
			dict.Add("MTWThSa", "Monday/Tuesday/Wednesday/Thursday/Saturday");
			dict.Add("MTSaSu", "Monday/Tuesday/Saturday/Sunday");
			dict.Add("FSaSu", "Friday/Saturday/Sunday");
			dict.Add("MFSa", "Monday/Friday/Saturday");
			dict.Add("ThFSa", "Thursday/Friday/Saturday");
			dict.Add("MTWThFSa", "Monday/Tuesday/Wednesday/Thursday/Friday/Saturday");
			dict.Add("MSa", "Monday/Saturday");
			dict.Add("TSu", "Tuesday/Sunday");
			dict.Add("TWFSa", "Tuesday/Wednesday/Friday/Saturday");
			dict.Add("TFSu", "Tuesday/Friday/Sunday");
			dict.Add("TThSu", "Tuesday/Thursday/Sunday");
			dict.Add("TThSaSu", "Tuesday/Thursday/Saturday/Sunday");
			dict.Add("MWFSa", "Monday/Wednesday/Friday/Saturday");
			dict.Add("TWThSaSu", "Tuesday/Wednesday/Thursday/Saturday/Sunday");
			dict.Add("TWFSaSu", "Tuesday/Wednesday/Friday/Saturday/Sunday");
			dict.Add("MTSu", "Monday/Tuesday/Sunday");
			dict.Add("TFSa", "Tuesday/Friday/Saturday");
			dict.Add("TWThFSa", "Tuesday/Wednesday/Thursday/Friday/Saturday");
			dict.Add("TFSaSu", "Tuesday/Friday/Saturday/Sunday");
			dict.Add("TWSa", "Tuesday/Wednesday/Saturday");
			dict.Add("ThSaSu", "Thursday/Saturday/Sunday");
			dict.Add("MTWSa", "Monday/Tuesday/Wednesday/Saturday");
			dict.Add("MWSaSu", "Monday/Wednesday/Saturday/Sunday");
			dict.Add("FSu", "Friday/Sunday");
			dict.Add("WThFSa", "Wednesday/Thursday/Friday/Saturday");
			dict.Add("TWThSu", "Tuesday/Wednesday/Thursday/Sunday");
			dict.Add("TWThFSu", "Tuesday/Wednesday/Thursday/Friday/Sunday");
			dict.Add("MWThSa", "Monday/Wednesday/Thursday/Saturday");
			dict.Add("ThFSaSu", "Thursday/Friday/Saturday/Sunday");
			dict.Add("ThFSu", "Thursday/Friday/Sunday");
			dict.Add("MTWThFSu", "Monday/Tuesday/Wednesday/Thursday/Friday/Sunday");
			dict.Add("TSaSu", "Tuesday/Saturday/Sunday");
			dict.Add("WFSu", "Wednesday/Friday/Sunday");
			dict.Add("MTThSa", "Monday/Tuesday/Thursday/Saturday");
			dict.Add("MTWThSaSu", "Monday/Tuesday/Wednesday/Thursday/Saturday/Sunday");
			dict.Add("MTSa", "Monday/Tuesday/Saturday");
			dict.Add("WFSaSu", "Wednesday/Friday/Saturday/Sunday");
			dict.Add("MSaSu", "Monday/Saturday/Sunday");
			dict.Add("MThSa", "Monday/Thursday/Saturday");
			dict.Add("MFSaSu", "Monday/Friday/Saturday/Sunday");
			dict.Add("TWSu", "Tuesday/Wednesday/Sunday");
			dict.Add("TThFSa", "Tuesday/Thursday/Friday/Saturday");
			dict.Add("MTWFSa", "Monday/Tuesday/Wednesday/Friday/Saturday");
			dict.Add("MTWThSu", "Monday/Tuesday/Wednesday/Thursday/Sunday");
			dict.Add("TThFSaSu", "Tuesday/Thursday/Friday/Saturday/Sunday");
			dict.Add("MTWSu", "Monday/Tuesday/Wednesday/Sunday");
			dict.Add("MWSu", "Monday/Wednesday/Sunday");
			dict.Add("MTFSaSu", "Monday/Tuesday/Friday/Saturday/Sunday");
			dict.Add("MTFSa", "Monday/Tuesday/Friday/Saturday");
			dict.Add("MWThFSa", "Monday/Wednesday/Thursday/Friday/Saturday");

			return dict;
		}

		public static string BuildCourseID(string CourseNumber, string CourseSubject, bool IsCommonCourse)
		{
			char[] CourseID = new char[8];

			//fill the char array with spaces
			for (int i = 0; i < CourseID.Length; i++)
			{
				CourseID[i] = ' ';
			}

			//assign the subject to the first 2/3/4/5 char array slots
			for (int i = 0; i < CourseSubject.Count(); i++)
			{
				CourseID[i] = CourseSubject[i];
			}

			//add in the ampersand if it's a common course
			if (IsCommonCourse)
			{
				CourseID[CourseSubject.Length] = '&';
			}

			//if the coursenumber is 3 chars or longer, assign it to the last 3 slots in the return array
			if (CourseNumber.Length >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					CourseID[i + 5] = CourseNumber[i];
				}
			}

			//if the coursenumber is one of the exception 4 alphanumeric courseids, such as NURS 101X (2503B124),
			//create a new 9 character return array, copy the 8 character array to it, and append the last character of
			//the CourseNum to the last slot in the return array.
			if (CourseNumber.Length > 3)
			{
				char[] TempCourseID = CourseID;
				CourseID = new char[9];

				for (int i = 0; i < TempCourseID.Count(); i++)
				{
					CourseID[i] = TempCourseID[i];
				}

				CourseID[8] = CourseNumber[CourseNumber.Length - 1];
			}



			return new string(CourseID);
		}

		public static string SubjectWithCommonCourseFlag(SectionWithSeats sec)
		{
			return SubjectWithCommonCourseFlag(sec.CourseSubject, sec.IsCommonCourse);
		}

		public static string SubjectWithCommonCourseFlag(Course course)
		{
			return SubjectWithCommonCourseFlag(course.Subject, course.IsCommonCourse);
		}

		private static string SubjectWithCommonCourseFlag(string subject, bool isCommonCourse)
		{
			return isCommonCourse ? subject.Trim() + "&" : subject.Trim();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sectionBlock"></param>
		/// <returns></returns>
		public static IList<string> ExtractCommonFootnotes(IList<SectionWithSeats> sectionBlock)
		{
			IEnumerable<string> rollupFootnotes = new string[]{};

			for (int i = 0; i < sectionBlock.Count; i++)
			{
				IEnumerable<string> f = i > 0 ? rollupFootnotes.ToArray() : sectionBlock[0].Footnotes;

				rollupFootnotes = f.Intersect(sectionBlock[i].Footnotes);
			}

			return rollupFootnotes.ToList();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sectionBlock"></param>
		/// <returns></returns>
		public static IList<string> ExtractCommonFootnotes(IEnumerable<SectionWithSeats> sectionBlock)
		{
			return ExtractCommonFootnotes(sectionBlock.ToList());
		}

		/// <summary>
		/// Applies keyword markup to all instances of the <paramref name="searchTerm"/> found in the provided <paramref name="text"/>
		/// </summary>
		/// <param name="searchTerm"></param>
		/// <param name="text"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		static public string FormatWithSearchTerm(string searchTerm, string text, params object[] args)
		{
			if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchTerm))
			{
				return text;
			}

			if (args != null && args.Length > 0)
			{
				text = string.Format(text, args);
			}
			return Regex.Replace(text, searchTerm, @"<em class='keyword'>$&</em>", RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Takes a string that represents a time (ie "6:45pm") and outputs the parsed hour and minute integers
		/// based on a 24 hour clock. If a time cannot be parsed, the output defaults to either 12am or 11:59pm
		/// depending on the time default parameter passed
		/// </summary>
		/// <param name="time">String representing a time value</param>
		/// <param name="hour">Reference to an integer storage for the parsed hour value</param>
		/// <param name="minute">Reference to an integer storage for the parsed minute value</param>
		/// <param name="defaultResultMode">Flag which determines the default result values if a time values were unable to be parsed</param>
		static private void getHourAndMinuteFromString(string time, out int hour, out int minute, DefaultTimeResult defaultResultMode = DefaultTimeResult.StartTime)
		{
			// In case the method is unable to convert a time, default to either a start/end time
			switch (defaultResultMode)
			{
				case DefaultTimeResult.EndTime:
					hour = 23;
					minute = 59;
					break;
				default:
					hour = 0;
					minute = 0;
					break;
			}

			// Determine integer values for time hours and minutes
			if (!String.IsNullOrWhiteSpace(time))
			{
				string timeTrimmed = time.Trim();
				bool period = (timeTrimmed.Length > 2 ? timeTrimmed.Substring(timeTrimmed.Length - 2) : timeTrimmed).Equals("PM", StringComparison.OrdinalIgnoreCase);

				// Adjust the conversion to integers if the user leaves off a leading 0
				// (possible by using tab instead of mouseoff on the time selector)
				if (time.IndexOf(':') == 2)
				{
					hour = Convert.ToInt16(time.Substring(0, 2)) + (period ? 12 : 0);
					if (time.IndexOf(':') != -1)
					{
						minute = Convert.ToInt16(time.Substring(3, 2));
					}
				}
				else
				{
					hour = Convert.ToInt16(time.Substring(0, 1)) + (period ? 12 : 0);
					if (time.IndexOf(':') != -1)
					{
						minute = Convert.ToInt16(time.Substring(2, 2));
					}
				}
			}
		}

		static private string getYRQValueForBookstoreURL(YearQuarter yrq)
		{
			string quarter = string.Empty;
			string year = string.Empty;

			if (yrq != null)
			{
				quarter = yrq.FriendlyName.ToUpper();
				year = quarter.Substring(quarter.Length - 2);
				if (quarter.Contains("FALL"))
				{
					quarter = "F";
				}
				else if (quarter.Contains("WINTER"))
				{
					quarter = "W";
				}
				else if (quarter.Contains("SPRING"))
				{
					quarter = "S";
				}
				else // Summer
				{
					quarter = "X";
				}
			}

			return String.Concat(quarter, year);
		}
	}
}
