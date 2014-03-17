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
			jQuery.browser.msie = false;
			jQuery.browser.version = 0;
			if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
				jQuery.browser.msie = true;
				jQuery.browser.version = RegExp.$1;
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
				$(this).load("/Account/JsonChangeRoles");
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