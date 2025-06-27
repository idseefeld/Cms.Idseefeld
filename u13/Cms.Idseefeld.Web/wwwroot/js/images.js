const preventImageCloseClick = false;
function showImageBox(event, src, alt, captionText) {
    var width = window.innerWidth;
    var height = window.innerHeight;
    if (width < 600 || height < 600) { return; }

    var landscape = width / height > 1;

    var box = document.getElementById("imageBox");
    if (!box) return;

    var captionHeight = captionText ? 20 : 0;
    var margin = 20;
    var padding = 10;
    var horizontalOffset = (margin + padding) * 2;
    var verticalOffset = captionHeight + horizontalOffset;

    var thumbWidth = event.target.width;
    var thumbHeight = event.target.height;
    var aspectRatio = thumbWidth / thumbHeight;

    var imgWidth = landscape ? (height - verticalOffset) * aspectRatio : width - horizontalOffset;
    if (imgWidth > width - horizontalOffset) {
        imgWidth = width - horizontalOffset;
    }
    var imgHeight = imgWidth / aspectRatio;
    if (imgHeight > height - verticalOffset) {
        imgHeight = height - verticalOffset;
        imgWidth = imgHeight * aspectRatio;
    }

    var imageBoxContent = document.getElementById("imageBoxContent");
    imageBoxContent.style.width = (imgWidth + (padding * 2)) + 'px';
    imageBoxContent.style.height = (imgHeight + (padding * 2) + captionHeight) + 'px';
    imageBoxContent.style.padding = padding + 'px';
    imageBoxContent.style.margin = margin + 'px auto';
    imageBoxContent.style.cursor = preventImageCloseClick ? "default" : "pointer";

    box.style.display = "block";

    var img = document.getElementById("imageBoxImg");
    img.setAttribute("src", src);
    img.setAttribute("alt", alt);
    if (imgWidth && imgHeight) {
        img.setAttribute("width", imgWidth);
        img.setAttribute("height", imgHeight);
    }

    var caption = document.getElementById("imageBoxCaption");
    if (caption) {
        try {
            caption.textContent = captionText;
        }
        catch (error) {
            console.debug(error);
        }
    }
}

function imageClick(event) {
    if (preventImageCloseClick) { event.stopPropagation(); }
}

function hideImageBox() {
    var box = document.getElementById("imageBox");
    if (!box) return;

    box.style.display = "none";
}