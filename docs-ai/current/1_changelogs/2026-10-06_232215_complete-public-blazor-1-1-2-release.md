# Complete public Blazor 1.1.2 release

Source commit 108fbff8950925b909a70b0b9830226ae8226521 and the new v1.1.2 tag were pushed after the complete preparation gate. GitHub run 37560362408 passed its build and Trusted Publishing jobs; all six manifest packages returned Created. Public indexes contain 1.1.2, and four fresh-cache consumers using only NuGet.org passed 129 checks. Essential remains 1.3.0, with no package ID, dependency-order or old-tag change.

Evidence is retained in artifacts/release-1.1.2-preparation-final.log, artifacts/public-release-1.1.2.log and artifacts/package-consumers/b06a15d2f8fc4661be3792ff26ebf464. Earlier indexing-pending and old-HTTP-index restore failures are separate logs, not claimed passes. Temporary verification output was cleaned without removing Debug/Release, source regressions, historical release evidence or human documentation.

This record supersedes the earlier publication deferral for the included source repairs. Consumers can now upgrade their two central Flourish versions together; real rendered/event tests and user-operated browser checks remain necessary. No Colligere deployment or Aspire restart is part of this publication.
