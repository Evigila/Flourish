# Space editable avatars and size grids to content

Recorded 2026-11-09 12:50:25 America/Sao_Paulo (UTC-03).

An editable Avatar followed by another control in a Section now reserves 24px below its own content. The host controls portrait diameter through the existing Size parameter. The crop dialog and image processing are unchanged.

UniformGrid ContentHeight is an opt-in rectangle layout for cards with labels or descriptions that may wrap. It sizes rows from their content and removes the cell aspect ratio and height cap. It cannot be combined with CellHeight or Square. Colligere's service cards use this mode and the continuous icon layout so the icon and availability text have room to render at narrow widths.
