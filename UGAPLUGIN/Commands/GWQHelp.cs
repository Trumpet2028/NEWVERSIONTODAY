using System;
using CommandSystem;

using RemoteAdmin;

namespace accolplugin.handlers
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Help : ICommand
    {
        public string[] Aliases { get; } = { "commands" };
        public string Command { get; } = "commands";
        public string Description { get; } = "Tells people general information about the server!";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                response = ".GWQ: Shows Gambling With Quarters info.";
                return false;
            }
            else
            {
                response = ".GWQ: Shows Gamblig with Quarters info.";
                return true;
            }
        }
    }
        
    public class DetroitBecomeHuman : ICommand
    {
        public string Command { get; } = "human";

        public string[] Aliases { get; } = { "human" };

        public string Description => "Turns an SCP player into a human!";
        // Track the time in the server. I'll come back to this later. 

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            throw new NotImplementedException();
        }
    }
    public class GWQHelp : ICommand 
    { 
        public string[] Aliases { get; } = { "GWQ" };
        public string Command { get; } = "GWQ";
        public string Description { get; } = "Tells people how GWQ works! See >Execute function.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                response = "Do .bet 5 to gamble your quarters! \n(Any values higher than 5 cannot be used!) \nOr do .deposit to deposit quarters into your inventory!gg \n.withdraw withdraws them.";
                return false;
            }
            else
            {
                response = "Do .bet 5 to gamble your quarters! \n(Any values higher than 5 cannot be used!) \nOr do .deposit to deposit quarters into your inventory! \n.withdraw withdraws them.";
                return true;
            }
        }
    }
}