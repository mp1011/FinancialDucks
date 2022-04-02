function getSelectedText()
{
    return window.getSelection().toString();
}

function showModal(id)
{
    var modal = new bootstrap.Modal(document.getElementById(id), {});
    modal.show();
}