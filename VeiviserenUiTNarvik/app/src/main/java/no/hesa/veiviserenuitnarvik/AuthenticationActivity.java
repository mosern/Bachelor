package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.net.http.SslError;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.text.TextUtils;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.webkit.SslErrorHandler;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Toast;

import java.math.BigInteger;
import java.security.SecureRandom;

/**
 * Defines a simple web view that allows the user to log in using OAuth and an authentication server
 * Based on example from https://www.learn2crack.com/2014/01/android-oauth2-webview.html
 */
public class AuthenticationActivity extends AppCompatActivity implements SimpleGestureFilter.SimpleGestureListener {

    //Set up variables needed for the webview
    private static String CLIENT_ID = "and";
    private static String REDIRECT_URI = "http://localhost:123";
    private static String OAUTH_URL = "https://bacheloridsrv3.azurewebsites.net/identity/connect/authorize";
    private static String OAUTH_SCOPE = "api roles openid";
    private SharedPreferences pref;
    private SimpleGestureFilter detector;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_authentication);

        detector = new SimpleGestureFilter(this,this);
        WebView web = (WebView) findViewById(R.id.webv);
        web.getSettings().setJavaScriptEnabled(true);
        String nonceString = generateNonce();
        //Set up shared prefrences to save the token in private mode
        pref = getSharedPreferences("AppPref", MODE_PRIVATE);
        //Load the webview
        web.loadUrl(OAUTH_URL + "?redirect_uri=" + REDIRECT_URI + "&response_type=id_token%20token&client_id=" + CLIENT_ID + "&scope=" + OAUTH_SCOPE + "&nonce="+nonceString);
        web.setWebViewClient(new WebViewClient() {

            boolean authComplete = false;
            Intent resultIntent = new Intent();

            @Override
            public void onPageStarted(WebView view, String url, Bitmap favicon) {
                super.onPageStarted(view, url, favicon);

            }

            String authCode;

            /**
             * Runs when the authentication page is loaded, checks if URL contains a token and returns
             * @param view Specifies the view
             * @param url Specifies the url
             */
            @Override
            public void onPageFinished(WebView view, String url) {
                super.onPageFinished(view, url);
                if (url.contains("access_token=") && !authComplete) {
                    int start = TextUtils.indexOf(url, "access_token=") + "access_token=".length();
                    int end = TextUtils.indexOf(url, "&token_type=");
                    authCode = url.substring(start, end);

                    Log.i("", "CODE : " + authCode);
                    authComplete = true;
                    resultIntent.putExtra("code", authCode);
                    AuthenticationActivity.this.setResult(Activity.RESULT_OK, resultIntent);
                    setResult(Activity.RESULT_CANCELED, resultIntent);

                    SharedPreferences.Editor edit = pref.edit();
                    edit.putString("Code", authCode);
                    edit.putBoolean("LoggedInThisSession", true);
                    edit.commit();
                    Toast.makeText(getApplicationContext(), "Authorization Code is: " + authCode, Toast.LENGTH_SHORT).show();
                    Intent intent = new Intent(getApplicationContext(), MapActivity.class);
                    startActivity(intent);

                } else if (url.contains("error=access_denied")) {
                    Log.i("", "ACCESS_DENIED_HERE");
                    resultIntent.putExtra("code", authCode);
                    authComplete = true;
                    setResult(Activity.RESULT_CANCELED, resultIntent);
                    Toast.makeText(getApplicationContext(), "Error Occured", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onReceivedSslError(WebView view, SslErrorHandler handler, SslError error) {
                handler.proceed(); // Ignore SSL certificate errors
            }
        });
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_authentication, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();
        return super.onOptionsItemSelected(item);
    }

    private static String generateNonce() {
        SecureRandom random = new SecureRandom();
        return new BigInteger(130,random).toString(32);
    }

    /**
     * Handles swipe events
     * @param direction direction of the swipe
     */
    @Override
    public void onSwipe(int direction) {
        switch (direction) {
            case SimpleGestureFilter.SWIPE_RIGHT :
                finish();
                break;
            case SimpleGestureFilter.SWIPE_LEFT :
                finish();
                break;
            case SimpleGestureFilter.SWIPE_DOWN :
                break;
            case SimpleGestureFilter.SWIPE_UP :
                break;
        }
    }

    /**
     * Handles double taps
     */
    @Override
    public void onDoubleTap() {

    }

    /**
     * Handles touch events via SimpleGestureFilter-class
     * @param me motionevent
     * @return touch event
     */
    @Override
    public boolean dispatchTouchEvent(MotionEvent me) {
        // Call onTouchEvent of SimpleGestureFilter class
        this.detector.onTouchEvent(me);
        return super.dispatchTouchEvent(me);
    }
}