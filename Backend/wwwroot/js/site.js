$select = $('#role_select');
$('#group_select').hide();


$select.change(function () {
    if ($(this).val() == "3") {
        if ($('#group_select').is(":hidden")) {
            $('#group_select').show();
        }
    }
    if ($(this).val() == "2") {
        $('#group_select').hide();
    }
    if ($(this).val() == "1") {
        $('#group_select').hide();
});