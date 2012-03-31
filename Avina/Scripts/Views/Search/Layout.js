$('#content').css('margin-top', window.innerHeight / 2 - 30 + 'px'); // ' :300px;
if (window.innerWidth <= 320) {
    $('input[type="text"]').removeClass('input-xxlarge')
                           .addClass('input-medium');
}
else if (window.innerWidth <= 768) {
    $('input[type="text"]').removeClass('input-xxlarge')
                           .addClass('input-large');
}