using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Telegram
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InlineKeyboardActionAttribute : Attribute
    {
        public string ActionName { get; }

        public InlineKeyboardActionAttribute(string actionName)
        {
            ActionName = actionName;
        }
    }
}
