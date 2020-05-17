using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Interacts With Books Table 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public BooksController(IBookRepository bookRepository, ILoggerService loggerService, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        /// <summary>
        /// Get all Books
        /// </summary>
        /// <returns>A List Of Books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerNames();
            try
            {
                _loggerService.LogInfo($"{location}: Atenpted Call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<List<BookDTO>>(books);
                _loggerService.LogInfo($"{location}: Successful");
                return Ok(response);
            }
            catch (Exception e)
            {

                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// Get a Book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerNames();
            try
            {
                _loggerService.LogInfo($"{location}: Atenpted Call For Id:{id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _loggerService.LogWarn($"{location}: Faild To  Retrieve Record with Id:{id}");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _loggerService.LogInfo($"{location}: Successfully Got Record With id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {

                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// Creates a new Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns>Book Object</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerNames();
            try
            {
                _loggerService.LogInfo($"{location}: Create Attempted");
                if (bookDTO == null)
                {
                    _loggerService.LogWarn($"{location}: Empty Request Was Submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Data was Incomplete");
                    return BadRequest(ModelState);
                }
                Book book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Create Failed");
                }
                _loggerService.LogInfo($"{location}: Creation Was Successful");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// Udate a Book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerNames();
            try
            {
                _loggerService.LogInfo($"{location}: Update Attempted on Record with Id: {id}");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _loggerService.LogWarn($"{location}:Update failed with bad data - id: {id}");
                    return BadRequest();
                }
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                {
                    _loggerService.LogInfo($"{location}: Failed to retrieve record with Id: {id}");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.UpDate(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Update Failed for record with id:{id}");
                }
                _loggerService.LogInfo($"{location}: Record with id:{id} successfully updated");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// Delete a Book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerNames();
            try
            {
                _loggerService.LogInfo($"{location}: Delete Attempted on Record with Id: {id}");
                if (id < 1)
                {
                    _loggerService.LogWarn($"{location}:Delete failed with bad data - id: {id}");
                    return BadRequest();
                }
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                {
                    _loggerService.LogInfo($"{location}: Failed to retrieve record with Id: {id}");
                    return NotFound();
                }
                var author = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Delete Failed for record with id:{id}");
                }
                _loggerService.LogInfo($"{location}: Record with id:{id} successfully Deleted");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        private string GetControllerNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the administrator");
        }
    }
}