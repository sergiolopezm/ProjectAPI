using Business.Contracts;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Shared.AuthDTO;
using ProjectAPI.Shared.GeneralDTO;
using ProjectAPI.Util;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Business
{
    public class AuthRepository : IAuthRepository
    {
        private readonly JujuTestContext _context;
        private readonly ITokenRepository _tokenRepository;

        public AuthRepository(JujuTestContext context, ITokenRepository tokenRepository)
        {
            _context = context;
            _tokenRepository = tokenRepository;
        }

        public async Task<RespuestaDto> AutenticarAsync(LoginDto loginDto)
        {
            // Validar acceso de sitio
            if (!await ValidarAccesoAsync(loginDto.Sitio, loginDto.Clave))
                return RespuestaDto.ParametrosIncorrectos("Acceso denegado", "Sitio o clave incorrectos");

            // Obtener usuario
            var contraseñaHash = GetSHA256Hash(loginDto.Contraseña);
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == loginDto.NombreUsuario &&
                                        u.Contraseña == contraseñaHash &&
                                        u.Activo);

            if (usuario == null)
                return RespuestaDto.ParametrosIncorrectos("Credenciales incorrectas", "Usuario o contraseña incorrectos");

            // Actualizar último acceso
            usuario.FechaUltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            // Generar token
            var token = await _tokenRepository.GenerarTokenAsync(usuario, loginDto.Ip ?? "");

            var usuarioDto = Mapping.Convertir<Usuarios, UsuarioDto>(usuario);
            usuarioDto.Rol = usuario.Rol.Nombre;

            return RespuestaDto.Exitoso("Login exitoso", "Usuario autenticado correctamente",
                new LoginResponseDto { Usuario = usuarioDto, Token = token });
        }

        public async Task<RespuestaDto> RegistrarAsync(RegistroDto registroDto)
        {
            // Validar usuario único
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == registroDto.NombreUsuario))
                return RespuestaDto.ParametrosIncorrectos("Usuario existente", "El nombre de usuario ya existe");

            if (await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email))
                return RespuestaDto.ParametrosIncorrectos("Email existente", "El email ya está registrado");

            var usuario = new Usuarios
            {
                Id = Guid.NewGuid(),
                NombreUsuario = registroDto.NombreUsuario,
                Contraseña = GetSHA256Hash(registroDto.Contraseña),
                Nombre = registroDto.Nombre,
                Apellido = registroDto.Apellido,
                Email = registroDto.Email,
                RolId = registroDto.RolId,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return RespuestaDto.Exitoso("Usuario registrado", "Usuario creado exitosamente");
        }

        public async Task<bool> ValidarAccesoAsync(string sitio, string clave)
        {
            return await _context.Accesos.AnyAsync(a => a.Sitio == sitio && a.Contraseña == clave && a.Activo);
        }

        public async Task<UsuarioDto?> ObtenerUsuarioPorIdAsync(Guid id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return null;

            var usuarioDto = Mapping.Convertir<Usuarios, UsuarioDto>(usuario);
            usuarioDto.Rol = usuario.Rol.Nombre;
            return usuarioDto;
        }

        private static string GetSHA256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
