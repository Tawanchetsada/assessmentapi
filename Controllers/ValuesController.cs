using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project1.DataAccess; 
using Project1.Module;
using SQLitePCL;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class ProductController : ControllerBase
       
    {
        private readonly ApplicationContext _context;
        private string GenerateJwtToken(string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your_secret_key_here_please_change_it"); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, $"user_{role}"),
                new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

 
        [HttpGet("loginAdmin")]
        
        public IActionResult LoginAdmin()
        {
            var token = GenerateJwtToken("Admin");
            return Ok(new { Token = token });
        }

  
        [HttpGet("loginUser")]
        public IActionResult LoginUser()
        {
            var token = GenerateJwtToken("User");
            return Ok(new { Token = token });
        }
        public ProductController(ApplicationContext context) {
            _context = context;
        
        }

       
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Get()
        {
           var products = _context.Products.ToList();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Product([FromRoute]int id)
        {
            var products = _context.Products.FirstOrDefault(x =>x.Id == id);
            if (products == null)
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpPost("")]
        [Authorize(Roles = "Admin")]
        public IActionResult Save([FromBody] Product product)
        {
           _context.Products.Add(product);
           _context.SaveChanges();
            return Ok();
        }

        [HttpPut("")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update([FromBody] Product product)
        {
            var products = _context.Products.AsNoTracking().FirstOrDefault(x => x.Id == product.Id);
            if (products == null)
            {
                return NotFound();
            }
            _context.Products.Update(product);
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete([FromRoute] int id)
        {
            var products = _context.Products.FirstOrDefault(x => x.Id == id);
            if (products == null)
            {
                return NotFound();
            }
            _context.Products.Entry(products).State = EntityState.Deleted;
            //_context.Products.Remove(products);
            _context.SaveChanges();
            return Ok(products);
        }





    }
}
