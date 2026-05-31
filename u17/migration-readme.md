# Migration from Version 13.13.1 with Legacy Grid to Version 17.4.2 with Block Grid

## My First Approach

- Dependency to `Our.Umbraco.DictionaryLocalizer` has been removed, because it currently does not support version 17+ and is not neccessary any more.
- removed second language (English).
- created BlockGrid and the necessary data types and document types for the new block grid.
- copy pasted all content from the old grid to the new block grid, because there is no migration path from legacy grid to block grid. uSync.Migration works in a way. But in the end I found it more efficient to do it manually, because there are only a few pages with the grid and I had to do some adjustments to the content anyway.
- upgraded to Umbraco 17.4.2 and updated all dependencies to the latest versions.
- temporarily removed dependency to project `Cms.Idseefeld.Core` because the generated model are not supported by version 17.
- excluded all views to get the backoffice compiled and run.
- generate new model with *ModelsBuilder*.
- Added dependency to project `Cms.Idseefeld.Core` and fix or removed legacy code.
- included all views again and adjusted them to the new block grid and removed legacy code.
- removed `MacroPartials` and replaced them with `Partials` because the new block grid does not support macros anymore.

## Second Step

- added package `Umbraco.Community.WysiwgUmbracoCommunityExtensions` to get the new WYSIWYG editor in the block grid working.
- ToDo: transfer the block grid content