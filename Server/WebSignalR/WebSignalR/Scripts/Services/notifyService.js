window.agileApp = window.agileApp || {};

window.agileApp.notifyService = (function () {

	var notifyService = {
		success: success,
		error: error,
		info: info,
		warning: warning
	};

	return notifyService;

	function error(msg, data, showToast) {
		if (showtoast) {
			toastr.options = {
				"closeButton": true,
				"debug": false,
				"positionClass": "toast-bottom-full-width",
				"onclick": null,
				"showDuration": "300",
				"hideDuration": "1000",
				"timeOut": "300000",
				"extendedTimeOut": "1000",
				"showEasing": "swing",
				"hideEasing": "linear",
				"showMethod": "fadeIn",
				"hideMethod": "fadeOut"
			};
			toastr.error(msg);
		}
		console.error(msg, data);
	};

	function info(msg, data, showtoast) {

		if (showtoast) {
			toastr.options = {
				"closeButton": true,
				"debug": false,
				"positionClass": "toast-bottom-right",
				"onclick": null,
				"showDuration": "300",
				"hideDuration": "1000",
				"timeOut": "300000",
				"extendedTimeOut": "1000",
				"showEasing": "swing",
				"hideEasing": "linear",
				"showMethod": "fadeIn",
				"hideMethod": "fadeOut"
			};
			toastr.info(msg);
		}
		console.info(msg, data);
	};

	function warning (msg, data, showtoast) {

		if (showtoast) {
			toastr.options = {
				"closeButton": false,
				"debug": false,
				"positionClass": "toast-bottom-right",
				"onclick": null,
				"showDuration": "300",
				"hideDuration": "1000",
				"timeOut": "5000",
				"extendedTimeOut": "1000",
				"showEasing": "swing",
				"hideEasing": "linear",
				"showMethod": "fadeIn",
				"hideMethod": "fadeOut"
			};
			toastr.warning(msg);
		}
		console.warning(msg, data);
	};

	function success (msg, data, showtoast) {

		if (showtoast) {
			toastr.options = {
				"closeButton": false,
				"debug": false,
				"positionClass": "toast-bottom-right",
				"onclick": null,
				"showDuration": "300",
				"hideDuration": "1000",
				"timeOut": "5000",
				"extendedTimeOut": "1000",
				"showEasing": "swing",
				"hideEasing": "linear",
				"showMethod": "fadeIn",
				"hideMethod": "fadeOut"
			};

			toastr.success(msg);
		}
		console.info(msg, data);
	};

})();