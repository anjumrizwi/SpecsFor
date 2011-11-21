﻿using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper;
using MvcContrib.TestHelper.Fakes;
using OpenQA.Selenium;
using Microsoft.Web.Mvc;

namespace SpecsFor.Web
{
	public class MvcWebApp : IDisposable
	{
		public static string BaseUrl = "http://localhost";
		public static Func<IWebDriver> BrowserFactory = Web.Browser.InternetExplorer.Factory;

		private bool _hasQuit;

		public IWebDriver Browser { get; private set; }

		public MvcWebApp()
		{
			Browser = BrowserFactory();
		}

		public FormHelper<T> FindFormFor<T>()
		{
			return new FormHelper<T>(this);
		}

		public IWebElement ValidationSummary
		{ 
			get
			{
				return Browser.FindElement(By.ClassName("validation-summary-errors"));
			}
		}

		public RouteData Route
		{
			get
			{
				//Strip the host, port, etc. off the route.
				var url = Browser.Url.Replace(BaseUrl, "~");

				return url.Route();
			}
		}

		private class FakeViewDataContainer : IViewDataContainer
		{
			private ViewDataDictionary _viewData = new ViewDataDictionary();
			public ViewDataDictionary ViewData { get { return _viewData; } set { _viewData = value; } }
		}

		public void NavigateTo<TController>(Expression<Action<TController>> action) where TController : Controller
		{
			var helper = new HtmlHelper(new ViewContext { HttpContext = FakeHttpContext.Root() }, new FakeViewDataContainer());

			var url = helper.BuildUrlFromExpression(action);

			Browser.Navigate().GoToUrl(BaseUrl + url);
		}

		public void Dispose()
		{
			if (!_hasQuit)
			{
				_hasQuit = true;
				Browser.Quit();
			}

			Browser.Dispose();
		}
	}
}