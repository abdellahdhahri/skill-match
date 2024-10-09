namespace SkillMatchAPI.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; }

    }
    public class UserSkill
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int SkillId { get; set; }
        public Skill Skill { get; set; }
    }

}
