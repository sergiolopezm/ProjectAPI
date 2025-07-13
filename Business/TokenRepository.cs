using Business.Contracts;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectAPI.Util;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Business
{
    public class TokenRepository : ITokenRepository
    {
        private readonly JujuTestContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _jwtKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _tiempoExpiracionMinutos;

        public TokenRepository(JujuTestContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _jwtKey = _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            _issuer = _configuration["JwtSettings:Issuer"] ?? "ProjectAPI";
            _audience = _configuration["JwtSettings:Audience"] ?? "ProjectAPI";
            _tiempoExpiracionMinutos = int.Parse(_configuration["JwtSettings:TiempoExpiracionMinutos"] ?? "30");
        }

        public async Task<string> GenerarTokenAsync(Usuarios usuario, string ip)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.RolId.ToString())
            };

            var expiration = DateTime.UtcNow.AddMinutes(_tiempoExpiracionMinutos);
            var token = JwtHelper.GenerateJwtToken(claims, _jwtKey, _issuer, _audience, expiration);

            var tokenEntity = new Tokens
            {
                Id = Guid.NewGuid(),
                Token = token,
                UsuarioId = usuario.Id,
                Ip = ip,
                FechaCreacion = DateTime.Now,
                FechaExpiracion = expiration
            };

            _context.Tokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            return await _context.Tokens.AnyAsync(t => t.Token == token && t.FechaExpiracion > DateTime.Now);
        }

        public async Task ExpiraTokenAsync(string token)
        {
            var tokenEntity = await _context.Tokens.FirstOrDefaultAsync(t => t.Token == token);
            if (tokenEntity != null)
            {
                _context.Tokens.Remove(tokenEntity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
