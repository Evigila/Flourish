# Add the Record list section heading

Record list rendered its DataTable directly beneath PageHeading without a Section. DataTable.Label supplies accessible labeling and a hidden caption, so the existing Demo records text was not a visible subsection heading.

Wrapped the existing table in the production Section with the existing localized Records_Label: Demo records / 演示记录 / Registros de demonstração. The stable section ID `records-list` supplies the standard heading ID and aria-labelledby relationship. The page gutters apply once to the Section; no custom heading, spacing style, table contract or record/search/edit/delete callback was introduced or changed.

Updated the existing Gallery page case to require this H2 through language changes, and added a concise three-language 1.1.4-preview release note.

Verification: Gallery Release build passed with zero warnings and errors; 7,828 Gallery checks passed, including the actual Records H2 in three cultures and 211 ChangeLog checks. Culture integrity passed 22,072 checks; ChangeLog validation passed 79 checks; diff whitespace is clean. No Computer Use or browser visual testing was performed.

Manual acceptance: open Record list and verify the first subsection heading appears above the table with the same typography and gutters as other example sections; switch languages and narrow the viewport; confirm query filtering, row navigation, creation, editing and delete confirmation still behave as before.
