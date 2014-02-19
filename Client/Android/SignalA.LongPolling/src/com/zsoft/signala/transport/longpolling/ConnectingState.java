package com.zsoft.signala.transport.longpolling;

import org.json.JSONException;
import org.json.JSONObject;

import com.turbomanage.httpclient.AsyncCallback;
import com.turbomanage.httpclient.HttpResponse;
import com.zsoft.signala.ConnectionBase;
import com.zsoft.signala.ConnectionState;
import com.zsoft.signala.SignalAUtils;
import com.zsoft.signala.SendCallback;
import com.zsoft.signala.transport.TransportHelper;
import com.zsoft.parallelhttpclient.ParallelHttpClient;

import java.util.Map;

public class ConnectingState extends StopableStateWithCallback {
	public ConnectingState(ConnectionBase connection) {
		super(connection);
	}

	@Override
	public ConnectionState getState() {
		return ConnectionState.Connecting;
	}

	@Override
	public void Start() {
	}

	@Override
	public void Send(CharSequence text, SendCallback callback) {
		callback.OnError(new Exception("Not connected"));
	}

	@Override
	protected void OnRun() {
		if(DoStop()) return; 

        // negotiate
		String url = SignalAUtils.EnsureEndsWith(mConnection.getUrl(), "/") + "negotiate";
		TransportHelper.AppendCustomQueryString(mConnection, url);
		AsyncCallback cb = new AsyncCallback() {
			
			@Override
			public void onComplete(HttpResponse httpResponse) {
				try
				{
					if(DoStop()) return; 

					
					if(httpResponse!=null && httpResponse.getStatus()==200 && !httpResponse.getBodyAsString().isEmpty())
					{
						JSONObject json = JSONHelper.ToJSONObject(httpResponse.getBodyAsString());
						String connectionId="";
						String connectionToken="";
						String protocolVersion = "";
						try {
							connectionId = json.getString("ConnectionId");
							connectionToken = json.getString("ConnectionToken");
							protocolVersion = json.getString("ProtocolVersion");
	
							if(mConnection.VerifyProtocolVersion(protocolVersion))
							{
								mConnection.setConnectionId(connectionId);
								mConnection.setConnectionToken(connectionToken);
								mConnection.SetNewState(new ConnectedState(mConnection));
								return;
							}
							else
							{
								mConnection.setError(new Exception("Not supported protocol version."));
								mConnection.SetNewState(new DisconnectedState(mConnection));
								return;
							}
						
						} catch (JSONException e) {
							mConnection.setError(new Exception("Unable to parse negotiation response."));
							return;
						}
					}
					else
					{
						mConnection.SetNewState(new DisconnectedState(mConnection));
					}
				}
				finally
				{
					mIsRunning.set(false);
				}
			}
            @Override
            public void onError(Exception ex) {
				mConnection.setError(ex);
				mConnection.SetNewState(new DisconnectedState(mConnection));
            }
		};
		
		synchronized (mCallbackLock) {
			//mCurrentCallback = cb;
		}
		
		ParallelHttpClient httpClient = new ParallelHttpClient();
        httpClient.setMaxRetries(1);
        for (Map.Entry<String, String> entry : mConnection.getHeaders().entrySet())
        {
            httpClient.addHeader(entry.getKey(), entry.getValue());
        }
        httpClient.get(url, null, cb);
	}

}
