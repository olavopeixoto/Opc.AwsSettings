namespace Opc.AwsSettings.Settings;

public record ParameterStoreSettings
{
    /// <summary>
    /// Prefix used to load values from Parameter Store. All keys matching the given path prefix will be loaded.<br/>
    /// The default mapping is to remove the given prefix and use the remaining path to map to a object hierarchy.
    /// </summary>
    /// <example>
    /// "Paths": ["/myservice"]
    ///
    /// Settings with keys:
    /// /myservice/myoption/mykey = 1234
    /// /myservice/myoption2/myproperty/mykey = "abc"
    ///
    /// will be mapped to:
    /// myoption:mykey = 1234
    /// myoption2:myproperty:mykey = "abc"
    ///
    /// So you can have objects like so:
    /// public record MyOption(int MyKey);
    /// public record MyOption2(MyOtherObject MyProperty);
    /// public record MyOtherObject(string MyKey);
    /// </example>
    public string[] Paths { get; init; } = Array.Empty<string>();

    /// <summary>
    /// If you need custom mapping between Parameter Store names and your Options objects you may use this settings instead of just Paths.
    /// </summary>
    public ParameterStoreKeySettings[] Keys { get; init; } = Array.Empty<ParameterStoreKeySettings>();
}