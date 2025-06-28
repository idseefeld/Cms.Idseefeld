const preventImageCloseClick = false;

(function () {
    const pictures = document.querySelectorAll("picture.noscript.hidden");
    if (pictures.length) {
        pictures.forEach((picture) => {
            picture.classList.remove("hidden");
        });
    }
})();

function showImageBox(event, src, alt, captionText) {
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    if (viewportWidth < 600 || viewportHeight < 600) {
        window.open(src, "_blank");
        return;
    }

    const box = document.getElementById("imageBox");
    if (!box) return;

    const captionHeight = captionText ? 20 : 0;
    const margin = 20;
    const padding = 10;
    const horizontalOffset = (margin + padding) * 2;
    const verticalOffset = captionHeight + horizontalOffset;

    const thumbRect = event.target.getBoundingClientRect();
    const thumbTop = thumbRect.top;
    const thumbLeft = thumbRect.left;
    const thumbWidth = event.target.width;
    const thumbHeight = event.target.height;
    const aspectRatio = thumbWidth / thumbHeight;

    const imgWidth = thumbWidth;
    const imgHeight = thumbHeight;

    const landscape = viewportWidth / viewportHeight > 1;
    let zoomedWidth = landscape ? (viewportHeight - verticalOffset) * aspectRatio : viewportWidth - horizontalOffset;
    if (zoomedWidth > viewportWidth - horizontalOffset) {
        zoomedWidth = viewportWidth - horizontalOffset;
    }
    let zoomedHeight = zoomedWidth / aspectRatio;
    if (zoomedHeight > viewportHeight - verticalOffset) {
        zoomedHeight = viewportHeight - verticalOffset;
        zoomedWidth = zoomedHeight * aspectRatio;
    }

    const zoomedBoxWidth = (zoomedWidth + (padding * 2)) + 'px';
    const zoomedBoxHeight = (zoomedHeight + (padding * 2) + captionHeight) + 'px';

    const imageBoxContent = document.getElementById("imageBoxContent");

    imageBoxContent.style.width = (imgWidth + (padding * 2)) + 'px';
    imageBoxContent.style.height = (imgHeight + (padding * 2) + captionHeight) + 'px';
    imageBoxContent.style.padding = padding + 'px';
    imageBoxContent.style.cursor = preventImageCloseClick ? "default" : "zoom-out";

    imageBoxContent.style.top = (thumbTop - padding) + "px";
    imageBoxContent.style.left = (thumbLeft - padding) + "px";
    imageBoxContent.style.margin = "0";

    box.style.display = "block";
    document.querySelector("body").style.overflow = "hidden";

    const img = document.getElementById("imageBoxImg");
    img.setAttribute("src", src);
    img.setAttribute("alt", alt);
    if (imgWidth && imgHeight) {
        img.setAttribute("width", imgWidth);
        img.setAttribute("height", imgHeight);
    }

    const caption = document.getElementById("imageBoxCaption");
    if (caption) {
        caption.textContent = captionText;
    }

    setTimeout(function () {
        zoomInImageBox(viewportWidth, viewportHeight, imageBoxContent, zoomedBoxWidth, zoomedBoxHeight, img, zoomedWidth, zoomedHeight, margin);
    }, 0);
}

function zoomInImageBox(viewportWidth, viewportHeight, imageBoxContent, zoomedBoxWidth, zoomedBoxHeight, img, zoomedWidth, zoomedHeight, margin) {
    // Force reflow to ensure transition triggers
    void imageBoxContent.offsetWidth;

    const boxWidth = parseFloat(zoomedBoxWidth);
    const boxHeight = parseFloat(zoomedBoxHeight);

    const top = Math.max((viewportHeight - boxHeight) / 2, margin);
    const left = Math.max((viewportWidth - boxWidth) / 2, margin);

    imageBoxContent.style.width = zoomedBoxWidth;
    imageBoxContent.style.height = zoomedBoxHeight;
    imageBoxContent.style.top = top + "px";
    imageBoxContent.style.left = left + "px";
    imageBoxContent.style.margin = "0";

    if (zoomedWidth && zoomedHeight) {
        img.setAttribute("width", zoomedWidth);
        img.setAttribute("height", zoomedHeight);
    }
}
function imageClick(event) {
    if (preventImageCloseClick) { event.stopPropagation(); }
}

function hideImageBox() {
    var box = document.getElementById("imageBox");
    if (!box) return;

    document.querySelector("body").style.overflow = "unset";
    box.style.display = "none";
}