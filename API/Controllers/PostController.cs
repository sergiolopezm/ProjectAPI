using API.Attributes;
using Business.Contracts;
using DataAccess.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Shared.GeneralDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers.Post
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    [ServiceFilter(typeof(AccesoAttribute))]
    [ServiceFilter(typeof(LogAttribute))]
    [ServiceFilter(typeof(ExceptionAttribute))]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status500InternalServerError)]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpGet]
        public async Task<ActionResult<RespuestaDto>> GetAll()
        {
            try
            {
                var posts = await _postRepository.ObtenerTodosAsync();
                return Ok(RespuestaDto.Exitoso(
                    "Posts obtenidos",
                    $"Se obtuvieron {posts.Count} posts",
                    posts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al obtener posts: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RespuestaDto>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0"));
                }

                var post = await _postRepository.ObtenerPorIdAsync(id);

                if (post == null)
                {
                    return NotFound(RespuestaDto.ParametrosIncorrectos(
                        "Post no encontrado",
                        $"No se encontró el post con ID {id}"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "Post encontrado",
                    "Post obtenido exitosamente",
                    post));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al obtener post: {ex.Message}"));
            }
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<ActionResult<RespuestaDto>> Create([FromBody] DataAccess.Data.Post entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Datos requeridos",
                        "Debe proporcionar los datos del post"));
                }

                var resultado = await _postRepository.CrearAsync(entity);

                if (resultado.Exito)
                {
                    var postCreado = (DataAccess.Data.Post)resultado.Resultado!;
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = postCreado.PostId },
                        resultado);
                }

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al crear post: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<ActionResult<RespuestaDto>> Update(int id, [FromBody] DataAccess.Data.Post entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Datos requeridos",
                        "Debe proporcionar los datos del post"));
                }

                var resultado = await _postRepository.ActualizarAsync(id, entity);

                if (resultado.Exito)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al actualizar post: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<RespuestaDto>> Delete(int id)
        {
            try
            {
                var resultado = await _postRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al eliminar post: {ex.Message}"));
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<RespuestaDto>> Buscar([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Término requerido",
                        "Debe proporcionar un término de búsqueda"));
                }

                var posts = await _postRepository.BuscarAsync(termino);

                return Ok(RespuestaDto.Exitoso(
                    "Búsqueda completada",
                    $"Se encontraron {posts.Count} posts",
                    posts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error en búsqueda: {ex.Message}"));
            }
        }

        [HttpGet("usuario/{userId}")]
        public async Task<ActionResult<RespuestaDto>> GetByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID de usuario inválido",
                        "El ID de usuario debe ser un número mayor a 0"));
                }

                var posts = await _postRepository.ObtenerPorUsuarioAsync(userId);

                return Ok(RespuestaDto.Exitoso(
                    "Posts del usuario obtenidos",
                    $"Se encontraron {posts.Count} posts para el usuario {userId}",
                    posts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al obtener posts del usuario: {ex.Message}"));
            }
        }

        [HttpPost("crear-multiples")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public async Task<ActionResult<RespuestaDto>> CreateMultiple([FromBody] List<DataAccess.Data.Post> entities)
        {
            try
            {
                if (entities == null || entities.Count == 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Datos requeridos",
                        "Debe proporcionar al menos un post para crear"));
                }

                var resultado = await _postRepository.CrearMultiplesAsync(entities);

                if (resultado.Exito)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al crear posts múltiples: {ex.Message}"));
            }
        }
    }
}