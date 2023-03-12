using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WooperUtility.Models;

namespace WooperUtility.Utility
{
    public static class APIHelper
    {
        /* starting from a string that needs some value from the user object 
         * get those and return the complete string
         */
        public static string CreateCaption(string message, List<string> p, User from)
        {
            string[] par = new string[p.Count];

            for (int i = 0; i < p.Count; i++)
            {
                var param = p[i];
                string prop = GetValue(from, param);
                par[i] = prop;
            }

            return String.Format(message, par);
        }

        /* given an object and a string return the value of the field of the object
         * named as the param  
         */
        public static string GetValue(object update, string param)
        {
            var p = update.GetType().GetProperties().Where(x => x.Name.ToLower() == param.ToLower()).FirstOrDefault();
            string ret = String.Empty;
            if (p is not null)
            {
                ret = p.GetValue(update) is not null ? p.GetValue(update).ToString() : "";
            }

            return ret;
        }

        /* given a list of Buttons create the InlineKeyboard
         * optional parameters are needed to manage navigation between menu voices 
         * and between pages if a menù voice has too many buttons
         */
        public static InlineKeyboardMarkup CreateKeyboard(List<Button> btns, int currentPage = 0, int index = 0, int prevPage = -1)
        {
            List<InlineKeyboardButton>? arrows = null;
            //here we need to split the list and manage multiple pages
            if(btns.Count > 10)
            {
                int btnsCount = btns.Count;
                btns = btns.Skip(10 * index).Take(10).ToList();
                arrows = new List<InlineKeyboardButton>();
                //we're at least at second page so te user needs to be able to go back
                if(index > 0)
                {
                    arrows.Add(new InlineKeyboardButton($"◀️ Page {index}") { CallbackData = $"{currentPage}.{index-1}" });
                }
                //we're not at the end of the list so the user needs to be able to get another page
                if (index < (btnsCount / 10) - 1)
                {
                    arrows.Add(new InlineKeyboardButton($"Page {index + 2} ▶️") { CallbackData = $"{currentPage}.{index + 1}" });
                }
            }
            List<List<InlineKeyboardButton>> buttons = btns.Select(b => CreateButtonRow(b)).ToList();
            if (arrows is not null) buttons.Add(arrows);
            //if there's a menu voice before this add the button to go to that
            if (prevPage >= 0) buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton("🔙 Back") { CallbackData = prevPage.ToString() } });
            InlineKeyboardMarkup keyboard = new(buttons);
            return keyboard;
        }

        /* given a Button return what the library needs to create a row with an InlineButton
         */
        public static List<InlineKeyboardButton> CreateButtonRow(Button btn)
        {
            List<InlineKeyboardButton> inlineKeyboardButtons = new List<InlineKeyboardButton>()
                                                                {
                                                                    new InlineKeyboardButton(btn.Text)
                                                                    {
                                                                        CallbackData = btn.PageId?.ToString(),
                                                                        Url = btn.Url
                                                                    }
                                                                };
            return inlineKeyboardButtons;
        }
    }
}
