﻿using Microsoft.AspNetCore.Identity;

namespace EventManagementSystem.Application.DTOs.Auth;

public record AuthResult(
    bool Success,
    string Token = "",
    string Message = "",
    IEnumerable<IdentityError>? Errors = null);