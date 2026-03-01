using InTagViewModelLayer.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace InTagLogicLayer.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseVm> LoginAsync(LoginRequestVm request);
        Task<AuthResponseVm> RefreshTokenAsync(RefreshTokenRequestVm request);
        Task<AuthResponseVm> RegisterAsync(RegisterRequestVm request);
        Task RevokeRefreshTokenAsync(string token);
    }
}
