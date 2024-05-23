using p3rpc.expShare.Template.Configuration;
using System.ComponentModel;

namespace p3rpc.expShare.Configuration;
public class Config : Configurable<Config>
{

    [DisplayName("Debug Mode")]
    [Description("Logs additional information to the console that is useful for debugging.")]
    [DefaultValue(false)]
    public bool DebugEnabled { get; set; } = false;

    [DisplayName("Growth 3 Mod")]
    [Description("EXP Share also applies to stock personas.")]
    [DefaultValue(true)]
    public bool Growth3 { get; set; } = true;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}