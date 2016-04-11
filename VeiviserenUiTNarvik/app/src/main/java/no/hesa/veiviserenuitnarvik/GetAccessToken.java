package no.hesa.veiviserenuitnarvik;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.URL;
import org.json.JSONObject;

import android.net.Uri;
import android.util.Log;

import javax.net.ssl.HttpsURLConnection;

/**
 * Created by ThaSimon on 08.04.2016.
 */
public class GetAccessToken {
    static InputStream is = null;
    static JSONObject jObj = null;
    static String json = "";
    public GetAccessToken() {
    }
    public JSONObject gettoken(String address,String token,String client_id,String client_secret,String redirect_uri,String grant_type) {
        try {
            URL url = new URL(address);
            HttpsURLConnection conn = (HttpsURLConnection) url.openConnection();
            conn.setReadTimeout(10000);
            conn.setConnectTimeout(15000);
            conn.setRequestMethod("POST");
            conn.setDoInput(true);
            conn.setDoOutput(false);
            conn.setRequestProperty("Content-Type","application/x-www-form-urlencoded");
            Uri.Builder builder = new Uri.Builder().appendQueryParameter("code",token).appendQueryParameter("client_id",client_id).
                                                    appendQueryParameter("client_secret",client_secret).appendQueryParameter("redirect_uri",redirect_uri).appendQueryParameter("grant_type",grant_type);
            String query = builder.build().getEncodedQuery();

            OutputStream os = conn.getOutputStream();
            BufferedWriter writer = new BufferedWriter(new OutputStreamWriter(os,"UTF-8"));
            writer.write(query);
            writer.flush();
            writer.close();
            os.close();
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
        // Return JSON String
        return jObj;
    }
}
