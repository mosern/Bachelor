package no.hesa.positionlibrary.api;

import android.net.Uri;
import android.os.AsyncTask;
import android.util.Pair;

import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.URL;
import java.util.List;

import javax.net.ssl.HttpsURLConnection;

/**
 * A simple asynctask that communicates with the API
 */
public class ApiAsyncTask extends AsyncTask<List<Pair<String,String>>,Void,JSONObject>{
    private ActionInterface actionInterface;
    private String actionString;
    private String url;
    private String dataType;
    private String token = null;
    private boolean authenticationError = false;
/*
    ProgressDialog dialog;
    SearchResultsActivity activity;

    private ProgressDialog progressDialog;
*/
    /**
     * An overloaded constructor that defaults the data method to get
     * @param actionInterface The interface that is used to callback to the activity
     * @param actionString An actionstring that identifies the action so the activity can handle multiple callbacks
     * @param url The URL which points to the API
     */
    public ApiAsyncTask(ActionInterface actionInterface,String actionString, String url) {
        this(actionInterface,actionString,url,"GET");
    }

    /**
     * Main constructor
     * @param actionInterface The interface that is used to callback to the activity
     * @param actionString An actionstring that identifies the action so the activity can handle multiple callbacks
     * @param url The URL which points to the API
     * @param dataType POST or GET data
     */
    public ApiAsyncTask(ActionInterface actionInterface, String actionString,String url,String dataType) {

        this.actionInterface = actionInterface;
        this.actionString = actionString;
        this.url = url;
        this.dataType = dataType;
    }

    /**
     * A construction that accepts a token for authentication
     * @param actionInterface The interface that is used to callback to the activity
     * @param actionString An actionstring that identifies the action so the activity can handle multiple callbacks
     * @param url The URL which points to the API
     * @param dataType POST or GET data
     * @param token The bearer token for authentication
     */
    public ApiAsyncTask(ActionInterface actionInterface, String actionString, String url, String dataType, String token) {
        this(actionInterface,actionString,url,dataType);
        this.token = token;
    }

    /**
     * Do the execution of the asynctask, get the jsonobject from the API, does authentication if set
     * @param params List of parameters to include in the request
     * @return JSON return from the API
     */
    @Override
    protected JSONObject doInBackground(List<Pair<String,String>>... params) {
        JSONObject jObj = null;
        try {
            URL urlObject = new URL(url);
            if (dataType.equals("GET")) {
                urlObject = new URL(buildGetUrl(params[0]));
            }
            HttpsURLConnection conn = (HttpsURLConnection) urlObject.openConnection();
            conn.setReadTimeout(10000);
            conn.setConnectTimeout(15000);
            conn.setRequestMethod(dataType);
            conn.setDoInput(true);
            conn.setDoOutput(false);
            if (token != null) {
                conn.setRequestProperty("Authorization","Bearer "+token);
            }
            if (dataType.equals("POST") && params[0].size() > 0) {
                String query = getQueryString(params[0]);
                OutputStream os = conn.getOutputStream();
                BufferedWriter writer = new BufferedWriter(new OutputStreamWriter(os,"UTF-8"));
                writer.write(query);
                writer.flush();
                writer.close();
                os.close();
            }
            int responseCode = conn.getResponseCode();
            if (responseCode == HttpsURLConnection.HTTP_OK) {
                String line;
                StringBuilder sb = new StringBuilder();
                BufferedReader br = new BufferedReader(new InputStreamReader(conn.getInputStream(),"UTF-8"),8);
                while ((line = br.readLine()) != null) {
                    sb.append(line+"\n");
                }
                String json = sb.toString();
                jObj = new JSONObject(json);
            }
            else if (responseCode == HttpsURLConnection.HTTP_UNAUTHORIZED) {
                authenticationError = true;
            }

        }
        catch (Exception ex) {
            ex.printStackTrace();
        }
        return jObj;
    }
/*
    @Override
    protected void onPreExecute() {
        dialog = ProgressDialog.show(activity, "title", "message");
        super.onPreExecute();
    }
*/
    /**
     * Is called when the execution is completed. Does a callback to the activity
     * @param jsonObject JSONObject to parse
     */
    @Override
    protected void onPostExecute(JSONObject jsonObject) {
        super.onPostExecute(jsonObject);
//        dialog.cancel();
        if (!authenticationError) {
            actionInterface.onCompletedAction(jsonObject,actionString); //Everything is okay, return to activity
        }
        else {
            actionInterface.onAuthorizationFailed(); //Authorization failed
        }

    }

    /**
     * Build an encoded query which is used to send POST-data
     * @param list The list of params to be included in the request
     * @return The encoded query if any params exist, blank if not
     */
    private String getQueryString(List<Pair<String,String>> list) {
        if (list.size() > 0) {
            Uri.Builder builder = new Uri.Builder();
            for (Pair<String,String> pair : list) {
                builder.appendQueryParameter(pair.first,pair.second);
            }
            return builder.build().getEncodedQuery();
        }
        return "";
    }

    /**
     * Build an URL for a GET-request
     * @param list The list of params to be included in the request
     * @return URL with all params included if any exist, the original URL if not
     */
    private String buildGetUrl(List<Pair<String,String>> list) {
        if (list.size() > 0) {
            Uri.Builder builder = Uri.parse(url).buildUpon();
            for (Pair<String,String> pair : list) {
                builder.appendQueryParameter(pair.first,pair.second);
            }
            return  builder.build().toString();
        }
        return url;
    }
}
