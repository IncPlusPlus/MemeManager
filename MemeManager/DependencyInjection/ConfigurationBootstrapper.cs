using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls;
using MemeManager.ViewModels.Configuration;
using Microsoft.Extensions.Configuration;
using Splat;

namespace MemeManager.DependencyInjection;

public static class ConfigurationBootstrapper
{
    public static void RegisterConfiguration(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        var configuration = BuildConfiguration();

        // Required for Serilog to get the portion of the config that starts with the Serilog key
        RegisterGlobalAppSettings(services, configuration);
        // Register class instances that represent a section of the config
        RegisterAboutDialogConfiguration(services, configuration);
        RegisterMemesConfiguration(services, configuration);
        RegisterLayoutConfiguration(services, configuration);
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddYamlFile("appsettings.yaml")
            .Build();

    private static void RegisterGlobalAppSettings(IMutableDependencyResolver services, IConfiguration configuration)
    {
        /*
         * Needed for Serilog because it won't take a ConfigurationSection anymore and wants to have the whole
         * IConfiguration. It specifically needs to find a section named Serilog.
         */
        services.RegisterConstant(configuration);
    }

    private static void RegisterAboutDialogConfiguration(IMutableDependencyResolver services,
        IConfiguration configuration)
    {
        services.RegisterConstant(configuration.GetRequiredSection("About").Get<AboutDialogConfiguration>());
    }

    private static void RegisterMemesConfiguration(IMutableDependencyResolver services, IConfiguration configuration)
    {
        services.RegisterConstant(configuration.GetRequiredSection("Memes").Get<MemesConfiguration>());
    }

    private static void RegisterLayoutConfiguration(IMutableDependencyResolver services, IConfiguration configuration)
    {
        TypeDescriptor.AddAttributes(typeof(GridLength),
            new TypeConverterAttribute(typeof(MyCustomGridLengthConverter)));
        services.RegisterConstant(configuration.GetRequiredSection("Layout").Get<LayoutConfiguration>());
    }

    /// <summary>
    /// A TypeConverter that enables the conversion between GridLength and string.
    /// </summary>
    public class MyCustomGridLengthConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            // We can only handle strings, integral and floating types
            TypeCode tc = Type.GetTypeCode(sourceType);
            switch (tc)
            {
                case TypeCode.String:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            // GridLengthConverter in WPF throws a NotSupportedException on a null value as well.
            if (value == null)
            {
                return GridLength.Auto;
            }

            if (value is string stringValue)
            {
                return GridLength.Parse(stringValue);
            }

            // Conversion from numeric type, WPF lets Convert exceptions bubble out here as well
            double doubleValue = Convert.ToDouble(value, culture ?? CultureInfo.CurrentCulture);
            if (double.IsNaN(doubleValue))
            {
                // WPF returns Auto in this case as well
                return GridLength.Auto;
            }
            else
            {
                return new GridLength(doubleValue);
            }
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }
            if (destinationType != typeof(string))
            {
                throw new NotSupportedException(String.Format("Cannot convert from GridLength to {0}.", destinationType.ToString()));
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
