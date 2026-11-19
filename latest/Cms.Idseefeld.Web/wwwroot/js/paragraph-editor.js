(() => {
    const panel = document.getElementById("paragraph-editor");
    const paragraphs = [...document.querySelectorAll("[data-paragraph-key]")];
    if (!panel || paragraphs.length === 0) return;

    const startButton = panel.querySelector('[data-action="start"]');
    const controls = panel.querySelector("[data-editor-controls]");
    const message = panel.querySelector("[data-editor-message]");
    const sourceButton = panel.querySelector('[data-action="source"]');
    const endpoint = `/umbraco/management/api/v1/document/${encodeURIComponent(panel.dataset.documentKey)}/paragraph-editor`;
    const headers = { Authorization: "Bearer [redacted]" };
    let snapshot;
    let editors = [];
    let active;
    let selection;
    let source;
    let busy = false;
    let saveBlocked = false;

    async function request(url, options = {}) {
        const response = await fetch(url, {
            credentials: "same-origin",
            ...options,
            headers: { ...headers, ...options.headers }
        });
        if (response.ok) {
            const body = await response.text();
            return body ? JSON.parse(body) : null;
        }
        const problem = await response.json().catch(() => null);
        const description = problem?.title && problem?.detail
            ? `${problem.title} ${problem.detail}`
            : problem?.detail || problem?.title;
        const error = new Error(response.status === 401
            ? "Die Backoffice-Sitzung ist abgelaufen. Melden Sie sich im Backoffice erneut an und versuchen Sie es noch einmal."
            : description || `Anfrage fehlgeschlagen (${response.status}).`);
        error.status = response.status;
        throw error;
    }

    function showMessage(text) {
        message.textContent = text;
    }

    function setBusy(value) {
        busy = value;
        panel.querySelectorAll("button").forEach(button => { button.disabled = value; });
        if (!value && saveBlocked) {
            panel.querySelector('[data-action="save"]').disabled = true;
            panel.querySelector('[data-action="publish"]').disabled = true;
        }
    }

    function closeSource() {
        if (!source) return;
        active.element.innerHTML = source.value;
        active.element.hidden = false;
        source.remove();
        source = null;
        sourceButton.setAttribute("aria-pressed", "false");
    }

    function reset() {
        closeSource();
        for (const editor of editors) {
            editor.element.innerHTML = editor.rendered;
            editor.element.hidden = editor.wasHidden;
            editor.element.removeAttribute("contenteditable");
            editor.element.removeAttribute("role");
            editor.element.removeAttribute("aria-label");
            editor.element.removeAttribute("aria-multiline");
        }
        editors = [];
        active = undefined;
        selection = undefined;
        saveBlocked = false;
        controls.hidden = true;
        startButton.hidden = false;
    }

    function selectEditor(editor) {
        if (active !== editor) {
            closeSource();
            active = editor;
            selection = undefined;
        }
    }

    async function load() {
        snapshot = await request(endpoint);
        if (!snapshot || !Array.isArray(snapshot.paragraphs)) {
            throw new Error("Die Paragraphen konnten nicht geladen werden.");
        }
        return snapshot;
    }

    async function start() {
        setBusy(true);
        showMessage("");
        try {
            const data = await load();
            const byKey = new Map(data.paragraphs.map(item => [item.key.toLowerCase(), item]));
            const keys = new Set();
            editors = paragraphs.map(element => {
                const key = element.dataset.paragraphKey.toLowerCase();
                if (!byKey.has(key) || keys.has(key)) {
                    throw new Error("Die angezeigten Paragraphen stimmen nicht mit der gespeicherten Seite überein.");
                }
                keys.add(key);
                return {
                    element,
                    key,
                    markup: byKey.get(key).markup,
                    rendered: element.innerHTML,
                    wasHidden: element.hidden,
                    dirty: false
                };
            });
            for (const editor of editors) {
                editor.element.innerHTML = editor.markup;
                editor.element.hidden = false;
                editor.element.contentEditable = "true";
                editor.element.setAttribute("role", "textbox");
                editor.element.setAttribute("aria-label", "Paragraph bearbeiten");
                editor.element.setAttribute("aria-multiline", "true");
            }
            startButton.hidden = true;
            controls.hidden = false;
            selectEditor(editors[0]);
            active.element.focus();
            showMessage("Wählen Sie einen Paragraphen zur Bearbeitung aus.");
        } catch (error) {
            editors = [];
            showMessage(error.message);
        } finally {
            setBusy(false);
        }
    }

    async function save(publish) {
        closeSource();
        const changes = editors.filter(editor => editor.dirty)
            .map(editor => ({ key: editor.key, markup: editor.element.innerHTML }));
        if (changes.length === 0) {
            showMessage("Es gibt keine Änderungen zu speichern.");
            return;
        }
        setBusy(true);
        showMessage("");
        try {
            await request(endpoint, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    RequestVerificationToken: snapshot.antiforgeryToken
                },
                body: JSON.stringify({ publishedVersionId: snapshot.publishedVersionId, publish, changes })
            });
            reset();
            showMessage(publish
                ? "Die Seite wurde gespeichert und veröffentlicht. Die Seite wird neu geladen."
                : "Der Entwurf wurde gespeichert. Weitere Änderungen sind im Backoffice möglich.");
            if (publish) window.location.reload();
            else startButton.disabled = true;
        } catch (error) {
            if (error.status === 409) {
                saveBlocked = true;
                showMessage(`${error.message} Kopieren Sie Ihre Änderungen vor dem Abbrechen.`);
            } else {
                showMessage(error.message);
            }
        } finally {
            setBusy(false);
            if (!editors.length && !publish) startButton.disabled = true;
        }
    }

    document.addEventListener("selectionchange", () => {
        if (!active || source) return;
        const selected = window.getSelection();
        if (selected.rangeCount && active.element.contains(selected.anchorNode)) {
            selection = selected.getRangeAt(0).cloneRange();
        }
    });

    for (const element of paragraphs) {
        element.addEventListener("focus", () => {
            const editor = editors.find(item => item.element === element);
            if (editor) selectEditor(editor);
        });
        element.addEventListener("input", () => {
            const editor = editors.find(item => item.element === element);
            if (editor) editor.dirty = true;
        });
    }

    panel.addEventListener("mousedown", event => {
        if (event.target.closest("[data-command], [data-action='link']")) event.preventDefault();
    });
    panel.addEventListener("click", async event => {
        const button = event.target.closest("button");
        if (!button || busy) return;
        const action = button.dataset.action;
        if (action === "start") return start();
        if (action === "save") return save(false);
        if (action === "publish") return save(true);
        if (action === "cancel") {
            if (editors.some(editor => editor.dirty) && !window.confirm("Ungespeicherte Änderungen verwerfen?")) return;
            reset();
            showMessage("");
            return;
        }
        if (!active) return;
        if (action === "source") {
            if (source) {
                closeSource();
                active.element.focus();
            } else {
                source = document.createElement("textarea");
                source.className = "paragraph-editor__source";
                source.setAttribute("aria-label", "HTML-Quelltext des Paragraphen");
                source.value = active.element.innerHTML;
                source.addEventListener("input", () => { active.dirty = true; });
                active.element.after(source);
                active.element.hidden = true;
                sourceButton.setAttribute("aria-pressed", "true");
                source.focus();
            }
            return;
        }
        if (source) return;
        const command = action === "link" ? "createLink" : button.dataset.command;
        if (!command) return;
        const value = action === "link" ? window.prompt("Link-Adresse:") : button.dataset.value;
        if (action === "link" && !value) return;
        active.element.focus();
        if (selection) {
            const selected = window.getSelection();
            selected.removeAllRanges();
            selected.addRange(selection);
        }
        if (!document.execCommand(command, false, value || null)) {
            showMessage("Die Formatierung konnte nicht angewendet werden.");
            return;
        }
        active.dirty = true;
    });

    load().then(() => {
        panel.hidden = false;
    }).catch(error => {
        if (error.status === 401 || error.status === 403 || error.status === 404) return;
        panel.hidden = false;
        startButton.disabled = true;
        showMessage(error.message);
    });
})();
