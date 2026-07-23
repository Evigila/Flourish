---
name: creating-page
description: Create or refactor Flourish WPF pages with a PageBody root, one leading HeaderChunk, full-width Chunk sections, merged Button and Card contracts, ActionCard actions, Document and Paragraph prose, CodeSpace snippets, Presenter compositions, DataGrid boundary scrolling, and Overlay content. Use when adding or editing page XAML, reviewing page information architecture, choosing layout or content controls, or validating a Flourish page before delivery.
---

# Creating Flourish Pages

Build every content page as one explicit hierarchy: one page-leading header, several full-width sections, purpose-built content controls, and clearly bounded actions. Keep structural copy concise and let each control perform only its documented role.

## Build the page hierarchy

1. Use `flourish:PageBody` as the page's root content container. It owns the Flourish scroll viewer, vertical stack, and standard page margin.
2. Place only `HeaderChunk` and `Chunk` as direct children of `PageBody`.
3. Start with exactly one `HeaderChunk`. It must be the first direct child; never omit it, move it below a chunk, or add another one.
4. Follow the header with one or more `Chunk` controls.
5. Keep every `HeaderChunk` and `Chunk` full-width and on its own row.
6. Put headings, cards, documents, presenters, actions, and custom layouts inside a chunk region rather than beside chunks at the `PageBody` level.
7. Give each distinct topic or task its own `Chunk`. Arrange related peer controls within one chunk body only when they share that section topic.
8. End every Gallery page with exactly one `Chunk Title="Reference"`. Its two peer CardButtons point to the repository and the most relevant canonical documentation; keep them disabled while Gallery does not provide external navigation.

Every `Chunk` has these semantic fields:

- `Title` is required and names the section's subject.
- `Content` is optional supporting copy. Add it only when the title cannot communicate essential context by itself.
- `Body` is required and contains the actual content control or layout tree. It is the default XAML content property.

Empty or `null` optional regions must collapse together with their spacing. Keep the default large gap between chunks and between `HeaderChunk` and the first ordinary chunk.

`HeaderChunk` follows the Presenter contract. Every declaration explicitly supplies `Title`, `Content`, `PresenterMode`, and `PresenterPosition`; the title uses the page-header role. Unlike an ordinary Presenter, `Body` is the default XAML content property for `HeaderChunk`. Assign `Presentation` through an explicit `HeaderChunk.Presentation` property element when the header includes a presented visual. HeaderChunk always occupies one complete row, including in TopDown mode; never put it in a multi-column layout.

## Use the typography contract

Flourish defines six size tiers. When no tier is explicitly selected, use `Standard`.

- `Small`: navigation group labels, OutputCard output, and compact control-owned caption or status text.
- `Standard`: ordinary body and control text, including every unspecified case.
- `Icon`: general icon glyphs; a specialized icon control may apply its own local correction.
- `Large`: card-title emphasis and every `Paragraph` inside a `Document`.
- `ExtraLarge`: section-title family, including `Chunk.Title`.
- `HeaderSize`: `HeaderChunk.Title` only.

Do not select a larger tier merely to make text look more important. Select the control or text role that expresses the correct hierarchy.

## Choose the content control

Use the smallest control whose semantic contract matches the content:

- Use `Card` for an optional title, one optional paragraph in `Content`, and at most one optional icon.
- Use `Document` for continuous multi-paragraph prose composed from direct `Paragraph` children.
- Use `CodeSpace` for exact source or command text that readers may copy.
- Use `Presenter` for an image, several icons, an illustration, a preview, or other composed presentation content.
- Use `ActionCard` for one setting or local action with a single interactive body.
- Use `OutputCard` for append-only raw messages, logs, progress, completed results, and failures.
- Use `CardButton` when the complete card-shaped surface is one action.
- Use `Overlay` to host floating content, normally a vertical ActionCard or a deliberate custom layout.

One surface communicates one subject or behavior. Split unrelated subjects into peer controls instead of building a large nested surface.

## Compose Cards

`Card` is a terminal presentation surface. It does not have a general-purpose `Body` and must not host arbitrary nested content.

- `Title`, `Content`, and `Icon` are independently optional. Each missing field collapses together with its spacing.
- `Content` contains one paragraph only.
- `Icon` contains exactly one semantic icon-font glyph, not an image, icon group, or arbitrary content tree.
- `IconPosition` uses `Dock.Left`, `Top`, `Right`, or `Bottom`; the default is `Left`.
- `Variant` may be `Standard`, `Tonal`, `Filled`, or `Elevated` according to surface emphasis.
- Peer Cards may form two or more columns when the available width keeps copy readable.

Use Presenter for an image, several icons, or composed visual content. Do not attach click, command, hover-trigger, or popup-trigger behavior to a non-interactive Card. Use `CardButton` when the entire surface acts as one control, or `ActionCard` when only its body is interactive.

## Compose ActionCards

Use `ActionCard` for one independent setting or local action. Its optional `Icon`, `Title`, and `Content` regions collapse independently. Its `Body` holds exactly one interactive control and is the default XAML content property.

- `Variant="Horizontal"` is the default. It places Icon on the left, vertically stacked Title and Content in the middle, and Body on the right. The row is vertically centered.
- `Variant="Vertical"` stacks Icon, Title, Content, and Body from top to bottom. Every region is left-aligned, including the Body at the bottom.
- Keep Title and Content concise. Use the Body for one `FlourishComboBox`, `FlourishCheckBox`, `FlourishTextBox`, `FlourishRadioButton`, or Button as appropriate.
- Apply selections, toggles, and edits immediately. Do not add a separate Apply action unless the workflow itself requires transactional confirmation.
- Stack related ActionCards in their own column and use `FlourishActionCardPeerMargin` only between consecutive cards.

An adjacent column may contain an `OutputCard`. Put the complete ActionCard column and OutputCard in one auto-sized Grid row, let the ActionCard column determine the row height, and stretch OutputCard to match it. Output overflow stays inside its scrolling viewport.

## Compose Documents

Use `Document` as a `Chunk`'s only body when the section is continuous multi-paragraph prose. Add one or more direct `Paragraph` children; each child is a normalized text element and represents exactly one paragraph. Set its copy through the `Text` property. Do not add raw TextBlocks or other controls to a Document.

Document owns the gap between paragraphs and the visual first-line indentation equivalent to four standard spaces. Do not insert literal leading spaces or local item margins. It uses a transparent background, a rounded thin low-contrast border, and the standard outer separation from Chunk Title and Content. Preserve those values. Every Paragraph renders at the `Large` tier with regular body treatment; do not override its font size. Document has no title, supporting-copy field, or visual variants. Use Card when the copy is only one paragraph.

## Compose CodeSpaces

Use `CodeSpace` for one exact source-code or command-text value. Assign the complete snippet explicitly through `Text`; CodeSpace is not a content container and must not receive child controls.

- Preserve source whitespace and line endings. Do not add Document-style indentation or rewrite the value before display or copy.
- Keep the built-in transparent background, rounded thin low-contrast border, padding, and outer separation from Chunk copy.
- Keep the temporary fixed presentation: the effective Large tier, Normal style, Bold weight, Consolas family, and adaptive blue foreground.
- Treat this presentation as a placeholder for future syntax highlighting. Do not create per-language token colors or a language-selection API yet.
- Use the built-in upper-right copy action. Do not wrap CodeSpace with a second copy button.
- Keep the copy tooltip on the shared Tip typography; it remains Normal and Regular instead of inheriting the Bold code presentation.
- Expect long lines to remain unwrapped and scroll horizontally. The copy action always targets the complete `Text` value and is disabled when it is empty.

## Compose Presenters

Use `Presenter` as a chunk body when rich visual content must be arranged with copy or supporting controls. Split and Overlay are full-width; TopDown may use the documented multi-column exception.

- `Title` and `Content` provide required copy and are explicitly declared.
- `Body` contains controls or supporting content in the same region as the copy. Assign it only through an explicit `Presenter.Body` property element.
- `Presentation` contains an image, icon group, illustration, preview, or composed visual tree and is the default XAML content property.
- Every declaration explicitly sets `PresenterMode` and `PresenterPosition`. Runtime fallback values are `Split` and `Left`, but they do not replace the authoring requirement.
- `PresenterMode="Split" PresenterPosition="Left"` is the default horizontal layout: Presentation on the left, with Title, Content, and Body together on the right.
- `PresenterMode="Split" PresenterPosition="Right"` reverses the two regions without changing their internal structure or left alignment.
- `PresenterMode="Overlay"` places Presentation behind Title, Content, and Body. Position does not change the visual result.
- `PresenterMode="TopDown"` places Presentation across the top, then places Title, Content, and Body below it as one left-aligned group. Position does not change the visual result.
- A missing Body collapses with its spacing.
- Split and Overlay Presenters occupy the full row and must not share a multi-column layout. A TopDown Presenter may be placed in a multi-column layout when each cell remains readable; this is the only Presenter mode that may share a row.
- Keep the copy-and-body region transparent and aligned as one unit. Only the Presentation region uses the adaptive light-neutral background and shared surface corner radius. Presentation content fills the available region while its own content remains centered.
- When several Presenters are stacked vertically in one section, apply `FlourishPresenterPeerMargin` only to each Presenter after the first.

Do not add an external Grid or local margins to reposition copy, Body, or Presentation inside one Presenter; the Presenter template owns those regions. A parent grid may arrange peer TopDown Presenters into columns. Choose Overlay presentation content that keeps text readable in both light and dark themes. `HeaderChunk` exposes the same fields and modes but supplies the larger, emphasized page-leading treatment. Its independent Split fallback remains `Right`, keeping copy on the left and Presentation on the right. HeaderChunk never receives the TopDown multi-column exception.

## Use Button-family actions

The complete visual boundary of a button is interactive.

- Use `Button` for ordinary actions. `Icon` and `Content` are independently optional and each empty region collapses completely. Supply both for an icon-and-text action, or omit Content for an icon-only action.
- Use `CardButton` when a complete card-shaped surface is one action. It supports optional `Title`, `Content`, and `Icon`; each missing field collapses. Choose the Card surface variants `Standard`, `Tonal`, `Filled`, or `Elevated` according to emphasis.
- Use `WindowCaptionButton` only in a window caption or title-bar toolbar.

Do not intentionally create an empty Button or CardButton even though their visual fields are nullable. Give every icon-only Button a visible tooltip and a meaningful `AutomationProperties.Name`. Use one Filled primary action per action group, lower-emphasis variants for supporting actions, and Danger for destructive actions. When only one small region of a card should be interactive, put the control in an ActionCard Body instead of making the whole surface clickable.

## Preserve DataGrid page scrolling

Use Flourish `DataGrid` directly in a chunk layout. Its internal viewport consumes the mouse wheel while it can scroll in that direction. At the top or bottom boundary, the wheel event continues to the surrounding `PageBody`, so hovering the grid never traps page navigation.

Do not wrap DataGrid in another wheel-handling ScrollViewer or add a preview-wheel handler that marks every event handled. When changing its template or hosting layout, manually verify both internal scrolling and boundary handoff in each direction.

## Compose Overlays

Use `Overlay` as the floating-layer container, not as a replacement for page structure.

- Prefer one `ActionCard Variant="Vertical"` as the floating view. It gives a popup a clear Icon, Title, Content, and bottom action while keeping the layout left-aligned.
- A custom layout is allowed when the floating view has a distinct composition, such as a Profile view. Keep its hierarchy intentional rather than forcing unrelated controls into an ActionCard.
- Preserve the Overlay's dismissal, focus, and placement behavior. Put those responsibilities on Overlay and keep its hosted content focused on presentation and interaction.

## Structure control Gallery pages

Pages beneath the Gallery's Controls navigation node use one consistent learning sequence after the single `HeaderChunk`:

Give each public control its own Gallery page. Combine named variants or modes of that control on the same page, but do not combine sibling controls merely because they share a family or base type. For example, `Button`, `CardButton`, and `WindowCaptionButton` each use a dedicated page.

The Controls parent page is a navigation overview, not a control demonstration. After its HeaderChunk, give it exactly one control-navigation Chunk containing a complete grid of CardButtons that navigate to every control page, followed only by the final Reference Chunk. Reuse the same icon glyph registered for each navigation route so the overview and navigation tree stay synchronized.

1. `Variant`, when the demonstrated control exposes variants or another named mode users must choose between.
2. `Table`, when public members or named options benefit from a compact reference. Use Flourish `DataGrid` with native WPF columns.
3. One or more specific example chunks. Use `Example` only when there is one general example.
4. Topic-specific content chunks such as `Content`, `Alignment`, `Presentation`, `Selection`, or `Dismissal`.
5. `Usage`, containing one `CodeSpace` with a concrete XAML declaration, C# configuration call, or runtime invocation. Put explanatory prose in an earlier example or topic-specific Chunk, never beside the snippet in Usage.
6. `Reference`, always last, with two peer `CardButton` links for the repository and canonical documentation.

Use the exact singular titles `Variant`, `Table`, `Usage`, and `Reference`. Omit inapplicable optional sections instead of inventing content. Usage and Reference are required. A header preview does not replace an example when interaction is part of the control's contract.

On every Gallery page, ordinary `PresenterMode="Split"` examples explicitly use `PresenterPosition="Left"` so Presentation remains on the left and Title, Content, and Body remain together on the right. This Gallery authoring default does not change HeaderChunk: its Split default continues to place copy on the left and Presentation on the right.

Keep Table content selective rather than duplicating the generated API reference. Use a public member or option name in the first column and one short functional summary in the second. Use purpose-built controls directly in the chunk body or within a Presenter; do not put arbitrary demonstrations inside a terminal Card.

In a `Variant` Chunk, normally give each named variant its own `PresenterMode="TopDown"` Presenter cell. Put only that variant's live example in `Presentation`, put its explanation in `Title` and `Content` below the presentation region, and leave `Body` unset. Arrange these TopDown Presenters in two or more columns when the available width remains readable. `WindowCaptionButton` is the exception: keep its complete caption group in one Split Presenter with `PresenterPosition="Left"` so the sample is on the left and the explanatory copy is on the right.

Every multi-column Chunk uses one shared row layout that stretches all peer cells to the same arranged height. Prefer `UniformGrid` for equivalent peers. Do not use independent vertical stacks as columns when their heights can diverge. In Variant grids, give every `Presentation` example in the same row the same meaningful minimum height, with enough room around the demonstrated control.

Use adaptive color to enrich semantic icons without coloring ordinary copy. Navigation glyphs and Card, ActionCard, and CardButton icons use the primary foreground on neutral surfaces. Filled surfaces use their on-primary foreground, and disabled interactive cards use the disabled foreground.

## Example

```xml
<flourish:PageBody>
  <flourish:HeaderChunk
    Title="Synchronization"
    Content="Review and control workspace synchronization."
    PresenterMode="Split"
    PresenterPosition="Right">
    <flourish:HeaderChunk.Presentation>
      <ContentControl Content="{StaticResource SynchronizationIllustration}" />
    </flourish:HeaderChunk.Presentation>
  </flourish:HeaderChunk>

  <flourish:Chunk
    Title="Settings"
    Content="Changes apply immediately and their results appear in the output history.">
    <Grid>
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="16" />
        <ColumnDefinition Width="*" />
      </Grid.ColumnDefinitions>

      <StackPanel>
        <flourish:ActionCard
          Icon="&#xE895;"
          Title="Sync mode"
          Content="Choose when workspace changes synchronize."
          Variant="Horizontal">
          <flourish:FlourishComboBox
            Width="160"
            ItemsSource="{Binding SynchronizationModes}"
            SelectedItem="{Binding SynchronizationMode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
        </flourish:ActionCard>
        <flourish:ActionCard
          Margin="{DynamicResource FlourishActionCardPeerMargin}"
          Icon="&#xE72C;"
          Title="Refresh now"
          Content="Request a new synchronization pass."
          Variant="Horizontal">
          <flourish:Button Click="Refresh_Click" Content="Refresh" />
        </flourish:ActionCard>
      </StackPanel>

      <flourish:OutputCard
        x:Name="SynchronizationOutput"
        Grid.Column="2"
        AutomationProperties.Name="Synchronization output"
        VerticalAlignment="Stretch" />
    </Grid>
  </flourish:Chunk>
</flourish:PageBody>
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

- Confirm `PageBody` is the root content container and its direct children are only HeaderChunk or Chunk.
- Confirm exactly one HeaderChunk is the first direct child and every other page element belongs inside it or a full-width Chunk.
- Confirm every Chunk has a concise Title and real Body; Content is optional and omitted when redundant.
- Confirm empty optional regions leave no placeholder or spacing.
- Confirm unspecified typography uses Standard and specialized tiers follow their assigned roles.
- Confirm Card uses only optional Title, Content, and one Icon, and never receives a general Body.
- Confirm Button uses optional Content and Icon; confirm icon-only actions have accessible names and tooltips.
- Confirm a single paragraph uses Card, multi-paragraph prose uses a Document of Paragraph children, exact copyable code uses CodeSpace, and images or composed visuals use Presenter.
- Confirm Document is the chunk's only body, uses Large Paragraph text, and owns its border, outer separation, paragraph gaps, and indentation.
- Confirm every Presenter and HeaderChunk explicitly declares Title, Content, PresenterMode, and PresenterPosition.
- Confirm Split Right places copy plus Body on the left and Presentation on the right; Split Left reverses only the two regions.
- Confirm TopDown fills the upper Presentation region and keeps Title, Content, and Body left-aligned below it.
- Confirm Split and Overlay Presenters occupy a complete row; only TopDown Presenters may share a multi-column row.
- Confirm HeaderChunk always occupies a complete row, even when it uses TopDown.
- Confirm Presentation receives direct XAML content, Body is assigned explicitly, and Overlay mode preserves readability in both themes.
- Confirm vertically stacked Presenters use `FlourishPresenterPeerMargin` after the first item.
- Confirm each ActionCard has at most one Body control; Horizontal centers the row and Vertical left-aligns the complete stack.
- Confirm CardButton is used only when the whole card is interactive; use ActionCard when interaction belongs to one contained control.
- Confirm DataGrid scrolls internally before handing the wheel to PageBody at both vertical boundaries.
- Confirm Overlay normally hosts a vertical ActionCard and custom content remains a deliberate, accessible layout.
- Confirm output is appended through `WriteLine`, uses no title or body, and scrolls without driving adjacent layout height.
- Confirm card grids use consistent row and column gaps and peer cards have compatible arranged heights.
- Confirm Gallery control pages follow the applicable Variant, Table, examples, topic-specific content, Usage, and final Reference sequence.
- Confirm every Gallery page ends with one Reference Chunk containing two disabled CardButtons.
- Confirm each Gallery page demonstrates one public control only; keep variants together but split sibling controls into dedicated pages.
- Confirm each Variant Chunk uses one Body-free TopDown Presenter per variant in a readable grid, except the Split Left WindowCaptionButton group.
- Confirm the Controls parent has one control-navigation Chunk whose CardButtons cover every registered control route and reuse its icon, followed only by Reference.
- Confirm every ordinary Split Presenter in Gallery uses PresenterPosition Left; do not apply that rule to HeaderChunk.
- Confirm every control Usage Chunk contains one CodeSpace and no prose control.
- Confirm multi-column peers stretch to one row height and Variant Presentation examples share a sufficient minimum height.
- Confirm navigation and card icons use adaptive semantic foregrounds while ordinary copy remains neutral.
- Recommend manual checks for light and dark themes, keyboard focus order, enlarged or localized text, collapsed optional regions, Document and CodeSpace surfaces, CodeSpace copying, all Presenter modes, both ActionCard variants, DataGrid boundary scrolling, Overlay dismissal, and output scrolling.
