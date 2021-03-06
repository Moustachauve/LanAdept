﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace LanAdeptCore.HtmlExtension
{
	public static class ListItemCategoryExtension
    {
		/// <summary>
		/// Create a link wrapped inside a <li> tag and add the "active" class to it if the user is in this controller.
		/// This will also check if the user has permission for the action
		/// </summary>
		/// <param name="name">Text for the link</param>
		/// <param name="actionName">Name of the action</param>
		public static MvcHtmlString ListItemCategory(this HtmlHelper helper, string linkText, string actionName)
		{
			return ListItemCategory(helper, linkText, actionName, null, null, null);
		}

		/// <summary>
		/// Create a link wrapped inside a <li> tag and add the "active" class to it if the user is in this controller.
		/// This will also check if the user has permission for the action
		/// </summary>
		/// <param name="name">Text for the link</param>
		/// <param name="actionName">Name of the action</param>
		/// <param name="controllerName">Name of the controller</param>
		public static MvcHtmlString ListItemCategory(this HtmlHelper helper, string linkText, string actionName, string controllerName)
		{
			return ListItemCategory(helper, linkText, actionName, controllerName, null, null);
		}

		/// <summary>
		/// Create a link wrapped inside a <li> tag and add the "active" class to it if the user is in this controller.
		/// This will also check if the user has permission for the action
		/// </summary>
		/// <param name="name">Text for the link</param>
		/// <param name="actionName">Name of the action</param>
		/// <param name="controllerName">Name of the controller</param>
		/// <param name="controllerName">Controller for the link</param>
		public static MvcHtmlString ListItemCategory(this HtmlHelper helper, string linkText, string actionName, string controllerName, object routeValues)
		{
			return ListItemCategory(helper, linkText, actionName, controllerName, routeValues, null);
		}

		/// <summary>
		/// Create a link wrapped inside a <li> tag and add the "active" class to it if the user is in this controller.
		/// This will also check if the user has permission for the action
		/// </summary>
		/// <param name="name">Text for the link</param>
		/// <param name="actionName">Name of the action</param>
		/// <param name="controllerName">Name of the controller</param>
		/// <param name="controllerName">Controller for the link</param>
		/// <param name="routeValues">routeValues for the link</param>
		public static MvcHtmlString ListItemCategory(this HtmlHelper helper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
		{
			var currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];
			var url = helper.AuthorizeActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);

			var sb = new StringBuilder();

			if (url != MvcHtmlString.Empty)
			{
				sb.Append("<li");
				if ((string.IsNullOrWhiteSpace(controllerName) || currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase)))
				{
					sb.Append(" class=\"active\"");
				}
				sb.Append(">");

				sb.AppendFormat(url.ToHtmlString());
				sb.Append("</li>");
			}

			return new MvcHtmlString(sb.ToString());
		}
	}
}
