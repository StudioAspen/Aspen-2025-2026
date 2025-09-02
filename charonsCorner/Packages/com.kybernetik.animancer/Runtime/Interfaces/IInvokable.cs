// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer
{
    /// <summary>An object with an <see cref="Invoke"/> method.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IInvokable
#if UNITY_ASSERTIONS
    [AnimancerHelpUrl(Strings.DocsURLs.AnimancerEventParameters)]
#endif
    public interface IInvokable : IPolymorphic
    {
        /************************************************************************************************************************/

        /// <summary>Executes the main function of this object.</summary>
        void Invoke();

        /************************************************************************************************************************/
    }
}

