using Exiled.API.Interfaces;

namespace acoolplugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;

        public string HelpCommand { get; set; } = "Do the .GWQ command to see all comamnds related to Gambling With Quarters!"; //Add this into the Commands/Help section.
    }
}
