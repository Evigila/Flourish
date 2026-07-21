---
name: creating-page
description: Create or refactor Flourish WPF pages with exactly one leading ChunkHero, full-width Chunk sections, standardized typography and spacing, non-nesting Card-family surfaces, Paragraph prose, Presenter compositions, compact ListCard actions, OutputCard histories, and the correct Button-family interaction semantics. Use when adding or editing page XAML, reviewing page information architecture, choosing layout or content controls, or validating a Flourish page before delivery.
---

# Creating Flourish Pages

Build every content page as one explicit hierarchy: one page-leading hero, several full-width sections, purpose-built content controls, and clearly bounded actions. Keep structural copy concise and let each control perform only its documented role.

## Build the page hierarchy

1. Place page content in a Flourish scroll viewer and use the standard page margin resource.
2. Start with exactly one `ChunkHero`. Never omit it or create a second hero.
3. Follow the hero with one or more `Chunk` controls.
4. Make every `ChunkHero` and `Chunk` full-width and place each in its own row.
5. Put every page element inside the hero or a chunk. Do not add peer-level headings, cards, or manually spaced regions.
6. Give each distinct topic or task its own `Chunk`.
7. Arrange related cards within one chunk body only when they share the same section topic.

Every `Chunk` has these semantic fields:

- `Title` is required and names the section's subject.
- `Description` is optional. Add it only when the title cannot communicate essential context by itself.
- `Body` is required and contains the actual content control or layout tree. It is the default XAML content property.

Empty or `null` optional regions must collapse together with their spacing. Keep the default large gap between chunks and between `ChunkHero` and the first ordinary chunk.

`ChunkHero` inherits the Presenter contract: `Title`, `Description`, `Body`, `Presentation`, `PresenterMode`, and `PresenterPosition`. Its title is required and uses the page-header role. Use `Body` for supporting controls in the copy region and `Presentation` for the visual being presented.

## Use the typography contract

Flourish defines six size tiers. When no tier is explicitly selected, use `Standard`.

- `Small`: navigation group labels, OutputCard output, and compact control-owned caption or status text.
- `Standard`: ordinary body and control text, including every unspecified case.
- `Icon`: general icon glyphs; a specialized icon control may apply its own local correction.
- `Large`: card-title-level emphasis.
- `ExtraLarge`: section-title family, including `Chunk.Title`.
- `HeaderSize`: `ChunkHero.Title` only.

Do not select a larger tier merely to make text look more important. Select the control or text role that expresses the correct hierarchy.

## Choose the content control

Use the smallest control whose semantic contract matches the content:

- Use `Card` for an optional title and one optional paragraph in `MainText`.
- Use `IconCard` for the same copy plus one icon.
- Use `Paragraph` for several text paragraphs.
- Use `Presenter` for an image, several icons, an illustration, a preview, or composed presentation content.
- Use `ListCard` for one compact setting or local action row.
- Use `OutputCard` for append-only raw messages, logs, progress, completed results, and failures.
- Use `CardButton` when the complete card-shaped surface is one action.

One surface communicates one subject or behavior. Split unrelated subjects into peer controls instead of building a large nested surface.

## Compose cards

`Card`, `IconCard`, and `ListCard` are terminal presentation surfaces. They do not have a general-purpose `Body` and must not host arbitrary nested content.

For `Card`:

- `Title` and `MainText` are both optional.
- `MainText` contains one paragraph only.
- Missing fields collapse together with their spacing.
- `Variant` may be `Standard`, `Tonal`, `Filled`, or `Elevated` according to surface emphasis.
- Card and IconCard may form two or more columns when the available width keeps copy readable.

For `IconCard`:

- `Icon` contains exactly one semantic icon, not an image, icon group, or arbitrary content tree.
- `IconPosition` uses `Dock.Left`, `Top`, `Right`, or `Bottom`; the default is `Left`.
- It has no `Body`, `Presentation`, Presenter modes, or overlay layout.

Do not attach click, command, hover-trigger, or popup-trigger behavior to non-interactive cards. Use the appropriate Button-family control when the surface is interactive.

## Compose ListCards

Treat `ListCard` as a compact row for one independent setting. Its layout is fixed: optional `Icon` on the left, vertically stacked `Title` and `MainText` in the center, and `ActionBody` on the right. The complete row is vertically centered and left-oriented.

- Keep `Title` and `MainText` concise. Each is one line and uses ellipsis overflow.
- Put exactly one local interactive control in `ActionBody`; it is the default XAML content property.
- Prefer `FlourishComboBox`, `FlourishCheckBox`, or `Button`; use `FlourishTextBox` or `FlourishRadioButton` when required.
- Apply selections, toggles, and edits immediately. Never add a separate Apply action.
- Keep `Variant` at its coerced `Standard` value.
- Stack related ListCards in their own column and use `FlourishListCardPeerMargin` only between consecutive rows.
- Do not interleave another card type in the same ListCard column.

An adjacent column may contain an `OutputCard`. Put the complete ListCard column and `OutputCard` in one auto-sized Grid row, let the ListCard column determine the row height, and stretch `OutputCard` to match it. Output overflow stays inside its scrolling viewport.

## Compose Paragraphs

Use `Paragraph` as a `Chunk`'s only body when the section is continuous multi-paragraph prose. Add any number of direct `TextBlock` children; each child is one paragraph.

`Paragraph` owns the gap between paragraphs and the visual first-line indentation equivalent to four standard spaces. Do not insert literal leading spaces or local margins. It always remains transparent and borderless, has no title or description, and exposes no visual variants. Use `Card` instead when the content is one paragraph.

Do not confuse the container with `FlourishTextRole.Paragraph`, which styles one `FlourishTextBlock`.

## Compose Presenters

Use `Presenter` as a full-width chunk body when rich visual content must be arranged with copy or supporting controls.

- `Title` and `Description` provide optional copy.
- `Body` contains controls or supporting content in the same region as the copy and is the default content property.
- `Presentation` contains an image, icon group, illustration, preview, or composed visual tree.
- `PresenterMode="Split"` places the presentation beside the copy. `PresenterPosition` accepts only `Left` or `Right` and names the presentation side.
- `PresenterMode="Overlay"` places the presentation behind copy and Body; `PresenterPosition` is ignored.
- Missing optional regions collapse with their spacing.
- Ordinary Presenter is transparent and borderless by default and occupies the full row.

Choose or compose Overlay presentation content that keeps text readable in both light and dark themes. `ChunkHero` uses the same Presenter fields and modes but supplies the larger, emphasized page-leading treatment.

## Use Button-family actions

The complete visual boundary of a button is interactive.

- Use `Button` for the default text action without an icon.
- Use `IconButton` when the action has an icon; omit Content only for an icon-only button.
- Use `CardButton` for a whole-card action with IconCard-like presentation.
- Use `WindowCaptionButton` only in a window caption or title-bar toolbar.

Give every icon-only button a visible tooltip and a meaningful `AutomationProperties.Name`. Use one `Filled` primary action per action group, lower-emphasis variants for supporting actions, and `Danger` for destructive actions.

## Structure control Gallery pages

Pages beneath the Gallery's Controls navigation node use one consistent learning sequence after the single `ChunkHero`:

1. `Variant`, when the demonstrated control exposes variants or another named mode users must choose between.
2. `Table`, when public members or named options benefit from a compact reference. Use Flourish `DataGrid` with native WPF columns.
3. One or more specific example chunks. Use `Example` only when there is one general example.
4. Topic-specific content chunks such as `Content`, `Alignment`, `Presentation`, `Selection`, or `Dismissal`.
5. `Usage`, connecting the visual control to its XAML host, command or event, service, and state ownership.
6. `Reference`, always last, with two peer `CardButton` links for the repository and canonical documentation.

Use the exact singular titles `Variant`, `Table`, `Usage`, and `Reference`. Omit inapplicable optional sections instead of inventing content. `Usage` and `Reference` are required. A hero preview does not replace an example when interaction is part of the control's contract.

Keep Table content selective rather than duplicating the generated API reference. Use a public member or option name in the first column and one short functional description in the second. Use purpose-built controls directly in the chunk body or within a Presenter; do not restore the removed Card Body pattern to contain demonstrations.

## Example

```xml
<ScrollViewer>
  <StackPanel>
    <flourish:ChunkHero
      Title="Synchronization"
      Description="Review and control workspace synchronization."
      Presentation="{StaticResource SynchronizationIllustration}" />

    <flourish:Chunk
      Title="Settings"
      Description="Changes apply immediately and their results appear in the output history.">
      <Grid>
        <Grid.ColumnDefinitions>
          <ColumnDefinition Width="*" />
          <ColumnDefinition Width="16" />
          <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <StackPanel>
          <flourish:ListCard
            Icon="&#xE895;"
            Title="Sync mode"
            MainText="Choose when workspace changes synchronize.">
            <flourish:FlourishComboBox
              Width="160"
              ItemsSource="{Binding SynchronizationModes}"
              SelectedItem="{Binding SynchronizationMode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
          </flourish:ListCard>
          <flourish:ListCard
            Margin="{DynamicResource FlourishListCardPeerMargin}"
            Icon="&#xE72C;"
            Title="Refresh now"
            MainText="Request a new synchronization pass.">
            <flourish:Button Click="Refresh_Click" Content="Refresh" />
          </flourish:ListCard>
        </StackPanel>

        <flourish:OutputCard
          x:Name="SynchronizationOutput"
          Grid.Column="2"
          AutomationProperties.Name="Synchronization output"
          VerticalAlignment="Stretch" />
      </Grid>
    </flourish:Chunk>
  </StackPanel>
</ScrollViewer>
```

Append every observable outcome instead of replacing history:

```csharp
private void Refresh_Click(object sender, RoutedEventArgs e)
{
    SynchronizationOutput.WriteLine("Synchronization requested.");
    SynchronizationOutput.WriteLine("Synchronization completed.");
}
```

## Validate before delivery

- Confirm exactly one `ChunkHero` leads the page and every other page element belongs to a full-width `Chunk`.
- Confirm every Chunk has a concise `Title` and real `Body`; descriptions are optional and omitted when redundant.
- Confirm empty optional regions leave no placeholder or spacing.
- Confirm unspecified typography uses Standard and specialized tiers follow their assigned roles.
- Confirm Card-family controls use `MainText` and never attempt a general `Body`.
- Confirm one paragraph uses Card, several paragraphs use Paragraph, one icon uses IconCard, and images or composed visuals use Presenter.
- Confirm Paragraph is the chunk's only body and owns paragraph gaps and indentation.
- Confirm Presenter is full-width, uses only Left or Right in Split, and preserves Overlay readability in both themes.
- Confirm each ListCard uses one `ActionBody` control, immediate application, Standard variant, and compact peer spacing.
- Confirm output is appended through `WriteLine`, uses no title or body, and scrolls without driving adjacent layout height.
- Confirm the complete interactive surface uses the correct Button-family member and icon-only actions have accessible names and tooltips.
- Confirm card grids use consistent row and column gaps and peer cards have compatible arranged heights.
- Confirm Gallery control pages follow the applicable `Variant`, `Table`, examples, topic-specific content, `Usage`, and final `Reference` sequence.
- Recommend manual checks for light and dark themes, keyboard focus order, enlarged or localized text, collapsed optional regions, Split and Overlay layouts, and output scrolling.
