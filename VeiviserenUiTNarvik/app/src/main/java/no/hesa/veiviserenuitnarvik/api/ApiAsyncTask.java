package no.hesa.veiviserenuitnarvik.api;

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
 * Created by ThaSimon on 09.04.2016.
 */
public class ApiAsyncTask extends AsyncTask<List<Pair<String,String>>,Void,JSONObject>{
    private ActionInterface actionInterface;
    private String actionString;
    private String url;
    private String dataType;
    public ApiAsyncTask(ActionInterface actionInterface,String actionString, String url) {
        this(actionInterface,actionString,url,"GET");
    }

    public ApiAsyncTask(ActionInterface actionInterface, String actionString,String url,String dataType) {
        this.actionInterface = actionInterface;
        this.actionString = actionString;
        this.url = url;
        this.dataType = dataType;
    }
    @Override
    protected JSONObject doInBackground(List<Pair<String,String>>... params) {
        JSONObject jObj = null;
        try {
            URL urlObject = new URL(url);
            HttpsURLConnection conn = (HttpsURLConnection) urlObject.openConnection();
            conn.setReadTimeout(10000);
            conn.setConnectTimeout(15000);
            conn.setRequestMethod(dataType);
            conn.setDoInput(true);
            conn.setDoOutput(false);
            if (dataType.equals("POST")) {
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
                BufferedReader br = new BufferedReader(new InputStreamReader(conn.getInputStream(),"iso-8859-1"),8);
                while ((line = br.readLine()) != null) {
                    sb.append(line+"\n");
                }
                String json = sb.toString();
                jObj = new JSONObject(json);
            }

        }
        catch (Exception ex) {
            ex.printStackTrace();
        }
        return jObj;
    }

    @Override
    protected void onPostExecute(JSONObject jsonObject) {
        super.onPostExecute(jsonObject);
        actionInterface.onCompletedAction(jsonObject,actionString);
    }

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
}
