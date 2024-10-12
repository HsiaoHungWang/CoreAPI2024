using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreAPI2024.Models;
using CoreAPI2024.Models.DTO;

namespace CoreAPI2024.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IWebHostEnvironment _host;

        private readonly MyDBContext _context;
        private readonly UserService _userService;


        public MembersController(IWebHostEnvironment host, MyDBContext context, UserService userService)
        {
            _context = context;
            _host = host;
            _userService = userService;

        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Members/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.MemberId)
            {
                return BadRequest();
            }

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Members
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember([FromForm] Member member, IFormFile theFile)
        {
            //結合路徑及檔案名稱
            string filePath = Path.Combine(_host.WebRootPath, "images", theFile.FileName);
            //將檔案存到資料夾中
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                theFile.CopyTo(fileStream);
            }
            //將檔案轉成二進位
            byte[] imgByte;
            using (var memoryStream = new MemoryStream())
            {
                theFile.CopyTo(memoryStream);
                imgByte = memoryStream.ToArray();
            }

            member.FileName = theFile.FileName;
            member.FileData = imgByte;

            //密碼加密加鹽
            var (hashedPassword, salt) = _userService.HashPassword(member.Password);

            member.Password = hashedPassword;
            member.Salt = salt;

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.MemberId }, member);
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }

        [HttpGet("paths")]
        public IActionResult GetPaths()
        {
            var webRootPath = _host.WebRootPath;
            var contentRootPath = _host.ContentRootPath;

            return Ok(new { WebRootPath = webRootPath, ContentRootPath = contentRootPath });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO login)
        {
            var member = _context.Members.Where(m => m.Email.Equals(login.Email)).SingleOrDefault();
            if (member != null) {
                // 從數據庫中獲取已加密的密碼和鹽值
                var hashedPassword = member.Password;
                var salt = member.Salt;
                var isPasswordValid = _userService.VerifyPassword(login.Password, hashedPassword, salt);

                if (!isPasswordValid)
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // 其他登入邏輯

                return Ok(new { Message = "Login successful!" });
            }

            return Ok(new { Message = "查無此帳號" });

        }
    }
}
