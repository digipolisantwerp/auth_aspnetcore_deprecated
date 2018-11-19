using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;

namespace Digipolis.Auth.Utilities
{
    public static class HttpContextHelper
    {
        public static AuthenticationTicket GetCookieAuthenticationTicket(this HttpContext context, CookieAuthenticationOptions options)
        {
            try
            {
                //Get the encrypted cookie value
                string cookieValue = context.Request.Cookies[".AspNetCore.CookieAuth"];

                //Get a data protector to use with either approach
                var dataProtector = options.DataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", "CookieAuth", "v2");
                
                //Get the decrypted cookie as plain text
                UTF8Encoding specialUtf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                byte[] protectedBytes = Base64UrlTextEncoder.Decode(cookieValue);
                byte[] plainBytes = dataProtector.Unprotect(protectedBytes);
                string plainText = specialUtf8Encoding.GetString(plainBytes);

                //Get the decrypted cookie as a Authentication Ticket
                TicketDataFormat ticketDataFormat = new TicketDataFormat(dataProtector);
                AuthenticationTicket ticket = ticketDataFormat.Unprotect(cookieValue);

                return ticket;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
