# Edit account portraits in Flourish

2026-11-07 · America/Sao_Paulo · UTC−03:00.

Avatar uses a full-width row. The circular portrait, name and optional description are centered vertically. A direct UniformGrid child spans all columns. The optional HelpText appears beside the portrait and can be opened by hovering, focusing or tapping its help control. Escape dismisses it.

SaveAsync enables the native file picker. Choosing a supported file opens the existing Dialog with a circular preview, pointer panning and keyboard-operable ranges for zoom and both axes. The browser checks the file header before decoding JPEG, PNG or WebP. The source limits are 12 MiB, 8192 pixels per side and 24 megapixels. Closing or replacing a crop releases its bitmap and listeners. Late decoding results are discarded.

Canvas produces a square JPEG at quality 0.75 with a maximum side of 512 pixels. It uses the source resolution for smaller crops and reduces dimensions as needed to stay within 256 KiB. The new image contains the drawn pixels. JPEG bytes cross the Blazor interop boundary through IJSStreamReference and are converted to a data URL before the host callback. The host authorizes and saves the update. It updates Url after success and returns false to retain a rejected crop for retry. Disabled prevents edits while an open crop remains cancellable.

The frozen 1.4.7-preview.2 feed contains six verified archives and a provenance manifest under `Colligere/artifacts/flourish-preview/1.4.7-preview.2`. The manifest records the 5e1a152 baseline, runtime build inputs and each archive hash. Its SHA256 is `A27FEF553B2F9D7D143A042D53946BD1F6D50B88DC07DBF66FD67C66B926A6F7`. The packaged avatar-editor.js SHA256 matches the source. Package build overrides apply only to this local candidate.

The 12-project Blazor Release build passed without warnings or errors. Framework passed 533 checks, Gallery passed 9270, the crop module passed nine Node tests and isolated package consumers passed 155. The culture catalog passed 26265 checks, ChangeLog passed 216 and the CSS bundle passed 21. The original read-only UG regression matched a span root. Its assertion now matches the div used by the full-width layout and retains the check for read-only composition.

For manual acceptance, open the Gallery Avatar example. Hover the portrait, open the file chooser with keyboard or touch and select a landscape JPEG or PNG. Verify the centered preview appears, drag it and adjust each range with arrow keys. Save and confirm the updated portrait. Cancel a second edit and verify the original portrait remains. Try a corrupt or oversized image and verify a useful error. Focus the help icon and dismiss it with Escape. Repeat at narrow width and high contrast. Browser rendering and the native chooser require this manual check.
