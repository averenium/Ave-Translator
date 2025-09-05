using AveTranslatorM.Components.Pages;
using AveTranslatorM.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Storage;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

public enum GameType
{
    Unselected,
    WormsUMH,
    KCD2
}

public class GameSelectionService
{
    public static readonly string WorkingDir = Path.Combine(Environment.CurrentDirectory, "Working");
    private NavigationManager? _navigationManager;
    private readonly HashSet<string> _availableRoutes;
    public GameType SelectedGame { get; private set; } = GameType.Unselected;

    public event Action? OnGameChanged;

    public void SetGame(GameType game)
    {
        if (SelectedGame == game) return;

        string newRoute = "/";
        var currentRoute = string.Empty;

        if (_navigationManager != null)
        {
            try
            {
                currentRoute = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
                newRoute = GetNewRoute(currentRoute, game);
            }
            catch
            {
            }
        }

        SelectedGame = game;
        OnGameChanged?.Invoke();

        if (_navigationManager != null && !string.IsNullOrEmpty(currentRoute) && currentRoute != newRoute)
        {
            try
            {
                _navigationManager.NavigateTo(newRoute, false, true);
            }
            catch (NavigationException)
            {
                _navigationManager.NavigateTo("/");
            }
        }
    }

    private string GetNewRoute(string currentRoute, GameType newGame)
    {
        if (string.IsNullOrEmpty(currentRoute) || currentRoute == "/")
            return "/";

        if (!currentRoute.Contains(SelectedGame.ToString(), StringComparison.InvariantCultureIgnoreCase))
        {
            return currentRoute;
        }
        var maybeNewRoute = currentRoute.Replace(SelectedGame.ToString(), newGame.ToString().ToLower(), StringComparison.CurrentCultureIgnoreCase);
        if(IsRouteValid(maybeNewRoute))
             return maybeNewRoute; 
        return "/";
    }

    public GameSelectionService()
    {
        _availableRoutes = GetAvailableRoutes();
    }

    private HashSet<string> GetAvailableRoutes()
    {
        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "/" };

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var components = assembly.GetTypes()
            .Where(t => t.Namespace?.Contains("Components.Pages") == true);

        foreach (var component in components)
        {
            var routeAttribute = component.GetCustomAttribute<RouteAttribute>();
            if (routeAttribute != null)
            {
                routes.Add(routeAttribute.Template);
            }
        }

        return routes;
    }

    private bool IsRouteValid(string route)
    {
        if(string.IsNullOrEmpty(route))
            return false;

        if(!route.StartsWith("/"))
            route = "/" + route;
        return _availableRoutes.Contains(route);
    }
    public void Initialize(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
}