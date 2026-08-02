# Heading 1

## Heading 2

### Heading 3

#### Heading 4

##### Heading 5

###### Heading 6

Plain paragraph text renders at normal weight — only markdown syntax applies formatting.

**Bold via `**` markers.** *Italic via `*` markers.* *Italic also via `_` markers.* ~~Strikethrough via `~~` markers.~~ ***Bold and italic together via `***` markers.***

Inline `code snippets` render in monospace.

Escaping: \*not italic\*, \`not code\`, \[not a link\], \_not italic\_, \~not strikethrough\~, literal backslash \\ works.

Explicit link: [AniList](https://anilist.co). Reference link: [MangaDex][md] and short-form [md][]. Autolink: <https://myanimelist.net> renders as a bare URL, as does <https://mangadex.org/title/xyz/berserk> (with punctuation trimmed).

[md]: https://mangadex.org

Here's a footnote reference[^1] and another one[^bur].

[^1]: This is the first footnote text.
[^bur]: Berserk was authored by Kentaro Miura until his death in 2021.

---

## Lists

Bullet list:

- First bullet
- Second **with bold**
  - Nested bullet (2-space indent → ◦ marker)
  - Another nested item
- Third bullet with `code`

* Alternate marker `*` also works

Numbered list:

1. First item
2. Second item ~~strikethrough~~
   1. Nested numbered item
   2. Another nested one
3. Third item with a [link](https://example.com)

## Blockquotes

> This is a blockquote in italic muted gray with a `┃` vertical bar.
> Second line — each is its own quote.

## Fenced code block

```
Line 1 of code — indent preserved
    Line 2 with extra spaces
{ "id": 42, "title": "Berserk" }
```

***

## Real-world sample

**Berserk** notes:

- Currently on *indefinite hiatus* since [Miura's death](https://myanimelist.net/manga/2/Berserk)
- Grab the `Deluxe Edition` hardcovers if you can find them
- ~~Waiting for Vol 43~~ — released 2024-06[^1]
- Author was **Kentaro Miura**[^bur]

> "Do not depend on others. This is a battle you must fight alone."

```
ISBN: 978-1506742793
Publisher: Dark Horse
Format: Deluxe Hardcover
```

Reference-style link to the source[md] appears above too. Autolink at end of sentence: <https://anilist.co/manga/30002>.

---
