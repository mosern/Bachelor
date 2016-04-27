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
import android.os.Build;
import android.os.Bundle;
import android.os.Looper;
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
import com.google.android.gms.maps.model.LatLngBounds;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.gms.maps.model.Polyline;
import com.google.android.gms.maps.model.PolylineOptions;
import com.google.maps.android.SphericalUtil;
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

import java.util.ArrayList;
import java.util.List;

import no.hesa.positionlibrary.PositionLibrary;
//import no.hesa.veiviserenuitnarvik.api.ActionInterface;
//import no.hesa.veiviserenuitnarvik.api.Api;
import no.hesa.positionlibrary.api.ActionInterface;
import no.hesa.positionlibrary.api.Api;
import no.hesa.veiviserenuitnarvik.dataclasses.Coordinate;

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

    private LatLng currentPosition = null;
    private LatLng targetPosition = null;

    private Polyline polyline = null;

    Menu menuRef = null;

    private Circle circleOne;
    private Circle circleTwo;
    private Circle circleThree;

    private LatLngBounds elevationChangeBounds = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_map);

        // instantiate IALocationManager and IAResourceManager
        mIALocationManager = IALocationManager.create(this);
        mResourceManager = IAResourceManager.create(this);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        //Api api = new Api(this, getApplicationContext().getResources());
        Api api = new Api(this);
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

        fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_1_floorplanid)); // 1 etg
     //   fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_2_floorplanid)); // 2 etg
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
                    targetPosition = latLng;
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

        List<Coordinate> list = new ArrayList<>();
        list.add(new Coordinate(1, 68.4362723, 17.4353580, 2.0));
        list.add(new Coordinate(2, 68.4361468, 17.4352132, 2.0));
        list.add(new Coordinate(3, 68.4361089, 17.4352722, 2.0));
        list.add(new Coordinate(4, 68.4360255, 17.4351280, 2.0));
        list.add(new Coordinate(5, 68.4359827, 17.4353097, 2.0));


        LatLng endPoint = drawFloorPath(list);
        if (endPoint != null)
        {
            elevationChangeBounds = toBounds(endPoint, 5);
        }
    }

    // http://stackoverflow.com/questions/15319431/how-to-convert-a-latlng-and-a-radius-to-a-latlngbounds-in-android-google-maps-ap
    // http://googlemaps.github.io/android-maps-utils/
    public LatLngBounds toBounds(LatLng center, double radius) {
        LatLng southwest = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 225);
        LatLng northeast = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 45);
        return new LatLngBounds(southwest, northeast);
    }

    private void drawRoute(LatLng a, LatLng b)
    {
        // draws a line between point a and point b
        if (polyline != null)
            polyline.remove();

        PolylineOptions po = new PolylineOptions()
                .add(a, b)
                .width(POLYLINEWIDTH)
                .color(getResources().getColor(R.color.blue))
                .geodesic(false); // lat/long lines curved by the shape of the planet

        polyline = mMap.addPolyline(po);
    }

    /**
     * Places markers (circles) with polylines connecting them along provided path.
     * Colors first and last circle differently.
     *
     * @param coordinateList
     * @return LatLang containing the last point added
     */
    private LatLng drawFloorPath(List<Coordinate> coordinateList)
    {
        ArrayList<Polyline> polylineList = new ArrayList<Polyline>();
        ArrayList<LatLng> latLngList = new ArrayList<LatLng>();
        ArrayList<Circle> circleList = new ArrayList<Circle>();
        LatLng currentPoint = null;
        LatLng previousPoint = null;

        final double CIRCLERADIUS = 0.25;

        // mMap.clear(); // clears map of all markers

        for(int i = 0; i < coordinateList.size(); i++)
        {
            currentPoint = new LatLng(coordinateList.get(i).getLat(), coordinateList.get(i).getLng());
            latLngList.add(currentPoint);

            // first point
            if (latLngList.size() == 1)
            {
                CircleOptions co = new CircleOptions()
                        .center(currentPoint)
                        .radius(CIRCLERADIUS)
                        .strokeColor(getResources().getColor(R.color.route_circle_start_color))
                        .fillColor(getResources().getColor(R.color.route_circle_start_color))
                        .zIndex(0.7f);

                circleList.add(mMap.addCircle(co));
            }

            if (latLngList.size() > 1)
            {
                previousPoint = new LatLng(coordinateList.get(i - 1).getLat(), coordinateList.get(i - 1).getLng());

                PolylineOptions po = new PolylineOptions()
                        .add(previousPoint, currentPoint)
                        .width(POLYLINEWIDTH)
                        .color(getResources().getColor(R.color.route_polyline_color))
                        .geodesic(false)
                        .zIndex(0.4f); // lat/long lines curved by the shape of the planet

                polylineList.add(mMap.addPolyline(po));

                // all points except first and last
                if (latLngList.size() < coordinateList.size()) {
                    CircleOptions co = new CircleOptions()
                            .center(currentPoint)
                            .radius(CIRCLERADIUS)
                            .strokeColor(getResources().getColor(R.color.route_circle_color))
                            .fillColor(getResources().getColor(R.color.route_circle_color))
                            .zIndex(0.7f);

                    circleList.add(mMap.addCircle(co));
                }
                else // last point
                {
                    CircleOptions co = new CircleOptions()
                            .center(currentPoint)
                            .radius(CIRCLERADIUS)
                            .strokeColor(getResources().getColor(R.color.route_circle_end_color))
                            .fillColor(getResources().getColor(R.color.route_circle_end_color))
                            .zIndex(0.7f);

                    circleList.add(mMap.addCircle(co));
                }
            }
        }
        return currentPoint;
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
        positionLibOutputReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                if (intent.getAction().equals("no.hesa.positionlibrary.Output")) {
                    double[] pos = intent.getDoubleArrayExtra("position");
                    int floor = intent.getIntExtra("floor", 1);
                    if (pos[0] != 0 && pos[1] != 0)
                    {
                        LatLng latLng = new LatLng(pos[0], pos[1]);
                        //Move position marker
                        circleThree.setCenter(latLng);
                        circleTwo.setCenter(latLng);
                        circleOne.setCenter(latLng);
                        currentPosition = latLng;
                        //TODO: remove temp drawroute
                        if (targetPosition != null) {
                            drawRoute(currentPosition, targetPosition);
                        }
                        //TODO: add code for elevation change upon received position
                        if(elevationChangeBounds.contains(latLng))
                        {
                            // reguest elevation change
                        }
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
                targetPosition = latLng;
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
                    .bearing(floorPlan.getBearing())
                    .zIndex(0.1f);

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
