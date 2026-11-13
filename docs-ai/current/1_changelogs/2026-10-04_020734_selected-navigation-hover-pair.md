# Selected navigation hover pair

A final review of the restored route-link/disclosure pair found that the generic link hover rule could recolor only one half of an already selected row. Design now applies the same accent foreground and hover background to both halves when that selected row is hovered. Selection remains derived from the route, independently of expansion; native link/button focus targets remain separate.

Gallery was rebuilt successfully with zero warnings/errors. CSS bundle verification and git diff --check passed. This is a style-only follow-up to workspace-controls-and-typography-alignment; browser pointer appearance remains in the manual acceptance checklist. No Git commit or browser test was performed.