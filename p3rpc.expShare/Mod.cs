using p3rpc.expShare.Configuration;
using p3rpc.expShare.Template;
using Reloaded.Mod.Interfaces;
using static p3rpc.expShare.Utils;
using static p3rpc.expShare.Native;
using Reloaded.Hooks.Definitions;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace p3rpc.expShare;
/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private IHook<SetupPartyExpDelegate> _setupPartyExpHook;
    private static GetUGlobalWorkDelegate _getUGlobalWork;
    private IAsmHook _growthThreeFirstHook;
    private IAsmHook _growthThreeSecondHook;

    internal static UGlobalWork* GlobalWork => _getUGlobalWork();

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        Utils.Initialise(_logger, _configuration, _modLoader);

        SigScan("48 8B C4 53 55 48 81 EC 88 00 00 00", "SetupPartyExp", address =>
        {
            _setupPartyExpHook = _hooks.CreateHook<SetupPartyExpDelegate>(SetupPartyExp, address).Activate();
        });

        Utils.SigScan("48 89 5C 24 ?? 57 48 83 EC 20 48 8B 0D ?? ?? ?? ?? 33 DB", "GetUGlobalWork", address =>
        {
            _getUGlobalWork = _hooks.CreateWrapper<GetUGlobalWorkDelegate>(address, out _);
        });

        if (_configuration.Growth3)
        {
            SigScan("0F 85 ?? ?? ?? ?? 4D 85 C0 0F 84 ?? ?? ?? ?? 41 8B 40 ?? 41 3B C1 7D ?? 0F B7 C8", "SetGrowth3First", address =>
            {
                var hookLength = _hooks.Utilities.GetHookLength((nint)address, 7, true);
                LogDebug($"Hook length is: {hookLength}");
                List<string> enableGrowth3 = new()
                {
                "use64", "nop", "nop", "nop", "nop", "nop", "nop"
                };
                if (hookLength > 7)
                    enableGrowth3.Add("test r8,r8");

                _growthThreeFirstHook = _hooks.CreateAsmHook(enableGrowth3.ToArray(), address, Reloaded.Hooks.Definitions.Enums.AsmHookBehaviour.DoNotExecuteOriginal).Activate();
            });

            SigScan("0F 85 ?? ?? ?? ?? E8 ?? ?? ?? ?? BF 01 00 00 00", "SetGrowth3Second", address =>
            {
                var hookLength = _hooks.Utilities.GetHookLength((nint)address, 7, true);
                LogDebug($"Hook length is: {hookLength}");
                List<string> enableGrowth3 = new()
                {
                "use64", "cmp eax,eax"
                };

                _growthThreeSecondHook = _hooks.CreateAsmHook(enableGrowth3.ToArray(), address, Reloaded.Hooks.Definitions.Enums.AsmHookBehaviour.ExecuteFirst).Activate();
            });
        }
    }

    private void SetupPartyExp(ABtlPhaseResult* result)
    {
        _setupPartyExpHook.OriginalFunction(result);

        var members = GetValues<PartyMember>();
        var experiences = Enumerable.Range(0, members.Count()).Select(z => result->EarnedExp[z]);
        var expSum = experiences.Sum();
        var maxExp = experiences.Max();
        var distributed_exp = expSum / (experiences.Count() - 1); // Exclude PartyMember.None

        LogDebug($"Experiences gained are: {string.Join(' ', experiences)} | Total Gain: {expSum}");
        LogDebug($"Max EXP is: {maxExp}");
        LogDebug($"Total EXP if distributed will be: {distributed_exp}");

        var ct = -1;
        foreach (var member in members)
        {
            ct += 1;
            if (member == PartyMember.None || !IsValid(member, GlobalWork->Date)) 
                continue;
            result->EarnedExp[ct] = maxExp;
            LogDebug($"Party member {member} gained {maxExp}");
        }
    }

    private bool IsValid(PartyMember member, int Date) => member == PartyMember.Shinjiro ? Date <= ShinjiroDeath && (int)member <= Date : (int)member <= Date;

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        LogDebug($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}