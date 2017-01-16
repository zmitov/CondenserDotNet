﻿using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Server;
using CondenserDotNet.Service;
using CondenserDotNet.Service.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class RoutingFacts
    {
        [Fact]
        public async Task CanWeFindARouteAndGetAPage()
        {
            var registry = new FakeServiceRegistry();
            var informationService = new InformationService
            {
                Address = "www.google.com",
                Port = 80
            };
            registry.SetServiceInstance(informationService);

            var router = new CustomRouter();
            var service = new Service(new[] {"/search"}, "Service1",
                "node1", new string[0], registry);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";

            var routeContext = new RouteContext(context);

            await router.RouteAsync(routeContext);

            await routeContext.Handler.Invoke(routeContext.HttpContext);

            Assert.Equal(200, routeContext.HttpContext.Response.StatusCode);
        }

        [Fact]
        public async Task CanRegisterRoutes()
        {
            var wait = new AsyncManualResetEvent<bool>();

            var agentAddress = "localhost";
            var config = new CondenserConfiguration
            {
                AgentAddress = agentAddress,
                AgentPort = 8500
            };
            var router = new CustomRouter();
            var host = new RoutingHost(router, config)
            {
                OnRouteBuilt = servers => {
                {
                    if (servers.ContainsKey("Google"))
                    {
                        wait.Set(true);
                    }
                } }
            };

            var google = new ServiceManager("Google",
                agentAddress, 8500)
            {
                ServiceAddress = "www.google.com",
                ServicePort = 80
            };

            await google.AddApiUrl("/search")
                .RegisterServiceAsync();

            await wait.WaitAsync();

            var routeContext = await RouteRequest(router, "/search");

            Assert.Equal(200, routeContext.HttpContext.Response.StatusCode);
/*
            var gmail = new ServiceManager("Gmail",
                agentAddress, 8500)
            {
                ServiceAddress = "www.google.com",
                ServicePort = 80
            };

            await gmail.AddApiUrl("/gmail")
                .RegisterServiceAsync();

            await wait.WaitAsync();

            routeContext = await RouteRequest(router, "/gmail");

            Assert.Equal(200, routeContext.HttpContext.Response.StatusCode);
            */
        }

        private static async Task<RouteContext> RouteRequest(CustomRouter router, 
            string requestPath)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = requestPath;

            var routeContext = new RouteContext(context);

            await router.RouteAsync(routeContext);
            await routeContext.Handler.Invoke(routeContext.HttpContext);
            return routeContext;
        }
    }
}