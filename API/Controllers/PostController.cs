using API.Attributes;
using Business;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Shared.GeneralDTO;
using System;
using System.Linq;
using PostEntity = DataAccess.Data.Post;

namespace API.Controllers.Post
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly BaseService<PostEntity> _postService;

        public PostController(BaseService<PostEntity> postService)
        {
            _postService = postService;
        }

        [HttpGet]
        [ServiceFilter(typeof(LogAttribute))]
        public ActionResult<RespuestaDto> GetAll()
        {
            var posts = _postService.GetAll().ToList();
            return Ok(RespuestaDto.Exitoso("Posts obtenidos", $"Se obtuvieron {posts.Count} posts", posts));
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        [ServiceFilter(typeof(LogAttribute))]
        public ActionResult<RespuestaDto> Create([FromBody] PostEntity entity)
        {
            try
            {
                var resultado = _postService.Create(entity);
                return Ok(RespuestaDto.Exitoso("Post creado", "Post creado exitosamente", resultado));
            }
            catch (Exception ex)
            {
                return BadRequest(RespuestaDto.ErrorInterno($"Error al crear post: {ex.Message}"));
            }
        }

        [HttpPut]
        [ServiceFilter(typeof(ValidarModeloAttribute))]
        [ServiceFilter(typeof(LogAttribute))]
        public ActionResult<RespuestaDto> Update([FromBody] PostEntity entity)
        {
            try
            {
                var resultado = _postService.Update(entity.PostId, entity, out bool changed);
                if (!changed)
                    return BadRequest(RespuestaDto.ParametrosIncorrectos("Sin cambios", "No se detectaron cambios"));

                return Ok(RespuestaDto.Exitoso("Post actualizado", "Post actualizado exitosamente", resultado));
            }
            catch (Exception ex)
            {
                return BadRequest(RespuestaDto.ErrorInterno($"Error al actualizar post: {ex.Message}"));
            }
        }

        [HttpDelete]
        [ServiceFilter(typeof(LogAttribute))]
        public ActionResult<RespuestaDto> Delete([FromBody] PostEntity entity)
        {
            try
            {
                var resultado = _postService.Delete(entity);
                return Ok(RespuestaDto.Exitoso("Post eliminado", "Post eliminado exitosamente", resultado));
            }
            catch (Exception ex)
            {
                return BadRequest(RespuestaDto.ErrorInterno($"Error al eliminar post: {ex.Message}"));
            }
        }
    }
}
