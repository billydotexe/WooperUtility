using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WooperUtility.Datacontext;

[Table("BotRequests")]
public class BotRequest
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id {get; set;}
	public DateTime RequestDate {get; set;}
	public required string RequestType {get; set;}

	public virtual User User {get; set;} = new();
}
