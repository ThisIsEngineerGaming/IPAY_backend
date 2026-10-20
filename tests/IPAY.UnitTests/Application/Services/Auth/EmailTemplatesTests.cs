using IPAY.Application.Services.Auth;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class EmailTemplatesTests
    {
        [Test]
        public void SignInCode_ContainsTheCodeAndTheLifetime()
        {
            var html = EmailTemplates.SignInCode("123456", 10);

            Assert.That(html, Does.Contain("123456"));
            Assert.That(html, Does.Contain("expires in 10 minutes"));
            Assert.That(html, Does.Contain("IPAY"));
        }

        [Test]
        public void SignInCode_HtmlEncodesTheCode()
        {
            var html = EmailTemplates.SignInCode("<script>", 5);

            Assert.That(html, Does.Not.Contain("<script>"));
            Assert.That(html, Does.Contain("&lt;script&gt;"));
        }

        [Test]
        public void EmailNotVerifiedException_HasAFriendlyMessage()
        {
            var ex = new EmailNotVerifiedException();

            Assert.That(ex.Message, Does.Contain("verify your email"));
        }
    }
}
