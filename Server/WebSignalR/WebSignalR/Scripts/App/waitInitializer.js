$(document).ready(function () {
	$body = $("body");
	console.log("READY!");

	$(document).on({
		ajaxStart: function () { $body.addClass("waitLoading"); },
		ajaxStop: function () { $body.removeClass("waitLoading"); }
	});
});