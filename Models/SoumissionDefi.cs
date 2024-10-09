namespace SkillMatchAPI.Models
{
    public class SoumissionDefi
    {
        public int Id { get; set; }  

        public int DefiId { get; set; }  
        public Defi Defi { get; set; }  

        public int CandidatId { get; set; } 
        public User Candidat { get; set; }  

        public string FichierSoumis { get; set; } 

        public int? RecruteurId { get; set; } 
        public User Recruteur { get; set; }  

        public float? Note { get; set; }  
        public string Commentaire { get; set; }
    }
}
