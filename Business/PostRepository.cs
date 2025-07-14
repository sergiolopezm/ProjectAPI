using Business.Contracts;
using DataAccess;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Shared.GeneralDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business
{
    public class PostRepository : IPostRepository
    {
        private readonly BaseModel<Post> _baseModel;
        private readonly JujuTestContext _context;

        public PostRepository(BaseModel<Post> baseModel, JujuTestContext context)
        {
            _baseModel = baseModel;
            _context = context;
        }

        public async Task<List<Post>> ObtenerTodosAsync()
        {
            return await _baseModel.GetAll.ToListAsync();
        }

        public async Task<Post?> ObtenerPorIdAsync(int id)
        {
            return await _context.Post.FindAsync(id);
        }

        public async Task<RespuestaDto> CrearAsync(Post post)
        {
            try
            {
                // VALIDACIÓN 1: Verificar que el usuario asociado (Customer) exista
                if (post.CustomerId <= 0)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "CustomerId inválido",
                        "Debe proporcionar un CustomerId válido mayor a 0");
                }

                var customerExiste = await _context.Customer.AnyAsync(c => c.CustomerId == post.CustomerId);
                if (!customerExiste)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Usuario no encontrado",
                        $"No existe un customer con ID {post.CustomerId}");
                }
               
                post = AplicarReglasDeNegocio(post);

                var resultado = _baseModel.Create(post);

                return RespuestaDto.Exitoso(
                    "Post creado",
                    $"Post '{resultado.Title}' creado exitosamente",
                    resultado);
            }
            catch (Exception ex)
            {
                return RespuestaDto.ErrorInterno($"Error al crear post: {ex.Message}");
            }
        }

        public async Task<RespuestaDto> ActualizarAsync(int id, Post post)
        {
            try
            {
                if (id <= 0)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0");
                }

                var existePost = await _context.Post.AnyAsync(p => p.PostId == id);
                if (!existePost)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Post no encontrado",
                        $"No se encontró el post con ID {id}");
                }

                post.PostId = id;
                var originalPost = _baseModel.FindById(id);
                var resultado = _baseModel.Update(post, originalPost, out bool changed);

                if (!changed)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Sin cambios",
                        "No se detectaron cambios en el post");
                }

                return RespuestaDto.Exitoso(
                    "Post actualizado",
                    "Post actualizado exitosamente",
                    resultado);
            }
            catch (Exception ex)
            {
                return RespuestaDto.ErrorInterno($"Error al actualizar post: {ex.Message}");
            }
        }

        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0");
                }

                var post = await _context.Post.FindAsync(id);
                if (post == null)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Post no encontrado",
                        $"No se encontró el post con ID {id}");
                }

                var resultado = _baseModel.Delete(post);

                return RespuestaDto.Exitoso(
                    "Post eliminado",
                    "Post eliminado exitosamente",
                    resultado);
            }
            catch (Exception ex)
            {
                return RespuestaDto.ErrorInterno($"Error al eliminar post: {ex.Message}");
            }
        }

        public async Task<List<Post>> BuscarAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return new List<Post>();

            return await _context.Post
                .Where(p => p.Title.Contains(termino) || p.Body.Contains(termino))
                .ToListAsync();
        }

        public async Task<List<Post>> ObtenerPorUsuarioAsync(int userId)
        {
            return await _context.Post
                .Where(p => p.CustomerId == userId)
                .ToListAsync();
        }

        public async Task<RespuestaDto> CrearMultiplesAsync(List<Post> posts)
        {
            try
            {
                if (posts == null || !posts.Any())
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Datos requeridos",
                        "Debe proporcionar al menos un post para crear");
                }

                var postsProcesados = new List<Post>();
                var errores = new List<string>();

                foreach (var post in posts)
                {
                    // Validar cada post individualmente
                    var validacionResult = await ValidarPostParaCreacion(post);
                    if (!validacionResult.Exito)
                    {
                        errores.Add($"Post '{post.Title}': {validacionResult.Detalle}");
                        continue;
                    }

                    // Aplicar las mismas reglas de negocio que en CrearAsync
                    var postProcesado = AplicarReglasDeNegocio(post);
                    postsProcesados.Add(postProcesado);
                }

                if (errores.Any())
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Errores en validación",
                        string.Join("; ", errores));
                }

                // Crear todos los posts válidos
                var resultados = new List<Post>();
                foreach (var post in postsProcesados)
                {
                    var resultado = _baseModel.Create(post);
                    resultados.Add(resultado);
                }

                return RespuestaDto.Exitoso(
                    "Posts creados",
                    $"Se crearon {resultados.Count} posts exitosamente",
                    resultados);
            }
            catch (Exception ex)
            {
                return RespuestaDto.ErrorInterno($"Error al crear posts múltiples: {ex.Message}");
            }
        }

        private async Task<RespuestaDto> ValidarPostParaCreacion(Post post)
        {
            // Validar CustomerId
            if (post.CustomerId <= 0)
            {
                return RespuestaDto.ParametrosIncorrectos(
                    "CustomerId inválido",
                    "Debe proporcionar un CustomerId válido mayor a 0");
            }

            // Verificar que existe el Customer
            var customerExiste = await _context.Customer.AnyAsync(c => c.CustomerId == post.CustomerId);
            if (!customerExiste)
            {
                return RespuestaDto.ParametrosIncorrectos(
                    "Usuario no encontrado",
                    $"No existe un customer con ID {post.CustomerId}");
            }

            return RespuestaDto.Exitoso("Validación exitosa", "Post válido para creación", null);
        }

        private Post AplicarReglasDeNegocio(Post post)
        {
            // Regla para Body
            if (!string.IsNullOrEmpty(post.Body) && post.Body.Length > 20)
            {
                if (post.Body.Length > 97)
                {
                    post.Body = post.Body.Substring(0, 97) + "...";
                }
            }

            // Regla para Category basado en Type
            switch (post.Type)
            {
                case 1:
                    post.Category = "Farándula";
                    break;
                case 2:
                    post.Category = "Política";
                    break;
                case 3:
                    post.Category = "Futbol";
                    break;
                    // Si Type no es 1, 2 o 3, mantener la Category que el usuario ingresó
            }

            return post;
        }
    }
}
