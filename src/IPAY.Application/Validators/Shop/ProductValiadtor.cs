using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Validators.Shop
{
    using FluentValidation;
    using IPAY.Application.DTOs.Shop;

    public static class ProductValiadtor
    {
        public static bool BeFinite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);

        public static bool HaveMax2Decimals(double v) =>
            Math.Abs(v * 100 - Math.Round(v * 100)) < 1e-6;

        public static bool BeValidHttpUrl(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var u) &&
            (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
    }
}
