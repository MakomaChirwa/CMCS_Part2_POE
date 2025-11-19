$(document).ready(function () {
    function updateTotal() {
        let hours = parseFloat($("#hours").val()) || 0;
        let rate = parseFloat($("#rate").val()) || 0;

        $("#total").val((hours * rate).toFixed(2));
    }

    $("#hours, #rate").on("input", updateTotal);
});
