using API.Attributes;
using Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Shared.GeneralDTO;
using System;
using System.Linq;
using PostEntity = DataAccess.Data.Post;

namespace API.Controllers.Post
{
    [Route("api/[controller]")]
    [ApiController]
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
        private readonly BaseService<PostEntity> _postService;

        public PostController(BaseService<PostEntity> postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Obtiene todos los posts
        /// </summary>
        /// <returns>Lista de todos los posts</returns>
        [HttpGet]
        public ActionResult<RespuestaDto> GetAll()
        {
            try
            {
                var posts = _postService.GetAll().ToList();
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

        /// <summary>
        /// Obtiene un post por ID
        /// </summary>
        /// <param name="id">ID del post</param>
        /// <returns>Post encontrado</returns>
        [HttpGet("{id}")]
        public ActionResult<RespuestaDto> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0"));
                }

                var post = _postService.GetAll().FirstOrDefault(p => p.PostId == id);

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

        /// <summary>
        /// Crea un nuevo post
        /// </summary>
        /// <param name="entity">Datos del post a crear</param>
        /// <returns>Post creado</returns>
        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public ActionResult<RespuestaDto> Create([FromBody] PostEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Datos requeridos",
                        "Debe proporcionar los datos del post"));
                }

                var resultado = _postService.Create(entity);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = resultado.PostId },
                    RespuestaDto.Exitoso(
                        "Post creado",
                        "Post creado exitosamente",
                        resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al crear post: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza un post existente
        /// </summary>
        /// <param name="id">ID del post a actualizar</param>
        /// <param name="entity">Datos del post actualizados</param>
        /// <returns>Post actualizado</returns>
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        public ActionResult<RespuestaDto> Update(int id, [FromBody] PostEntity entity)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0"));
                }

                if (entity == null)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Datos requeridos",
                        "Debe proporcionar los datos del post"));
                }

                if (entity.PostId != id)
                {
                    entity.PostId = id;
                }

                var resultado = _postService.Update(entity.PostId, entity, out bool changed);

                if (!changed)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Sin cambios",
                        "No se detectaron cambios en el post"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "Post actualizado",
                    "Post actualizado exitosamente",
                    resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al actualizar post: {ex.Message}"));
            }
        }

        /// <summary>
        /// Elimina un post
        /// </summary>
        /// <param name="id">ID del post a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        public ActionResult<RespuestaDto> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID inválido",
                        "El ID debe ser un número mayor a 0"));
                }

                var post = _postService.GetAll().FirstOrDefault(p => p.PostId == id);

                if (post == null)
                {
                    return NotFound(RespuestaDto.ParametrosIncorrectos(
                        "Post no encontrado",
                        $"No se encontró el post con ID {id}"));
                }

                var resultado = _postService.Delete(post);

                return Ok(RespuestaDto.Exitoso(
                    "Post eliminado",
                    "Post eliminado exitosamente",
                    resultado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, RespuestaDto.ErrorInterno(
                    $"Error al eliminar post: {ex.Message}"));
            }
        }

        /// <summary>
        /// Busca posts por criterio
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <returns>Lista de posts que coinciden con el criterio</returns>
        [HttpGet("buscar")]
        public ActionResult<RespuestaDto> Buscar([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Término requerido",
                        "Debe proporcionar un término de búsqueda"));
                }

                var posts = _postService.GetAll()
                    .Where(p => p.Title.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                               p.Body.Contains(termino, StringComparison.OrdinalIgnoreCase))
                    .ToList();

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

        /// <summary>
        /// Obtiene posts por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de posts del usuario</returns>
        [HttpGet("usuario/{userId}")]
        public ActionResult<RespuestaDto> GetByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "ID de usuario inválido",
                        "El ID de usuario debe ser un número mayor a 0"));
                }

                var posts = _postService.GetAll()
                    .Where(p => p.CustomerId == userId)
                    .ToList();

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
    }
}