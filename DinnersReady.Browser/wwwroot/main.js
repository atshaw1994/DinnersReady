import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

// Expose loader removal function globally so C# can invoke it when MainView is ready
globalThis.hideAppLoader = () => {
    const loader = document.getElementById('app-loader');
    if (loader) {
        // Force layout check to ensure canvas bounds are captured
        window.dispatchEvent(new Event('resize'));

        loader.classList.add('hidden');
        setTimeout(() => loader.remove(), 500);
    }
};

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);