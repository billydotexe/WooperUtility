using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WooperUtility.Datacontext;

[Table("Admins")]
public class Admin
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id {get; set;}
	public DateTime AdminSince {get; set;}

	public virtual User User {get; set;} = new();
}
