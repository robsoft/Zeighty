using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Xna.Framework;

var services = new ServiceCollection();

services.AddLogging(b =>
{
    b.AddDebug();
    b.AddConsole();
    b.SetMinimumLevel(LogLevel.Information);
});

services.AddSingleton<Zeighty.ZeightyGame>();

using var provider = services.BuildServiceProvider();

using var game = provider.GetRequiredService<Zeighty.ZeightyGame>();
game.Run();


