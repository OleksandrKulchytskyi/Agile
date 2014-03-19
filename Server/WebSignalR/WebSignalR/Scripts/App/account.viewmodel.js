window.agileApp.accountViewModel = (function (ko, datacontext, notifyService) {
	/// <field name="roomList" value="[new datacontext.roomList()]"></field>
	var allPrivileges = ko.observableArray(),
        error = ko.observable(),
        addNewUser = function () {
        	$("#createUserDialog").dialog("open");
        },
        changeUserPassword = function () {
        	$("#changePasswordDialog").dialog("open");
        },
		changeRoles = function () {
			$("#changeRolesDialog").dialog("open");
		};

	function initDialogs() {

		jQuery.browser = {};
		(function () {
			console.log(navigator.userAgent);
			jQuery.browser.msie = false;
			jQuery.browser.firefox = false;
			jQuery.browser.chrome = false;
			jQuery.browser.opera = false;
			jQuery.browser.version = 0;
			jQuery.browser.fullVersion = "";

			if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
				jQuery.browser.msie = true;
				jQuery.browser.version = RegExp.$1;
			}
			else if (window.navigator.appVersion.match(/Chrome\/(.*?) /)) { //navigator.userAgent.match(/Chrome([0-9]+)\./)) {
				jQuery.browser.chrome = true;
				jQuery.browser.fullVersion = window.navigator.appVersion.match(/Chrome\/(.*?) /)[1];
				jQuery.browser.version = parseInt(window.navigator.appVersion.match(/Chrome\/(\d+)\./)[1], 10);
			}
			else if (navigator.userAgent.match(/Firefox\/(.*?)/)) {
				jQuery.browser.firefox = true;
				jQuery.browser.fullVersion = window.navigator.userAgent.match(/Firefox\/(.*?)/)[1];
				jQuery.browser.version = parseInt(window.navigator.userAgent.match(/Firefox\/(\d+)\./)[1], 10);
			}
		})();

		$("#createUserDialog").dialog({
			autoOpen: false,
			height: 380,
			width: 370,
			modal: true,
			buttons: {
				"Create an account": function () {
					var form = $("#registerForm");
					var registerVM = datacontext.createRegisterVM();
					registerVM.userName($('#registerName').val());
					registerVM.password($('#Password').val());
					registerVM.confirmPassword($('#ConfirmPassword').val());

					console.log(registerVM.toJson());
					notifyService.info("Registering new user", null, true);
					registerVM.registerUser()
					.done(function () {
						notifyService.success("User has been successfully registered.", null, true);
					})
					.fail(function () {
						notifyService.error("User registration was fail.", null, true);
					});
				},
				Cancel: function () {
					$(this).dialog("close");
					var from = $("#registerForm");
					from.find('input:text').val('');
					from.find('input:password').val('');
				}
			},
			close: function () {
				var from = $("#registerForm");
				from.find('input:text').val('');
				from.find('input:password').val('');
			}
		});

		$("#changePasswordDialog").dialog({
			autoOpen: false,
			height: 380,
			width: 370,
			modal: true,
			buttons: {
				"Change": function () {
					var passwordVM = datacontext.createChangePasswordVM();
					passwordVM.password($("#changePasswordForm input[id=Password]").val());
					passwordVM.confirmPassword($("#changePasswordForm input[id=ConfirmPassword]").val());
					notifyService.info("Changing user password.", null, true);
					passwordVM.change()
					.done(function () {
						notifyService.success("User password has been successfully changed.", null, true);
					})
					.fail(function () {
						notifyService.error("Unable to change user password.", null, true);
					});
				},
				Cancel: function () {
					$(this).dialog("close");
					var from = $("#changePasswordForm");
					from.find('input:password').val('');
				}
			},
			close: function () {
				var from = $("#changePasswordForm");
				from.find('input:password').val('');
			}
		});

		$("#changeRolesDialog").dialog({
			autoOpen: false,
			height: 500,
			width: 450,
			modal: true,
			buttons: {
				Cancel: function () {
					$(this).dialog("close");
					var from = $("#changePasswordForm");
					from.find('input:password').val('');
				}
			},
			open: function (event, ui) {
				var url = agileApp.baseAddress + "Account/JsonChangeRoles"
				console.log(url);
				$(this).load(url);
			},
			close: function () {
				$("#changeRolesForm").empty();
			}
		});

		$("#btnSubmit").hide();
	}

	initDialogs();

	// load privileges
	datacontext.getPrivileges()
	.done(function (data) {
		var mappedData = $.map(data, function (item) {
			return new datacontext.createRegisterVM(item);
		});
		allPrivileges(mappedData);
	})
	.fail(function () {
		if (window.agileApp.notifyService !== undefined) {
			window.agileApp.notifyService.error("Unable to retrieve privilege list.", null, true);
		}
		error("Error retrieving privileges.");
	});

	return {
		allPrivileges: allPrivileges,
		error: error,
		addNewUser: addNewUser,
		changeUserPassword: changeUserPassword,
		changeRoles: changeRoles
	};

})(ko, agileApp.datacontext, agileApp.notifyService);

// Initiate the Knockout bindings
ko.applyBindings(window.agileApp.accountViewModel);