using FluentValidation;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Validators.Auth;
using IPAY.WebApi.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Filters
{
    [TestFixture]
    public class ValidationFilterTests
    {
        private ValidationFilter _sut = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddScoped<IValidator<ChangePhoneDto>, ChangePhoneValidator>();
            _sut = new ValidationFilter(services.BuildServiceProvider());
        }

        private static ActionExecutingContext ContextWith(params object?[] arguments)
        {
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var args = new Dictionary<string, object?>();
            for (var i = 0; i < arguments.Length; i++)
                args["arg" + i] = arguments[i];

            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), args, controller: new object());
        }

        private static ActionExecutionDelegate Next(Action onCalled) => () =>
        {
            onCalled();
            var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new object()));
        };

        [Test]
        public async Task ValidArgument_ContinuesToTheAction()
        {
            var context = ContextWith(new ChangePhoneDto { NewPhone = "+380123456789" });
            var called = false;

            await _sut.OnActionExecutionAsync(context, Next(() => called = true));

            Assert.That(called, Is.True);
            Assert.That(context.Result, Is.Null);
        }

        [Test]
        public async Task InvalidArgument_ShortCircuitsWithA400AndTheErrors()
        {
            var context = ContextWith(new ChangePhoneDto { NewPhone = "abc" });
            var called = false;

            await _sut.OnActionExecutionAsync(context, Next(() => called = true));

            Assert.That(called, Is.False);
            var bad = (BadRequestObjectResult)context.Result!;
            var problem = (ValidationProblemDetails)bad.Value!;
            Assert.That(problem.Status, Is.EqualTo(400));
            Assert.That(problem.Errors, Does.ContainKey("NewPhone"));
        }

        [Test]
        public async Task ArgumentsWithoutARegisteredValidator_AreSkipped()
        {
            var context = ContextWith("a plain string", 42);
            var called = false;

            await _sut.OnActionExecutionAsync(context, Next(() => called = true));

            Assert.That(called, Is.True);
            Assert.That(context.Result, Is.Null);
        }

        [Test]
        public async Task NullArguments_AreSkipped()
        {
            var context = ContextWith(new object?[] { null });
            var called = false;

            await _sut.OnActionExecutionAsync(context, Next(() => called = true));

            Assert.That(called, Is.True);
        }

        [Test]
        public async Task OneInvalidArgumentAmongSeveral_StillStopsTheRequest()
        {
            var context = ContextWith("ok", new ChangePhoneDto { NewPhone = "" });
            var called = false;

            await _sut.OnActionExecutionAsync(context, Next(() => called = true));

            Assert.That(called, Is.False);
            Assert.That(context.Result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}
