Wiki pages are stored as reviewable Markdown sources under `docs/wiki` in the application repository. Published pages live in GitHub's separate Wiki Git repository. Keep the sources and published pages together when updating documentation.

## First publication

GitHub requires an initial page to be saved through the repository's Wiki tab before its Git repository can be cloned. Save a temporary **Home** page first. [GitHub's editing guide](https://docs.github.com/en/communities/documenting-your-project-with-wikis/adding-or-editing-wiki-pages) describes this setup and the Git workflow.

## Publish a reviewed update

Run these commands from the application repository in PowerShell with Git installed:

```powershell
git clone https://github.com/SoraRS/osrs-world-ping.wiki.git ../osrs-world-ping.wiki
Copy-Item docs/wiki/*.md ../osrs-world-ping.wiki/
Copy-Item LICENSE ../osrs-world-ping.wiki/
git -C ../osrs-world-ping.wiki status --short
git -C ../osrs-world-ping.wiki add -- *.md LICENSE
git -C ../osrs-world-ping.wiki diff --cached
git -C ../osrs-world-ping.wiki commit -m "Update project documentation"
git -C ../osrs-world-ping.wiki push
```

For an existing Wiki clone, pull its current branch before copying files. If a page was edited online, bring that edit back into `docs/wiki` before publishing so it is not overwritten. Review the diff before committing. The copy step preserves unrelated Wiki pages. Changes become visible when pushed to the Wiki's default branch.

## Navigation and links

- `Home.md` is the grouped contents page.
- `_Sidebar.md` groups pages by users, help, development, and releases.
- `_Footer.md` links to the project, support, and privacy page.
- Use stable page names and update Home/sidebar links when adding or renaming a page.
- Keep the main README short. Link to the Wiki for setup, detailed behavior, troubleshooting, and technical references.
- The old `docs/*.md` entry points remain short links to the related Wiki/source pages, so existing links and release-workflow instructions still lead to the guide.
