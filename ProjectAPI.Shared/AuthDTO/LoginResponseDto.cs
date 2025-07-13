namespace ProjectAPI.Shared.AuthDTO
{
    public class LoginResponseDto
    {
        public UsuarioDto Usuario { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
