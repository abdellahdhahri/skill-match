namespace SkillMatchAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public ICollection<UserSkill> UserSkills { get; set; }
        public bool IsRecruiter { get; set; }
        public string Role { get; set; } 

    }
}
