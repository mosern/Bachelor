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
import android.graphics.drawable.Drawable;
import android.os.Build;
import android.os.Bundle;
import android.os.Looper;
import android.support.design.widget.FloatingActionButton;
import android.support.v4.content.ContextCompat;
import android.support.v4.view.MenuItemCompat;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.SearchView;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.Window;
import android.widget.ProgressBar;
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

import no.hesa.positionlibrary.Point;
import no.hesa.positionlibrary.PositionLibrary;
//import no.hesa.veiviserenuitnarvik.api.ActionInterface;
//import no.hesa.veiviserenuitnarvik.api.Api;
import no.hesa.positionlibrary.api.ActionInterface;
import no.hesa.positionlibrary.api.Api;

@TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR2)
public class MapActivity extends AppCompatActivity implements OnMapReadyCallback,ActionInterface{

    private static final String TAG = "MapActivity";
    private static final int POLYLINEWIDTH = 4;
    private static String DEFAULT_FLOORPLAN;

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
    private BroadcastReceiver pathPositionLibReceiver = null;
    private BroadcastReceiver searchLocationReceiver = null;

    private LatLng currentPosition = null;
    private LatLng targetPosition = null;

    private Polyline polyline = null;

    private Menu menuRef = null;

    private LatLngBounds elevationChangeBounds = null;

    private String currentFloorPlan;
    private int currentFloor;
    private ArrayList<ArrayList<Point>> receivedPath = null;

    private ProgressBar fetchMapSpinner;
    private boolean positioningEnabled = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        requestWindowFeature(Window.FEATURE_ACTION_BAR_OVERLAY);
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_map);

        DEFAULT_FLOORPLAN = getResources().getString(R.string.indooratlas_floor_2_floorplanid);

        // instantiate IALocationManager and IAResourceManager
        mIALocationManager = IALocationManager.create(this);
        mResourceManager = IAResourceManager.create(this);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        //getSupportActionBar().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));

        fetchMapSpinner = (ProgressBar)findViewById(R.id.fetch_map_progress_bar);
        positionLibrary = new PositionLibrary();
        registerButtonListeners();

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
    protected void onPause() {
        super.onPause();

        if (mMap != null) {
            SharedPreferences.Editor sharedPreferences = getSharedPreferences("MapActivityPrefs", MODE_PRIVATE).edit();

            float zoomLevel = mMap.getCameraPosition().zoom;
            sharedPreferences.putFloat("ZoomLevel", zoomLevel);


            if (currentPosition != null) {
                float lat = (float) currentPosition.latitude;
                float lng = (float) currentPosition.longitude;

                sharedPreferences.putFloat("CurrentLat", lat);
                sharedPreferences.putFloat("CurrentLng", lng);
            }

            if (!currentFloorPlan.isEmpty())
            {
                sharedPreferences.putString("CurrentFloorPlan", currentFloorPlan);
            }

            sharedPreferences.commit();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        // get map fragment reference
        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager().findFragmentById(R.id.map);
        mapFragment.getMapAsync(this);
    }

    @Override
    protected void onStart()
    {
        super.onStart();
        // TODO: check for sharedpreference
        SharedPreferences sharedPreferences = getSharedPreferences("MapActivityPrefs",MODE_PRIVATE);

        currentFloorPlan = sharedPreferences.getString("CurrentFloorPlan", DEFAULT_FLOORPLAN);
        fetchFloorPlan(currentFloorPlan);
    }

    @Override
    protected void onStop()
    {
        super.onStop();
        /*
        if (positionLibrary != null) {
            positionLibrary.wifiPosition.unRegisterBroadcast(this);
        }
*/
        if (positionLibOutputReceiver != null) {
            unregisterReceiver(positionLibOutputReceiver);
        }

        if (pathPositionLibReceiver != null) {
            unregisterReceiver(pathPositionLibReceiver);
        }

        if (searchLocationReceiver != null) {
            unregisterReceiver(searchLocationReceiver);
        }
    }

    @Override
    public void onMapReady(GoogleMap googleMap) {
        mMap = googleMap;

        mMap.getUiSettings().setMapToolbarEnabled(false); // disable map toolbar:
        mMap.getUiSettings().setIndoorLevelPickerEnabled(true);

        SharedPreferences sharedPreferences = getSharedPreferences("MapActivityPrefs",MODE_PRIVATE);

        float zoomLevel = sharedPreferences.getFloat("ZoomLevel", 17.0f);
        float lat = sharedPreferences.getFloat("CurrentLat", 68.436135f);
        float lng = sharedPreferences.getFloat("CurrentLng", 17.434950f);

        positioningEnabled = sharedPreferences.getBoolean("PositioningEnabled", false);
        enablePositioning(positioningEnabled, false);

        // TODO: 28/04/2016 first run only, maybe change to users position via GPS
        mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(lat, lng), zoomLevel));

        if (returnedCoordsFromSearchIntent != null) {
            if (returnedCoordsFromSearchIntent.getAction() != null) {
                if (returnedCoordsFromSearchIntent.getAction().equals("no.hesa.veiviserennarvik.LAT_LNG_RETURN")) {
                    LatLng latLng = new LatLng(returnedCoordsFromSearchIntent.getDoubleExtra("lat",0),returnedCoordsFromSearchIntent.getDoubleExtra("lng",0));
                    double floor = returnedCoordsFromSearchIntent.getDoubleExtra("floor", 1.0);
                    changeFloor((int)floor);
                    mMap.addMarker(new MarkerOptions().position(latLng).title("TestLoc2"));
                    mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(latLng, zoomLevel));
                    targetPosition = latLng;
                }
            }
        }

        registerOnMapClickReceiver();
//        registerPositionReceiver();
//        registerPathReceiver();
//        registerSearchLocationReceiver();


//region RECEIVEDPATH PLACEHOLDER
        receivedPath = new ArrayList<ArrayList<Point>>();
        ArrayList<Point> andreetg = new ArrayList<>();
        andreetg.add(new Point(68.4362723, 17.4353580, 2));
        andreetg.add(new Point(68.4361468, 17.4352132, 2));
        andreetg.add(new Point(68.4361089, 17.4352722, 2));
        andreetg.add(new Point(68.4360255, 17.4351280, 2));
        andreetg.add(new Point(68.4359827, 17.4353097, 2));

        ArrayList<Point> forstetg = new ArrayList<>();
        forstetg.add(new Point(68.4361723, 17.4353580, 1));
        forstetg.add(new Point(68.4360468, 17.4352132, 1));
        forstetg.add(new Point(68.4360089, 17.4352722, 1));
        forstetg.add(new Point(68.4359255, 17.4351280, 1));
        forstetg.add(new Point(68.4358827, 17.4353097, 1));

        receivedPath.add(andreetg);
        receivedPath.add(forstetg);
//endregion

        // order of floor traversal
        if (receivedPath != null) {
        //    drawNextFloor();
        }

        // TODO: add button to change floor after a path has been drawn
    }



    /**
     * Places markers (circles) with polylines connecting them along provided path.
     * Colors first and last circle differently.
     *
     * @param coordinateList
     * @return LatLang containing the last point added
     */
    private LatLng drawFloorPath(List<Point> coordinateList)
    {
        ArrayList<Polyline> polylineList = new ArrayList<Polyline>();
        ArrayList<LatLng> latLngList = new ArrayList<LatLng>();
        ArrayList<Circle> circleList = new ArrayList<Circle>();
        LatLng currentPoint = null;
        LatLng previousPoint = null;

        currentFloor = coordinateList.get(0).getFloor(); // all points will be on the same floor

        final double CIRCLERADIUS = 0.25;

        // mMap.clear(); // clears map of all markers

        for(int i = 0; i < coordinateList.size(); i++)
        {
            currentPoint = new LatLng(coordinateList.get(i).getLatitude(), coordinateList.get(i).getLongitude());
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
                previousPoint = new LatLng(coordinateList.get(i - 1).getLatitude(), coordinateList.get(i - 1).getLongitude());

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
/*
    private void drawNextFloor()
    {
        if (receivedPath.get(0) != null) {
 //           changeFloor(receivedPath.get(0).get(0).getFloor()); // downloads the floorplan

            LatLng endPoint = drawFloorPath(receivedPath.get(0)); // draws path
            receivedPath.remove(0); // removes the floor the path has already been drawn on

            if (endPoint != null) {
                elevationChangeBounds = toBounds(endPoint, 5); // registers a position + radius for elevationChangeBounds
            }
        }
        else {
            Toast.makeText(MapActivity.this, "You have arrived at your destination", Toast.LENGTH_SHORT).show();
            elevationChangeBounds = null; // unregister listener
        }
    }
*/
    private void drawUserPosition(LatLng currentPosition)
    {
        //Painting position marker
        Circle circleThree = mMap.addCircle(new CircleOptions()
                .center(currentPosition)
                .radius(3)
                .strokeColor(getResources().getColor(R.color.user_location_outer_ring))
                .fillColor(getResources().getColor(R.color.user_location_outer_ring))
                .zIndex(50));

        Circle circleTwo = mMap.addCircle(new CircleOptions()
                .center(currentPosition)
                .radius(1.5)
                .strokeColor(getResources().getColor(R.color.user_location_middle_ring))
                .fillColor(getResources().getColor(R.color.user_location_middle_ring))
                .zIndex(51));

        Circle circleOne = mMap.addCircle(new CircleOptions()
                .center(currentPosition)
                .radius(0.5)
                .strokeColor(getResources().getColor(R.color.user_location_inner_ring))
                .fillColor(getResources().getColor(R.color.user_location_inner_ring))
                .zIndex(52));

        circleThree.setCenter(currentPosition);
        circleTwo.setCenter(currentPosition);
        circleOne.setCenter(currentPosition);
    }

    private void changeFloor(int floor)
    {
        switch (floor) {
            case 0:
                break;
            case 1:
                fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_1_floorplanid));
                currentFloorPlan = getResources().getString(R.string.indooratlas_floor_1_floorplanid);
                currentFloor = 1;
                break;
            case 2:
                fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_2_floorplanid));
                currentFloorPlan = getResources().getString(R.string.indooratlas_floor_2_floorplanid);
                currentFloor = 2;
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            default:
                break;
        }
    }
/*
    // http://stackoverflow.com/questions/15319431/how-to-convert-a-latlng-and-a-radius-to-a-latlngbounds-in-android-google-maps-ap
    // http://googlemaps.github.io/android-maps-utils/
    public LatLngBounds toBounds(LatLng center, double radius) {
        LatLng southwest = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 225);
        LatLng northeast = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 45);
        return new LatLngBounds(southwest, northeast);
    }
    */
//region BUTTON LISTENERS
    //// TODO: 28/04/2016 hardcoded floor values when changing floor with buttons 
    private void registerButtonListeners()
    {
        FloatingActionButton floorUpFab = (FloatingActionButton) findViewById(R.id.floor_up);
        floorUpFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {                
                if (currentFloorPlan.compareTo(getResources().getString(R.string.indooratlas_floor_1_floorplanid)) == 0) {
                    changeFloor(2);
                    mMap.clear();
                }
            }
        });

        FloatingActionButton floorDownFab = (FloatingActionButton) findViewById(R.id.floor_down);
        floorDownFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (currentFloorPlan.compareTo(getResources().getString(R.string.indooratlas_floor_2_floorplanid)) == 0) {
                    changeFloor(1);
                    mMap.clear();
                }
            }
        });

        FloatingActionButton automaticPositioningFab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
        automaticPositioningFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                enablePositioning(positioningEnabled, true);
            }
        });

        FloatingActionButton myLocationFab = (FloatingActionButton) findViewById(R.id.fab_my_location);
        myLocationFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (currentPosition != null) {
                    mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(currentPosition, 20));
                }
            }
        });
    }
//endregion

    private void enablePositioning(boolean positioningEnabled, boolean showToast)
    {
        SharedPreferences.Editor sharedPreferences = getSharedPreferences("MapActivityPrefs", MODE_PRIVATE).edit();
        if (!positioningEnabled) // turn on
        {
            FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
            fab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_on_white_24dp));
            this.positioningEnabled = true;
            sharedPreferences.putBoolean("PositioningEnabled", positioningEnabled);

            if (showToast) {
                Toast.makeText(getApplicationContext(), getResources().getString(R.string.positioning_automatic_on), Toast.LENGTH_SHORT).show();
            }
            registerPositionReceiver();
        }
        else // turn off
        {
            FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
            fab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_off_white_24dp));
            this.positioningEnabled = false;
            sharedPreferences.putBoolean("PositioningEnabled", positioningEnabled);

            if (positionLibOutputReceiver != null) {
                unregisterReceiver(positionLibOutputReceiver);
                positionLibOutputReceiver = null;
                if (showToast) {
                    Toast.makeText(getApplicationContext(), getResources().getString(R.string.positioning_automatic_off), Toast.LENGTH_SHORT).show();
                }
            }
        }
        sharedPreferences.commit();
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
                currentPosition = new LatLng(point.latitude, point.longitude);
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

                        currentPosition = latLng;

                        drawUserPosition(currentPosition);

                        if (floor != currentFloor) {
                            changeFloor(floor);
                            mMap.clear();
                        }
/*
                        if (elevationChangeBounds != null) {
                            if (elevationChangeBounds.contains(latLng)) {
                                drawNextFloor(); // loads the next map and draws the path when end point is reached
                            }
                        }
*/
                    }
                    else
                    {
                        Toast.makeText(MapActivity.this, getResources().getString(R.string.positioning_unable_to_locate_user), Toast.LENGTH_SHORT).show();
                    }
                }
            }
        };

        positionLibrary.wifiPosition.registerBroadcast(this);
        registerReceiver(positionLibOutputReceiver, new IntentFilter("no.hesa.positionlibrary.Output"));
    }

    private void registerPathReceiver()
    {
        pathPositionLibReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                if (intent.getAction().equals("no.hesa.positionlibrary.Output")) { // change

                }
            }
        };


        positionLibrary.wifiPosition.registerBroadcast(this);
        registerReceiver(pathPositionLibReceiver, new IntentFilter("no.hesa.positionlibrary.Output")); // change
    }
/*
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
    */
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
            fetchMapSpinner.setVisibility(View.GONE);
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
                    fetchMapSpinner.setVisibility(View.GONE);
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
        fetchMapSpinner.setVisibility(View.VISIBLE);

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
            /*
            case R.id.action_settings:

                break;
            case R.id.action_measurement:
                startActivity(new Intent(getApplicationContext(), MeasurementActivity.class));
                break;
                */
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
