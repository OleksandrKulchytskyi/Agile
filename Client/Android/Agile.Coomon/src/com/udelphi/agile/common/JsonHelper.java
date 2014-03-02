package com.udelphi.agile.common;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.URL;
import java.net.URLConnection;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.util.Log;

public class JsonHelper {
	public static Object toJSON(Object object) throws JSONException {
		if (object instanceof Map) {
			JSONObject json = new JSONObject();
			Map map = (Map) object;
			for (Object key : map.keySet()) {
				json.put(key.toString(), toJSON(map.get(key)));
			}
			return json;
		} else if (object instanceof Iterable) {
			JSONArray json = new JSONArray();
			for (Object value : ((Iterable) object)) {
				json.put(value);
			}
			return json;
		} else {
			return object;
		}
	}

	public static boolean isEmptyObject(JSONObject object) {
		return object.names() == null;
	}

	public static Map<String, Object> getMap(JSONObject object, String key)
			throws JSONException {
		return toMap(object.getJSONObject(key));
	}

	public static Map<String, Object> toMap(JSONObject object)
			throws JSONException {
		Map<String, Object> map = new HashMap();
		Iterator keys = object.keys();
		while (keys.hasNext()) {
			String key = (String) keys.next();
			map.put(key, fromJson(object.get(key)));
		}
		return map;
	}

	public static List toList(JSONArray array) throws JSONException {
		List list = new ArrayList();
		for (int i = 0; i < array.length(); i++) {
			list.add(fromJson(array.get(i)));
		}
		return list;
	}

	private static Object fromJson(Object json) throws JSONException {
		if (json == JSONObject.NULL) {
			return null;
		} else if (json instanceof JSONObject) {
			return toMap((JSONObject) json);
		} else if (json instanceof JSONArray) {
			return toList((JSONArray) json);
		} else {
			return json;
		}
	}

	public static JSONArray GetFromRequest(String url) {

		JSONArray ret = null;
		try {
			URL urlws = new URL(url);
			URLConnection tc = urlws.openConnection();
			BufferedReader in;

			in = new BufferedReader(new InputStreamReader(tc.getInputStream()));
			String line;
			StringBuilder sb = new StringBuilder();
			while ((line = in.readLine()) != null) {
				sb.append(line);
			}
			ret = new JSONArray(sb.toString());
		} catch (IOException e) {
			e.printStackTrace();
		} catch (Exception e) {
			e.printStackTrace();
		}
		return ret;
	}
	
	public static String GetStringFromRequest(String url) {

		String result="";
		try {
			URL urlws = new URL(url);
			URLConnection tc = urlws.openConnection();
			BufferedReader in;

			in = new BufferedReader(new InputStreamReader(tc.getInputStream()));
			String line;
			StringBuilder sb = new StringBuilder();
			while ((line = in.readLine()) != null) {
				sb.append(line);
			}
			result=sb.toString();
		} catch (IOException e) {
			e.printStackTrace();
		} catch (Exception e) {
			e.printStackTrace();
		}
		return result;
	}

}