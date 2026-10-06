window.downloadFile = function(fileName, base64Content) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = "data:application/xml;base64," + base64Content;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.showModal = function(modalId) {
    const modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
};
