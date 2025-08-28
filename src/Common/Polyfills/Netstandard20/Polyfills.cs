

// IsExternalInit
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved for compiler use to support the `init` accessor in C# 9.0 and later.
    /// </summary>
    /// <remarks>This type is used internally by the C# compiler to enable the `init` accessor, which allows 
    /// properties to be set during object initialization but remain immutable thereafter. It is not  intended for
    /// direct use in application code.</remarks>
    internal static class IsExternalInit { }
}
#endif

#if !NET7_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Specifies that a class or struct requires all members to be explicitly initialized.
    /// </summary>
    /// <remarks>This attribute is used to indicate that all members of the decorated type must be explicitly
    /// initialized, typically to enforce stricter initialization rules. It is applied to classes or structs and cannot
    /// be inherited or applied multiple times.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    /// <summary>
    /// Indicates that a specific compiler feature is required for the target element.
    /// </summary>
    /// <remarks>This attribute is used to signal that the annotated element depends on a specific compiler
    /// feature. It can be applied to any program element and supports multiple usages on the same element.</remarks>
    /// <param name="featureName">The name of the compiler feature required by the annotated element. This value cannot be null or empty.</param>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
    {
        /// <summary>
        /// Gets the name of the compiler feature required by the annotated element.
        /// </summary>
        public string FeatureName { get; } = featureName;

        /// <summary>
        /// Gets a value indicating whether the parameter is optional.
        /// </summary>
        public bool IsOptional { get; init; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Indicates that the attributed constructor sets all required members for the containing type.
    /// </summary>
    /// <remarks>This attribute is used to signal that the constructor ensures all required members of the
    /// type are initialized, satisfying the requirements of the <c>required</c> modifier in C#.</remarks>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}
#endif

