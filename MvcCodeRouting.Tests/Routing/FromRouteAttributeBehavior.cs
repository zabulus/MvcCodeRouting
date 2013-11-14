﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MvcCodeRouting.Tests.Routing {
   
   [TestClass]
   public class FromRouteAttributeBehavior {

      readonly RouteCollection routes;
      readonly UrlHelper Url;

      public FromRouteAttributeBehavior() {

         routes = TestUtil.GetRouteCollection();
         Url = TestUtil.CreateUrlHelper(routes);
      }

      [TestMethod]
      public void UseCustomName() {

         var controller = typeof(FromRouteAttr.CustomNameController);

         routes.Clear();
         routes.MapCodeRoutes(controller);

         Assert.IsNotNull(routes.At(0).Url.Contains("{b}"));
      }

      [TestMethod]
      public void BindCustomName() {

         var controller = typeof(FromRouteAttr.BindCustomNameController);

         routes.Clear();
         routes.MapCodeRoutes(controller);

         var httpContextMock = new Mock<HttpContextBase>();
         httpContextMock.Setup(c => c.Request.AppRelativeCurrentExecutionFilePath).Returns("~/Foo/hello");

         var httpResponseMock = new Mock<HttpResponseBase>();
         httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);
         
         var routeData = routes.GetRouteData(httpContextMock.Object);

         var controllerInstance = new FromRouteAttr.BindCustomNameController { 
            ValidateRequest = false
         };

         var requestContext = new RequestContext(httpContextMock.Object, routeData);
         var controllerContext = new ControllerContext(requestContext, controllerInstance);

         controllerInstance.ValueProvider = new ValueProviderCollection(new IValueProvider[] { new RouteDataValueProvider(controllerContext) });

         ((IController)controllerInstance).Execute(requestContext);

         httpResponseMock.Verify(c => c.Write(It.Is<string>(s => s == "hello")), Times.AtLeastOnce());
      }

      [TestMethod]
      public void BindCustomName_Property() {

         var controller = typeof(FromRouteAttr.BindCustomNamePropertyController);

         routes.Clear();
         routes.MapCodeRoutes(controller);

         var httpContextMock = new Mock<HttpContextBase>();
         httpContextMock.Setup(c => c.Request.AppRelativeCurrentExecutionFilePath).Returns("~/true");

         var routeData = routes.GetRouteData(httpContextMock.Object);

         var controllerInstance = new FromRouteAttr.BindCustomNamePropertyController {
            ValidateRequest = false
         };

         var requestContext = new RequestContext(httpContextMock.Object, routeData);
         var controllerContext = new ControllerContext(requestContext, controllerInstance);

         controllerInstance.ValueProvider = new ValueProviderCollection(new IValueProvider[] { new RouteDataValueProvider(controllerContext) });

         ((IController)controllerInstance).Execute(requestContext);

         Assert.IsTrue(controllerInstance.a);
      }

      [TestMethod]
      public void UsesSpecifiedBinder() {

         var controller = typeof(FromRouteAttr.SpecifiedBinderController);

         routes.Clear();
         routes.MapCodeRoutes(controller);

         var httpContextMock = new Mock<HttpContextBase>();
         httpContextMock.Setup(c => c.Request.AppRelativeCurrentExecutionFilePath).Returns("~/Foo/yes");

         var httpResponseMock = new Mock<HttpResponseBase>();
         httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

         var routeData = routes.GetRouteData(httpContextMock.Object);

         var controllerInstance = new FromRouteAttr.SpecifiedBinderController {
            ValidateRequest = false
         };

         var requestContext = new RequestContext(httpContextMock.Object, routeData);
         var controllerContext = new ControllerContext(requestContext, controllerInstance);
         
         controllerInstance.ValueProvider = new ValueProviderCollection(new IValueProvider[] { new RouteDataValueProvider(controllerContext) });

         ((IController)controllerInstance).Execute(requestContext);

         httpResponseMock.Verify(c => c.Write(It.Is<string>(s => s == "True")), Times.AtLeastOnce());
      }

      [TestMethod]
      public void UsesSpecifiedBinder_Property() {

         var controller = typeof(FromRouteAttr.SpecifiedBinderPropertyController);

         routes.Clear();
         routes.MapCodeRoutes(controller);

         var httpContextMock = new Mock<HttpContextBase>();
         httpContextMock.Setup(c => c.Request.AppRelativeCurrentExecutionFilePath).Returns("~/yes");

         var routeData = routes.GetRouteData(httpContextMock.Object);

         var controllerInstance = new FromRouteAttr.SpecifiedBinderPropertyController {
            ValidateRequest = false
         };

         var requestContext = new RequestContext(httpContextMock.Object, routeData);
         var controllerContext = new ControllerContext(requestContext, controllerInstance);

         controllerInstance.ValueProvider = new ValueProviderCollection(new IValueProvider[] { new RouteDataValueProvider(controllerContext) });

         ((IController)controllerInstance).Execute(requestContext);

         Assert.IsTrue(controllerInstance.a);
      }
   }
}

namespace MvcCodeRouting.Tests.Routing.FromRouteAttr {
   using FromRouteAttribute = MvcCodeRouting.Web.Mvc.FromRouteAttribute;

   public class CustomNameController : Controller {

      public string Foo([FromRoute("b")]string a) {
         return a;
      }
   }

   public class BindCustomNameController : Controller {

      public string Foo([FromRoute("b")]string a) {
         return a;
      }
   }

   public class BindCustomNamePropertyController : Controller {

      [FromRoute("b")]
      public bool a { get; set; }

      protected override void Initialize(RequestContext requestContext) {
         base.Initialize(requestContext);
         MvcCodeRouting.Web.Mvc.MvcExtensions.BindRouteProperties(this);
      }

      public void Index() { }
   }

   public class SpecifiedBinderController : Controller {

      public bool Foo([FromRoute(Constraint = "yes|no", BinderType = typeof(YesNoModelBinder))]bool a) {
         return a;
      }
   }

   public class SpecifiedBinderPropertyController : Controller {

      [FromRoute(Constraint = "yes|no", BinderType = typeof(YesNoModelBinder))]
      public bool a { get; set; }

      protected override void Initialize(RequestContext requestContext) {
         base.Initialize(requestContext);
         MvcCodeRouting.Web.Mvc.MvcExtensions.BindRouteProperties(this);
      }
      
      public void Index() { }
   }

   class YesNoModelBinder : IModelBinder {

      public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {

         switch ((string)bindingContext.ValueProvider.GetValue(bindingContext.ModelName).RawValue) {
            case "yes":
               return true;
            case "no":
               return false;
            
            default:
               throw new InvalidOperationException();
         }
      }
   }
}