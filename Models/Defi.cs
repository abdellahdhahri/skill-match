using System.ComponentModel.DataAnnotations;

namespace SkillMatchAPI.Models
{
    public class Defi
    {
        [Key]
        public int Id { get; set; }  

        public string Titre { get; set; }  

        public string Description { get; set; }  

        public List<string> CompetencesRequises { get; set; }  


        public DateTime DateLimiteSoumission { get; set; }  

        public string Entreprise { get; set; }  

        public bool IsPublic { get; set; } 



        public DateTime DateCreation { get; set; }  

        public string Etat { get; set; } 


    }
}
