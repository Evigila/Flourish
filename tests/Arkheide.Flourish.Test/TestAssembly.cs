using Xunit;

// WPF owns process-wide pack URI, theme, dispatcher, and Application state.
// Serializing collections prevents independent STA tests from racing those globals.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
