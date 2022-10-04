﻿namespace Demo.Shared.Auth
{
    public class ConditionalAuthenticationMessage : IAuthenticationFormMessage
    {
        public bool RequireAuthentication { get; set; }
        public string RequiredRole { get; set; }
    }
}
