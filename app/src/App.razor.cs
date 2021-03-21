// This file makes Live Unit Testing work, see
// https://developercommunity.visualstudio.com/t/live-unit-testing-fails-to-build-blazor-server-pro/853116#T-N1094227

using Microsoft.AspNetCore.Components;

namespace Mips.App
{
    public partial class App : ComponentBase { }

    public partial class MainLayout : LayoutComponentBase { }
}
