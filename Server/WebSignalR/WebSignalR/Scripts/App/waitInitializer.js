$(document).ready(function () {
	$body = $("body");
	
	$(document).on({
		ajaxStart: function () { $body.addClass("waitLoading"); },
		ajaxStop: function () { $body.removeClass("waitLoading"); }
	});
});