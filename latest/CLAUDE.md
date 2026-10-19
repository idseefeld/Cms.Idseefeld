# CLAUDE.md

Referenz: [../idseefeld-ai-workspace/CLAUDE.md](../idseefeld-ai-workspace/CLAUDE.md)

`idseefeld-ai-workspace` ist die Basis für alle neuen Kundenprojekte. Diese Datei ergänzt diese Basis nur um den lokalen Kontext für `cms.idseefeld` im Ordner `latest/`.

## Projektkontext

- Solution: `Cms.Idseefeld.sln`
- Zielplattform: `.NET 10`
- Web-Stack: ASP.NET Core mit Razor Pages
- CMS: Umbraco
- Zweck: Website für neu zu gründendes Unternehmen *ContorDigital*

## Arbeitskontext

- Beziehe die Basisregeln aus `../idseefeld-ai-workspace/CLAUDE.md` immer mit ein.
- Priorisiere bei Web-Änderungen Razor Pages und die bestehende Umbraco-Struktur dieses Projekts.
- Nimm möglichst kleine, gezielte Änderungen vor und orientiere dich an bestehenden Mustern im Repository.

## Konfiguration

- Entwicklungsgeheimnisse nicht im Repository ablegen.
- SMTP-Passwort wird lokal über User Secrets verwaltet.
- Änderungen im Ordner `latest/` sollen diese Datei aktuell halten, wenn sich Struktur oder Arbeitsregeln ändern.

## Regeln

- Beziehe die Basisregeln aus `../idseefeld-ai-workspace/CLAUDE.md` immer mit ein.