using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillMatchAPI.Models;
using SkillMatchAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace SkillMatchAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;

            _context = context;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == userLogin.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password))
            {
                return Unauthorized("Nom d'utilisateur ou mot de passe incorrect.");
            }

            var token = GenerateJwtToken(user.Email,user.Role);
            return Ok(new { token });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistration userRegistration)
        {
            // Vérification si l'email est déjà utilisé
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userRegistration.Email);

            if (existingUser != null)
            {
                return BadRequest("L'utilisateur existe déjà.");
            }

            var user = new User
            {
                Email = userRegistration.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userRegistration.Password),
                Name = userRegistration.Name,
                LastName = userRegistration.LastName,
                IsRecruiter = userRegistration.IsRecruiter,
                Role = userRegistration.Role,
                UserSkills = new List<UserSkill>() // Initialisez la liste des UserSkills
            };

            // Ajoutez les compétences à l'utilisateur
            if (userRegistration.Skills != null && userRegistration.Skills.Count > 0)
            {
                foreach (var skillName in userRegistration.Skills)
                {
                    var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == skillName);

                    // Si la compétence n'existe pas, créez-la
                    if (skill == null)
                    {
                        skill = new Skill { Name = skillName };
                        _context.Skills.Add(skill);
                        await _context.SaveChangesAsync(); // Sauvegarde pour obtenir l'Id
                    }

                    user.UserSkills.Add(new UserSkill
                    {
                        User = user,
                        Skill = skill
                    });
                }
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Utilisateur enregistré avec succès.");
        }

        private string GenerateJwtToken(string Email,string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, Email),new Claim(ClaimTypes.Role, role)
}),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult GetAdminData()
        {
            return Ok("This data is only accessible to admins");
        }

        [HttpPost("{userId}/skills")]
        public async Task<IActionResult> AddSkillsToUser(int userId, [FromBody] List<string> skillNames)
        {
            // Vérifier si l'utilisateur existe
            var user = await _context.Users.Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            foreach (var skillName in skillNames)
            {
                var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == skillName);

                // Si la compétence n'existe pas, créez-la
                if (skill == null)
                {
                    skill = new Skill { Name = skillName };
                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync(); // Sauvegarde pour obtenir l'Id
                }

                // Vérifiez si l'utilisateur a déjà cette compétence
                if (!user.UserSkills.Any(us => us.SkillId == skill.Id))
                {
                    user.UserSkills.Add(new UserSkill
                    {
                        UserId = user.UserId,
                        SkillId = skill.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Compétences ajoutées avec succès.");
        }

        [HttpGet("{userId}/skills")]
        public async Task<IActionResult> GetSkillsByUser(int userId)
        {
            // Vérifier si l'utilisateur existe
            var user = await _context.Users.Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            var skills = user.UserSkills.Select(us => us.Skill.Name).ToList();
            return Ok(skills);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            // Récupération de l'utilisateur par son ID
            var user = await _context.Users
                .Include(u => u.UserSkills) // Inclure les compétences
                .ThenInclude(us => us.Skill) // Inclure les compétences des UserSkills
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            var skills = user.UserSkills
                .Select(us => us.Skill?.Name) // Utilisation de l'opérateur null-conditional (?.)
                .Where(name => name != null) // Filtrer les compétences nulles
                .ToList();

            // Exclure le mot de passe avant de renvoyer les informations
            var userResponse = new UserDto
            {
                userId = user.UserId,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Skills = skills
            };

            return Ok(userResponse);
        }



        public class UserLogin
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        public class UserRegistration
        {
            public string Name { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public List<string> Skills { get; set; } // Ajoutez cette propriété
            public bool IsRecruiter { get; set; }
            public string Role { get; set; }
        }

        public class UserDto
        {
            public int userId { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public List<string> Skills { get; set; } // Changer cela pour les compétences
        }

    }

}
