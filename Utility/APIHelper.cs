using System.Collections.ObjectModel;
using System.Globalization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WooperUtility.Models;

namespace WooperUtility.Utility;

internal static class APIHelper
{
	/* starting from a string that needs some value from the user object
	 * get those and return the complete string
	 */
	public static string CreateCaption(string message, List<string> p, User from)
	{
		var par = new string[p.Count];

		for (var i = 0; i < p.Count; i++)
		{
			var param = p[i];
			var prop = GetValue(from, param);
			par[i] = prop;
		}

		return string.Format(CultureInfo.InvariantCulture, message, par);
	}

	/* given an object and a string return the value of the field of the object
	 * named as the param
	 */
	public static string GetValue(object update, string param)
	{
		var p = update.GetType().GetProperties().Where(x => string.Equals(x.Name, param, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
		var ret = string.Empty;
		if (p is not null)
		{
			ret = p.GetValue(update) is not null ? p.GetValue(update)!.ToString()! : "";
		}

		return ret;
	}

	/* given a list of Buttons create the InlineKeyboard
	 * optional parameters are needed to manage navigation between menu voices
	 * and between pages if a menù voice has too many buttons
	 */
	public static InlineKeyboardMarkup CreateKeyboard(ICollection<Button> btns, int currentPage = 0, int index = 0, int prevPage = -1)
	{
		List<InlineKeyboardButton>? arrows = null;
		//here we need to split the list and manage multiple pages
		if(btns.Count > 10)
		{
			var btnsCount = btns.Count;
			btns = btns.Skip(10 * index).Take(10).ToList();
			arrows = new List<InlineKeyboardButton>();
			//we're at least at second page so te user needs to be able to go back
			if(index > 0)
			{
				arrows.Add(new InlineKeyboardButton($"◀️ Page {index}") { CallbackData = $"{currentPage}.{index-1}" });
			}
			//we're not at the end of the list so the user needs to be able to get another page
			if (index < btnsCount / 10f - 1f)
			{
				arrows.Add(new InlineKeyboardButton($"Page {index + 2} ▶️") { CallbackData = $"{currentPage}.{index + 1}" });
			}
		}
		var buttons = btns.Select(b => CreateButtonRow(b)).ToList();
		if (arrows is not null)
		{
			buttons.Add(arrows);
		}
		//if there's a menu voice before this add the button to go to that
		if (prevPage >= 0)
		{
			buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton("🔙 Back") { CallbackData = prevPage.ToString(CultureInfo.InvariantCulture) } });
		}

		InlineKeyboardMarkup keyboard = new(buttons);
		return keyboard;
	}

	public static InlineKeyboardMarkup CreateNavigationKeyboard(int numberOfUsers, int page = 0)
	{
		var arrows = new List<InlineKeyboardButton>();
		if(page > 0)
		{
			arrows.Add(new InlineKeyboardButton($"◀️ Page {page - 1}") { CallbackData = $"users.{page - 1}" });
		}
		if(numberOfUsers > 20)
		{
			arrows.Add(new InlineKeyboardButton($"Page {page + 1} ") { CallbackData = $"users.{page + 1}" });
		}

		return new InlineKeyboardMarkup(arrows);
	}

	/* given a Button return what the library needs to create a row with an InlineButton
	 */
	public static ICollection<InlineKeyboardButton> CreateButtonRow(Button btn)
	{
		var inlineKeyboardButtons = new List<InlineKeyboardButton>()
															{
																new InlineKeyboardButton(btn.Text)
																{
																	CallbackData = btn.PageId?.ToString(CultureInfo.InvariantCulture),
																	Url = btn.Url
																}
															};
		return inlineKeyboardButtons;
	}


}
