# Frontend-Bearbeitung von Paragraphen im Block Grid

## Ziel und Ansatz

Angemeldete und berechtigte Umbraco-Backoffice-Benutzer können auf einer
Website-Seite einen Bearbeitungsmodus aktivieren und alle Rich-Text-Felder
der Paragraph-Blockelemente gemeinsam bearbeiten. Die Änderungen werden
über die Umbraco Management API als Seitenentwurf gespeichert oder auf
Wunsch veröffentlicht.

Ausgangspunkt sind die bestehende Paragraph-Partial
`Cms.Idseefeld.Web/Views/Partials/blockgrid/wysiwg65/wysiwg65_paragraph.cshtml`,
das Element `wysiwg65_paragraph` mit dem Feld `text` und die
Block-Grid-Eigenschaft `blockGrid`.

## Umsetzungsschritte

1. Authentifizierung und Autorisierung für Umbraco 18 prüfen: Zugang nur für
   angemeldete Backoffice-Benutzer mit den erforderlichen Rechten für die
   betreffende Seite und Aktion. Den unterstützten Zugriff auf die Management
   API ermitteln, ohne privilegierte Zugangsdaten im Browser offenzulegen.
2. Einen Bearbeiten-Button und eine Razor-Partial für den Editor in die
   bestehende Seitendarstellung integrieren. Alle Paragraph-Blöcke der Seite,
   einschließlich verschachtelter und leerer Blöcke, editierbar machen.
3. Den Rich-Text-Funktionsumfang an der bestehenden Datentyp-Konfiguration
   `wysiwg65_ParagaphRTE` orientieren und vorhandene Inhalte und
   Formatierungen beim Bearbeiten erhalten.
4. Vorhandene unveröffentlichte Seitenänderungen erkennen und in diesem Fall
   die Frontend-Bearbeitung sperren; auf das Backoffice verweisen.
5. Alle geänderten Paragraphen gemeinsam im aktuellen Block Grid der Seite
   über die Management API speichern. Andere Blöcke, Einstellungen,
   Eigenschaften und nicht bearbeitete Inhalte müssen erhalten bleiben.
   Der Benutzer wählt zwischen Entwurf speichern und Veröffentlichung.
6. Konkurrierende Änderungen erkennen und Überschreiben verhindern. Hierzu
   die tatsächlich verfügbaren Versions- und Konfliktmechanismen der
   Management API prüfen; falls diese nicht ausreichen, keine unsichere
   Speichergarantie annehmen. CSRF-Schutz sowie sichtbare Fehler- und
   Erfolgsmeldungen vorsehen. Bei fehlgeschlagener Veröffentlichung nach
   erfolgreichem Speichern den tatsächlichen Entwurfsstatus anzeigen.
7. Mit zielgerichteten Tests und einem Build die Darstellung, verschachtelte
   und leere Paragraphen, Formatierung, Berechtigungen, Entwurfssperre,
   Konflikte und beide Speicherwege validieren. Die unveränderte öffentliche
   Darstellung prüfen.

## Hinweise und Grenzen

- Bearbeitet werden ausschließlich Rich-Text-Felder von
  `wysiwg65_paragraph`-Elementen im Block Grid der aktuellen Seite, nicht
  andere Rich-Text-Felder oder die Blockstruktur.
- Veröffentlichen betrifft die Seite und nicht nur einzelne Paragraphen.
- Die genaue Authentifizierung, das API-Datenformat und sichere
  Konfliktverfahren müssen anhand der tatsächlich vorhandenen
  Umbraco-18-Management-API vor der Implementierung verifiziert werden.
