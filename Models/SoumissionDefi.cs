namespace SkillMatchAPI.Models
{
    public class SoumissionDefi
    {
        public int Id { get; set; }  

        public int DefiId { get; set; }  
        public Defi Defi { get; set; }  

        public int CandidatId { get; set; } 
        public User Candidat { get; set; }  

       
    }
}
