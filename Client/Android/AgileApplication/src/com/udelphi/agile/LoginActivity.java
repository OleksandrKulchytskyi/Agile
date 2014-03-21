package com.udelphi.agile;

import java.util.List;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;
import org.apache.http.client.CookieStore;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.cookie.Cookie;
import org.apache.http.impl.client.AbstractHttpClient;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.params.BasicHttpParams;
import org.apache.http.params.HttpConnectionParams;
import org.apache.http.params.HttpParams;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.annotation.TargetApi;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;
import android.text.TextUtils;
import android.util.Base64;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

/**
 * Activity which displays a login screen to the user
 **/
public class LoginActivity extends BaseActivity {
	/**
	 * The default email to populate the email field with.
	 */
	public static final String EXTRA_EMAIL = "com.example.android.authenticatordemo.extra.EMAIL";

	/**
	 * Keep track of the login task to ensure we can cancel it if requested.
	 */
	private UserLoginTask mAuthTask = null;

	// Values for email and password at the time of the login attempt.
	private String mUrl;
	private String mEmail;
	private String mPassword;

	// UI references.
	private EditText mUrlView;
	private EditText mEmailView;
	private EditText mPasswordView;
	private View mLoginFormView;
	private View mLoginStatusView;
	private TextView mLoginStatusMessageView;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		setContentView(R.layout.activity_login);

		mUrlView = (EditText) findViewById(R.id.serverUrl);
		mUrl = mUrlView.getEditableText().toString();

		// Set up the login form.
		mEmail = getIntent().getStringExtra(EXTRA_EMAIL);
		mEmailView = (EditText) findViewById(R.id.email);
		mEmailView.setText(mEmail);

		mPasswordView = (EditText) findViewById(R.id.password);
		mPasswordView
				.setOnEditorActionListener(new TextView.OnEditorActionListener() {
					@Override
					public boolean onEditorAction(TextView textView, int id,
							KeyEvent keyEvent) {
						if (id == R.id.login || id == EditorInfo.IME_NULL) {
							attemptLogin();
							return true;
						}
						return false;
					}
				});

		mLoginFormView = findViewById(R.id.login_form);
		mLoginStatusView = findViewById(R.id.login_status);
		mLoginStatusMessageView = (TextView) findViewById(R.id.login_status_message);

		findViewById(R.id.sign_in_button).setOnClickListener(
				new View.OnClickListener() {
					@Override
					public void onClick(View view) {
						attemptLogin();
					}
				});
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		super.onCreateOptionsMenu(menu);
		getMenuInflater().inflate(R.menu.login, menu);
		return true;
	}

	/**
	 * Attempts to sign in or register the account specified by the login form.
	 * If there are form errors (invalid email, missing fields, etc.), the
	 * errors are presented and no actual login attempt is made.
	 */
	public void attemptLogin() {
		if (mAuthTask != null)
			return;

		// Reset errors.
		mUrlView.setError(null);
		mEmailView.setError(null);
		mPasswordView.setError(null);

		// Store values at the time of the login attempt.
		mUrl = mUrlView.getText().toString();
		if (mUrl.isEmpty()) {
			mUrl = mUrlView.getHint().toString();
			mUrlView.setText(mUrl);
		}
		mEmail = mEmailView.getText().toString();
		mPassword = mPasswordView.getText().toString();

		boolean cancel = false;
		View focusView = null;

		// check for the server address
		if (TextUtils.isEmpty(mUrl)) {
			mUrlView.setError("Server address cannot be empty.");
			focusView = mUrlView;
			cancel = true;
		}
		// Check for a valid password.
		if (TextUtils.isEmpty(mPassword)) {
			mPasswordView.setError(getString(R.string.error_field_required));
			focusView = mPasswordView;
			cancel = true;
		} else if (mPassword.length() < 4) {
			mPasswordView.setError(getString(R.string.error_invalid_password));
			focusView = mPasswordView;
			cancel = true;
		}

		// Check for a valid email address.
		if (TextUtils.isEmpty(mEmail)) {
			mEmailView.setError(getString(R.string.error_field_required));
			focusView = mEmailView;
			cancel = true;
		}

		if (cancel) {
			// There was an error; don't attempt login and focus the first
			// form field with an error.
			focusView.requestFocus();
		} else {
			// Show a progress spinner, and kick off a background task to
			// perform the user login attempt.
			mLoginStatusMessageView.setText(R.string.login_progress_signing_in);
			showProgress(true);
			mAuthTask = new UserLoginTask();
			Base64.encodeToString(mPassword.getBytes(), Base64.DEFAULT);
			mAuthTask.execute(mEmail
					+ ":"
					+ Base64.encodeToString(mPassword.getBytes(),
							Base64.DEFAULT).replaceAll("\n", ""));
		}
	}

	/**
	 * Shows the progress UI and hides the login form.
	 */
	@TargetApi(Build.VERSION_CODES.HONEYCOMB_MR2)
	private void showProgress(final boolean show) {
		// On Honeycomb MR2 we have the ViewPropertyAnimator APIs, which allow
		// for very easy animations. If available, use these APIs to fade-in
		// the progress spinner.
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB_MR2) {
			int shortAnimTime = getResources().getInteger(
					android.R.integer.config_shortAnimTime);

			mLoginStatusView.setVisibility(View.VISIBLE);
			mLoginStatusView.animate().setDuration(shortAnimTime)
					.alpha(show ? 1 : 0)
					.setListener(new AnimatorListenerAdapter() {
						@Override
						public void onAnimationEnd(Animator animation) {
							mLoginStatusView.setVisibility(show ? View.VISIBLE
									: View.GONE);
						}
					});

			mLoginFormView.setVisibility(View.VISIBLE);
			mLoginFormView.animate().setDuration(shortAnimTime)
					.alpha(show ? 0 : 1)
					.setListener(new AnimatorListenerAdapter() {
						@Override
						public void onAnimationEnd(Animator animation) {
							mLoginFormView.setVisibility(show ? View.GONE
									: View.VISIBLE);
						}
					});
		} else {
			// The ViewPropertyAnimator APIs are not available, so simply show
			// and hide the relevant UI components.
			mLoginStatusView.setVisibility(show ? View.VISIBLE : View.GONE);
			mLoginFormView.setVisibility(show ? View.GONE : View.VISIBLE);
		}

		if (AgileApplication.container.containsKey("IsLogged")) {
			boolean isLogged = ((Boolean) AgileApplication.container
					.get("IsLogged"));
			Log.d("Logged", isLogged == true ? "Logged" : "Not logged");
			if (!show && isLogged) {
				Intent nav = new Intent(getBaseContext(), MainActivity.class);
				startActivity(nav);
			}
		}
	}

	/**
	 * Represents an asynchronous login/registration task used to authenticate
	 * the user.
	 */
	public class UserLoginTask extends AsyncTask<String, Void, Integer> {

		Exception occurredExc;

		@Override
		protected Integer doInBackground(String... params) {
			CookieStore sCookieStore;
			occurredExc = null;
			Integer result = 0;
			try {
				String logInformation = "";
				for (String logInfo : params) {
					logInformation = logInfo;
					break;
				}
				if (logInformation.isEmpty()) {
					return 1;
				}

				AgileApplication.container.put("ServerUrl", mUrl);

				String getURL = mUrl + "/handlers/loginhandler.ashx";
				Log.d("Concatenated URL", getURL);
				HttpParams httpParameters = new BasicHttpParams();
				int timeoutConnection = 30000;
				HttpConnectionParams.setConnectionTimeout(httpParameters,
						timeoutConnection);
				int timeoutSocket = 30000;
				HttpConnectionParams
						.setSoTimeout(httpParameters, timeoutSocket);
				HttpClient client = new DefaultHttpClient(httpParameters);
				HttpGet get = new HttpGet(getURL);
				String data = Base64.encodeToString(logInformation.getBytes(),
						Base64.DEFAULT);
				get.setHeader("Authorization", "Basic " + data);
				HttpResponse responseGet = client.execute(get);

				sCookieStore = ((AbstractHttpClient) client).getCookieStore();
				if (sCookieStore != null) {
					List<Cookie> cookies = sCookieStore.getCookies();
					boolean found = false;
					for (Cookie c : cookies) {
						if (c.getName().equalsIgnoreCase(".ASPXAUTH")) {
							AgileApplication.container.put(".ASPXAUTH",
									c.getValue());
							found = true;
							break;
						}
					}
					if (!found)
						return 1;
				}
				HttpEntity resEntityGet = responseGet.getEntity();
				if (resEntityGet != null) {
					StatusLine status = responseGet.getStatusLine();
					Log.d("Status", status.toString());

					if (status.getStatusCode() == 200) {
						result = 0;
					} else if (status.getStatusCode() == 401) {
						result = 1;
						Log.e("Statuscode",
								String.valueOf(status.getStatusCode()));
					} else if (status.getStatusCode() == 404) {
						result = 2;
						Log.e("Statuscode",
								String.valueOf(status.getStatusCode()));
					} else
						result = 3;
				} else
					result = 1;
			} catch (Exception exc) {
				result = 2;
				occurredExc = exc;
				Log.e("GET ", exc.getMessage());
			}

			return result;
		}

		@Override
		protected void onPostExecute(final Integer loginResult) {

			Log.d("Login result is", String.valueOf(loginResult));
			if (loginResult == 0)
				AgileApplication.container.put("IsLogged", true);
			else if (loginResult == 1) {
				AgileApplication.container.put("IsLogged", false);
				mEmailView.setError(getString(R.string.error_invalid_email));
				mPasswordView
						.setError(getString(R.string.error_incorrect_password));
				mPasswordView.requestFocus();
			} else if (loginResult == 2) {
				AgileApplication.container.put("IsLogged", false);
				mUrlView.requestFocus();
				if (occurredExc != null) {
					Toast.makeText(getBaseContext(), occurredExc.getMessage(),
							Toast.LENGTH_SHORT).show();
				}
			} else if (loginResult == 3) {
				AgileApplication.container.put("IsLogged", false);
				Log.d("Unknown response result", "Login result is 3");
			}

			showProgress(false);
			mAuthTask = null;
		}

		@Override
		protected void onCancelled() {
			mAuthTask = null;
			AgileApplication.container.put("IsLogged", false);
			showProgress(false);
		}
	}
}
