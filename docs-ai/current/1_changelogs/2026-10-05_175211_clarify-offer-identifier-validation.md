# Offer identifier validation boundary

This clarifies the identifier sentence in the [display extraction record](2026-10-05_174832_classify-components-and-extract-display-scenes.md). Rendering rejects duplicate OfferCard.Id values within the same OfferStage. OfferStage.Id is not checked against every other element in a document. Consumers must still supply document-unique card and stage IDs for native fragment navigation. The browser controller leaves absent or duplicate card targets within its stage unenhanced and readable.

No code or verification result changes. The source-owned API guide and the 166 component, 220 Colligere and 95 package-consumer results retain their stated scope.
