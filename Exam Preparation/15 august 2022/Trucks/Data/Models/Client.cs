using System.ComponentModel.DataAnnotations;

namespace Trucks.Data.Models
{
    public class Client
    {

        public Client()
        {
            this.ClientsTrucks = new HashSet<ClientTruck>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Name { get; set; }

        [Required]
        public string Nationality { get; set; }

        [Required]
        public string Type { get; set; }

        public ICollection<ClientTruck> ClientsTrucks { get; set; }
    }
}
