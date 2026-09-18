# Offline usability and save recovery improvements

## Shop

- Selected equipment gets a gold outline. Selection follows the actual owned item, so inventory changes cannot silently select a different item at the old index.
- Sell/combine actions appear after selecting equipment. Disabled actions explain `EŞİ YOK`, `SON SEVİYE` or `SON SİLAH`.
- Item page controls appear only when multiple pages exist; the heading shows the current page.
- The next-wave button shows its wave number and a boss warning using the same schedule as enemy spawning.
- Label lookups include inactive children, allowing hidden shop buttons to refresh safely during phase changes.

## Resume clarity

The lobby's continue button identifies the saved character and wave. Character selection also identifies the existing run so a new selection cannot be confused with its saved character. Save/storage errors use Turkish.

## Recovery fix

Previously, saving after fallback recovery could replace the healthy backup with the corrupt primary. Atomic replacement now updates the backup only when the primary decodes successfully. A healthy recovery backup survives replacement of a corrupt primary.

Regression checks exercise saving after recovery, preservation of the healthy backup, and another primary-file failure. Tests use temporary directories and do not touch real player saves.

## Validation limits

Runtime and Editor C# compilation and 157 model checks passed without Unity. The final UI changes still require in-Editor and device visual/input checks. No Editor, Play/Stop, build or phone session was launched.
