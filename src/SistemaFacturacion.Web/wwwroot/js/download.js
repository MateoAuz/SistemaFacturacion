window.downloadFile = function (filename, contentType, base64Content) {
    const linkSource = `data:${contentType};base64,${base64Content}`;
    const downloadLink = document.createElement("a");
    downloadLink.href = linkSource;
    downloadLink.download = filename;
    downloadLink.click();
};
