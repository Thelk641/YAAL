using Avalonia.Input;
using Avalonia.Interactivity;

namespace YAAL;

public partial class CLM : ScalableWindow
{
    private static CLM? window;
    private bool altMode = false;

    private CLM_Selector selector;
    private CLM_Commands commands;
    private CLM_Buttons buttons;
    private CLM_BottomBar bottomBar;

    public CLM()
    {
        InitializeComponent();

        selector = new CLM_Selector(this);
        Holder_Selector.Children.Add(selector);

        buttons = new CLM_Buttons(this, selector);
        Holder_Buttons.Children.Add(buttons);

        commands = new CLM_Commands(this);
        Holder_Commands.Children.Add(commands);

        bottomBar = new CLM_BottomBar(this, selector);
        Holder_BottomBar.Children.Add(bottomBar);

        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);

        AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (altMode)
            {
                buttons.ProcessKey(e.Key);
                return;
            } else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                altMode = true;
                buttons.SwitchMode(true);
            }
        }, RoutingStrategies.Tunnel);

        AddHandler(InputElement.KeyUpEvent, (_, e) =>
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                altMode = false;
                buttons.SwitchMode(false);
            }
        }, RoutingStrategies.Tunnel);

        this.Closing += (_, _) =>
        {
            SaveLauncher();
        };

        LoadLauncher(selector.GetCache().cache);
    }

    

    public static CLM GetWindow()
    {
        if(window == null)
        {
            window = new CLM();
            window.Closing += (_,_) =>
            {
                window = null;
            };
        }
        return window;
    } 

    public void LoadLauncher(Cache_CustomLauncher launcher)
    {
        commands.LoadCommands(launcher.instructionList);
        bottomBar.SwitchGameMode(launcher.settings[LauncherSettings.IsGame] == true.ToString());
    }

    public void SaveLauncher()
    {
        if (selector.Save())
        {
            return;
        }

        Cache_CustomLauncher toSave = selector.GetCache().cache;
        toSave.instructionList = commands.GetCommands();
        LauncherManager.SaveLauncher(toSave);
        selector.UpdateCache(toSave);
    }

    public void SaveLauncher(Cache_CustomLauncher toSave)
    {
        toSave.instructionList = commands.GetCommands();
        LauncherManager.SaveLauncher(toSave);
        selector.UpdateCache(toSave);
    }

    public void SaveLauncher(string newName)
    {
        Cache_DisplayLauncher toSave = selector.GetCache();
        toSave.cache.instructionList = commands.GetCommands();
        string trueName = LauncherManager.RenameLauncher(toSave, newName);
        selector.ReloadList(trueName);
    }
}