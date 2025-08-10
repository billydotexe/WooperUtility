using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WooperUtility.Datacontext;

[Table("Users")]
public class User
{
	[Key]
	public long Id { get; set; }
	public string? Username { get; set; }
	public DateTime JoinDate { get; set; }
	public DateTime LastActivity { get; set; }
}
