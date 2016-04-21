package no.hesa.veiviserenuitnarvik;

import android.annotation.TargetApi;
import android.app.SearchManager;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.hardware.Sensor;
import android.hardware.SensorManager;
import android.hardware.TriggerEvent;
import android.hardware.TriggerEventListener;
import android.os.Build;
import android.os.Bundle;
import android.os.Looper;
import android.preference.PreferenceManager;
import android.support.v4.view.MenuItemCompat;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.SearchView;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.Toast;

import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptor;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.Circle;
import com.google.android.gms.maps.model.CircleOptions;
import com.google.android.gms.maps.model.GroundOverlay;
import com.google.android.gms.maps.model.GroundOverlayOptions;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.gms.maps.model.PolylineOptions;
import com.indooratlas.android.sdk.IALocationManager;
import com.indooratlas.android.sdk.resources.IAFloorPlan;
import com.indooratlas.android.sdk.resources.IALatLng;
import com.indooratlas.android.sdk.resources.IAResourceManager;
import com.indooratlas.android.sdk.resources.IAResult;
import com.indooratlas.android.sdk.resources.IAResultCallback;
import com.indooratlas.android.sdk.resources.IATask;
import com.squareup.picasso.Picasso;
import com.squareup.picasso.RequestCreator;
import com.squareup.picasso.Target;

import org.json.JSONObject;

import no.hesa.positionlibrary.PositionLibrary;
import no.hesa.veiviserenuitnarvik.api.ActionInterface;
import no.hesa.veiviserenuitnarvik.api.Api;

@TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR2)
public class MapActivity extends AppCompatActivity implements OnMapReadyCallback,ActionInterface{

    private static final String TAG = "MapActivity";
    private static final int POLYLINEWIDTH = 4;

    /* used to decide when bitmap should be downscaled */
    private static final int MAX_DIMENSION = 2048;

    private GoogleMap mMap; // Might be null if Google Play services APK is not available.
    private Marker mMarker;
    private GroundOverlay mGroundOverlay;
    private IALocationManager mIALocationManager;
    private IAResourceManager mResourceManager;
    private IATask<IAFloorPlan> mFetchFloorPlanTask;
    private Target mLoadTarget;
    private Intent returnedCoordsFromSearchIntent;
    private PositionLibrary positionLibrary = null;

    private BroadcastReceiver positionLibOutputReceiver = null;
    private BroadcastReceiver searchLocationReceiver = null;

    private LatLng currentPosision = null;
    private LatLng targetPosition = null;

    Menu menuRef = null;

    private Circle circleOne;
    private Circle circleTwo;
    private Circle circleThree;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_map);

        // instantiate IALocationManager and IAResourceManager
        mIALocationManager = IALocationManager.create(this);
        mResourceManager = IAResourceManager.create(this);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        Api api = new Api(this, getApplicationContext().getResources());
        api.allUsers();
/*
        SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);
        String token = sharedPreferences.getString("Code",Api.NO_TOKEN);
        api.locationById(2,token);
*/
        returnedCoordsFromSearchIntent = getIntent();
    }

    @Override
    protected void onResume() {
        super.onResume();
        // get map fragment reference
        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager().findFragmentById(R.id.map);
        mapFragment.getMapAsync(this);

        fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_1_floorplanid));
    }

    @Override
    protected void onStop()
    {
        super.onStop();
        if (positionLibrary != null)
            positionLibrary.wifiPosition.unRegisterBroadcast(this);

        if (positionLibOutputReceiver != null)
            unregisterReceiver(positionLibOutputReceiver);
        //unregisterReceiver(searchLocationReceiver);
    }

    @Override
    public void onMapReady(GoogleMap googleMap) {
        mMap = googleMap;

        mMap.getUiSettings().setMapToolbarEnabled(false); // disable map toolbar:

        // Add a marker in Narvik and move the camera
        LatLng hin = new LatLng(68.436135, 17.434950);
        mMap.addMarker(new MarkerOptions().position(hin).title("UiT Narvik"));
        mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(hin, 17));


        if (returnedCoordsFromSearchIntent != null) {
            if (returnedCoordsFromSearchIntent.getAction() != null) {
                if (returnedCoordsFromSearchIntent.getAction().equals("no.hesa.veiviserennarvik.LAT_LNG_RETURN")) {
                    LatLng latLng = new LatLng(returnedCoordsFromSearchIntent.getDoubleExtra("lng",0),returnedCoordsFromSearchIntent.getDoubleExtra("lat",0));
                    mMap.addMarker(new MarkerOptions().position(latLng).title("TestLoc2"));
                    mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(latLng, 17));
                    if (currentPosision != null)
                    {
                        drawRoute(currentPosision, latLng);
                    }
                }
            }
        }

        registerOnMapClickReceiver();
//        registerPositionReceiver();
        registerSearchLocationReceiver();

        //Painting position marker
        circleThree = mMap.addCircle(new CircleOptions()
                .center(hin)
                .radius(3)
                .strokeColor(Color.rgb(202, 227, 247))
                .fillColor(Color.rgb(202, 227, 247)));

        circleTwo = mMap.addCircle(new CircleOptions()
                .center(hin)
                .radius(1.5)
                .strokeColor(Color.rgb(151, 200, 240))
                .fillColor(Color.rgb(151, 200, 240)));

        circleOne = mMap.addCircle(new CircleOptions()
                .center(hin)
                .radius(0.5)
                .strokeColor(Color.rgb(49, 146, 225))
                .fillColor(Color.rgb(49, 146, 225)));
    }

    private void drawRoute(LatLng a, LatLng b)
    {
        // draws a line between point a and point b
        mMap.addPolyline((new PolylineOptions())
                .add(a, b)
                .width(POLYLINEWIDTH)
                .color(getResources().getColor(R.color.blue))
                .geodesic(false)); // lat/long lines curved by the shape of the planet
    }

//region BROADCASTRECEIVERS
    private void registerOnMapClickReceiver()
    {
        // adds a marker
        mMap.setOnMapClickListener(new GoogleMap.OnMapClickListener() {
            @Override
            public void onMapClick(LatLng point) {
                if (mMarker != null) {
                    mMarker.remove();
                }
                // new CircleOptions().center(point).radius(3.0).fillColor(R.color.radiusfillcolor).strokeColor(R.color.radiusstrokecolor).strokeWidth(2)
                mMarker = mMap.addMarker(new MarkerOptions().position(point).title("Lat: " + point.latitude + " Lng: " + point.longitude));
                mMarker.setDraggable(true);
                mMarker.showInfoWindow();
            }
        });
    }

    private void registerPositionReceiver()
    {
        // TODO: Evgeniia 19.04 får Error receiving broadcast Intent { act=android.net.wifi.SCAN_RESULTS flg=0x4000010 (has extras) } in no.hesa.positionlibrary.WifiPosition$1@34b1cf2


        positionLibOutputReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                if (intent.getAction().equals("no.hesa.positionlibrary.Output")) {
                    double[] pos = intent.getDoubleArrayExtra("position");
                    if (pos[0] != 0 && pos[1] != 0)
                    {
                        //Toast.makeText(MapActivity.this, "User location received from library = " + pos[0] + "," + pos[1], Toast.LENGTH_LONG).show();
                        LatLng latLng = new LatLng(pos[0], pos[1]);
                        //mMap.clear();
                        //circleOne.setCenter(latLng);
                        //circleTwo.setCenter(latLng);
                        circleThree.setCenter(latLng);
                        circleTwo.setCenter(latLng);
                        circleOne.setCenter(latLng);
                        //mMap.addMarker(new MarkerOptions().position(latLng).title("UserLocAsDeterminedByLibrary\nLat:" + pos[0] + " Lng: " + pos[1]));
                        //mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(latLng, 17));
                        currentPosision = latLng;
                    }
                    else
                    {
                        Toast.makeText(MapActivity.this, "Sorry, can´t find your position.", Toast.LENGTH_LONG).show();
                    }
                }
            }
        };

        positionLibrary = new PositionLibrary();
        positionLibrary.wifiPosition.registerBroadcast(this);
        registerReceiver(positionLibOutputReceiver, new IntentFilter("no.hesa.positionlibrary.Output"));
    }

    private void registerSearchLocationReceiver()
    {
     //   searchLocationReceiver = new BroadcastReceiver() {
        class SearchLocationReceiver extends BroadcastReceiver {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (intent.getAction().equals("no.hesa.veiviserennarvik.LAT_LNG_RETURN")) {
                Toast.makeText(MapActivity.this, "intentReceiver onReceive method", Toast.LENGTH_LONG).show();
                LatLng latLng = new LatLng(intent.getDoubleExtra("lat",0),intent.getDoubleExtra("lng",0));
                mMap.addMarker(new MarkerOptions().position(latLng).title("TestLocFromBR"));
                mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(latLng, 17));
                mMap.addCircle(new CircleOptions().center(latLng).radius(3.0).fillColor(R.color.radiusfillcolor).strokeColor(R.color.radiusstrokecolor).strokeWidth(2));
                }
            }
        };
        searchLocationReceiver = new SearchLocationReceiver();
        registerReceiver(searchLocationReceiver, new IntentFilter("no.hesa.veiviserennarvik.LAT_LNG_RETURN"));
    }
//endregion

//region INDOORATLAS METHODS
        /**
         * Sets bitmap of floor plan as ground overlay on Google Maps
         */
    private void setupGroundOverlay(IAFloorPlan floorPlan, Bitmap bitmap) {

        if (mGroundOverlay != null)
        {
            mGroundOverlay.remove();
        }

        if (mMap != null)
        {
            BitmapDescriptor bitmapDescriptor = BitmapDescriptorFactory.fromBitmap(bitmap);
            IALatLng iaLatLng = floorPlan.getCenter();
            LatLng center = new LatLng(iaLatLng.latitude, iaLatLng.longitude);
            GroundOverlayOptions fpOverlay = new GroundOverlayOptions()
                    .image(bitmapDescriptor)
                    .position(center, floorPlan.getWidthMeters(), floorPlan.getHeightMeters())
                    .bearing(floorPlan.getBearing());

            mGroundOverlay = mMap.addGroundOverlay(fpOverlay);
        }
    }

    /**
     * Download floor plan using Picasso library.
     */
    private void fetchFloorPlanBitmap(final IAFloorPlan floorPlan) {

        final String url = floorPlan.getUrl();

        if (mLoadTarget == null) {
            mLoadTarget = new Target()
            {

                @Override
                public void onBitmapLoaded(Bitmap bitmap, Picasso.LoadedFrom from)
                {
                    Log.d(TAG, "onBitmap loaded with dimensions: " + bitmap.getWidth() + "x"
                            + bitmap.getHeight());
                    setupGroundOverlay(floorPlan, bitmap);
                }

                @Override
                public void onPrepareLoad(Drawable placeHolderDrawable)
                {
                    // N/A
                }

                @Override
                public void onBitmapFailed(Drawable placeHolderDraweble)
                {
                    Toast.makeText(MapActivity.this, "Failed to load bitmap",
                            Toast.LENGTH_SHORT).show();
                }
            };
        }

        RequestCreator request = Picasso.with(this).load(url);

        final int bitmapWidth = floorPlan.getBitmapWidth();
        final int bitmapHeight = floorPlan.getBitmapHeight();

        if (bitmapHeight > MAX_DIMENSION)
        {
            request.resize(0, MAX_DIMENSION);
        }
        else if (bitmapWidth > MAX_DIMENSION)
        {
            request.resize(MAX_DIMENSION, 0);
        }

        request.into(mLoadTarget);
    }

    /**
     * Fetches floor plan data from IndoorAtlas server.
     */
    private void fetchFloorPlan(String id) {

        // if there is already running task, cancel it
        cancelPendingNetworkCalls();

        final IATask<IAFloorPlan> task = mResourceManager.fetchFloorPlanWithId(id);

        task.setCallback(new IAResultCallback<IAFloorPlan>()
        {
            @Override
            public void onResult(IAResult<IAFloorPlan> result)
            {
                if (result.isSuccess() && result.getResult() != null) {
                    // retrieve bitmap for this floor plan metadata
                    fetchFloorPlanBitmap(result.getResult());
                }
                else
                {
                    // ignore errors if this task was already canceled
                    if (!task.isCancelled())
                    {
                        // do something with error
                        Toast.makeText(MapActivity.this,
                                "loading floor plan failed: " + result.getError(), Toast.LENGTH_LONG)
                                .show();
                        // remove current ground overlay
                        if (mGroundOverlay != null)
                        {
                            mGroundOverlay.remove();
                            mGroundOverlay = null;
                        }
                    }
                }
            }
        }, Looper.getMainLooper()); // deliver callbacks using main looper

        // keep reference to task so that it can be canceled if needed
        mFetchFloorPlanTask = task;

    }

    /**
     * Helper method to cancel current task if any.
     */
    private void cancelPendingNetworkCalls() {
        if (mFetchFloorPlanTask != null && !mFetchFloorPlanTask.isCancelled()) {
            mFetchFloorPlanTask.cancel();
        }
    }

//endregion

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        menuRef = menu;
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_map, menu);
        getSupportActionBar().setDisplayShowTitleEnabled(false); // hides toolbar title

        SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);
        String token = sharedPreferences.getString("Code",Api.NO_TOKEN);
        if (token.length() > 0) {
            menu.findItem(R.id.action_login).setVisible(false); // logIN menu item
            menu.findItem(R.id.action_logout).setVisible(true); // logOUT menu item
        } else {
            menu.findItem(R.id.action_login).setVisible(true); // logIN menu item
            menu.findItem(R.id.action_logout).setVisible(false); // logOUT menu item
        }

        // Retrieve the SearchView and plug it into SearchManager
        final SearchView searchView = (SearchView) MenuItemCompat.getActionView(menu.findItem(R.id.action_search));
        //searchView.setIconifiedByDefault(false); // autoexpands the search field
        SearchManager searchManager = (SearchManager) getSystemService(Context.SEARCH_SERVICE);
        searchView.setSearchableInfo(searchManager.getSearchableInfo(new ComponentName("no.hesa.veiviserenuitnarvik","no.hesa.veiviserenuitnarvik.SearchResultsActivity")));

        // listener for toolbar search submit
        searchView.setOnQueryTextListener(new SearchView.OnQueryTextListener() {

            @Override
            public boolean onQueryTextSubmit(String newText) {
                //Toast.makeText(MapActivity.this, "You searched for: " + newText, Toast.LENGTH_SHORT).show();
                Intent intent = new Intent(getApplicationContext(), SearchResultsActivity.class);
                intent.setAction(Intent.ACTION_SEARCH);
                intent.putExtra("query", newText);
                searchView.setQuery("", false); // clears the searchview without submitting
                searchView.clearFocus();
                startActivity(intent);
                return true;
            }

            @Override
            public boolean onQueryTextChange(String newText) {
                return false;
            }
        });
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        switch (item.getItemId()) {
            case R.id.action_login:
                Intent intent = new Intent(this,AuthenticationActivity.class);
                startActivity(intent);
                break;
            case R.id.action_logout:
                /*
        SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);
        String token = sharedPreferences.getString("Code",Api.NO_TOKEN);
        api.locationById(2,token);
        */
                SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);
                SharedPreferences.Editor editor = sharedPreferences.edit();
                editor.remove("Code");
                editor.apply();
                menuRef.findItem(R.id.action_login).setVisible(true); // logIN menu item
                menuRef.findItem(R.id.action_logout).setVisible(false); // logOUT menu item
                break;
            case R.id.action_settings:

                break;
            case R.id.action_measurement:
                startActivity(new Intent(getApplicationContext(), MeasurementActivity.class));
                break;
            case R.id.action_register_location_receiver:
                Toast.makeText(getApplicationContext(), "Listening for updates from positioning library", Toast.LENGTH_LONG).show();
                registerPositionReceiver();
                break;
            default:
                break;
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onCompletedAction(JSONObject jsonObject, String actionString) {

        switch (actionString) {
            case Api.ALL_USERS:
                //JSONObject dummyObject = jsonObject;
                break;
            case Api.DO_SEARCH:

            default:
                break;
        }
    }

    @Override
    public void onAuthorizationFailed() {
        //TODO: possible intent loop?
        SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);

        if (sharedPreferences.getBoolean("LoggedInThisSession", true)) {
            Intent startAuthorization = new Intent(this, AuthenticationActivity.class);
            startActivity(startAuthorization);
        }
    }
}
