using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WooperUtility.Datacontext;

[Table("BannedUsers")]
public class BannedUser
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id {get; set;}
	public User BannedBy {get; set;} = new();
	public DateTime BanDate {get; set;}

	public User User {get; set;} = new();
}
