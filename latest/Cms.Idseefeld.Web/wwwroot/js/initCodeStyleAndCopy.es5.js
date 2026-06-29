'use strict';

(function () {
    document.querySelectorAll('pre code').forEach(function (el, idx) {
        hljs.highlightElement(el);
        if (el) {
            // Ensure each <pre> has a unique id
            if (!el.id) {
                el.id = 'codeblock-' + idx;
            }

            var button = document.createElement('button');
            //button.textContent = 'copy';
            button.classList.add('btn', 'btn-copy');
            button.setAttribute('data-clipboard-target', '#' + el.id);
            button.setAttribute('title', 'In die Zwischenablage kopieren');
            button.setAttribute('type', 'button');

            // Create and append the image
            var img = document.createElement('img');
            img.src = '/css/copy-icon.svg';
            img.alt = 'Copy to clipboard';
            button.appendChild(img);

            button.addEventListener('click', function () {
                var code = el.innerText;
                navigator.clipboard.writeText(code);
            });

            el.appendChild(button);
        }
    });

    var clipboard = new ClipboardJS('.btn-copy');

    clipboard.on('success', function (e) {
        console.debug('Action:', e.action);
        console.debug('Text:', e.text);
        console.debug('Trigger:', e.trigger);

        e.clearSelection();
    });

    clipboard.on('error', function (e) {
        console.debug('Action:', e.action);
        console.debug('Trigger:', e.trigger);
    });
})();

