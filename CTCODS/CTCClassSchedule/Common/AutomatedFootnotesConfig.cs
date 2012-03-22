﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctc.Ods.Types;

namespace CTCClassSchedule
{
	/// <summary>
	/// A handler class for the Automated Footnote settings in the web.config
	/// </summary>
	public class AutomatedFootnotesConfig
	{
		/// <summary>
		/// Private dictionary that maps the name of each automated footnote message
		/// to the footnote element itself.
		/// </summary>
		protected static Dictionary<string, AutomatedFootnoteElement> _footnoteInstances;

		/// <summary>
		/// Constructor. Initiates the serialization of the automated footnotes section in the
		/// web.config file.
		/// </summary>
		static AutomatedFootnotesConfig()
		{
			AutomatedFootnotesSection sec = (AutomatedFootnotesSection)System.Configuration.ConfigurationManager.GetSection("ctcAutomatedFootnoteSettings");
			_footnoteInstances = GetFootnoteInstances(sec.FootnoteInstances);
		}

		/// <summary>
		/// Returns a footnote message element based on the name.
		/// </summary>
		/// <param name="footnoteName">The anme of the element you want.</param>
		/// <returns>A single footnote element if it exists.</returns>
		public static AutomatedFootnoteElement Footnotes(string footnoteName)
		{
			return _footnoteInstances[footnoteName];
		}

    /// <summary>
    /// Takes a SectionWithSeats and produces all automated messages that the section should display.
    /// </summary>
    /// <param name="section">The section to base the generated footnote messages on.</param>
    /// <returns>A string of all automated footnote messages concatenated.</returns>
    public static string getAutomatedFootnotesText(SectionWithSeats section)
    {
			string wSpace = section.Footnotes.Count() == 0 ? string.Empty : " ";
			string footenoteText = buildFootnoteText(section.IsLateStart,
																								section.IsDifferentEndDate,
																								section.IsHybrid,
																								section.IsContinuousEnrollment,
																								section.StartDate.GetValueOrDefault(DateTime.Now),
																								section.EndDate.GetValueOrDefault(DateTime.Now));
			return wSpace + footenoteText;
    }

		/// <summary>
		/// Takes a Section and produces all automated messages that the section should display.
		/// </summary>
		/// <param name="section">The section to base the generated footnote messages on.</param>
		/// <returns>A string of all automated footnote messages concatenated.</returns>
		public static string getAutomatedFootnotesText(Section section)
		{
			string wSpace = section.Footnotes.Count() == 0 ? string.Empty : " ";
			string footenoteText = buildFootnoteText(section.IsLateStart,
																								section.IsDifferentEndDate,
																								section.IsHybrid,
																								section.IsContinuousEnrollment,
																								section.StartDate.GetValueOrDefault(DateTime.Now),
																								section.EndDate.GetValueOrDefault(DateTime.Now));
			return wSpace + footenoteText;
		}

		/// <summary>
		/// Gets automated footnotes based on boolean values passed to the method.
		/// This is useful if you are handling either Section or SectionWithSeats objects.
		/// </summary>
		/// <param name="lateStartFlag">Is the course a late start course.</param>
		/// <param name="differentEndDateFlag">Does the course have a different end date than normal.</param>
		/// <param name="hybridFlag">Is this a hybrid course.</param>
		/// <param name="continuousEnrollmentFlag">Is this course a continuous enrollment.</param>
		/// <param name="startDate">The courses scheduled start date.</param>
		/// <param name="endDate">The courses scheduled end date.</param>
		/// <returns>All relevant automated footnotes in one concatenated string.</returns>
		private static string buildFootnoteText(Boolean lateStartFlag, Boolean differentEndDateFlag, Boolean hybridFlag, Boolean continuousEnrollmentFlag, DateTime startDate, DateTime endDate)
		{
			string footnoteTextResult = string.Empty;
			string dateParam = "{DATE}";
			string dateText;
			AutomatedFootnoteElement footnote;

			// If the section has a late start
			if (lateStartFlag)
			{
				footnote = Footnotes("lateStart");
				dateText = startDate.ToString(footnote.StringFormat);
				footnoteTextResult += footnote.Text.Replace(dateParam, dateText) + " ";
			}

			// If the section has a different end date than usual
			if (differentEndDateFlag)
			{
				footnote = Footnotes("endDate");
				dateText = endDate.ToString(footnote.StringFormat);
				footnoteTextResult += footnote.Text.Replace(dateParam, dateText) + " ";
			}

			// If the section is a hybrid section
			if (hybridFlag)
			{
				footnoteTextResult += Footnotes("hybrid").Text;
			}

			// If the section is a continuous enrollment section
			if (continuousEnrollmentFlag)
			{
				footnote = Footnotes("continuousEnrollment");
				dateText = endDate.ToString(footnote.StringFormat);
				footnoteTextResult += footnote.Text.Replace(dateParam, dateText) + " ";
			}

			return footnoteTextResult.Trim();
		}


		/// <summary>
		/// Returns a dictionary of all elements within a given collection of footnote messages.
		/// The key is the name of the footnote element, the value is the element itself.
		/// </summary>
		/// <param name="collection">The collection to convert to a dictionary.</param>
		/// <returns>A dictionary of footnote elements mapped by the element name.</returns>
		private static Dictionary<string, AutomatedFootnoteElement> GetFootnoteInstances(AutomatedFootnoteCollection collection)
		{
			Dictionary<string, AutomatedFootnoteElement> instances = new Dictionary<string, AutomatedFootnoteElement>();

			foreach (AutomatedFootnoteElement i in collection)
			{
				instances.Add(i.Name, i);
			}

			return instances;
		}
	}
}
