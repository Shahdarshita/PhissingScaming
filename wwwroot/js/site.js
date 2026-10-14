// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Clear textarea
function clearTextArea() {
    var ta = document.getElementById('MessageText');
    if (ta) ta.value = '';
}

// Preview uploaded image
function previewImage(event) {
    var input = event.target;
    var previewContainer = document.getElementById('previewContainer');
    var preview = document.getElementById('previewImage');

    if (input.files && input.files[0]) {
        var file = input.files[0];
        var reader = new FileReader();
        reader.onload = function(e) {
            preview.src = e.target.result;
            previewContainer.style.display = 'block';
        };
        reader.readAsDataURL(file);
    } else {
        preview.src = '#';
        previewContainer.style.display = 'none';
    }
}
