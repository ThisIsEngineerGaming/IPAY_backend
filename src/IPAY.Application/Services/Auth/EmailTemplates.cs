using System.Net;

namespace IPAY.Application.Services.Auth
{
    /// <summary>
    /// HTML for the emails we send. Email clients ignore stylesheets, so everything is inline and
    /// laid out with tables. Colors match the site (navy #232F3E, yellow #FFD814).
    /// </summary>
    public static class EmailTemplates
    {
        public static string SignInCode(string code, int minutesValid) =>
            Layout(
                "Your sign-in code",
                "<p style=\"margin:0 0 16px 0\">Use this code to finish signing in:</p>" +
                "<p style=\"margin:0 0 20px 0;padding:14px 0;background:#f3f4f6;border-radius:8px;text-align:center;" +
                $"font-size:30px;font-weight:bold;letter-spacing:8px;color:#111\">{Encode(code)}</p>" +
                $"<p style=\"margin:0\">It expires in {minutesValid} minutes. Never share this code with anyone.</p>",
                "If you didn't try to sign in, you can ignore this email - but consider changing your password.");

        private static string Layout(string title, string bodyHtml, string footer) =>
            "<div style=\"background:#f3f4f6;padding:24px 12px;font-family:Arial,Helvetica,sans-serif\">" +
            "<table align=\"center\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" " +
            "style=\"max-width:480px;background:#ffffff;border-radius:12px;padding:32px\">" +
            "<tr><td style=\"font-size:24px;font-weight:bold;color:#232F3E;padding-bottom:16px\">IPAY</td></tr>" +
            $"<tr><td style=\"font-size:18px;font-weight:bold;color:#111;padding-bottom:10px\">{Encode(title)}</td></tr>" +
            $"<tr><td style=\"font-size:14px;color:#444;line-height:1.5\">{bodyHtml}</td></tr>" +
            $"<tr><td style=\"font-size:12px;color:#999;padding-top:24px;line-height:1.5\">{Encode(footer)}</td></tr>" +
            "</table></div>";

        private static string Encode(string value) => WebUtility.HtmlEncode(value);
    }
}
