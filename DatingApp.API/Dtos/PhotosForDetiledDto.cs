namespace DatingApp.API.Dtos
{
    public class PhotosForDetiledDto
    {
         public int Id { get; set; }
        public string Url { get; set; } 
        public string Description { get; set; }
        public System.DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public bool IsApproved { get; set; }
    }
}