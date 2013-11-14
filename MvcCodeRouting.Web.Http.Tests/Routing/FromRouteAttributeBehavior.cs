﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvcCodeRouting.Web.Http.Tests.Routing {
   
   [TestClass]
   public class FromRouteAttributeBehavior {

      readonly HttpConfiguration config;
      readonly HttpRouteCollection routes;

      public FromRouteAttributeBehavior() {

         config = new HttpConfiguration();
         routes = config.Routes;
      }

      [TestMethod]
      public void UseCustomName() {

         var controller = typeof(FromRouteAttr.FromRouteAttributeController);

         config.MapCodeRoutes(controller, new CodeRoutingSettings { RootOnly = true });

         Assert.IsNotNull(routes.First().RouteTemplate.Contains("{b}"));
      }

      [TestMethod]
      public void BindCustomName() {

         var controller = typeof(FromRouteAttr.FromRouteAttributeController);

         config.MapCodeRoutes(controller, new CodeRoutingSettings { RootOnly = true });
         
         var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/hello");
         var routeData = routes.GetRouteData(request);

         var controllerInstance = new FromRouteAttr.FromRouteAttributeController();

         var controllerContext = new HttpControllerContext(config, routeData, request) {
            ControllerDescriptor = new HttpControllerDescriptor(config, (string)routeData.Values["controller"], controller),
            Controller = controllerInstance
         };

         var result = controllerInstance.ExecuteAsync(controllerContext, CancellationToken.None).Result;
         string value;

         Assert.IsTrue(result.TryGetContentValue(out value) && value == "hello");
      }

      [TestMethod, ExpectedException(typeof(InvalidOperationException))]
      public void FailIfUsingWrongAttribute() {

         var controller = typeof(FromRouteAttr.FromRouteAttribute2Controller);

         config.MapCodeRoutes(controller, new CodeRoutingSettings { RootOnly = true });
      }

      [TestMethod, ExpectedException(typeof(InvalidOperationException))]
      public void FailIfUsingWrongAttribute2() {

         var controller = typeof(FromRouteAttr.FromRouteAttribute3Controller);

         config.MapCodeRoutes(controller, new CodeRoutingSettings { RootOnly = true });
      }

      [TestMethod, ExpectedException(typeof(InvalidOperationException))]
      public void FailIfUsingWrongAttribute3() {

         var controller = typeof(FromRouteAttr.FromRouteAttribute4Controller);

         config.MapCodeRoutes(controller, new CodeRoutingSettings { RootOnly = true });
      }

      [TestMethod, ExpectedException(typeof(InvalidOperationException))]
      public void FailIfUsingWrongAttribute4() {

         var controller = typeof(FromRouteAttr.FromRouteAttribute5Controller);

         config.MapCodeRoutes(controller, new CodeRoutingSettings { RootOnly = true });
      }
   }
}

namespace MvcCodeRouting.Web.Http.Tests.Routing.FromRouteAttr {

   public class FromRouteAttributeController : ApiController {

      public string Get([FromRoute("b")]string a) {
         return a;
      }
   }

#pragma warning disable 0618

   public class FromRouteAttribute2Controller : ApiController {

      public void Get([MvcCodeRouting.FromRoute]string foo) { }
   }

   public class FromRouteAttribute3Controller : ApiController {

      public void Get([MvcCodeRouting.Web.Mvc.FromRoute]string foo) { }
   }

   public class FromRouteAttribute4Controller : ApiController {

      [MvcCodeRouting.FromRoute]
      public string Bar { get; set; }

      public void Get() { }
   }

   public class FromRouteAttribute5Controller : ApiController {

      [MvcCodeRouting.Web.Mvc.FromRoute]
      public string Bar { get; set; }

      public void Get() { }
   }

#pragma warning restore 0618
}