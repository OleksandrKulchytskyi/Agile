/// <reference path="Scripts/toastr.js"
window.agileApp = window.agileApp || {};

window.agileApp.notifyService = (function () {

	var notifyService = {
		success: success,
		error: error,
		info: info,
		warning: warning
	};

	init();

	return notifyService;

	function init() {
		toastr.options.newestOnTop = true;
		toastr.options.positionClass = 'toast-top-full-width';
		toastr.options.extendedTimeOut =1300;
		toastr.options.timeOut = 1000;
		toastr.options.fadeOut = 250;
		toastr.options.fadeIn = 250;
	}

	function error(msg, data, showToast) {
		if (showToast) {
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
			toastr.error(msg,'Error');
		}
		console.error(msg, data);
	};

	function info(msg, data, showToast) {

		if (showToast) {
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
			toastr.info(msg,'Information');
		}
		console.info(msg, data);
	};

	function warning (msg, data, showToast) {

		if (showToast) {
			toastr.options = {
				"closeButton": false,
				"debug": false,
				"positionClass": "toast-bottom-full-width",
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

	function success (msg, data, showToast) {

		if (showToast) {
			toastr.options = {
				"closeButton": false,
				"debug": false,
				"positionClass": "toast-top-full-width",
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

			toastr.success(msg,'Success');
		}
		console.info(msg, data);
	};

})();