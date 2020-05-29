using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        /// <summary>
        /// Interacts with the books table
        /// </summary>
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository, ILoggerService logger, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }


        /// <summary>
        /// Gets list of books
        /// </summary>
        /// <returns>list of books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location } Attempting call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location} Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Gets book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>book</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location } Attempting call");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"{location}: Failed to retreive record with id: {id}");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo($"{location} Successfully got with id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new book
        /// </summary>
        /// <param name="bookCDTO"></param>
        /// <returns>Book</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookCDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location} Create Attempted");
                if (bookCDTO == null)
                {
                    _logger.LogWarn($"{location} : Empty request submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location} : Data was incomplete");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookCDTO);
                var isSucess = await _bookRepository.Create(book);
                if (!isSucess)
                {
                    return InternalError($"{location} : Creation Failed");
                }
                _logger.LogInfo($"{location} : Creation successfull : {book}");
                return Created("Created", new { book });
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Gets book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>book</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDto)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location} Update Attempted with id: {id}");
                if (id < 1 || bookDto == null || id != bookDto.Id)
                {
                    _logger.LogWarn($"{location} : Update failed with bad date - id : {id} ");
                    return NotFound();
                }
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location} : Failed to find data with - id : {id} ");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDto);
                var isSucesss = await _bookRepository.Update(book);
                if (!isSucesss)
                {
                    return InternalError($"{location} : Failed  update id : {id}");
                }
                _logger.LogWarn($"{location} Record with id : {id} sucessfully updated");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Deletes a book
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location} : Attempting to delete book with id: {id}");
                if (id < 1)
                {
                    _logger.LogWarn($"{location} : Bad data {id}");
                    return BadRequest();
                }
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location} : Book id: {id} was not found");
                    return NotFound();
                }
                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"{location} :Something went wrong contact admin");
                }
                return NoContent();
            }
            catch (Exception e)
            {

                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }


        private ObjectResult InternalError(string message)
        {
            _logger.LogError($"{message}");
            return StatusCode(500, "Something went wrong. Please contact the administrator");
        }
    }
}